using FluentAssertions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.NotificationService.Tests.Domain;

public class NotificationEventGroupTests
{
    [Theory]
    [InlineData(NotificationEventType.OrderConfirmed,            NotificationEventGroup.OrderUpdates)]
    [InlineData(NotificationEventType.OrderCancelled,            NotificationEventGroup.OrderUpdates)]
    [InlineData(NotificationEventType.OrderDelivered,            NotificationEventGroup.OrderUpdates)]
    [InlineData(NotificationEventType.OrderShipped,              NotificationEventGroup.OrderUpdates)]
    [InlineData(NotificationEventType.PaymentSucceeded,          NotificationEventGroup.Payment)]
    [InlineData(NotificationEventType.PaymentFailed,             NotificationEventGroup.Payment)]
    [InlineData(NotificationEventType.RefundProcessed,           NotificationEventGroup.Payment)]
    [InlineData(NotificationEventType.NewOrderReceived,          NotificationEventGroup.SellerOrders)]
    [InlineData(NotificationEventType.LowStockAlert,             NotificationEventGroup.Inventory)]
    [InlineData(NotificationEventType.EmailVerificationRequested, NotificationEventGroup.AccountActivity)]
    [InlineData(NotificationEventType.EmailVerified,             NotificationEventGroup.AccountActivity)]
    [InlineData("promo.flash",                                   NotificationEventGroup.Promotions)]
    public void FromEventType_MapsToExpectedGroup(string eventType, string expectedGroup)
    {
        NotificationEventGroup.FromEventType(eventType).Should().Be(expectedGroup);
    }

    [Theory]
    [InlineData("Buyer",       new[] { NotificationEventGroup.OrderUpdates, NotificationEventGroup.Payment, NotificationEventGroup.Promotions, NotificationEventGroup.Surveys })]
    [InlineData("Seller",      new[] { NotificationEventGroup.SellerOrders, NotificationEventGroup.Inventory, NotificationEventGroup.OrderUpdates, NotificationEventGroup.Payment })]
    [InlineData("Admin",       new string[0])]
    [InlineData("SystemAdmin", new string[0])]
    [InlineData(null,          new string[0])]
    public void ForRole_String_ReturnsExpectedGroups(string? role, string[] expected)
    {
        NotificationEventGroup.ForRole(role).Should().BeEquivalentTo(expected);
    }

    public static TheoryData<FakeUserContext, string[]> ForRoleUserContextCases => new()
    {
        { new FakeUserContext { UserId = Guid.Empty, Roles = ["Buyer"] },       [NotificationEventGroup.OrderUpdates, NotificationEventGroup.Payment, NotificationEventGroup.Promotions, NotificationEventGroup.Surveys] },
        { new FakeUserContext { UserId = Guid.Empty, Roles = ["Seller"] },      [NotificationEventGroup.SellerOrders, NotificationEventGroup.Inventory, NotificationEventGroup.OrderUpdates, NotificationEventGroup.Payment] },
        { new FakeUserContext { UserId = Guid.Empty, Roles = ["Admin"] },       [] },
        { new FakeUserContext { UserId = Guid.Empty, Roles = ["SystemAdmin"] }, [] },
        { new FakeUserContext { UserId = Guid.Empty, Roles = [] },              [] },
    };

    [Theory]
    [MemberData(nameof(ForRoleUserContextCases))]
    public void ForRole_UserContext_ReturnsExpectedGroups(FakeUserContext ctx, string[] expected)
    {
        NotificationEventGroup.ForRole(ctx).Should().BeEquivalentTo(expected);
    }
}
