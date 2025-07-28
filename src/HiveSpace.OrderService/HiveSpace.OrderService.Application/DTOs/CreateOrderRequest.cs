using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Application.DTOs;

public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public double ShippingFee { get; set; }
    public double Discount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = default!;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class ShippingAddressDto
{
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string OtherDetails { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string Ward { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string Country { get; set; } = default!;
}

public class OrderItemDto
{
    public int SkuId { get; set; }
    public string ProductName { get; set; } = default!;
    public string VariantName { get; set; } = default!;
    public string Thumbnail { get; set; } = default!;
    public int Quantity { get; set; }
    public double Amount { get; set; }
    public Currency Currency { get; set; }
}