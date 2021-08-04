using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Models;
using CommandQueryApiSample.Domain.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommandQueryApiSample.Controllers
{

    [ApiController]
    public class WidgetController : ControllerBase
    {
        private readonly IBroker _commandBroker;

        public WidgetController(IBroker commandBroker)
        {
            _commandBroker = commandBroker;
        }

        [Route("widget")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateWidgetMessage request)
        {
            var response = await _commandBroker.HandleAsync<CreateWidgetMessage, CommandResponse<string>>(request);

            if (response.Success)
            {
                return Ok(response.Data);
            }

            return BadRequest(response.Message);
        }

        [Route("widget/{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            return Ok(await _commandBroker.HandleAsync<GetWidget, Widget>(new GetWidget { Id = id }));
        }
    }
}
