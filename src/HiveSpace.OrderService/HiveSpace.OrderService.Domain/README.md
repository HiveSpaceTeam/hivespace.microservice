# HiveSpace.OrderService.Domain

Domain layer của OrderService - **Core business logic** theo Clean Architecture principles.

## 🎯 Mục đích
- **Chứa toàn bộ business logic** của Order domain
- **Độc lập** với framework, database, UI
- **Rich domain model** theo DDD patterns
- **Không phụ thuộc** vào Application hay Infrastructure layers

## 📁 Cấu trúc Project

```
HiveSpace.OrderService.Domain/
├── 🔷 AggregateRoots/
│   └── Order.cs                    # Aggregate Root chính
├── 🔹 Entities/
│   └── OrderItem.cs                # Entity trong Order aggregate
├── 🔸 ValueObjects/
│   ├── Money.cs                    # Amount + Currency với validation
│   ├── PhoneNumber.cs              # Validate format VN phone
│   ├── Address.cs                  # Địa chỉ cơ bản
│   └── ShippingAddress.cs          # Địa chỉ giao hàng
├── 📋 Enums/
│   ├── OrderStatus.cs              # PendingApproval, Approved, Shipping...
│   ├── PaymentMethod.cs            # MoMo, CashOnDelivery, ZaloPay...
│   └── Currency.cs                 # VND, USD
├── 🔔 Events/
│   ├── OrderCreatedEvent.cs        # Domain event khi tạo order
│   └── OrderStatusChangedEvent.cs  # Domain event khi đổi status
├── ⚡ Exceptions/
│   ├── DomainException.cs          # Custom domain exception
│   └── OrderErrorCode.cs           # Domain-specific error codes
├── 🏭 Factories/
│   └── OrderFactory.cs             # Factory tạo Order từ primitives
└── 📚 Repositories/
    └── IOrderRepository.cs         # Repository interface
```

## 🔷 Domain Patterns

### **Aggregate Root**
- **Order**: Entry point vào Order aggregate, quản lý OrderItems
- **Invariants**: Order phải có ít nhất 1 OrderItem
- **Domain Events**: Publish OrderCreated, OrderStatusChanged

### **Entities** 
- **OrderItem**: Entity có identity, chứa thông tin sản phẩm

### **Value Objects**
- **Money**: Immutable, validation Amount > 0
- **PhoneNumber**: Regex validation cho số VN (84xxxxxxxxx)
- **Address**: Địa chỉ cơ bản (Street, Ward, District, Province)
- **ShippingAddress**: Composite với FullName + PhoneNumber + Address

### **Domain Events**
- **OrderCreatedEvent**: Khi Order được tạo thành công
- **OrderStatusChangedEvent**: Khi Status thay đổi

## 🎯 Business Rules

### **Order Aggregate**
```csharp
✅ Order phải có ít nhất 1 OrderItem
✅ TotalPrice = SubTotal + ShippingFee - Discount  
✅ Status mặc định: PendingApproval
✅ CustomerId bắt buộc
✅ ShippingAddress bắt buộc
```

### **OrderItem Entity**
```csharp
✅ Quantity > 0
✅ ProductName, VariantName, Thumbnail không rỗng
✅ Price.Amount > 0
✅ SkuId > 0
```

### **Money Value Object**
```csharp
✅ Amount > 0
✅ Currency: VND hoặc USD
✅ Immutable
```

### **PhoneNumber Value Object**
```csharp
✅ Format: 84xxxxxxxxx (VN phone number)
✅ Regex validation
✅ Immutable
```

## 🏭 Factory Pattern

### **OrderFactory Usage**
```csharp
// Tạo Order từ primitives
var order = OrderFactory.CreateOrder(
    customerId: Guid.NewGuid(),
    shippingFee: 30000,
    discount: 0,
    paymentMethod: PaymentMethod.CashOnDelivery,
    shippingFullName: "Nguyen Van A",
    shippingPhoneNumber: "84901234567",
    shippingOtherDetails: "",
    shippingStreet: "123 ABC Street",
    shippingWard: "Ward 1",
    shippingDistrict: "District 1",
    shippingProvince: "Ho Chi Minh",
    shippingCountry: "Vietnam",
    orderItems: new[]
    {
        (SkuId: 1, ProductName: "iPhone 14", VariantName: "128GB Black",
         Thumbnail: "image.jpg", Quantity: 1, Amount: 25000000.0, Currency: Currency.VND)
    }
);

// Update status
order.UpdateStatus(OrderStatus.Approved);

// Add/Remove items
order.AddItem(2, 2, "MacBook", "M2", "mac.jpg", 1, 50000000.0, Currency.VND);
order.RemoveItem(1);
```

