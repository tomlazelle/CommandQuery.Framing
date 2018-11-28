using System.Threading.Tasks;
using CommandQuery.Framing;
using CommandQueryApiSample.Domain.Messages;
using CommandQueryApiSample.Domain.Models;

namespace CommandQueryApiSample.Domain.Queries
{
    public class GetWidgetQuery : IAsyncQueryHandler<GetWidget, Widget>
    {
        public async Task<Widget> Execute(GetWidget message)
        {
            return new Widget
            {
                Id = message.Id
            };
        }
    }
}