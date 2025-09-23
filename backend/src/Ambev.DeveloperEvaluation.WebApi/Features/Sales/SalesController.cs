using MediatR;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddItem;
using Ambev.DeveloperEvaluation.Application.Sales.AddItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateItemQuantity;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateItemQuantity;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.RemoveItem;
using Ambev.DeveloperEvaluation.Application.Sales.RemoveItem;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelItems;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer,Manager,Admin")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var command = _mapper.Map<CreateSaleCommand>(request);
        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var currentUserId = GetCurrentUserId();
            command = command with { CustomerId = currentUserId };
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var branchId = GetCurrentUserBranchId();
            if (branchId is null)
                return Forbid();
            command = command with { BranchId = branchId.Value };
        }
        var result = await _mediator.Send(command, cancellationToken);

        return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale created successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpPost("{id}/items")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddItem([FromRoute] Guid id, [FromBody] AddItemRequest request, CancellationToken cancellationToken)
    {
        var validator = new AddItemRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            if (sale.CustomerId != GetCurrentUserId())
                return Forbid();
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || sale.BranchId != branchId.Value)
                return Forbid();
        }
        var command = new AddItemToSaleCommand(id, request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Item added successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpPatch("{id}/items/{productId}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateItemQuantity([FromRoute] Guid id, [FromRoute] Guid productId, [FromBody] UpdateItemQuantityRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateItemQuantityRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            if (sale.CustomerId != GetCurrentUserId())
                return Forbid();
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || sale.BranchId != branchId.Value)
                return Forbid();
        }
        var command = new UpdateItemQuantityCommand(id, productId, request.Quantity);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Item quantity updated successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpDelete("{id}/items/{productId}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem([FromRoute] Guid id, [FromRoute] Guid productId, [FromBody] RemoveItemRequest request, CancellationToken cancellationToken)
    {
        var validator = new RemoveItemRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            if (sale.CustomerId != GetCurrentUserId())
                return Forbid();
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || sale.BranchId != branchId.Value)
                return Forbid();
        }
        var command = new RemoveItemFromSaleCommand(id, productId, request.Quantity);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Item removed successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpPost("{id}:cancel-items")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelItems([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            if (sale.CustomerId != GetCurrentUserId())
                return Forbid();
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || sale.BranchId != branchId.Value)
                return Forbid();
        }
        var result = await _mediator.Send(new CancelItemsCommand(id), cancellationToken);
        return Ok(new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Items cancelled successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpPost("{id}:cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            if (sale.CustomerId != GetCurrentUserId())
                return Forbid();
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || sale.BranchId != branchId.Value)
                return Forbid();
        }
        var result = await _mediator.Send(new CancelSaleCommand(id), cancellationToken);
        return Ok(new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale cancelled successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase) && result.CustomerId != GetCurrentUserId())
            return Forbid();
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || result.BranchId != branchId.Value)
                return Forbid();
        }
        return Ok(new ApiResponseWithData<CreateSaleResponse>
        {
            Success = true,
            Message = "Sale retrieved successfully",
            Data = _mapper.Map<CreateSaleResponse>(result)
        });
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<List<CreateSaleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSales(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? order = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? number = null,
        CancellationToken cancellationToken = default)
    {
        // Enforce scoping by role
        var role = GetCurrentUserRole();
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase))
        {
            customerId = GetCurrentUserId();
            branchId = null; // ignore client-provided branch filter for customers
        }
        else if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var currentBranch = GetCurrentUserBranchId();
            if (currentBranch is null)
                return Forbid();
            branchId = currentBranch;
            // ignore client-provided customerId for managers
            customerId = null;
        }
        var query = new ListSalesQuery(page, size, order, customerId, branchId, number);
        var result = await _mediator.Send(query, cancellationToken);

        var response = result.Select(r => _mapper.Map<CreateSaleResponse>(r)).ToList();
        return Ok(new ApiResponseWithData<List<CreateSaleResponse>>
        {
            Success = true,
            Message = "Sales listed successfully",
            Data = response
        });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSale([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        // Scope checks similar to other mutations
        var role = GetCurrentUserRole();
        var sale = await _mediator.Send(new GetSaleQuery(id), cancellationToken);
        if (string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase) && sale.CustomerId != GetCurrentUserId())
            return Forbid();
        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            var branchId = GetCurrentUserBranchId();
            if (branchId is null || sale.BranchId != branchId.Value)
                return Forbid();
        }

        await _mediator.Send(new DeleteSaleCommand(id), cancellationToken);
        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Sale deleted successfully"
        });
    }
}
