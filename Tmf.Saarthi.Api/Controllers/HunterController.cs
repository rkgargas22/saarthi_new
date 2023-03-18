using Microsoft.AspNetCore.Mvc;
using Tmf.Saarthi.Core.ResponseModels.Hunter;

namespace Tmf.Saarthi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HunterController : ControllerBase
    {
        private readonly IHunterManager _hunterManager;

        public HunterController(IHunterManager hunterManager)
        {
            _hunterManager = hunterManager;
        }

        [HttpGet("TriggerHunterForChangedAddress")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(HunterResponseModel), StatusCodes.Status201Created)]
        public async Task<IActionResult> TriggerHunterForChangedAddress()
        {
          bool bol=await  _hunterManager.GetHunterResponse();
            return Ok(bol);
        }
    }
}
