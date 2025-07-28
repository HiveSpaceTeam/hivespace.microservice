# HiveSpace.OrderService.Application

Application layer của OrderService - **Use Cases & API Contracts** theo Clean Architecture.

## 🎯 Mục đích
- **Orchestrate business logic** từ Domain layer
- **Define API contracts** (DTOs for request/response)
- **Handle use cases** và application workflows
- **Coordinate** giữa Domain và Infrastructure layers
- **Entry point** cho external requests (HTTP APIs)

## 📁 Cấu trúc Project

```
HiveSpace.OrderService.Application/
├── 📄 DTOs/
│   ├── CreateOrderRequest.cs       # API input contract
│   └── OrderResponse.cs            # API output contract
├── 🎮 Controllers/ (planned)
│   └── OrderController.cs          # REST API endpoints
├── 🔧 Services/ (planned)
│   ├── IOrderService.cs            # Application service interface
│   └── OrderService.cs             # Application service implementation
├── ✅ Validators/ (planned)
│   └── CreateOrderValidator.cs     # Request validation rules
├── 🗺️ Mappers/ (planned)
│   └── OrderMapper.cs              # Domain ↔ DTO mapping
└── 📖 README.md
```

## 📄 DTOs (Data Transfer Objects)

### **Request DTOs**

#### **CreateOrderRequest**
```csharp
public class CreateOrderRequest
{
    public Guid CustomerId { get; set; }
    public double ShippingFee { get; set; }
    public double Discount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class ShippingAddressDto
{
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string OtherDetails { get; set; }
    public string Street { get; set; }
    public string Ward { get; set; }
    public string District { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
}

public class OrderItemDto
{
    public int SkuId { get; set; }
    public string ProductName { get; set; }
    public string VariantName { get; set; }
    public string Thumbnail { get; set; }
    public int Quantity { get; set; }
    public double Amount { get; set; }
    public Currency Currency { get; set; }
}
```

#### **UpdateOrderStatusRequest**
```csharp
public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
```

### **Response DTOs**

#### **OrderResponse**
```csharp
public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public double SubTotal { get; set; }
    public double ShippingFee { get; set; }
    public double Discount { get; set; }
    public double TotalPrice { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public ShippingAddressResponse ShippingAddress { get; set; }
    public List<OrderItemResponse> Items { get; set; }
}

public class ShippingAddressResponse
{
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string OtherDetails { get; set; }
    public string Street { get; set; }
    public string Ward { get; set; }
    public string District { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
}

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public int SkuId { get; set; }
    public string ProductName { get; set; }
    public string VariantName { get; set; }
    public string Thumbnail { get; set; }
    public int Quantity { get; set; }
    public double Amount { get; set; }
    public Currency Currency { get; set; }
}
```

## 🎮 Controllers (Planned)

### **OrderController Example**
```csharp
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IOrderRepository _orderRepository;

    public OrderController(IOrderService orderService, IOrderRepository orderRepository)
    {
        _orderService = orderService;
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// Tạo đơn hàng mới
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            // Validation (FluentValidation)
            // Map DTO → Domain via Factory
            var order = OrderFactory.CreateOrder(
                request.CustomerId,
                request.ShippingFee,
                request.Discount,
                request.PaymentMethod,
                request.ShippingAddress.FullName,
                request.ShippingAddress.PhoneNumber,
                request.ShippingAddress.OtherDetails,
                request.ShippingAddress.Street,
                request.ShippingAddress.Ward,
                request.ShippingAddress.District,
                request.ShippingAddress.Province,
                request.ShippingAddress.Country,
                request.Items.Select(item => (
                    item.SkuId, item.ProductName, item.VariantName,
                    item.Thumbnail, item.Quantity, item.Amount, item.Currency
                ))
            );

            // Save via Repository
            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();

            // Map Domain → DTO
            var response = MapToOrderResponse(order);
            
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.ErrorCode, message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy đơn hàng theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        var response = MapToOrderResponse(order);
        return Ok(response);
    }

    /// <summary>
    /// Lấy đơn hàng của customer
    /// </summary>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<OrderResponse>), 200)]
    public async Task<ActionResult<List<OrderResponse>>> GetOrdersByCustomer(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        var responses = orders.Select(MapToOrderResponse).ToList();
        return Ok(responses);
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            return NotFound();

        try
        {
            order.UpdateStatus(request.Status);
            await _orderRepository.UpdateAsync(order);
            await _orderRepository.SaveChangesAsync();
            
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.ErrorCode, message = ex.Message });
        }
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        // Mapping logic Domain → DTO
        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            SubTotal = order.SubTotal,
            ShippingFee = order.ShippingFee,
            Discount = order.Discount,
            TotalPrice = order.TotalPrice,
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentMethod = order.PaymentMethod,
            ShippingAddress = new ShippingAddressResponse
            {
                FullName = order.ShippingAddress.FullName,
                PhoneNumber = order.ShippingAddress.PhoneNumber.Value,
                OtherDetails = order.ShippingAddress.OtherDetails,
                Street = order.ShippingAddress.Address.Street,
                Ward = order.ShippingAddress.Address.Ward,
                District = order.ShippingAddress.Address.District,
                Province = order.ShippingAddress.Address.Province,
                Country = order.ShippingAddress.Address.Country
            },
            Items = order.Items.Select(item => new OrderItemResponse
            {
                Id = item.Id,
                SkuId = item.SkuId,
                ProductName = item.ProductName,
                VariantName = item.VariantName,
                Thumbnail = item.Thumbnail,
                Quantity = item.Quantity,
                Amount = item.Price.Amount,
                Currency = item.Price.Currency
            }).ToList()
        };
    }
}
```

