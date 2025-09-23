using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale
{
    public Guid Id { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; } = null;

    public string Number { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;

    public Guid BranchId { get; private set; }
    public string BranchName { get; private set; } = string.Empty;

    private readonly List<SaleItem> Items = new();
    public IReadOnlyCollection<SaleItem> ReadOnlyItems => Items.AsReadOnly();

    public bool IsCancelled { get; private set; } = false;

    public decimal TotalAmount { get; private set; } = 0m;
    public decimal TotalDiscount { get; private set; } = 0m;
    public decimal TotalPayable { get; private set; } = 0m;

    protected Sale() { } // For EF

    private Sale(
        Guid id,
        DateTime createdAt, DateTime updatedAt, DateTime? deletedAt,
        string number,
        Guid customerId, string customerName,
        Guid branchId, string branchName)

    {
        // Validations
        if (id == Guid.Empty)
            throw new DomainException("INVALID_SALE_ID", "Sale ID must be a valid GUID.");
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("INVALID_SALE_NUMBER", "Sale number cannot be empty.");
        if (customerId == Guid.Empty)
            throw new DomainException("INVALID_CUSTOMER_ID", "Customer ID must be a valid GUID.");
        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainException("INVALID_CUSTOMER_NAME", "Customer name cannot be empty.");
        if (branchId == Guid.Empty)
            throw new DomainException("INVALID_BRANCH_ID", "Branch ID must be a valid GUID.");
        if (string.IsNullOrWhiteSpace(branchName))
            throw new DomainException("INVALID_BRANCH_NAME", "Branch name cannot be empty.");
        if (deletedAt != null && deletedAt <= createdAt)
            throw new DomainException("INVALID_DELETION_DATE", "Deletion date must be greater than creation date.");

        // Initialization
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
        Number = number.Trim();
        CustomerId = customerId;
        CustomerName = customerName.Trim();
        BranchId = branchId;
        BranchName = branchName.Trim();

        IsCancelled = false;
        TotalAmount = 0m;
        TotalDiscount = 0m;
        TotalPayable = 0m;
    }

    public static Sale CreateNew(
        Guid id, string number,
        Guid customerId, string customerName,
        Guid branchId, string branchName,
        IEnumerable<(Guid productId, string productName, decimal unitPrice, int quantity)>? items = null)
    {
        var sale = new Sale(
            id: id,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow,
            deletedAt: null,
            number: number,
            customerId: customerId,
            customerName: customerName,
            branchId: branchId,
            branchName: branchName);
        if (items != null)
        {
            foreach (var (productId, productName, unitPrice, quantity) in items)
                sale.AddItem(productId, productName, unitPrice, quantity);
        }

        sale.Recalculate();
        return sale;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        EnsureNotCancelled();

        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existingItem is null)
        {
            var newItem = new SaleItem(productId, productName, unitPrice, quantity);
            Items.Add(newItem);
        }
        else
        {
            // Same line: ensure same price and add quantity (respecting the ceiling via Recalculate)
            existingItem.EnsureSameUnitPrice(unitPrice);
            existingItem.IncreaseQuantity(quantity);
        }

        Recalculate();
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        EnsureNotCancelled();

        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existingItem is null)
            throw new DomainException("ITEM_NOT_FOUND", "Cannot update quantity of a non-existing item.");

        existingItem.SetQuantity(newQuantity);
        Recalculate();
    }

    public void RemoveItem(Guid productId, int quantity)
    {
        EnsureNotCancelled();

        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (existingItem is null)
            throw new DomainException("ITEM_NOT_FOUND", "Cannot remove a non-existing item.");
        if (existingItem.Quantity - quantity < 1)
            throw new DomainException("QUANTITY_MUST_BE_POSITIVE", "Quantity must be at least 1 after removal.");

        existingItem.DecreaseQuantity(quantity);
        if (existingItem.Quantity == 0)
            existingItem.Cancel();

        Recalculate();
    }
    public void CancelItems()
    {
        EnsureNotCancelled();

        var activeItems = Items.Where(i => !i.IsCancelled).ToList();
        if (activeItems.Count == 0)
            throw new DomainException("NO_ACTIVE_ITEMS", "There are no active items to cancel.");

        foreach (var i in activeItems)
            i.Cancel();
        Recalculate();

    }

    public void CancelSale()
    {
        EnsureNotCancelled();

        if (Items.Any(i => !i.IsCancelled))
            throw new DomainException("ACTIVE_ITEMS_EXIST", "Cannot cancel sale with active items. Cancel or remove all items first.");

        IsCancelled = true;
        Recalculate();
    }

    public void Recalculate()
    {
        if (IsCancelled)
        {
            TotalAmount = 0m;
            TotalDiscount = 0m;
            TotalPayable = 0m;
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        foreach (var item in Items.Where(i => !i.IsCancelled))
            item.Recalculate();

        var activeItems = Items.Where(i => !i.IsCancelled).ToList();
        TotalAmount = activeItems.Sum(i => i.UnitPrice * i.Quantity);
        TotalPayable = activeItems.Sum(i => i.LineTotal);
        TotalDiscount = TotalAmount - TotalPayable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (DeletedAt is not null)
            throw new DomainException("SALE_ALREADY_DELETED", "Sale has already been deleted.");
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DeletedAt.Value;
    }

    private SaleItem FindItemOrThrow(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId && !i.IsCancelled);
        if (item is null)
            throw new DomainException("ITEM_NOT_FOUND", "Item not found in the sale.");
        return item;
    }

    private void EnsureNotCancelled()
    {
        if (IsCancelled)
            throw new DomainException("SALE_CANCELLED", "Cannot modify a cancelled sale.");
    }

}



    
