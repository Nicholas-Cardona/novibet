namespace Novibet.Api.Services;

public interface IWalletTransactionType
{
    Task<Wallet> UpdateFundsAsync(long walletId, decimal amount);
}