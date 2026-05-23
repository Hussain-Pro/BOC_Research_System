using System;
using System.Collections.Generic;
using BOC.Domain.Common;

namespace BOC.Domain.Entities;

public class ResearchAttachment : Entity
{
    public Guid ResearchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Sha256Hash { get; set; } = string.Empty;
    public int VersionNumber { get; set; } = 1;
    public bool IsLatestVersion { get; set; } = true;
    public Guid UploadedById { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ResearchPaper ResearchPaper { get; set; } = null!;
    public virtual AppUser UploadedBy { get; set; } = null!;
    public virtual ICollection<FileAccessToken> FileAccessTokens { get; set; } = new List<FileAccessToken>();
}
