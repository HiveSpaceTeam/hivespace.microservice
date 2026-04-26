using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HiveSpace.NotificationService.Core.Persistence.SeedData;

internal sealed class NotificationTemplateSeeder(
    NotificationDbContext           db,
    ILogger<NotificationTemplateSeeder> logger) : ISeeder
{
    public int Order => 1;

    private static readonly (string EventType, NotificationChannel Channel, string Locale, string Subject, string Body)[] Seeds =
    [
        (
            NotificationEventType.OrderConfirmed, NotificationChannel.InApp, "vi",
            "Đơn hàng đã được xác nhận",
            "Đơn hàng #{{ orderCode }} của bạn đã được người bán xác nhận và đang được chuẩn bị."
        ),
        (
            NotificationEventType.OrderConfirmed, NotificationChannel.Email, "vi",
            "Đơn hàng #{{ orderCode }} đã được xác nhận",
            """
            <p>Xin chào,</p>
            <p>Đơn hàng <strong>#{{ orderCode }}</strong> của bạn đã được người bán xác nhận và đang được chuẩn bị giao.</p>
            <p>Cảm ơn bạn đã mua sắm tại HiveSpace!</p>
            """
        ),
        (
            NotificationEventType.OrderCancelled, NotificationChannel.InApp, "vi",
            "Đơn hàng đã bị huỷ",
            "Đơn hàng #{{ orderCode }} của bạn đã bị huỷ.{{ if refundAmount > 0 }} Hoàn tiền {{ refundAmount }}đ sẽ được xử lý trong 3–5 ngày làm việc.{{ end }}"
        ),
        (
            NotificationEventType.OrderCancelled, NotificationChannel.Email, "vi",
            "Đơn hàng #{{ orderCode }} đã bị huỷ",
            """
            <p>Xin chào,</p>
            <p>Đơn hàng <strong>#{{ orderCode }}</strong> của bạn đã bị huỷ.</p>
            {{ if refundAmount > 0 }}
            <p>Khoản hoàn tiền <strong>{{ refundAmount }}đ</strong> sẽ được xử lý trong 3–5 ngày làm việc.</p>
            {{ end }}
            <p>Nếu bạn có thắc mắc, vui lòng liên hệ bộ phận hỗ trợ.</p>
            """
        ),
        (
            NotificationEventType.NewOrderReceived, NotificationChannel.InApp, "vi",
            "Đơn hàng mới",
            "Bạn có đơn hàng mới #{{ orderCode }} cần xác nhận."
        ),
        (
            NotificationEventType.NewOrderReceived, NotificationChannel.Email, "vi",
            "Đơn hàng mới #{{ orderCode }}",
            """
            <p>Xin chào,</p>
            <p>Bạn vừa nhận được đơn hàng mới <strong>#{{ orderCode }}</strong>.</p>
            <p>Vui lòng đăng nhập vào HiveSpace Seller Center để xác nhận đơn hàng trong thời gian sớm nhất.</p>
            """
        ),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var seedKeys = Seeds.Select(s => (s.EventType, s.Channel, s.Locale)).ToList();

        var existing = await db.NotificationTemplates
            .Where(t => seedKeys.Select(k => k.EventType).Contains(t.EventType))
            .Select(t => new { t.EventType, t.Channel, t.Locale })
            .ToListAsync(ct);

        var existingSet = existing
            .Select(t => (t.EventType, t.Channel, t.Locale))
            .ToHashSet();

        var toAdd = Seeds.Where(s => !existingSet.Contains((s.EventType, s.Channel, s.Locale))).ToList();
        if (toAdd.Count == 0)
        {
            logger.LogDebug("All expected NotificationTemplates already exist. Skipping.");
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            var currentExisting = await db.NotificationTemplates
                .Where(t => seedKeys.Select(k => k.EventType).Contains(t.EventType))
                .Select(t => new { t.EventType, t.Channel, t.Locale })
                .ToListAsync(ct);

            var currentSet = currentExisting
                .Select(t => (t.EventType, t.Channel, t.Locale))
                .ToHashSet();

            var toAddNow = Seeds.Where(s => !currentSet.Contains((s.EventType, s.Channel, s.Locale))).ToList();
            if (toAddNow.Count == 0) return;

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            foreach (var (eventType, channel, locale, subject, body) in toAddNow)
                db.NotificationTemplates.Add(
                    NotificationTemplate.Create(eventType, channel, locale, subject, body));

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });

        logger.LogInformation("Seeded {Count} NotificationTemplate(s).", toAdd.Count);
    }
}
