using Quartz;
using Microsoft.EntityFrameworkCore;
using Novibet.Data;
using Novibet.Domain.Interfaces;
using Novibet.Xml.Clients;
using Novibet.Domain.Services;
using Novibet.Domain.Parsers;
using Novibet.Worker;
using StackExchange.Redis;
using Novibet.Worker.Options;
using Microsoft.Extensions.Options;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder()
        .ConfigureServices((cxt, services) =>
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(cxt.Configuration.GetConnectionString("Postgres"));
            });

            services.AddHttpClient<IECBClient, ECBClient>();

            services.AddSingleton<IECBService, ECBService>();
            services.AddSingleton<IECBRatesParser, ECBRatesParser>();
            services.AddScoped<DbContext, AppDbContext>();

            services.Configure<ExchangeRateJobOptions>(cxt.Configuration.GetSection("ExchangeRateJobOptions"));

            var cacheConfig = cxt.Configuration.GetConnectionString("Redis");
            if (!string.IsNullOrWhiteSpace(cacheConfig))
            {

                var multiplexer = ConnectionMultiplexer.Connect(cacheConfig);

                services.AddSingleton<IConnectionMultiplexer>(multiplexer);

            }

            services.AddQuartz((q, serviceProvider) =>
            {
                var jobOptions = serviceProvider
                            .GetRequiredService<IOptions<ExchangeRateJobOptions>>()
                            .Value;

                var jobKey = new JobKey("UpdateDB");
                q.AddJob<ExchangeRateJob>(opts =>
                {
                    opts.WithIdentity(jobKey);
                });

                q.AddTrigger(opts =>
                {
                    opts.ForJob(jobKey)
                        .WithIdentity("UpdateDB-trigger")
                        .StartAt(DateBuilder.FutureDate(jobOptions.StartDelaySeconds, IntervalUnit.Second))
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(jobOptions.IntervalSeconds)
                        .RepeatForever());
                });
            });

            services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });


        }).Build();

        using (var scope = builder.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        await builder.RunAsync();
    }
}