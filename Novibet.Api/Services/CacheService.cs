
using StackExchange.Redis;

namespace Novibet.Api.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase? _cache;

    public CacheService(IConnectionMultiplexer? muxer)
    {
        _cache = muxer?.GetDatabase();
    }

    /// <summary>
    /// Retrieves the currency rate from the cache.
    /// </summary>
    /// <param name="currency">The Currency code (e.g. EUR)</param>
    /// <returns>
    /// The currency rate in decimal or null if the currency rate is not found in the cache.
    /// </returns>
    public async Task<decimal?> GetCurrencyRate(string currency)
    {
        if (_cache is null) return null;
        string? cached = await _cache.HashGetAsync("currency_rates", currency.ToUpper());

        if (string.IsNullOrEmpty(cached)) return null;

        if (!decimal.TryParse(cached, out decimal parsedRate)) return null;

        return parsedRate;
    }

    /// <summary>
    /// Attempts to retrieve a currency conversion rate using cached values.
    /// </summary>
    /// <param name="desiredCurrency">The target currency code (e.g., EUR).</param>
    /// <param name="walletCurrency">The source currency code (e.g., USD).</param>
    /// <returns>
    /// The conversion rate between the two currencies if available and valid in cache;
    /// otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method only uses cached values and does not fall back to a database.
    /// It returns null in the following cases:
    /// - Cache service is unavailable
    /// - Either currency is missing from cache
    /// - Cached values cannot be parsed as decimals
    /// - Cached values are zero or negative
    /// </remarks>
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