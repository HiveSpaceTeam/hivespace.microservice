namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;

public record DeliveryAddressDto
{
    public string  RecipientName { get; init; } = null!;
    public string  Phone         { get; init; } = null!;
    public string  StreetAddress { get; init; } = null!;
    public string  Commune       { get; init; } = null!;
    public string  Province      { get; init; } = null!;
    public string  Country       { get; init; } = "Vietnam";
    public string? Notes         { get; init; }
}
