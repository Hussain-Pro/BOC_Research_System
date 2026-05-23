using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using BOC.Application.Common.Interfaces;

namespace BOC.Application.Features.Files.Queries;

public record GetFileStreamQuery(string Token) : IRequest<FileStreamResultDto>;

public class FileStreamResultDto
{
    public Stream Stream { get; set; } = null!;
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class GetFileStreamQueryHandler : IRequestHandler<GetFileStreamQuery, FileStreamResultDto>
{
    private readonly IBOCDbContext _context;

    public GetFileStreamQueryHandler(IBOCDbContext context)
    {
        _context = context;
    }

    public async Task<FileStreamResultDto> Handle(GetFileStreamQuery request, CancellationToken cancellationToken)
    {
        // Placeholder for FileProxy logic resolving FTP stream via token
        throw new NotImplementedException();
    }
}
