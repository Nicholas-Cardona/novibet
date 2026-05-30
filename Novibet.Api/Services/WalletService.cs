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

    public WalletService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<WalletEntity> CreateAsync(CreateWalletRequest req)
    {
        var wallet = new WalletEntity() { Currency = req.Currency, Balance = req.Balance!.Value };

        await _context.Wallets.AddAsync(wallet);
        await _context.SaveChangesAsync();

        return wallet;
    }


    public async Task<decimal> GetWalletBalanceAsync(long id, [FromQuery] string? currency)
    {
        Console.WriteLine($"Received request for wallet id: {id} with currency: {currency}");
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

        var currencies = await _context.CurrencyRates
            .Where(cr =>
                cr.Currency == walletCurrency.ToUpper() ||
                cr.Currency == desiredCurrency.ToUpper())
            .ToListAsync();

        CurrencyRateEntity? desiredCurrencyEntity = currencies.FirstOrDefault(cr => cr.Currency == desiredCurrency.ToUpper());

        if (desiredCurrencyEntity == null)
        {
            throw new ArgumentException($"Currency: {desiredCurrency} is not supported.");
        }

        CurrencyRateEntity? walletCurrencyEntity = currencies.FirstOrDefault(cr => cr.Currency == walletCurrency);

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