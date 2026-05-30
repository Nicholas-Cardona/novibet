using System.ComponentModel.DataAnnotations ;

namespace Novibet.Domain.DTOs.Requests;

public class CreateWalletRequest
{
    [Required]
    public decimal? Balance { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
    public string Currency { get; set; } = null!;
}
