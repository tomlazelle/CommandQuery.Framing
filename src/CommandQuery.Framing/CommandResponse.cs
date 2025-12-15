using System;
using System.Reflection.Metadata.Ecma335;

namespace CommandQuery.Framing;

public interface ICommandResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    bool Success { get; set; }

    /// <summary>
    /// Gets or sets a message describing the result (typically used for errors).
    /// </summary>
    string Message { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred, if any.
    /// </summary>
    Exception Exception { get; set; }

    object RawData { get; }
}

/// <summary>
/// Represents the response from executing a command or query.
/// </summary>
public class CommandResponse : ICommandResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets a message describing the result (typically used for errors).
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred, if any.
    /// </summary>
    public Exception Exception { get; set; }

    public object RawData => null;
}

/// <summary>
/// Represents the response from executing a command or query.
/// </summary>
/// <typeparam name="T">The type of data returned in the response.</typeparam>
public class CommandResponse<T> : ICommandResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    private T _data;

    public T Data
    {
        get => _data;
        set => _data = value;
    }

    /// <summary>
    /// Gets or sets a message describing the result (typically used for errors).
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred, if any.
    /// </summary>
    public Exception Exception { get; set; }

    public object RawData => _data;
}