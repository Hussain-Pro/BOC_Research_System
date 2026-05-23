using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FluentFTP;
using BOC.Application.Common.Interfaces;

namespace BOC.Infrastructure.Storage;

public class FtpFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FtpFileStorageService> _logger;

    public FtpFileStorageService(IConfiguration configuration, ILogger<FtpFileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private FtpClient GetFtpClient()
    {
        var host = _configuration["Ftp:Host"];
        var port = int.TryParse(_configuration["Ftp:Port"], out var p) ? p : 21;
        var username = _configuration["Ftp:Username"];
        var password = _configuration["Ftp:Password"];

        return new FtpClient(host, username, password, port);
    }

    public async Task<string> UploadFileAsync(string fileName, Stream contentStream, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Ftp:Host"];
        var rootDir = _configuration["Ftp:RootDirectory"] ?? "/uploads";
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var ftpPath = $"{rootDir}/{uniqueName}";

        if (string.IsNullOrEmpty(host))
        {
            var localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp_ftp_uploads");
            Directory.CreateDirectory(localPath);
            var fullPath = Path.Combine(localPath, uniqueName);
            
            _logger.LogWarning("FTP Host not configured. Saving file locally to: {Path}", fullPath);
            
            using var fileStream = File.Create(fullPath);
            await contentStream.CopyToAsync(fileStream, cancellationToken);
            return fullPath;
        }

        _logger.LogInformation("Uploading {FileName} to FTP path {FtpPath}...", fileName, ftpPath);
        
        return await Task.Run(() =>
        {
            using var client = GetFtpClient();
            client.Connect();
            
            if (!client.DirectoryExists(rootDir))
            {
                client.CreateDirectory(rootDir);
            }

            var status = client.UploadStream(contentStream, ftpPath, FtpRemoteExists.Overwrite, true);
            if (status == FtpStatus.Failed)
            {
                throw new Exception($"Failed to upload file {fileName} to FTP server.");
            }

            return ftpPath;
        }, cancellationToken);
    }

    public async Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Ftp:Host"];

        if (string.IsNullOrEmpty(host) || File.Exists(filePath))
        {
            if (File.Exists(filePath))
            {
                _logger.LogInformation("Streaming local file: {Path}", filePath);
                return File.OpenRead(filePath);
            }
            throw new FileNotFoundException("Local fallback file not found.", filePath);
        }

        _logger.LogInformation("Downloading file from FTP: {FilePath}...", filePath);
        
        return await Task.Run(() =>
        {
            using var client = GetFtpClient();
            client.Connect();

            var memoryStream = new MemoryStream();
            var success = client.DownloadStream(memoryStream, filePath);
            if (!success)
            {
                throw new Exception($"Failed to download file {filePath} from FTP server.");
            }

            memoryStream.Position = 0;
            return (Stream)memoryStream;
        }, cancellationToken);
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Ftp:Host"];

        if (string.IsNullOrEmpty(host) || File.Exists(filePath))
        {
            if (File.Exists(filePath))
            {
                _logger.LogInformation("Deleting local file: {Path}", filePath);
                File.Delete(filePath);
            }
            return;
        }

        _logger.LogInformation("Deleting file from FTP: {FilePath}...", filePath);
        
        await Task.Run(() =>
        {
            using var client = GetFtpClient();
            client.Connect();
            client.DeleteFile(filePath);
        }, cancellationToken);
    }
}
