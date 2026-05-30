
using Novibet.Data.Entities;

namespace Novibet.Data.Mappers;

public static class WalletMapper
{
    public static Wallet ToDomain(WalletEntity entity)
    {
        return new Wallet
        {
            Id = entity.Id,
            Balance = entity.Balance,
            Currency = entity.Currency
        };
    }

    public static WalletEntity ToEntity(Wallet domain)
    {
        return new WalletEntity
        {
            Balance = domain.Balance,
            Currency = domain.Currency
        };
    }
}