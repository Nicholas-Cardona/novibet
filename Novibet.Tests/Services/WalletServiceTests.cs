using Novibet.Tests.Helpers;
using Novibet.Data.Entities;
using Moq;
using Novibet.Api.Services;
using Novibet.Domain.DTOs.Requests;
using FluentAssertions;

namespace Novibet.Tests.Services;

public class WalletServiceTests
{

    [Fact]
    public async Task CreateWallet_ShouldCreateWallet_WhenCurrencyIsValidAndLowerCase()
    {
        // Arrange
        var context = DbContextFactory.Create();

        context.CurrencyRates.Add(new CurrencyRateEntity
        {
            Currency = "USD",
            Rate = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetCurrencyRate("USD"))
             .ReturnsAsync((decimal?)null);

        var service = new WalletService(context, cache.Object);

        var request = new CreateWalletRequest
        {
            Currency = "usd",
            Balance = 100
        };

        var result = await service.CreateAsync(request);

        result.Currency.Should().Be("USD");
        result.Balance.Should().Be(100);
    }

    [Fact]
    public async Task CreateWallet_ShouldCreateWallet_WhenCurrencyIsValidAndUpperCase()
    {
        // Arrange
        var context = DbContextFactory.Create();

        context.CurrencyRates.Add(new CurrencyRateEntity
        {
            Currency = "USD",
            Rate = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetCurrencyRate("USD"))
             .ReturnsAsync((decimal?)null);

        var service = new WalletService(context, cache.Object);

        var request = new CreateWalletRequest
        {
            Currency = "USD",
            Balance = 100
        };

        var result = await service.CreateAsync(request);

        result.Currency.Should().Be("USD");
        result.Balance.Should().Be(100);
    }


    [Fact]
    public async Task CreateWallet_ShouldNotCreateWallet_WhenCurrencyIsInvalid()
    {
        // Arrange
        var context = DbContextFactory.Create();

        context.CurrencyRates.Add(new CurrencyRateEntity
        {
            Currency = "USD",
            Rate = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetCurrencyRate("USD"))
             .ReturnsAsync((decimal?)null);

        var service = new WalletService(context, cache.Object);

        var request = new CreateWalletRequest
        {
            Currency = "USC",
            Balance = 100
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request));
    }
}