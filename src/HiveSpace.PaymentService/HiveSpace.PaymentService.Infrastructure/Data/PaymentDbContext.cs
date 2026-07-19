using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.PaymentService.Domain.Aggregates.External;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HiveSpace.PaymentService.Infrastructure.Data;

public class PaymentDbContext : DbContext
{
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;
    public DbSet<PaymentLinkedOrder> PaymentLinkedOrders { get; set; } = null!;
    public DbSet<PlatformCurrencyPolicyRef> PlatformCurrencyPolicyRefs { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
        MassTransitExtensions.AddEntityOutBox(builder);
    }
}
