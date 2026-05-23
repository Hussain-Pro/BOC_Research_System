using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Quartz;
using BOC.Application.Common.Interfaces;
using BOC.Infrastructure.Persistence;
using BOC.Infrastructure.Persistence.Interceptors;
using BOC.Infrastructure.Caching;
using BOC.Infrastructure.Emails;
using BOC.Infrastructure.Storage;
using BOC.Infrastructure.BackgroundJobs;
using BOC.Infrastructure.Security;

namespace BOC.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditLogInterceptor>();
        services.AddScoped<OutboxInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=127.0.0.1,1433;Database=BOC_Research_Evaluation;User ID=sa;Password=12345;TrustServerCertificate=true;Column Encryption Setting=enabled;";

        services.AddDbContext<BOCDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditLogInterceptor>();
            var outboxInterceptor = sp.GetRequiredService<OutboxInterceptor>();

            options.UseSqlServer(connectionString)
                   .AddInterceptors(auditInterceptor, outboxInterceptor);
        });

        services.AddScoped<IBOCDbContext>(provider => provider.GetRequiredService<BOCDbContext>());

        var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379,abortConnect=false";
        try
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
            });
        }
        catch
        {
            var mockMultiplexer = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
            services.AddSingleton<IConnectionMultiplexer>(mockMultiplexer);
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IFileStorageService, FtpFileStorageService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<DatabaseSeeder>();

        services.AddQuartz(q =>
        {
            // ── 1. OutboxDispatcherJob — Every 30 seconds ─────────────────────
            var outboxJobKey = new JobKey(nameof(OutboxDispatcherJob));
            q.AddJob<OutboxDispatcherJob>(opts => opts.WithIdentity(outboxJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(outboxJobKey)
                .WithIdentity($"{nameof(OutboxDispatcherJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));

            // ── 2. SLABreachScannerJob — Every 6 hours ────────────────────────
            var slaJobKey = new JobKey(nameof(SLABreachScannerJob));
            q.AddJob<SLABreachScannerJob>(opts => opts.WithIdentity(slaJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(slaJobKey)
                .WithIdentity($"{nameof(SLABreachScannerJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(6).RepeatForever()));

            // ── 3. RetirementAgeScannerJob — Daily at 02:00 UTC ───────────────
            var retirementJobKey = new JobKey(nameof(RetirementAgeScannerJob));
            q.AddJob<RetirementAgeScannerJob>(opts => opts.WithIdentity(retirementJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(retirementJobKey)
                .WithIdentity($"{nameof(RetirementAgeScannerJob)}-trigger")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(2, 0)));

            // ── 4. PlagiarismLockoutExpiryJob — Daily at 03:00 UTC ────────────
            var plagiarismJobKey = new JobKey(nameof(PlagiarismLockoutExpiryJob));
            q.AddJob<PlagiarismLockoutExpiryJob>(opts => opts.WithIdentity(plagiarismJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(plagiarismJobKey)
                .WithIdentity($"{nameof(PlagiarismLockoutExpiryJob)}-trigger")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(3, 0)));

            // ── 5. SessionCleanupJob — Daily at 01:00 UTC ─────────────────────
            var sessionJobKey = new JobKey(nameof(SessionCleanupJob));
            q.AddJob<SessionCleanupJob>(opts => opts.WithIdentity(sessionJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(sessionJobKey)
                .WithIdentity($"{nameof(SessionCleanupJob)}-trigger")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(1, 0)));

            // ── 6. DataRetentionJob — Weekly Sunday at 04:00 UTC ─────────────
            var dataRetentionJobKey = new JobKey(nameof(DataRetentionJob));
            q.AddJob<DataRetentionJob>(opts => opts.WithIdentity(dataRetentionJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(dataRetentionJobKey)
                .WithIdentity($"{nameof(DataRetentionJob)}-trigger")
                .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 4, 0)));

            // ── 7. MinistryBatchPollerJob — Every 12 hours ────────────────────
            var ministryJobKey = new JobKey(nameof(MinistryBatchPollerJob));
            q.AddJob<MinistryBatchPollerJob>(opts => opts.WithIdentity(ministryJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(ministryJobKey)
                .WithIdentity($"{nameof(MinistryBatchPollerJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(12).RepeatForever()));

            // ── 8. EmailDispatcherJob — Every 5 minutes (event-triggered simulation) ──
            var emailJobKey = new JobKey(nameof(EmailDispatcherJob));
            q.AddJob<EmailDispatcherJob>(opts => opts.WithIdentity(emailJobKey).StoreDurably());
            q.AddTrigger(opts => opts
                .ForJob(emailJobKey)
                .WithIdentity($"{nameof(EmailDispatcherJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever()));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}
