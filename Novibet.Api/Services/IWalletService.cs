using Novibet.Data.Entities;
using Novibet.Domain.DTOs.Requests;

namespace Novibet.Api.Services;

public interface IWalletService
{
    Task<decimal> GetWalletBalanceAsync(long id, string? currency);
    Task<WalletEntity> CreateAsync(CreateWalletRequest req);
    Task<Wallet> UpdateFundsAsync(long walletId, decimal amount, UpdateFundsStrategy strategy);
}

public enum UpdateFundsStrategy
{
    Add,
    Subtract,
    ForceSubtract
}