
using StackExchange.Redis;

namespace Novibet.Api.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase? _cache;

    public CacheService(IConnectionMultiplexer? muxer)
    {
        _cache = muxer?.GetDatabase();
    }

    public async Task<decimal?> GetCurrencyRate(string currency)
    {

        if (_cache is null) return null;
        string? cached = await _cache.HashGetAsync("currency_rates", currency.ToUpper());

        if (string.IsNullOrEmpty(cached)) return null;

        if (!decimal.TryParse(cached, out decimal parsedRate)) return null;

        return parsedRate;
    }

    public async Task<decimal?> GetConversionRate(string desiredCurrency, string walletCurrency)
    {
        if (_cache is null) return null;

        string? desiredRate = await _cache.HashGetAsync("currency_rates", desiredCurrency.ToUpper());
        string? walletRate = await _cache.HashGetAsync("currency_rates", walletCurrency.ToUpper());

        if (string.IsNullOrEmpty(desiredRate) || string.IsNullOrEmpty(walletRate)) return null;

        if (!decimal.TryParse(desiredRate, out decimal desiredDecimal) || !decimal.TryParse(walletRate, out decimal walletDecimal))
            return null;

        if (desiredDecimal <= 0 || walletDecimal <= 0) return null;


        return desiredDecimal / walletDecimal;
    }
}