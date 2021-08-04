using System;

namespace CommandQuery.Framing
{
    public class CommandResponse<T>
    {

        public bool Success { get; set; }

        public T Data { get; set; }

        public string Message { get; set; }
        public Exception Exception { get; set; }


    }
}