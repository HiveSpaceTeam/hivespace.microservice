# Outbox Pattern Implementation

This directory contains the implementation of the Outbox Pattern for the HiveSpace project.

## Overview

The Outbox Pattern ensures reliable message delivery by storing domain events in a database table before publishing them to message brokers. This guarantees that events are not lost even if the message broker is temporarily unavailable.

## Components

### 1. OutboxMessage Entity
- **File**: `OutboxMessage.cs`
- **Purpose**: Represents a domain event stored in the outbox table
- **Key Properties**:
  - `Id`: Unique identifier
  - `Type`: Full type name of the domain event
  - `Content`: Serialized event payload (JSON)
  - `OccurredOnUtc`: When the event occurred
  - `ProcessedOnUtc`: When the message was successfully published
  - `Error`: Any error during publishing
  - `Attempts`: Number of publishing attempts

### 2. IOutboxRepository Interface
- **File**: `IOutboxRepository.cs`
- **Purpose**: Interface for adding domain events to the outbox
- **Usage**: Typically called from within a business transaction
- **Methods**:
  - `AddDomainEvent<TEvent>(TEvent domainEvent)`: Adds a domain event (auto-creates OutboxMessage)
  - `AddOutboxMessage(OutboxMessage outboxMessage)`: Adds a pre-created OutboxMessage directly

### 3. OutboxRepository Implementation
- **File**: `OutboxRepository.cs`
- **Purpose**: Implements IOutboxRepository to add domain events to the OutboxMessage table
- **Features**: Serializes domain events to JSON and stores them in the database

### 4. OutboxMessageProcessor Background Service
- **File**: `OutboxMessageProcessor.cs`
- **Purpose**: Background service that processes messages from the outbox table
- **Features**:
  - Polls for unprocessed messages every 5 seconds
  - Deserializes domain events
  - Publishes to message broker (TODO: implement actual publishing)
  - Handles errors and retries

### 5. Entity Configuration
- **File**: `OutboxMessageEntityConfiguration.cs`
- **Purpose**: Configures the OutboxMessage entity for Entity Framework Core

### 6. DomainEventToOutboxInterceptor
- **File**: `DomainEventToOutboxInterceptor.cs` (in Interceptors folder)
- **Purpose**: Automatically extracts domain events from aggregate roots and adds them to the outbox
- **Features**:
  - Intercepts SaveChanges operations
  - Automatically processes domain events from aggregate roots
  - Clears events after adding to outbox

## Usage

### 1. Register Services

In your `Program.cs` or service configuration:

```csharp
using HiveSpace.Infrastructure.Persistence;

// Add all persistence services including outbox
builder.Services.AddPersistenceInfrastructure();
```

### 2. Configure DbContext with Interceptor

Configure your DbContext to use the DomainEventToOutboxInterceptor:

```csharp
// In your DbContext configuration
services.AddDbContext<YourDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString)
           .AddDomainEventToOutboxInterceptor(serviceProvider); // Add this line
});
```

### 3. Create Domain Events

Create domain events that implement `IDomainEvent`:

```csharp
using HiveSpace.Domain.Shared;

namespace YourNamespace.Events;

public class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public DateTime RegisteredAt { get; }

    public UserRegisteredEvent(Guid userId, string email, DateTime registeredAt)
    {
        UserId = userId;
        Email = email;
        RegisteredAt = registeredAt;
    }
}
```

### 4. Add Events to Aggregate Roots

In your aggregate roots, add domain events:

```csharp
public class ApplicationUser : IdentityUser, IAggregateRoot, IAuditable
{
    // ... existing code ...

    public void RegisterUser(string email, string userName, string fullName)
    {
        // ... user creation logic ...
        
        // Add domain event
        AddDomainEvent(new UserRegisteredEvent(Id, email, DateTime.UtcNow));
    }
}
```

### 5. Use in Command Handlers

#### Option A: Automatic Processing (Recommended)

With the interceptor configured, domain events are automatically processed:

```csharp
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserRegistrationResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly DbContext _dbContext;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager, 
        DbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<UserRegistrationResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Create user
        var user = new ApplicationUser(request.Email, request.UserName, request.FullName);
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            // Domain events are automatically processed by the interceptor
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new UserRegistrationResult(true, "User registered successfully.");
        }

        return new UserRegistrationResult(false, result.Errors.First().Description);
    }
}
```

#### Option B: Manual Processing

For cases where you need more control:

```csharp
public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, NotificationResult>
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly DbContext _dbContext;

    public SendNotificationCommandHandler(IOutboxRepository outboxRepository, DbContext dbContext)
    {
        _outboxRepository = outboxRepository;
        _dbContext = dbContext;
    }

    public async Task<NotificationResult> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Create custom outbox message
        var customMessage = new OutboxMessage(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "CustomNotificationEvent",
            JsonSerializer.Serialize(new { 
                UserId = request.UserId, 
                Message = request.Message,
                Priority = request.Priority 
            })
        );

        // Add directly to outbox
        _outboxRepository.AddOutboxMessage(customMessage);

        // Save to database
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new NotificationResult(true, "Notification queued successfully.");
    }
}
```

## Configuration

### Database Migration

You'll need to create a migration to add the `outbox_messages` table:

```bash
dotnet ef migrations add AddOutboxMessages
dotnet ef database update
```

### Message Broker Integration

To complete the implementation, you'll need to:

1. Implement a message publisher interface
2. Update the `OutboxMessageProcessor` to use your actual message broker
3. Configure connection strings and settings

## Benefits

- **Reliability**: Events are never lost, even if the message broker is down
- **Consistency**: Events are saved in the same transaction as the business data
- **Retry Logic**: Failed messages can be retried automatically
- **Monitoring**: Track message processing status and errors
- **Flexibility**: Support both domain events and custom messages
- **Automatic Processing**: Domain events are automatically captured and processed

## Notes

- The current implementation uses `System.Text.Json` for serialization
- The background service polls every 5 seconds (configurable)
- Error handling and retry logic are included
- The implementation is designed to work with any DbContext
- You can use either domain events or direct OutboxMessage creation based on your needs
- The DomainEventToOutboxInterceptor automatically processes domain events from aggregate roots
- All persistence components are now consolidated in the `HiveSpace.Infrastructure.Persistence` project 