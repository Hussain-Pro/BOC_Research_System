using System;
using System.Collections.Generic;
using BOC.Domain.Common;
using BOC.Domain.Enums;

namespace BOC.Domain.Entities;

public class ResearchPaper : Entity
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public Guid ResearcherId { get; set; }
    public Guid? CategoryId { get; set; }
    public ResearchState State { get; set; } = ResearchState.Draft;
    public Guid? ReplacedResearchId { get; set; }
    public Guid? MeetingId { get; set; }
    public Guid? MeetingMinutesId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid DirectorateId { get; set; }
    public decimal? FinalScore { get; set; }
    public bool IsArchived { get; private set; } // Database computed column
    public DateTime? SubmissionDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public virtual AppUser Researcher { get; set; } = null!;
    public virtual ResearchCategory? Category { get; set; }
    public virtual ResearchPaper? ReplacedResearch { get; set; }
    public virtual Meeting? Meeting { get; set; }
    public virtual MeetingMinutes? MeetingMinutes { get; set; }
    public virtual Department Department { get; set; } = null!;
    public virtual Directorate Directorate { get; set; } = null!;

    public virtual ICollection<ResearchPaper> ReplacedByResearchPapers { get; set; } = new List<ResearchPaper>();
    public virtual ICollection<ResearchVersion> Versions { get; set; } = new List<ResearchVersion>();
    public virtual ICollection<ResearchCorrection> Corrections { get; set; } = new List<ResearchCorrection>();
    public virtual ICollection<Substitution> OriginalSubstitutions { get; set; } = new List<Substitution>();
    public virtual ICollection<Substitution> NewSubstitutions { get; set; } = new List<Substitution>();
    public virtual ICollection<PlagiarismLockout> PlagiarismLockouts { get; set; } = new List<PlagiarismLockout>();
    public virtual ICollection<EvaluatorAssignment> EvaluatorAssignments { get; set; } = new List<EvaluatorAssignment>();
    public virtual ICollection<TriageMapping> TriageMappings { get; set; } = new List<TriageMapping>();
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public virtual ICollection<ResearchAttachment> Attachments { get; set; } = new List<ResearchAttachment>();
    public virtual ICollection<ChairmanScore> ChairmanScores { get; set; } = new List<ChairmanScore>();
}
