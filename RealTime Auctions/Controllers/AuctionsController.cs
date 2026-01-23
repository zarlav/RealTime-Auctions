using Microsoft.AspNetCore.Mvc;
using RealTime_Auctions.Models;
using RealTime_Auctions.Services;

namespace RealTime_Auctions.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionService _auctionService;

        public AuctionsController(AuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction(Auction auction)
        {
            await _auctionService.CreateAuctionAsync(auction);
            return Ok(auction);
        }

        [HttpPost("{auctionId}/bid")]
        public async Task<IActionResult> PlaceBid(string auctionId, [FromQuery] string userId, [FromQuery] decimal amount)
        {
            try
            {
                await _auctionService.PlaceBidAsync(auctionId, userId, amount);
                return Ok(new { auctionId, userId, amount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAuctions()
        {
            var auctions = await _auctionService.GetActiveAuctionsAsync();
            return Ok(auctions);
        }

        [HttpGet("{auctionId}/bids")]
        public async Task<IActionResult> GetBids(string auctionId)
        {
            var bids = await _auctionService.GetBidsAsync(auctionId);
            return Ok(bids);
        }
    }
}
