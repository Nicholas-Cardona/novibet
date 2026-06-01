using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Novibet.Data;
using Novibet.Domain.Interfaces;
using Novibet.Domain.Parsers;
using Novibet.Domain.Services;
using Novibet.Worker;
using Novibet.Worker.Options;
using Novibet.Xml.Clients;
using Quartz;
using StackExchange.Redis;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)

            .UseServiceProviderFactory(new AutofacServiceProviderFactory())

            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(configuration.GetConnectionString("Postgres")));

                services.AddHttpClient<IECBClient, ECBClient>();

                services.AddQuartz((q, serviceProvider) =>
                {
                    var jobOptions = serviceProvider
                        .GetRequiredService<IOptions<ExchangeRateJobOptions>>()
                        .Value;

                    var jobKey = new JobKey("UpdateDB");

                    q.AddJob<ExchangeRateJob>(opts =>
                        opts.WithIdentity(jobKey));

                    q.AddTrigger(opts =>
                        opts.ForJob(jobKey)
                            .WithIdentity("UpdateDB-trigger")
                            .StartAt(DateBuilder.FutureDate(
                                jobOptions.StartDelaySeconds,
                                IntervalUnit.Second))
                            .WithSimpleSchedule(s => s
                                .WithIntervalInSeconds(jobOptions.IntervalSeconds)
                                .RepeatForever()));
                });

                services.AddQuartzHostedService(opt =>
                {
                    opt.WaitForJobsToComplete = true;
                });

                // Options
                services.Configure<ExchangeRateJobOptions>(
                    configuration.GetSection("ExchangeRateJobOptions"));
            })


            .ConfigureContainer<ContainerBuilder>((context, container) =>
            {
                var configuration = context.Configuration;

                // Services
                container.RegisterType<ECBService>()
                    .As<IECBService>()
                    .SingleInstance();

                container.RegisterType<ECBRatesParser>()
                    .As<IECBRatesParser>()
                    .InstancePerLifetimeScope();

                container.RegisterType<ExchangeRateJob>();

                var cacheConfig = configuration.GetConnectionString("Redis");

                if (!string.IsNullOrWhiteSpace(cacheConfig))
                {
                    var multiplexer = ConnectionMultiplexer.Connect(cacheConfig);

                    container.RegisterInstance<IConnectionMultiplexer>(multiplexer);
                }
            })

            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        await host.RunAsync();
    }
}