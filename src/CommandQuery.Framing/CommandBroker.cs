using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CommandQuery.Framing
{

    public class CommandBroker : ICommandBroker
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandBroker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public TResponse Handle<TRequest, TResponse>(TRequest message) where TRequest : IRqstMessage
        {
            var messageHandler = _serviceProvider.GetService<IHandler<TRequest, TResponse>>();

            var result = messageHandler.Execute(message);

            return result;
        }

        public async Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest message) where TRequest : IRqstMessage
        {
            var messageHandler = _serviceProvider.GetService<IAsyncHandler<TRequest, TResponse>>();

            var result = await messageHandler.Execute(message);

            return result;
        }


        public Type MakeCustomGenericType(Type handlerType, object impl, Type typeArguement)
        {

            if (typeArguement != null)
            {
                return handlerType.MakeGenericType(impl.GetType(), typeArguement);
            }

            return handlerType.MakeGenericType(impl.GetType());
        }
    }
}