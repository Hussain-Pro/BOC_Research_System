using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BOC.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string fileName, Stream contentStream, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
}
