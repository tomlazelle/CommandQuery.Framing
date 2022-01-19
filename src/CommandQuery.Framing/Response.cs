using System;
using System.Collections.Generic;

namespace CommandQuery.Framing
{
    public static class Response
    {
        public static CommandResponse<T> Failed<T>(List<string> errorMessages, Exception exception = null)
        {
            return new CommandResponse<T>
                   {
                       Success = false,
                       Message = string.Join(" ", errorMessages),
                       Exception = exception
                   };
        }

        public static CommandResponse<T> Ok<T>(T data)
        {
            return new CommandResponse<T>
                   {
                       Success = true,
                       Data = data
                   };
        }


    }
}