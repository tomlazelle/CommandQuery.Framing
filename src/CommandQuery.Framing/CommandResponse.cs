using System;
using CommandQuery.Framing.Enums;

namespace CommandQuery.Framing
{
    public class CommandResponse
    {
        public CommandResponse()
        {
            Result = CommandResultType.Success;
        }

        public CommandResultType Result { get; set; }

        public object StoredItem { get; set; }

        public string Message { get; set; }
        public Exception Exception { get; set; }
        public T Item<T>()
        {
            return (T) StoredItem;
        }

        public static CommandResponse Failed(string[] messages,CommandResultType result = CommandResultType.Failed,Exception exception = null)
        {
            return new CommandResponse
            {
                Result = result,
                Message = string.Join(" ", messages),
                Exception = exception
            };
        }

        public static CommandResponse Okay(object storedItem = null)
        {
            return new CommandResponse
            {
                Result = CommandResultType.Success,
                StoredItem = storedItem
            };
        }
    }
}