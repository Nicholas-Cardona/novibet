using Novibet.Domain.Interfaces;
using Novibet.Domain.Models;

namespace Novibet.Domain.Services;

public class ECBService : IECBService
{
    private readonly IECBClient _ecbClient;
    private readonly IECBRatesParser _ecbRateParser;

    private string ENDPOINT = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

    public ECBService(IECBClient ecbClient, IECBRatesParser eCBRatesParser)
    {
        _ecbClient = ecbClient;
        _ecbRateParser = eCBRatesParser;
    }

    public async Task<List<CurrencyRate>> GetValues()
    {
        string data = await _ecbClient.GetRatesXmlAsync(ENDPOINT);

        return await _ecbRateParser.Parse(data).ToListAsync();
    }
}

