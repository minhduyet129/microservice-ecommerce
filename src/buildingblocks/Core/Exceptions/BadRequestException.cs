namespace BuildingBlocks.Core.Exceptions;

public class BadRequestException : Exception
{
    public List<string>? Errors { get; set; }

    public BadRequestException(string message, List<string>? errors = null)
        : base(message)
    {
        Errors = errors;
    }
}