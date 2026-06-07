using System.Diagnostics;
using MassTransit;

namespace HiveSpace.Infrastructure.Messaging.Diagnostics;

public sealed class MassTransitTraceSendFilter<T> : IFilter<SendContext<T>>
    where T : class
{
    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        MassTransitTraceHeaders.Apply(context);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
        => context.CreateFilterScope("hivespace-trace-send");
}

public sealed class MassTransitTracePublishFilter<T> : IFilter<PublishContext<T>>
    where T : class
{
    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        MassTransitTraceHeaders.Apply(context);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
        => context.CreateFilterScope("hivespace-trace-publish");
}

public sealed class MassTransitTraceConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    public Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        MassTransitTraceHeaders.TagCurrentActivity(context);
        return next.Send(context);
    }

    public void Probe(ProbeContext context)
        => context.CreateFilterScope("hivespace-trace-consume");
}

internal static class MassTransitTraceHeaders
{
    public static void Apply(SendContext context)
    {
        var activity = Activity.Current;
        if (activity is null)
            return;

        var requestId = GetActivityValue(activity, "request.id");
        if (!string.IsNullOrWhiteSpace(requestId))
            context.Headers.Set("request-id", requestId);
    }

    public static void TagCurrentActivity(ConsumeContext context)
    {
        var activity = Activity.Current;
        if (activity is null)
            return;

        if (context.MessageId.HasValue)
            activity.SetTag("messaging.message_id", context.MessageId.Value.ToString());

        if (context.CorrelationId.HasValue)
            activity.SetTag("messaging.correlation_id", context.CorrelationId.Value.ToString());

        if (context.ConversationId.HasValue)
            activity.SetTag("messaging.conversation_id", context.ConversationId.Value.ToString());

        if (context.Headers.TryGetHeader("request-id", out var requestId) && requestId is not null)
        {
            var value = requestId.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                activity.SetTag("request.id", value);
                activity.AddBaggage("request.id", value);
            }
        }
    }

    private static string? GetActivityValue(Activity activity, string key)
    {
        foreach (var tag in activity.Tags)
        {
            if (string.Equals(tag.Key, key, StringComparison.Ordinal))
                return tag.Value;
        }

        foreach (var item in activity.Baggage)
        {
            if (string.Equals(item.Key, key, StringComparison.Ordinal))
                return item.Value;
        }

        return null;
    }
}