## 🔧 Application Services (Planned)

### **IOrderService**
```csharp
public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<OrderResponse>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}
```

### **OrderService Implementation**
```csharp
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly IMapper _mapper;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);

        // Business logic coordination
        var order = OrderFactory.CreateOrder(
            request.CustomerId,
            request.ShippingFee,
            request.Discount,
            request.PaymentMethod,
            request.ShippingAddress.FullName,
            request.ShippingAddress.PhoneNumber,
            request.ShippingAddress.OtherDetails,
            request.ShippingAddress.Street,
            request.ShippingAddress.Ward,
            request.ShippingAddress.District,
            request.ShippingAddress.Province,
            request.ShippingAddress.Country,
            request.Items.Select(item => (
                item.SkuId, item.ProductName, item.VariantName,
                item.Thumbnail, item.Quantity, item.Amount, item.Currency
            ))
        );

        await _orderRepository.AddAsync(order, cancellationToken);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created successfully", order.Id);

        return _mapper.Map<OrderResponse>(order);
    }

    // Other methods...
}
```

## ✅ Validation (Planned)

### **FluentValidation Example**
```csharp
public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");

        RuleFor(x => x.ShippingFee)
            .GreaterThanOrEqualTo(0).WithMessage("ShippingFee must be >= 0");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item")
            .Must(items => items.Count > 0).WithMessage("Order must have at least one item");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("ShippingAddress is required")
            .SetValidator(new ShippingAddressDtoValidator());

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
    }
}

public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("FullName is required");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("PhoneNumber is required")
            .Matches(@"^84(?:3[2-9]|5[6|8|9]|7[0|6-9]|8[1-9]|9[0-9])\d{7}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required");
    }
}
```

## 🗺️ Mapping (Planned)

### **AutoMapper Profile**
```csharp
public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress));

        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Price.Currency));

        CreateMap<ShippingAddress, ShippingAddressResponse>()
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.Ward, opt => opt.MapFrom(src => src.Address.Ward))
            .ForMember(dest => dest.District, opt => opt.MapFrom(src => src.Address.District))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Address.Province))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Address.Country));
    }
}
```

## 🧪 Testing Application Layer

```csharp
[Test]
public async Task CreateOrder_Should_Return_OrderResponse_When_Valid_Request()
{
    // Arrange
    var request = new CreateOrderRequest
    {
        CustomerId = Guid.NewGuid(),
        ShippingFee = 30000,
        Discount = 0,
        PaymentMethod = PaymentMethod.CashOnDelivery,
        ShippingAddress = new ShippingAddressDto
        {
            FullName = "Nguyen Van A",
            PhoneNumber = "84901234567",
            Street = "123 ABC Street",
            Ward = "Ward 1",
            District = "District 1",
            Province = "Ho Chi Minh",
            Country = "Vietnam"
        },
        Items = new List<OrderItemDto>
        {
            new()
            {
                SkuId = 1,
                ProductName = "iPhone 14",
                VariantName = "128GB",
                Thumbnail = "image.jpg",
                Quantity = 1,
                Amount = 25000000,
                Currency = Currency.VND
            }
        }
    };

    // Act
    var response = await _orderService.CreateOrderAsync(request);

    // Assert
    Assert.That(response, Is.Not.Null);
    Assert.That(response.TotalPrice, Is.EqualTo(25030000)); // SubTotal + ShippingFee
    Assert.That(response.Status, Is.EqualTo(OrderStatus.PendingApproval));
}
```

## 📦 Dependencies

```xml
<ProjectReference Include="..\HiveSpace.OrderService.Domain\HiveSpace.OrderService.Domain.csproj" />

<!-- Planned packages -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
```

## 🔄 Data Flow

```
HTTP Request → Controller → Validation → Application Service → Domain Factory → Repository → Database
                ↓                                     ↓                    ↓
           CreateOrderRequest              Domain Objects        Domain Events
                ↓                                     ↓                    ↓
         OrderResponse ← Mapper ← Domain Objects ← Repository ← Infrastructure
```

## 🚫 What Application Layer SHOULD NOT contain

❌ **Business rules** (belongs to Domain)  
❌ **Database logic** (belongs to Infrastructure)  
❌ **Framework-specific domain logic**  
❌ **Complex domain calculations**

## ✅ What Application Layer SHOULD contain

✅ **DTOs** for API contracts  
✅ **Controllers** for HTTP endpoints  
✅ **Application Services** for use case orchestration  
✅ **Validation rules** for input  
✅ **Mapping** between Domain and DTOs  
✅ **Cross-cutting concerns** (logging, caching)

---
**Clean Architecture**: ✅ **Application Layer - Use Cases & API Contracts** 