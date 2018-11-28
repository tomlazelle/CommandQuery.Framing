using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQuery.Framing.Enums;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Models;
using CommandQueryApiSample.Domain.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommandQueryApiSample.Controllers
{

    [ApiController]
    public class WidgetController : ControllerBase
    {
        private readonly ICommandBroker _commandBroker;

        public WidgetController(ICommandBroker commandBroker)
        {
            _commandBroker = commandBroker;
        }

        [Route("widget")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateWidget request)
        {
            var response = await _commandBroker.ExecuteAsync<CreateWidget, CommandResponse>(request);

            if (response.Result == CommandResultType.Success)
            {
                return Ok(response.Item<string>());
            }

            return BadRequest(response.Message);
        }

        [Route("widget/{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            return Ok(await _commandBroker.QueryAsync<GetWidget, Widget>(new GetWidget { Id = id }));
        }
    }
}
