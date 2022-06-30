using API.Channels;
using Commun.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HashesController : ControllerBase
    {
        
        [HttpGet]
        public async Task<IActionResult> GetHashesAsync([FromServices] HashDbContext context, CancellationToken cancellationToken)
        {
            var query = await context.Hashes
               .GroupBy(p => new { p.Date })
               .Select(g => new { date = g.Key.Date.ToString("yyyy-MM-dd"), count = g.Count() }).ToListAsync(cancellationToken);

            return Ok(new
            {
                hashes = query,
            });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateHashesAsync([FromServices] Channel<GenerateMessage> channel, CancellationToken cancellationToken)
        {
            await channel.Writer.WriteAsync(new GenerateMessage(), cancellationToken);
            return Ok();
        }
    }
}
