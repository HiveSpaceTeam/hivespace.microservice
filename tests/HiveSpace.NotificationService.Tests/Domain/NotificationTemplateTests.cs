using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.NotificationService.Core.DomainModels;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Domain;

public class NotificationTemplateTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var template = NotificationTemplate.Create(
            "order.confirmed", NotificationChannel.Email, Culture.Vi,
            "Order confirmed", "Hello {{name}}, your order is confirmed.");

        template.EventType.Should().Be("order.confirmed");
        template.Channel.Should().Be(NotificationChannel.Email);
        template.Locale.Should().Be(Culture.Vi);
        template.Subject.Should().Be("Order confirmed");
        template.BodyTemplate.Should().Be("Hello {{name}}, your order is confirmed.");
        template.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Update_ChangesSubjectAndBody()
    {
        var template = NotificationTemplate.Create(
            "order.confirmed", NotificationChannel.Email, Culture.Vi,
            "Original subject", "Original body");

        template.Update("Updated subject", "Updated body");

        template.Subject.Should().Be("Updated subject");
        template.BodyTemplate.Should().Be("Updated body");
        template.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
    }
}
