using Microsoft.AspNetCore.Mvc;
using Novibet.Data;
using Novibet.Data.Entities;
using Novibet.Domain.DTOs.Requests;

using Microsoft.EntityFrameworkCore;
using Novibet.Data.Mappers;
using Novibet.Domain.Enums;

namespace Novibet.Api.Services;

public class WalletService : IWalletService
{

    private readonly AppDbContext _context;
    private readonly ICacheService? _cache;

    /// <summary>
    /// Provides wallet-related operations such as creation, balance retrieval,
    /// and fund updates, including currency conversion support.
    /// </summary>
    /// <remarks>
    /// This service relies on both database-stored currency rates and an optional
    /// cache layer for performance optimization.
    /// </remarks>
    public WalletService(AppDbContext context, ICacheService? cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// Creates a new wallet with an initial balance and validated currency.
    /// </summary>
    /// <param name="req">The wallet creation request containing currency and initial balance.</param>
    /// <returns>The created <see cref="WalletEntity"/> persisted in the database.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided currency is not supported.
    /// </exception>
    public async Task<WalletEntity> CreateAsync(CreateWalletRequest req)
    {

        string currency = req.Currency.ToUpper();
        decimal? cachedCurrency = _cache is not null ? await _cache.GetCurrencyRate(currency) : null;

        bool validCurrency = cachedCurrency is not null
            || await _context.CurrencyRates.AnyAsync(cr => cr.Currency == currency);

        if (!validCurrency)
        {
            throw new ArgumentException($"Currency: {currency} is not supported.");
        }

        var wallet = new WalletEntity() { Currency = currency, Balance = req.Balance!.Value };

        await _context.Wallets.AddAsync(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }

    /// <summary>
    /// Retrieves the wallet balance, optionally converted to a target currency.
    /// </summary>
    /// <param name="id">The wallet identifier.</param>
    /// <param name="currency">
    /// Optional target currency. If null, the original wallet currency is used.
    /// </param>
    /// <returns>The wallet balance, optionally converted to the requested currency.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the wallet with the specified ID does not exist.
    /// </exception>
    public async Task<decimal> GetWalletBalanceAsync(long id, [FromQuery] string? currency)
    {
        var wallet = await _context.Wallets.FindAsync(id);

        if (wallet == null)
        {
            throw new KeyNotFoundException($"Wallet with id: {id} not found.");
        }

        if (currency is null) return wallet.Balance;

        if (wallet.Balance == 0) return 0;

        decimal conversionRate = await GetConversionRateAsync(wallet.Currency, currency);

        var balance = wallet.Balance * conversionRate;

        return balance;
    }

    /// <summary>
    /// Updates wallet funds by applying a transaction strategy (add, subtract, or force subtract),
    /// optionally converting the input amount into the wallet's base currency.
    /// </summary>
    /// <param name="walletId">The wallet identifier.</param>
    /// <param name="amount">The amount to apply to the wallet.</param>
    /// <param name="currency">The currency of the provided amount.</param>
    /// <param name="strategy">The transaction strategy defining how funds are applied.</param>
    /// <returns>The updated wallet in domain model form.</returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when the wallet with the specified ID does not exist.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when an invalid transaction strategy is provided or currency is unsupported.
    /// </exception>
    public async Task<Wallet> UpdateFundsAsync(long walletId, decimal amount, string currency, WalletTransactionType strategy)
    {
        WalletEntity? walletEntity = await _context.Wallets.FindAsync(walletId);

        if (walletEntity == null)
        {
            throw new KeyNotFoundException($"Wallet with id: {walletId} not found.");
        }

        decimal conversionRate = await GetConversionRateAsync(walletEntity.Currency, currency);
        decimal convertedAmount = amount * conversionRate;

        switch (strategy)
        {
            case WalletTransactionType.Add:
                walletEntity.AddFunds(convertedAmount);
                break;
            case WalletTransactionType.Subtract:
                walletEntity.SubtractFunds(convertedAmount);
                break;
            case WalletTransactionType.ForceSubtract:
                walletEntity.ForceSubtractFunds(convertedAmount);
                break;
            default:
                throw new ArgumentException("Invalid update strategy.");
        }

        await _context.SaveChangesAsync();

        return WalletMapper.ToDomain(walletEntity);
    }

    /// <summary>
    /// Resolves the conversion rate between two currencies using cache (if available)
    /// or database-stored currency rates as a fallback.
    /// </summary>
    /// <param name="walletCurrency">The source currency code.</param>
    /// <param name="desiredCurrency">The target currency code.</param>
    /// <returns>The conversion rate between the two currencies.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when either currency is not supported.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when no currency data is available for conversion.
    /// </exception>
    /// <exception cref="DivideByZeroException">
    /// Thrown when the wallet currency rate is zero.
    /// </exception>
    private async Task<decimal> GetConversionRateAsync(string walletCurrency, string desiredCurrency)
    {
        ArgumentException.ThrowIfNullOrEmpty(walletCurrency);
        ArgumentException.ThrowIfNullOrEmpty(desiredCurrency);

        string walletCurrUpper = walletCurrency.ToUpper();
        string desiredCurrUpper = desiredCurrency.ToUpper();

        if (walletCurrUpper == desiredCurrUpper) return 1;

        if (_cache is not null)
        {
            decimal? conversionRate = await _cache.GetConversionRate(walletCurrUpper, desiredCurrUpper);

            if (conversionRate is not null) return conversionRate.Value;
        }

        var currencies = await _context.CurrencyRates
        .Where(cr => cr.Currency == walletCurrUpper || cr.Currency == desiredCurrUpper)
        .GroupBy(cr => cr.Currency)
        .Select(cr => cr
            .OrderByDescending(cr => cr.UpdatedOn)
            .FirstOrDefault())
        .ToListAsync();

        if (currencies is null) throw new InvalidDataException("No matching currencies");

        CurrencyRateEntity? desiredCurrencyEntity = currencies.FirstOrDefault(cr => cr?.Currency == desiredCurrUpper);

        if (desiredCurrencyEntity == null)
        {
            throw new ArgumentException($"Currency: {desiredCurrency} is not supported.");
        }

        CurrencyRateEntity? walletCurrencyEntity = currencies.FirstOrDefault(cr => cr?.Currency == walletCurrUpper);

        if (walletCurrencyEntity == null)
        {
            throw new ArgumentException($"Wallet Currency: {walletCurrency} is not supported.");
        }

        if (walletCurrencyEntity.Rate == 0)
        {
            throw new DivideByZeroException("Wallet Currency Rate is zero");
        }

        return desiredCurrencyEntity.Rate / walletCurrencyEntity.Rate;
    }
}