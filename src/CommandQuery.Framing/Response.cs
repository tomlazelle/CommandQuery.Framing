using System;
using System.Collections.Generic;

namespace CommandQuery.Framing
{
    /// <summary>
    /// Static helper class for creating command responses.
    /// </summary>
    public static class Response
    {
        /// <summary>
        /// Creates a failed response with multiple error messages.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="errorMessages">The list of error messages.</param>
        /// <param name="exception">Optional exception that caused the failure.</param>
        /// <returns>A failed command response.</returns>
        public static CommandResponse<T> Failed<T>(List<string> errorMessages, Exception exception = null)
        {
            return new CommandResponse<T>
                   {
                       Success = false,
                       Message = string.Join(" ", errorMessages),
                       Exception = exception
                   };
        }

        /// <summary>
        /// Creates a failed response with a single error message.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="exception">Optional exception that caused the failure.</param>
        /// <returns>A failed command response.</returns>
        public static CommandResponse<T> Failed<T>(string errorMessage, Exception exception = null)
        {
            return new CommandResponse<T>
                   {
                       Success = false,
                       Message = errorMessage,
                       Exception = exception
                   };
        }

        /// <summary>
        /// Creates a failed response from an exception.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <returns>A failed command response with the exception message.</returns>
        public static CommandResponse<T> Failed<T>(Exception exception)
        {
            return new CommandResponse<T>
                   {
                       Success = false,
                       Message = exception?.Message ?? "An error occurred",
                       Exception = exception
                   };
        }

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="data">The response data.</param>
        /// <returns>A successful command response.</returns>
        public static CommandResponse<T> Ok<T>(T data)
        {
            return new CommandResponse<T>
                   {
                       Success = true,
                       Data = data
                   };
        }

        /// <summary>
        /// Creates a successful response without data.
        /// </summary>
        /// <returns>A successful command response.</returns>
        public static CommandResponse<object> Ok()
        {
            return new CommandResponse<object>
                   {
                       Success = true
                   };
        }
    }
}