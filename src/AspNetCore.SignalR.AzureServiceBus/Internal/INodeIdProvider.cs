using System;

namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal interface INodeIdProvider
    {
        Guid NodeId { get; }
    }
}