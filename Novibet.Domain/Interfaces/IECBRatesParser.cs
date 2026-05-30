using Novibet.Domain.Models;

namespace Novibet.Domain.Interfaces;

public interface IECBRatesParser
{
    public IAsyncEnumerable<CurrencyRate> Parse(string xml);
}
