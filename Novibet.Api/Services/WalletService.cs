using Microsoft.AspNetCore.Mvc;
using Novibet.Data;
using Novibet.Data.Entities;
using Novibet.Domain.DTOs.Requests;

using Microsoft.EntityFrameworkCore;
using Novibet.Data.Mappers;

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

        var currencies = await _context.CurrencyRates
            .Where(cr =>
                cr.Currency == currency.ToUpper() ||
                cr.Currency == wallet.Currency.ToUpper())
            .ToListAsync();

        CurrencyRateEntity? desiredCurrency = currencies.FirstOrDefault(cr => cr.Currency == currency.ToUpper());

        if (desiredCurrency == null)
        {
            throw new ArgumentException($"Currency: {currency} is not supported.");
        }

        CurrencyRateEntity? walletCurrency = currencies.FirstOrDefault(cr => cr.Currency == wallet.Currency);

        if (walletCurrency == null)
        {
            throw new ArgumentException($"Wallet Currency: {wallet.Currency} is not supported.");
        }

        decimal conversionRate = desiredCurrency.Rate / walletCurrency.Rate;

        var balance = wallet.Balance * conversionRate;

        return balance;
    }

    public async Task<Wallet> UpdateFundsAsync(long walletId, decimal amount, UpdateFundsStrategy strategy)
    {
        WalletEntity? walletEntity = await _context.Wallets.FindAsync(walletId);

        if (walletEntity == null)
        {
            throw new KeyNotFoundException($"Wallet with id: {walletId} not found.");
        }

        switch (strategy)
        {
            case UpdateFundsStrategy.Add:
                walletEntity.AddFunds(amount);
                break;
            case UpdateFundsStrategy.Subtract:
                walletEntity.SubtractFunds(amount);
                break;
            case UpdateFundsStrategy.ForceSubtract:
                walletEntity.ForceSubtractFunds(amount);
                break;
            default:
                throw new ArgumentException("Invalid update strategy.");
        }

        await _context.SaveChangesAsync();

        return WalletMapper.ToDomain(walletEntity);
    }
}