### **Individual Components**
```csharp
// Tạo OrderItem riêng
var orderItem = OrderFactory.CreateOrderItem(
    skuId: 1,
    productName: "iPhone 14",
    variantName: "128GB Black",
    thumbnail: "image.jpg",
    quantity: 1,
    amount: 25000000.0,
    currency: Currency.VND
);

// Tạo ShippingAddress riêng  
var shippingAddress = OrderFactory.CreateShippingAddress(
    fullName: "Nguyen Van A",
    phoneNumber: "84901234567",
    otherDetails: "",
    street: "123 ABC Street",
    ward: "Ward 1",
    district: "District 1",
    province: "Ho Chi Minh",
    country: "Vietnam"
);
```

## 🔔 Domain Events

### **Subscribing to Events**
```csharp
// OrderCreatedEvent - khi order được tạo
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Gửi email confirmation
        // Update inventory
        // Log analytics
    }
}

// OrderStatusChangedEvent - khi status thay đổi
public class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedEvent>
{
    public async Task Handle(OrderStatusChangedEvent notification, CancellationToken cancellationToken)
    {
        // Notify customer
        // Update external systems
        // Trigger workflows
    }
}
```

## ⚡ Exception Handling

### **Domain Exceptions**
```csharp
// Built-in validation exceptions
try
{
    var money = new Money(-100, Currency.VND); // ❌ Amount <= 0
}
catch (DomainException ex)
{
    // ex.ErrorCode = OrderErrorCode.InvalidMoney
    // ex.PropertyName = "Money"
}

try
{
    var phone = new PhoneNumber("123456"); // ❌ Invalid format
}
catch (DomainException ex)
{
    // ex.ErrorCode = OrderErrorCode.InvalidPhoneNumber
    // ex.PropertyName = "PhoneNumber"
}

try
{
    var order = new Order(..., new List<OrderItem>()); // ❌ No items
}
catch (DomainException ex)
{
    // ex.ErrorCode = OrderErrorCode.InvalidOrder
}
```

### **Error Codes**
```csharp
public class OrderErrorCode : DomainErrorCode
{
    public static readonly OrderErrorCode InvalidOrder = new(1, "InvalidOrder", "ORD0001");
    public static readonly OrderErrorCode OrderItemNotFound = new(2, "OrderItemNotFound", "ORD0002");
    public static readonly OrderErrorCode InvalidMoney = new(3, "InvalidMoney", "ORD0003");
    public static readonly OrderErrorCode InvalidPhoneNumber = new(4, "InvalidPhoneNumber", "ORD0004");
}
```

## 📚 Repository Pattern

### **IOrderRepository**
```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
}
```

## 🧪 Testing Domain Logic

```csharp
[Test]
public void Order_Should_Calculate_Total_Price_Correctly()
{
    // Arrange
    var order = OrderFactory.CreateOrder(...);
    
    // Act
    var totalPrice = order.TotalPrice;
    
    // Assert
    Assert.That(totalPrice, Is.EqualTo(order.SubTotal + order.ShippingFee - order.Discount));
}

[Test]
public void Order_Should_Publish_Domain_Event_When_Created()
{
    // Arrange & Act
    var order = OrderFactory.CreateOrder(...);
    
    // Assert
    Assert.That(order.DomainEvents, Contains.Type<OrderCreatedEvent>());
}

[Test]
public void Money_Should_Throw_Exception_When_Amount_Is_Negative()
{
    // Act & Assert
    Assert.Throws<DomainException>(() => new Money(-100, Currency.VND));
}
```

## 📦 Dependencies

```xml
<ProjectReference Include="..\..\..\libs\HiveSpace.Domain.Shared\HiveSpace.Domain.Shared.csproj" />
```

**HiveSpace.Domain.Shared** provides:
- `AggregateRoot<T>` base class
- `Entity<T>` base class  
- `ValueObject` base class
- `IDomainEvent` interface
- `IRepository<T>` interface
- `DomainErrorCode` base class

## 🚫 What Domain Layer SHOULD NOT contain

❌ **DTOs** (moved to Application layer)  
❌ **Database concerns** (Infrastructure layer)  
❌ **API controllers** (Application layer)  
❌ **External service calls** (Infrastructure layer)  
❌ **Framework dependencies** (stay framework-agnostic)

## ✅ Best Practices Applied

✅ **Rich Domain Model**: Business logic in entities  
✅ **Aggregate Consistency**: Order controls OrderItems  
✅ **Domain Events**: Loose coupling between aggregates  
✅ **Factory Pattern**: Clean object creation  
✅ **Value Objects**: Immutable, self-validating  
✅ **Repository Interface**: Data access abstraction  
✅ **Custom Exceptions**: Domain-specific errors

---
**Clean Architecture**: ✅ **Domain Layer - 100% Framework Independent** 