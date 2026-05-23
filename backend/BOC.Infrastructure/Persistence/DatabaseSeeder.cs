using System;
using System.Linq;
using System.Threading.Tasks;
using BOC.Application.Common.Interfaces;
using BOC.Domain.Entities;
using BOC.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BOC.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly BOCDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(BOCDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        if (!await _context.AppRoles.AnyAsync())
        {
            await SeedRolesAsync();
        }

        if (!await _context.AppUsers.AnyAsync())
        {
            await SeedUsersAsync();
        }
        
        if (!await _context.ResearchCategories.AnyAsync())
        {
            await SeedResearchDataAsync();
        }
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            new AppRole { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN", Description = "System Administrator" },
            new AppRole { Id = Guid.NewGuid(), Name = "Researcher", NormalizedName = "RESEARCHER", Description = "Normal Researcher" },
            new AppRole { Id = Guid.NewGuid(), Name = "Committee Chairman", NormalizedName = "COMMITTEE CHAIRMAN", Description = "Chairman of the Evaluation Committee" },
            new AppRole { Id = Guid.NewGuid(), Name = "Evaluator", NormalizedName = "EVALUATOR", Description = "Research Evaluator" }
        };

        await _context.AppRoles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync()
    {
        var adminRole = await _context.AppRoles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");
        var researcherRole = await _context.AppRoles.FirstOrDefaultAsync(r => r.NormalizedName == "RESEARCHERS");
        var evaluatorRole = await _context.AppRoles.FirstOrDefaultAsync(r => r.NormalizedName == "EXTERNAL EVALUATORS");

        var adminUser = new AppUser
        {
            Id = Guid.NewGuid(),
            EmployeeID = "ADM-001",
            NationalID = "123456789012",
            FullName = "مدير النظام",
            Email = "admin@boc.oil.gov.iq",
            NormalizedEmail = "ADMIN@BOC.OIL.GOV.IQ",
            PasswordHash = _passwordHasher.HashPassword("Admin@123"),
            RoleId = adminRole!.Id,
            AccountStatus = AccountStatus.Active,
            IsEmailConfirmed = true,
            TwoFactorEnabled = false
        };

        var researcherUser = new AppUser
        {
            Id = Guid.NewGuid(),
            EmployeeID = "RES-001",
            NationalID = "223456789012",
            FullName = "د. باحث عراقي",
            Email = "researcher@boc.oil.gov.iq",
            NormalizedEmail = "RESEARCHER@BOC.OIL.GOV.IQ",
            PasswordHash = _passwordHasher.HashPassword("User@123"),
            RoleId = researcherRole!.Id,
            AccountStatus = AccountStatus.Active,
            IsEmailConfirmed = true,
            TwoFactorEnabled = false
        };
        
        var evaluatorUser = new AppUser
        {
            Id = Guid.NewGuid(),
            EmployeeID = "EVL-001",
            NationalID = "323456789012",
            FullName = "أ. مقيم خبير",
            Email = "evaluator@boc.oil.gov.iq",
            NormalizedEmail = "EVALUATOR@BOC.OIL.GOV.IQ",
            PasswordHash = _passwordHasher.HashPassword("User@123"),
            RoleId = evaluatorRole!.Id,
            AccountStatus = AccountStatus.Active,
            IsEmailConfirmed = true,
            TwoFactorEnabled = false
        };

        await _context.AppUsers.AddRangeAsync(adminUser, researcherUser, evaluatorUser);
        await _context.SaveChangesAsync();
    }

    private async Task SeedResearchDataAsync()
    {
        var directorate = new Directorate { Id = Guid.NewGuid(), Name = "هيئة الحقول", Code = "DIR-01", IsActive = true };
        var department = new Department { Id = Guid.NewGuid(), Name = "قسم هندسة وتطوير الحقول", Code = "D-001", DirectorateId = directorate.Id, IsActive = true };
        var specialization = new Specialization { Id = Guid.NewGuid(), Name = "هندسة البترول والمكامن", Code = "SPEC-01", IsActive = true };
        
        await _context.Directorates.AddAsync(directorate);
        await _context.Departments.AddAsync(department);
        await _context.Specializations.AddAsync(specialization);
        await _context.SaveChangesAsync();

        var category = new ResearchCategory { Id = Guid.NewGuid(), Name = "هندسة المكامن", SpecializationId = specialization.Id, IsActive = true };
        await _context.ResearchCategories.AddAsync(category);
        await _context.SaveChangesAsync();

        var researcher = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == "researcher@boc.oil.gov.iq");

        var research = new ResearchPaper
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "BOC-RES-2026-001",
            Title = "تطوير أنظمة حقن المياه في حقل مجنون",
            Abstract = "بحث يدرس تحسين ضغط المكمن عبر حقن المياه",
            ResearcherId = researcher!.Id,
            CategoryId = category.Id,
            DepartmentId = department.Id,
            DirectorateId = directorate.Id,
            State = ResearchState.Pending_Secretary_Screening,
            SubmissionDate = DateTime.UtcNow.AddDays(-5)
        };

        await _context.ResearchPapers.AddAsync(research);
        await _context.SaveChangesAsync();
    }
}
