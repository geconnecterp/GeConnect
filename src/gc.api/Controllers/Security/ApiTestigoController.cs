using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace gc.api.Controllers.Security
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTestigoController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Test(string id)
        {
            return Ok(id);
        }

    }
}
