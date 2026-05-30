using Microsoft.EntityFrameworkCore;
using Novibet.Domain.Models;

namespace Novibet.Data.Entities;


[PrimaryKey(nameof(Date), nameof(Currency))]
public class CurrencyRateEntity
{
    public required string Currency { get; set; }
    public required decimal Rate { get; set; }
    public required DateOnly Date { get; set; }

    public required DateTime CreatedOn { get; set; }
    public required DateTime UpdatedOn { get; set; }
}
