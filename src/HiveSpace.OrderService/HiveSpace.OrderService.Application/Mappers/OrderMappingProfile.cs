using AutoMapper;
using HiveSpace.OrderService.Application.Commands;
using HiveSpace.OrderService.Application.DTOs;
using HiveSpace.OrderService.Domain.AggregateRoots;
using HiveSpace.OrderService.Domain.Entities;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Application.Mappers;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Request to Command mappings
        CreateMap<CreateOrderRequest, CreateOrderCommand>();

        // DTO to Domain mappings
        CreateMap<ShippingAddressDto, ShippingAddress>()
            .ConstructUsing(src => new ShippingAddress(
                src.FullName,
                src.PhoneNumber,
                src.OtherDetails,
                src.Street,
                src.Ward,
                src.District,
                src.Province,
                src.Country));

        CreateMap<OrderItemBaseDto, OrderItem>()
            .ConstructUsing(src => new OrderItem(
                src.SkuId,
                src.ProductName,
                src.VariantName,
                src.Thumbnail,
                src.Quantity,
                src.Amount,
                src.Currency));

        // Domain to Response mappings
        CreateMap<Order, OrderResponse>()
            .ConstructUsing(src => new OrderResponse(
                src.Id,
                src.CustomerId,
                src.SubTotal,
                src.ShippingFee,
                src.Discount,
                src.TotalPrice,
                src.OrderDate,
                src.Status,
                src.PaymentMethod,
                new ShippingAddressDto(
                    src.ShippingAddress.FullName,
                    src.ShippingAddress.PhoneNumber.Value,
                    src.ShippingAddress.OtherDetails,
                    src.ShippingAddress.Address.Street,
                    src.ShippingAddress.Address.Ward,
                    src.ShippingAddress.Address.District,
                    src.ShippingAddress.Address.Province,
                    src.ShippingAddress.Address.Country
                ),
                src.Items.Select(item => new OrderItemResponse(
                    item.Id,
                    item.SkuId,
                    item.ProductName,
                    item.VariantName,
                    item.Thumbnail,
                    item.Quantity,
                    item.Price.Amount,
                    item.Price.Currency
                )).ToList()
            ));

        // Simplified mappings for records (constructor-based)
        CreateMap<ShippingAddress, ShippingAddressDto>()
            .ConstructUsing(src => new ShippingAddressDto(
                src.FullName,
                src.PhoneNumber.Value,
                src.OtherDetails,
                src.Address.Street,
                src.Address.Ward,
                src.Address.District,
                src.Address.Province,
                src.Address.Country
            ));

        CreateMap<OrderItem, OrderItemResponse>()
            .ConstructUsing(src => new OrderItemResponse(
                src.Id,
                src.SkuId,
                src.ProductName,
                src.VariantName,
                src.Thumbnail,
                src.Quantity,
                src.Price.Amount,
                src.Price.Currency
            ));
    }
}