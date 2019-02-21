using System;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal class NodeIdProvider : INodeIdProvider
    {
        public Guid NodeId { get; } = Guid.NewGuid();
    }
}