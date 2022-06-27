using API.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HashesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHashes(CancellationToken cancellationToken)
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateHashesAsync([FromServices] Channel<GenerateMessage> channel, CancellationToken cancellationToken)
        {
            await channel.Writer.WriteAsync(new GenerateMessage(), cancellationToken);
            return Ok();
        }
    }
}
