using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Entities;
using HiveSpace.OrderService.Domain.Enums;
using HiveSpace.OrderService.Domain.Events;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.AggregateRoots;

public class Order : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public double SubTotal => Items.Sum(x => x.Price.Amount);

    public double ShippingFee { get; private set; }

    public double Discount { get; private set; }

    public double TotalPrice => SubTotal + ShippingFee - Discount;

    public DateTimeOffset OrderDate { get; private set; }

    public ShippingAddress ShippingAddress { get; private set; }

    public OrderStatus Status { get; private set; } = OrderStatus.PendingApproval;

    public PaymentMethod PaymentMethod { get; private set; }

    private Order()
    {
        ShippingAddress = null!;
    }

    public Order(Guid customerId, double shippingFee, double discount, DateTimeOffset orderDate, ShippingAddress shippingAddress, PaymentMethod paymentMethod, IEnumerable<OrderItem> orderItems)
    {
        CustomerId = customerId;
        ShippingFee = shippingFee;
        Discount = discount;
        OrderDate = orderDate;
        ShippingAddress = shippingAddress;
        PaymentMethod = paymentMethod;

        // Add order items
        foreach (var item in orderItems)
        {
            _items.Add(item);
        }

        if (IsInvalid()) throw new DomainException(400, OrderErrorCode.InvalidOrder, nameof(Order));

        // Publish domain event
        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId, TotalPrice, OrderDate));
    }

    private bool IsInvalid()
    {
        return Items.Count == 0;
    }

    public void AddItem(int quantity, int skuId, string productName, string variantName, string thumbnail, double amount, Currency currency)
    {
        var item = new OrderItem(skuId, productName, variantName, thumbnail, quantity, amount, currency);
        _items.Add(item);
    }

    public void RemoveItem(int skuId)
    {
        var deletedItemIndex = _items.FindIndex(x => x.SkuId == skuId);
        if (deletedItemIndex == -1) throw new DomainException(404, OrderErrorCode.OrderItemNotFound, nameof(Order));
        _items.RemoveAt(deletedItemIndex);
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;

        // Publish domain event
        AddDomainEvent(new OrderStatusChangedEvent(Id, oldStatus, newStatus, DateTimeOffset.UtcNow));
    }
}