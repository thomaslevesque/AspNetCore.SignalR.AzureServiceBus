namespace AspNetCore.SignalR.AzureServiceBus.Internal
{
    internal interface IMessageSenderProvider
    {
        IMessageSender GetMessageSenderForHub(string hubTypeName);
    }
}