namespace Novibet.Domain.Models;

public class CurrencyRate
{
    public required string Currency { get; set; }
    public required decimal Rate { get; set; }
    public required DateOnly Date { get; set; }

    public override string ToString()
    {
        return string.Format("{0} {1} : {2}", Date, Currency, Rate);
    }

}
