using Microsoft.AspNetCore.Mvc;
using RealTime_Auctions.Models;
using RealTime_Auctions.Services;

namespace RealTime_Auctions.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionService _service;
    public AuctionsController(AuctionService service) => _service = service;

    [HttpGet("active")]
    public async Task<IActionResult> GetActive() => Ok(await _service.GetActiveAuctionsAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Auction auction)
    {
        auction.Id = Guid.NewGuid().ToString();
        auction.StartAt = DateTime.UtcNow;
        if (auction.EndAt <= auction.StartAt) auction.EndAt = auction.StartAt.AddMinutes(5);
        auction.Status = AuctionStatusEnum.Active;
        await _service.CreateAuctionAsync(auction);
        return Ok(auction);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAuctionAsync(id);
        return Ok();
    }

    [HttpPost("{id}/bid")]
    public async Task<IActionResult> Bid(string id, [FromQuery] string userId, [FromQuery] decimal amount)
    {
        await _service.PlaceBidAsync(id, userId, amount);
        return Ok();
    }
}