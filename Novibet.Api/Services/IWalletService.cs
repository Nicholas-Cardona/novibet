using Novibet.Data.Entities;
using Novibet.Domain.DTOs.Requests;
using Novibet.Domain.Enums;

namespace Novibet.Api.Services;

public interface IWalletService
{
    Task<decimal> GetWalletBalanceAsync(long id, string? currency);
    Task<WalletEntity> CreateAsync(CreateWalletRequest req);
    Task<Wallet> UpdateFundsAsync(long walletId, decimal amount, string currency, WalletTransactionType strategy);
}
