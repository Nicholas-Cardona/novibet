using System.Data;
using Microsoft.EntityFrameworkCore;
using Novibet.Data;
using Novibet.Domain.Interfaces;
using Novibet.Domain.Models;
using Quartz;
using StackExchange.Redis;

namespace Novibet.Worker;

public class ExchangeRateJob : IJob
{
    private readonly IECBService _ecbService;
    private readonly AppDbContext _dbContext;

    public ExchangeRateJob(IECBService ecbService, AppDbContext dbContext)
    {
        _ecbService = ecbService;
        _dbContext = dbContext;
    }


    public async Task Execute(IJobExecutionContext context)
    {
        List<CurrencyRate> rates = await _ecbService.GetValues();

        if (rates.Any(r => !IsValidCurrencyCode(r.Currency))) throw new InvalidOperationException("Invalid currency code detected.");

        var valueRows = string.Join(",\n", rates.Select(r =>
        $"('{r.Currency}', DATE '{r.Date:yyyy-MM-dd}', {r.Rate})"));

        await _dbContext.Database.ExecuteSqlRawAsync($@"
            MERGE INTO ""CurrencyRates"" cr
            USING (
                VALUES
                    {valueRows}
            ) AS source(""Currency"", ""Date"", ""Rate"")
            ON cr.""Currency"" = source.""Currency""
            AND cr.""Date""     = source.""Date""
            WHEN MATCHED THEN
                UPDATE SET
                    ""Rate""      = source.""Rate"",
                    ""UpdatedOn"" = NOW()
            WHEN NOT MATCHED THEN
                INSERT (""Currency"", ""Date"", ""Rate"", ""CreatedOn"", ""UpdatedOn"")
                VALUES (source.""Currency"", source.""Date"", source.""Rate"", NOW(), NOW());
          ");

        var muxer = ConnectionMultiplexer.Connect("localhost:6379");
        var db = muxer.GetDatabase();

        var entries = rates.Select(r => new HashEntry(r.Currency, r.Rate.ToString())).ToArray();

        await db.HashSetAsync("currency_rates",entries);
    }

    private static bool IsValidCurrencyCode(string code)
    {
        return !string.IsNullOrWhiteSpace(code) && code.Length == 3 && code.All(char.IsAsciiLetterUpper);
    }
}