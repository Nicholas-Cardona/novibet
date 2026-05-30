using Novibet.Domain.Models;

namespace Novibet.Domain.Interfaces;
  
public interface IECBService
{
    public Task<List<CurrencyRate>> GetValues();
}