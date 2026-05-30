namespace Novibet.Domain.Interfaces;

public interface IECBClient
{
    public Task<string> GetRatesXmlAsync(string uri);
}
