# HiveSpace.OrderService 

OrderService microservice triển khai theo **Clean Architecture** và **Domain-Driven Design (DDD)**.

## 🏗️ Clean Architecture Overview

```
┌─────────────────────────────────────────────────┐
│                 🌐 External                     │
│            (HTTP, Database, etc.)               │
└─────────────────────┬───────────────────────────┘
                     │
┌─────────────────────▼───────────────────────────┐
│         📱 HiveSpace.OrderService.Application   │
│              (Controllers, DTOs)                │
│    ✅ Use Cases & API Contracts                 │
└─────────────────────┬───────────────────────────┘
                     │
┌─────────────────────▼───────────────────────────┐
│         🔷 HiveSpace.OrderService.Domain        │
│        (Entities, Value Objects, Events)        │
│    ✅ Core Business Logic - Framework Free      │
└─────────────────────────────────────────────────┘
```

## 📁 Solution Structure

```
HiveSpace.OrderService/
├── 🔷 HiveSpace.OrderService.Domain/      # Core business logic
│   ├── AggregateRoots/Order.cs            # Main aggregate
│   ├── Entities/OrderItem.cs              # Order items
│   ├── ValueObjects/                      # Money, Address, PhoneNumber
│   ├── Events/                            # Domain events
│   ├── Factories/OrderFactory.cs          # Object creation
│   ├── Exceptions/                        # Domain exceptions
│   └── 📖 README.md                       # Domain documentation
│
├── 📱 HiveSpace.OrderService.Application/ # Use cases & APIs
│   ├── DTOs/                              # Request/Response contracts
│   ├── Controllers/ (planned)             # HTTP endpoints
│   ├── Services/ (planned)                # Application services
│   └── 📖 README.md                       # Application documentation
│
├── 🗃️ HiveSpace.OrderService.Infrastructure/ (planned)
│   ├── Data/OrderDbContext.cs             # EF Core context
│   ├── Repositories/OrderRepository.cs    # Data access
│   └── Configurations/                    # Entity configurations
│
└── 📖 README.md                           # This overview
```

## 🎯 Business Domain

### **Order Aggregate**
OrderService quản lý toàn bộ lifecycle của đơn hàng:

```csharp
Order (Aggregate Root)
├── CustomerId: Guid
├── OrderItems: List<OrderItem>
├── ShippingAddress: ShippingAddress (Value Object)
├── PaymentMethod: PaymentMethod (Enum)
├── Status: OrderStatus (Enum)
├── TotalPrice = SubTotal + ShippingFee - Discount
└── Events: OrderCreated, StatusChanged
```

### **Business Rules**
- ✅ Order phải có ít nhất 1 OrderItem
- ✅ TotalPrice = SubTotal + ShippingFee - Discount
- ✅ Status mặc định: PendingApproval
- ✅ Money amount > 0, currency VND|USD
- ✅ PhoneNumber format: 84xxxxxxxxx

## 🔄 Data Flow Example

### **Create Order Use Case**
```
HTTP POST /api/orders
        ↓
[Application] CreateOrderRequest (DTO)
        ↓ 
[Application] Validation
        ↓
[Domain] OrderFactory.CreateOrder()
        ↓
[Domain] Order aggregate with business rules
        ↓
[Domain] OrderCreatedEvent published
        ↓
[Infrastructure] Repository.AddAsync()
        ↓
[Application] OrderResponse (DTO)
        ↓
HTTP 201 Created
```

## 🚀 Getting Started

### **1. Build Projects**
```bash
# Domain layer (core business logic)
cd HiveSpace.OrderService.Domain
dotnet build

# Application layer (API contracts)
cd ../HiveSpace.OrderService.Application  
dotnet build
```

### **2. Create Order Example**
```csharp
// Domain layer - using Factory
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

// Business logic
order.UpdateStatus(OrderStatus.Approved);
Console.WriteLine($"Total: {order.TotalPrice:C}"); // 25,030,000 VND
```

### **3. API Usage (Planned)**
```http
POST /api/orders
Content-Type: application/json

{
    "customerId": "123e4567-e89b-12d3-a456-426614174000",
    "shippingFee": 30000,
    "discount": 0,
    "paymentMethod": "CashOnDelivery",
    "shippingAddress": {
        "fullName": "Nguyen Van A",
        "phoneNumber": "84901234567",
        "street": "123 ABC Street",
        "ward": "Ward 1",
        "district": "District 1",
        "province": "Ho Chi Minh",
        "country": "Vietnam"
    },
    "items": [
        {
            "skuId": 1,
            "productName": "iPhone 14",
            "variantName": "128GB Black",
            "thumbnail": "image.jpg",
            "quantity": 1,
            "amount": 25000000,
            "currency": "VND"
        }
    ]
}
```

## 🔔 Domain Events

### **OrderCreatedEvent**
```csharp
{
    "OrderId": "guid",
    "CustomerId": "guid", 
    "TotalAmount": 25030000,
    "OrderDate": "2024-01-15T10:30:00Z"
}
```

### **OrderStatusChangedEvent**
```csharp
{
    "OrderId": "guid",
    "OldStatus": "PendingApproval",
    "NewStatus": "Approved", 
    "ChangedAt": "2024-01-15T10:35:00Z"
}
```

## ⚡ Exception Handling

### **Domain Exceptions**
```csharp
// Automatic validation in Domain layer
try {
    var money = new Money(-100, Currency.VND);
} catch (DomainException ex) {
    // ex.ErrorCode = "ORD0003" (InvalidMoney)
    // HTTP 400 Bad Request
}

try {
    var order = OrderFactory.CreateOrder(..., orderItems: []);
} catch (DomainException ex) {
    // ex.ErrorCode = "ORD0001" (InvalidOrder) 
    // HTTP 400 Bad Request
}
```

## 🧪 Testing

### **Domain Testing**
```bash
# Test business logic
cd HiveSpace.OrderService.Domain.Tests
dotnet test
```

### **Application Testing**
```bash
# Test API contracts & use cases
cd HiveSpace.OrderService.Application.Tests  
dotnet test
```

## 📋 Roadmap

### ✅ **Completed**
- ✅ Domain layer với DDD patterns
- ✅ Application DTOs và API contracts
- ✅ Factory pattern cho object creation
- ✅ Domain events và exceptions
- ✅ Clean Architecture compliance

### 🔄 **In Progress**
- 🔄 Infrastructure layer (EF Core, Repository)
- 🔄 API Controllers và endpoints
- 🔄 Validation với FluentValidation

### ⏳ **Planned**
- ⏳ Unit tests cho Domain layer
- ⏳ Integration tests cho API
- ⏳ Docker containerization
- ⏳ Event Bus integration

## 📚 Documentation

- **[Domain README](HiveSpace.OrderService.Domain/README.md)** - Core business logic
- **[Application README](HiveSpace.OrderService.Application/README.md)** - Use cases & APIs

## 🏆 Clean Architecture Benefits

✅ **Testability**: Domain logic hoàn toàn independent  
✅ **Maintainability**: Clear separation of concerns  
✅ **Flexibility**: Dễ thay đổi UI, database, framework  
✅ **Business Focus**: Domain logic là trung tâm  
✅ **Team Collaboration**: Clear boundaries giữa layers

---
**Architecture**: ✅ **Clean Architecture + DDD**  
**Status**: ✅ **Ready for Infrastructure Layer** 