using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Nyxon.Server.Controllers
{
    [ApiController]
    [Route("api/prekeys")]
    [Authorize]
    public class PreKeyController : ControllerBase
    {
        private readonly IPrekeyService _prekeyService;

        public PreKeyController(IPrekeyService prekeyService)
        {
            _prekeyService = prekeyService;
        }

        [HttpGet]
        public async Task<ActionResult<PrekeyBundleResponse>> GetPrekeyBundle([FromBody] PrekeyBundleRequest request)
        {
            try
            {
                var response = await _prekeyService.GetPrekeyBundle(request.Username);

                if (response == null)
                    throw new("Prekey bundle generation failed");

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});
            }
        }
    }
}