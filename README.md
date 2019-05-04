# AspNetCore.SignalR.AzureServiceBus

[![NuGet version](https://img.shields.io/nuget/v/AspNetCore.SignalR.AzureServiceBus.svg)](https://www.nuget.org/packages/AspNetCore.SignalR.AzureServiceBus)
[![AppVeyor build](https://img.shields.io/appveyor/ci/thomaslevesque/aspnetcore-signalr-azureservicebus.svg)](https://ci.appveyor.com/project/thomaslevesque/aspnetcore-signalr-azureservicebus)
[![AppVeyor tests](https://img.shields.io/appveyor/tests/thomaslevesque/aspnetcore-signalr-azureservicebus.svg)](https://ci.appveyor.com/project/thomaslevesque/aspnetcore-signalr-azureservicebus/build/tests)

Provides scale-out support for ASP.NET Core SignalR using an Azure Service Bus topic to dispatch messages to all server instances.

## How to use it

Install the `AspNetCore.SignalR.AzureServiceBus` package, and add this to your `Startup.ConfigureServices` method:

```csharp
services.AddSignalR()
        .AddAzureServiceBus(options =>
        {
            options.ConnectionString = "(your service bus connection string)";
            options.TopicName = "(your topic name)";
        });
```

See [this blog post](https://thomaslevesque.com/2019/03/18/scaling-out-asp-net-core-signalr-using-azure-service-bus/) for details.
