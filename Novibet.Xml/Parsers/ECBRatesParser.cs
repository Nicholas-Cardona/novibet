using System.Xml.Linq;
using Novibet.Domain.Interfaces;
using Novibet.Domain.Models;
namespace Novibet.Domain.Parsers;

 public class ECBRatesParser : IECBRatesParser
    {
        public async IAsyncEnumerable<CurrencyRate> Parse(string xml)
        {
            XNamespace ns = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref";

            XDocument xmlDoc = XDocument.Parse(xml);
            IEnumerable<XElement> dateCubes = xmlDoc.Descendants(ns + "Cube").Where(x => x.Attribute("time") != null);
            IEnumerable<XElement> currencyCubes = dateCubes.Descendants(ns + "Cube");

            XElement? firstCube = dateCubes.FirstOrDefault();
            XAttribute? dateAttribute = firstCube?.Attribute("time");


            if (dateAttribute is null)
            {
                throw new Exception("There was an error when parsing the XML. No date was found!");
            }

            DateOnly date = DateOnly.FromDateTime(DateTime.Parse((string)dateAttribute));


            foreach (var cube in currencyCubes)
            {
                var currency = cube.Attribute("currency");
                var rate = cube.Attribute("rate");

                if (currency is null || rate is null)
                {
                    continue;
                }

                var currencyVal = currency.Value;
                var rateVal = decimal.Parse(rate.Value ?? "0");

                yield return new CurrencyRate
                {
                    Currency = currencyVal,
                    Rate = rateVal,
                    Date = date
                };
            }
        }
    }