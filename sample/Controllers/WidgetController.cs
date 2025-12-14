using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Models;
using CommandQueryApiSample.Domain.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CommandQueryApiSample.Controllers
{
    /// <summary>
    /// Controller for managing widget operations.
    /// </summary>
    [ApiController]
    public class WidgetController : ControllerBase
    {
        private readonly IBroker _commandBroker;

        /// <summary>
        /// Initializes a new instance of the <see cref="WidgetController"/> class.
        /// </summary>
        /// <param name="commandBroker">The broker for executing commands and queries.</param>
        public WidgetController(IBroker commandBroker)
        {
            _commandBroker = commandBroker;
        }

        /// <summary>
        /// Creates a new widget.
        /// </summary>
        /// <param name="request">The widget creation request.</param>
        /// <param name="cancellationToken">Cancellation token provided by ASP.NET Core.</param>
        /// <returns>The created widget ID or error message.</returns>
        [Route("widget")]
        [HttpPost]
        public async Task<IActionResult> Post(
            [FromBody] CreateWidgetMessage request, 
            CancellationToken cancellationToken)
        {
            var response = await _commandBroker.HandleAsync<CreateWidgetMessage, CommandResponse<string>>(
                request, 
                cancellationToken);

            if (response.Success)
            {
                return Ok(response.Data);
            }

            return BadRequest(response.Message);
        }

        /// <summary>
        /// Gets a widget by ID.
        /// </summary>
        /// <param name="id">The widget ID.</param>
        /// <param name="cancellationToken">Cancellation token provided by ASP.NET Core.</param>
        /// <returns>The widget data.</returns>
        [Route("widget/{id}")]
        [HttpGet]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var widget = await _commandBroker.HandleAsync<GetWidget, Widget>(
                new GetWidget { Id = id }, 
                cancellationToken);
            
            return Ok(widget);
        }
    }
}
