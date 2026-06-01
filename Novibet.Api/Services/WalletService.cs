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

    public WalletService(AppDbContext context, ICacheService? cache)
    {
        _context = context;
        _cache = cache;
    }

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

        var currencies =await _context.CurrencyRates
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