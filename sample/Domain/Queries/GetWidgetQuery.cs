using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Models;

namespace CommandQueryApiSample.Domain.Queries
{
    public class GetWidgetQuery : IAsyncHandler<GetWidget, Widget>
    {
        public async Task<Widget> Execute(GetWidget message, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult( new Widget
            {
                Id = message.Id
            });
        }
    }
}