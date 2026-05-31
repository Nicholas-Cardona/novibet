using Microsoft.Extensions.Hosting;
using Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Novibet.Data;
using Novibet.Domain.Interfaces;
using Novibet.Xml.Clients;
using Novibet.Domain.Services;
using Novibet.Domain.Parsers;
using Novibet.Worker;


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

            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("UpdateDB");
                q.AddJob<ExchangeRateJob>(opts =>
                {
                    opts.WithIdentity(jobKey);
                });

                q.AddTrigger(opts =>
                {
                    opts.ForJob(jobKey)
                        .WithIdentity("UpdateDB-trigger")
                        .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Second))
                        .WithSimpleSchedule(s => s.WithIntervalInSeconds(60)
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