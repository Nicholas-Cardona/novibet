using System.ComponentModel.DataAnnotations;
using Novibet.Domain.Enums;


namespace Novibet.Domain.DTOs.Requests;

public class AdjustBalanceRequest
{
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
    public string Currency { get; set; } = null!;
    [Required]
    public WalletTransactionType Strategy { get; set; }
}