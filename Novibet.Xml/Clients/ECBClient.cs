using Novibet.Domain.Interfaces;

namespace Novibet.Xml.Clients;

public class ECBClient(HttpClient httpClient) : IECBClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<string> GetRatesXmlAsync(string url = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml")
    {
        Uri uri = new(url);
        var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}