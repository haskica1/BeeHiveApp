using System.Threading.Channels;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Models;

namespace BeeHive.Infrastructure.Email;

/// <summary>
/// In-memory <see cref="IEmailQueue"/> backed by an unbounded channel. Emails are best-effort:
/// anything still queued when the process stops is lost, which is acceptable for
/// notification mail (the in-app notification is already persisted).
/// </summary>
public sealed class ChannelEmailQueue : IEmailQueue
{
    private readonly Channel<QueuedEmail> _channel = Channel.CreateUnbounded<QueuedEmail>();

    public void Enqueue(QueuedEmail email) => _channel.Writer.TryWrite(email);

    public ValueTask<QueuedEmail> DequeueAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAsync(cancellationToken);
}
