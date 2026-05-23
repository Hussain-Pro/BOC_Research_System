using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Research.Queries;

public record DocumentStreamDto(Stream Stream, string ContentType, string FileName);

public record GetResearchDocumentQuery(Guid ResearchId) : IRequest<DocumentStreamDto>;

public class GetResearchDocumentQueryHandler : IRequestHandler<GetResearchDocumentQuery, DocumentStreamDto>
{
    private readonly IBOCDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public GetResearchDocumentQueryHandler(IBOCDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<DocumentStreamDto> Handle(GetResearchDocumentQuery request, CancellationToken cancellationToken)
    {
        var paper = await _context.ResearchPapers
            .AsNoTracking()
            .Include(p => p.Attachments)
            .FirstOrDefaultAsync(p => p.Id == request.ResearchId, cancellationToken)
            ?? throw new ValidationException("Research paper not found.");

        var attachment = paper.Attachments
            .OrderByDescending(a => a.UploadedAt)
            .FirstOrDefault();

        if (attachment == null)
        {
            throw new ValidationException("No document attached to this research paper.");
        }

        var stream = await _fileStorageService.DownloadFileAsync(attachment.FilePath, cancellationToken);
        return new DocumentStreamDto(stream, attachment.ContentType ?? "application/pdf", attachment.FileName);
    }
}
