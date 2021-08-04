﻿using System.Threading.Tasks;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Asynchronous request handler
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IAsyncHandler<in TRequest, TResponse> where TRequest : IRqstMessage
    {
        Task<TResponse> Execute(TRequest message);
    }
}