namespace Novibet.Api.Services;

public interface IUpdateFundsStrategy
{
    Task<Wallet> UpdateFundsAsync(long walletId, decimal amount);
}