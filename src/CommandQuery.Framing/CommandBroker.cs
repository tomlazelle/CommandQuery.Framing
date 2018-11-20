using System;
using System.Threading.Tasks;

namespace CommandQuery.Framing
{

    public class CommandBroker : ICommandBroker
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandBroker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TResponse Execute<TRequest,TResponse>(TRequest message) where TRequest : ICommandMessage
        {
            var genericType = MakeCustomGenericType(typeof(ICommandHandler<,>), message, typeof(TResponse));
            var messageHandler = (ICommandHandler<TRequest,TResponse>)_serviceProvider.GetService(genericType);


            var result =  messageHandler.Execute(message);

            return result;
        }
        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest message) where TRequest : ICommandMessage
        {
            var genericType = MakeCustomGenericType(typeof(IAsyncCommandHandler<,>), message, typeof(TResponse));
            var messageHandler = (IAsyncCommandHandler<TRequest, TResponse>)_serviceProvider.GetService(genericType);


            var result = await messageHandler.Execute(message);


            return result;
        }

        public TResponse Query<TRequest, TResponse>(TRequest message) where TRequest : IQueryMessage
        {
            var genericType = MakeCustomGenericType(typeof(IQueryHandler<,>), message, typeof(TResponse));

            var messageHandler = (IQueryHandler<TRequest, TResponse>)_serviceProvider.GetService(genericType);

            return messageHandler.Execute(message);
        } 
        

        public async Task<TResponse> QueryAsync<TRequest, TResponse>(TRequest message) where TRequest : IQueryMessage
        {
            var genericType = MakeCustomGenericType(typeof(IAsyncQueryHandler<,>), message, typeof(TResponse));

            var messageHandler = (IAsyncQueryHandler<TRequest, TResponse>)_serviceProvider.GetService(genericType);

            return await messageHandler.Execute(message);
        }

        public Type MakeCustomGenericType(Type handlerType,object impl,Type typeArguement){

            if (typeArguement != null)
            {
                return handlerType.MakeGenericType(impl.GetType(), typeArguement);
            }

            return handlerType.MakeGenericType(impl.GetType());
        }
    }
}