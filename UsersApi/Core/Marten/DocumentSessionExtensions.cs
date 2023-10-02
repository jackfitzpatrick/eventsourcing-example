﻿using Marten;

namespace UsersApi.Core.Marten
{
    public static class DocumentSessionExtensions
    {
        public static async Task Add<T>(this IDocumentSession documentSession, Guid id, object @event, CancellationToken ct) where T : class
        {
            documentSession.Events.StartStream<T>(id, @event);
            await documentSession.SaveChangesAsync(token: ct);
        }

        public static async Task GetAndUpdate<T>(this IDocumentSession documentSession, Guid id, int version, Func<T, object> handle, CancellationToken ct) where T : class
        {
            await documentSession.Events.WriteToAggregate<T>(id, version, stream => stream.AppendOne(handle(stream.Aggregate)), ct);
        }
    }
}
