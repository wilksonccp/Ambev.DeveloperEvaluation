using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Common;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
public class BaseController : ControllerBase
{
    protected Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new NullReferenceException());

    protected string GetCurrentUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? throw new NullReferenceException();

    protected Guid? GetCurrentUserBranchId()
    {
        var claim = User.FindFirst("branchId")?.Value;
        if (Guid.TryParse(claim, out var id)) return id;
        return null;
    }

    protected string GetCurrentUserEmail() =>
        User.FindFirst(ClaimTypes.Email)?.Value ?? throw new NullReferenceException();

    protected IActionResult Ok<T>(T data) =>
        base.Ok(new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult Created<T>(string routeName, object routeValues, T data) =>
        base.CreatedAtRoute(routeName, routeValues, new ApiResponseWithData<T> { Data = data, Success = true });

    protected IActionResult BadRequest(string message) =>
        base.BadRequest(CreateProblemDetails(StatusCodes.Status400BadRequest, "Invalid request", message));

    protected IActionResult BadRequest(IEnumerable<ValidationFailure> errors)
    {
        var problemDetails = CreateProblemDetails(StatusCodes.Status400BadRequest, "Validation failed", "One or more validation errors occurred.");
        problemDetails.Extensions["errors"] = errors
            .GroupBy(failure => failure.PropertyName ?? string.Empty)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());

        return base.BadRequest(problemDetails);
    }

    protected IActionResult NotFound(string message = "Resource not found") =>
        base.NotFound(CreateProblemDetails(StatusCodes.Status404NotFound, "Resource not found", message));

    protected IActionResult OkPaginated<T>(PaginatedList<T> pagedList) =>
        Ok(new PaginatedResponse<T>
        {
            Data = pagedList,
            CurrentPage = pagedList.CurrentPage,
            TotalPages = pagedList.TotalPages,
            TotalCount = pagedList.TotalCount,
            Success = true
        });

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return problemDetails;
    }
}
