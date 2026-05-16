using HiveSpace.Domain.Shared.Enumerations;
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

    private static readonly (string EventType, NotificationChannel Channel, Culture Locale, string Subject, string Body)[] Seeds =
    [
        (
            NotificationEventType.OrderConfirmed, NotificationChannel.InApp, Culture.Vi,
            "Đơn hàng đã được xác nhận",
            "Đơn hàng #{{ orderCode }} của bạn đã được người bán xác nhận và đang được chuẩn bị."
        ),
        (
            NotificationEventType.OrderConfirmed, NotificationChannel.Email, Culture.Vi,
            "Đơn hàng #{{ orderCode }} đã được xác nhận",
            """
            <p>Xin chào,</p>
            <p>Đơn hàng <strong>#{{ orderCode }}</strong> của bạn đã được người bán xác nhận và đang được chuẩn bị giao.</p>
            <p>Cảm ơn bạn đã mua sắm tại HiveSpace!</p>
            """
        ),
        (
            NotificationEventType.OrderCancelled, NotificationChannel.InApp, Culture.Vi,
            "Đơn hàng đã bị huỷ",
            "Đơn hàng #{{ orderCode }} của bạn đã bị huỷ.{{ if refundAmount > 0 }} Hoàn tiền {{ refundAmount }}đ sẽ được xử lý trong 3–5 ngày làm việc.{{ end }}"
        ),
        (
            NotificationEventType.OrderCancelled, NotificationChannel.Email, Culture.Vi,
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
            NotificationEventType.NewOrderReceived, NotificationChannel.InApp, Culture.Vi,
            "Đơn hàng mới",
            "Bạn có đơn hàng mới #{{ orderCode }} cần xác nhận."
        ),
        (
            NotificationEventType.NewOrderReceived, NotificationChannel.Email, Culture.Vi,
            "Đơn hàng mới #{{ orderCode }}",
            """
            <p>Xin chào,</p>
            <p>Bạn vừa nhận được đơn hàng mới <strong>#{{ orderCode }}</strong>.</p>
            <p>Vui lòng đăng nhập vào HiveSpace Seller Center để xác nhận đơn hàng trong thời gian sớm nhất.</p>
            """
        ),
        (
            NotificationEventType.EmailVerificationRequested, NotificationChannel.Email, Culture.Vi,
            "Kích hoạt tài khoản HiveSpace của bạn",
            """
            <p>Xin chào {{ userName }},</p>
            <p>Cảm ơn bạn đã đăng ký HiveSpace! Vui lòng nhấp vào nút bên dưới để xác minh địa chỉ email của bạn.</p>
            <p style="text-align:center;margin:24px 0;">
              <a href="{{ verificationLink }}"
                 style="background:#004d99;color:#fff;padding:12px 24px;border-radius:4px;text-decoration:none;display:inline-block;">
                Xác minh email
              </a>
            </p>
            <p>Liên kết có hiệu lực đến <strong>{{ expiresAt }}</strong>.</p>
            <p>Nếu nút không hoạt động, hãy sao chép và dán liên kết sau vào trình duyệt:<br/>{{ verificationLink }}</p>
            <p>Nếu bạn không đăng ký tài khoản, vui lòng bỏ qua email này.</p>
            """
        ),
        (
            NotificationEventType.EmailVerificationRequested, NotificationChannel.Email, Culture.En,
            "Activate your HiveSpace account",
            """
            <p>Hi {{ userName }},</p>
            <p>Thanks for signing up for HiveSpace! Please click the button below to verify your email address.</p>
            <p style="text-align:center;margin:24px 0;">
              <a href="{{ verificationLink }}"
                 style="background:#004d99;color:#fff;padding:12px 24px;border-radius:4px;text-decoration:none;display:inline-block;">
                Verify Email Address
              </a>
            </p>
            <p>This link expires at <strong>{{ expiresAt }}</strong>.</p>
            <p>If the button doesn't work, copy and paste the link below into your browser:<br/>{{ verificationLink }}</p>
            <p>If you did not create an account, please ignore this email.</p>
            """
        ),
        (
            NotificationEventType.EmailVerified, NotificationChannel.Email, Culture.Vi,
            "Email đã được xác minh thành công",
            """
            <p>Xin chào {{ userName }},</p>
            <p>Email của bạn đã được xác minh thành công. Bây giờ bạn có thể sử dụng đầy đủ các tính năng của HiveSpace.</p>
            <p>Cảm ơn bạn đã tin tưởng sử dụng HiveSpace!</p>
            """
        ),
        (
            NotificationEventType.EmailVerified, NotificationChannel.Email, Culture.En,
            "Email verified successfully",
            """
            <p>Hi {{ userName }},</p>
            <p>Your email address has been verified successfully. You now have full access to all HiveSpace features.</p>
            <p>Thank you for joining HiveSpace!</p>
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
