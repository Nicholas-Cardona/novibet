using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Novibet.Api.Services;

public interface ICacheService
{
    public Task<decimal?> GetCurrencyRate(string currency);
    public Task<decimal?> GetConversionRate(string desiredCurrency, string walletCurrency);
}