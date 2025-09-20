using System;

namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; } = "domain_error";
    public DomainException(string code, string message) : base(message)
    {
        ErrorCode = code;
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
