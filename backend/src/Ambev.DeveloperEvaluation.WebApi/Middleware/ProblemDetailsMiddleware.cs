using System.Collections.Generic;
using System.Linq;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(exception, "Unable to apply ProblemDetails, the response has already started.");
                throw;
            }

            _logger.LogError(exception, "Unhandled exception reached the global middleware.");
            await WriteProblemDetailsResponseAsync(context, exception);
        }
    }

    private static async Task WriteProblemDetailsResponseAsync(HttpContext context, Exception exception)
    {
        var problemDetails = BuildProblemDetails(context, exception);

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static ProblemDetails BuildProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        switch (exception)
        {
            case ValidationException validationException:
                problemDetails.Title = "Validation failed";
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(failure => failure.PropertyName ?? string.Empty)
                    .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
                break;

            case DomainException domainException:
                problemDetails.Title = "Domain rule violation";
                problemDetails.Detail = domainException.Message;
                problemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                problemDetails.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
                problemDetails.Extensions["errorCode"] = domainException.ErrorCode;
                break;

            case UnauthorizedAccessException:
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = "The current user is not authorized to perform this action.";
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
                break;

            case KeyNotFoundException keyNotFoundException:
                problemDetails.Title = "Resource not found";
                problemDetails.Detail = keyNotFoundException.Message;
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                break;

            case ArgumentException argumentException:
                problemDetails.Title = "Invalid request";
                problemDetails.Detail = argumentException.Message;
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                break;

            default:
                problemDetails.Title = "Unexpected error";
                problemDetails.Detail = "An unexpected error occurred while processing the request.";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                break;
        }

        return problemDetails;
    }
}
