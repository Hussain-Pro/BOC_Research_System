using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;
using BOC.Domain.Enums;
using BOC.Domain.Events;

namespace BOC.Application.Features.Research.Commands;

public record SubmitResearchCommand(
    string Title,
    string? Abstract,
    Guid? CategoryId,
    Guid ResearcherId,
    Guid DepartmentId,
    Guid DirectorateId,
    string FileName,
    Stream FileStream,
    string ContentType,
    bool SubmitImmediately) : IRequest<Guid>;

public class SubmitResearchCommandValidator : AbstractValidator<SubmitResearchCommand>
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx", ".xlsx", ".png", ".jpg" };

    public SubmitResearchCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500);

        RuleFor(x => x.ResearcherId)
            .NotEmpty().WithMessage("Researcher is required.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required.");

        RuleFor(x => x.DirectorateId)
            .NotEmpty().WithMessage("Directorate is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(ValidateExtension).WithMessage("Invalid file type. Only PDF, DOC, DOCX, XLSX, PNG, and JPG are allowed.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File content is required.")
            .Must(s => s.Length <= 50 * 1024 * 1024).WithMessage("File size exceeds the 50MB limit.");
    }

    private bool ValidateExtension(string filename)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}

public class SubmitResearchCommandHandler : IRequestHandler<SubmitResearchCommand, Guid>
{
    private readonly IBOCDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public SubmitResearchCommandHandler(IBOCDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(SubmitResearchCommand request, CancellationToken cancellationToken)
    {
        // 1. Calculate SHA-256 hash of the stream
        using var sha256 = SHA256.Create();
        request.FileStream.Position = 0;
        var hashBytes = await sha256.ComputeHashAsync(request.FileStream, cancellationToken);
        var fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        
        // 2. Upload file to FTP storage
        request.FileStream.Position = 0;
        var filePath = await _fileStorage.UploadFileAsync(request.FileName, request.FileStream, cancellationToken);

        // 3. Generate a unique Tracking Number
        var trackingSuffix = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        var trackingNumber = $"BOC-RES-{DateTime.UtcNow.Year}-{trackingSuffix}";

        // 4. Create ResearchPaper
        var state = request.SubmitImmediately ? ResearchState.Pending_Secretary_Screening : ResearchState.Draft;
        
        var paper = new ResearchPaper
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            Title = request.Title.Trim(),
            Abstract = request.Abstract?.Trim(),
            ResearcherId = request.ResearcherId,
            CategoryId = request.CategoryId,
            State = state,
            DepartmentId = request.DepartmentId,
            DirectorateId = request.DirectorateId,
            SubmissionDate = request.SubmitImmediately ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.ResearchPapers.Add(paper);

        // 5. Create Attachment
        var attachment = new ResearchAttachment
        {
            Id = Guid.NewGuid(),
            ResearchId = paper.Id,
            FileName = request.FileName,
            FilePath = filePath,
            FileSize = request.FileStream.Length,
            ContentType = request.ContentType,
            Sha256Hash = fileHash,
            VersionNumber = 1,
            IsLatestVersion = true,
            UploadedById = request.ResearcherId,
            UploadedAt = DateTime.UtcNow
        };

        _context.ResearchAttachments.Add(attachment);

        // 6. Create Version
        var version = new ResearchVersion
        {
            Id = Guid.NewGuid(),
            ResearchId = paper.Id,
            VersionNumber = 1,
            DocumentPath = filePath,
            ChangeSummary = "Initial submission",
            CreatedAt = DateTime.UtcNow
        };

        _context.ResearchVersions.Add(version);

        if (request.SubmitImmediately)
        {
            paper.AddDomainEvent(new ResearchSubmittedEvent(paper.Id, paper.TrackingNumber, paper.Title, paper.ResearcherId));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return paper.Id;
    }
}
