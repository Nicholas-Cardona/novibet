namespace Novibet.Data.Entities;

using System.ComponentModel.DataAnnotations;

public class WalletEntity
{
    [Key]
    public long Id { get; set; }
    public decimal Balance { get; set; }
    public required string Currency { get; set; }

    public Wallet ToDomain()
    {
        return new Wallet() { Id = Id, Currency = Currency, Balance = Balance };
    }
}

