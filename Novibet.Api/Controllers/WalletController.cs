using Microsoft.AspNetCore.Mvc;
using Novibet.Data;
using Novibet.Data.Entities;
using Novibet.Domain.DTOs.Requests;

using Novibet.Api.Services;
using System.ComponentModel.DataAnnotations;
using Novibet.Domain.Enums;

namespace Novibet.Api.Controllers;

[ApiController]
[Route("/api/[controller]s")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost(Name = "CreateWallet")]
    public async Task<ActionResult<WalletEntity>> Create([FromBody] CreateWalletRequest req)
    {
        var wallet = await _walletService.CreateAsync(req);

        return Ok(wallet);
    }

    [HttpGet("{id}", Name = "RetrieveWalletBalance")]
    public async Task<ActionResult<decimal>> GetWalletBalance(long id, [FromQuery] string? currency)
    {
        try
        {
            var balance = await _walletService.GetWalletBalanceAsync(id, currency);
            return Ok(balance);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/adjustbalance", Name = "UpdateWalletBalance")]
    public async Task<ActionResult> AdjustBalance(long id, [FromQuery] AdjustBalanceRequest req)
    {
        try
        {
            Wallet wallet = await _walletService.UpdateFundsAsync(id, req.Amount, req.Currency, req.Strategy);
            return Ok(wallet);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch
        {
            return BadRequest("The operation failed");
        }
    }
}
