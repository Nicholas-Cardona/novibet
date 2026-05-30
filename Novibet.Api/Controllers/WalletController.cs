using Microsoft.AspNetCore.Mvc;
using Novibet.Data;
using Novibet.Data.Entities;
using Novibet.Domain.DTOs.Requests;
using Novibet.Domain.Models;

namespace Novibet.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WalletController : ControllerBase
{
    private readonly AppDbContext _context;

    public WalletController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost(Name = "CreateWallet")]
    public ActionResult<Wallet> Create([FromBody] CreateWalletRequest req)
    {
        var wallet = new WalletEntity () { Currency = req.Currency, Balance = req.Balance!.Value };

        _context.Wallets.Add(wallet);
        _context.SaveChanges();
        return Ok(wallet);
    }

    
}
