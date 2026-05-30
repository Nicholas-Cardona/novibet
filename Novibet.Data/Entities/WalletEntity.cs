namespace Novibet.Data.Entities;

using System.ComponentModel.DataAnnotations;

public class WalletEntity
{
    [Key]
    public long Id { get; set; }
    public decimal Balance { get; set; }
    public required string Currency { get; set; }

    public decimal AddFunds(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount must be non-negative", nameof(amount));

        Balance += amount;
        return Balance;
    }

    public decimal SubtractFunds(decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount must be non-negative", nameof(amount));
        if (amount > Balance) throw new InvalidOperationException("Insufficient funds");

        Balance -= amount;
        return Balance;
    }

    public decimal ForceSubtractFunds(decimal amount)
    {
        if(amount < 0) throw new ArgumentException("Amount must be non-negative", nameof(amount));

        Balance -= amount; 
        return Balance;
    }
}

