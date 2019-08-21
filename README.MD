# ASP.NET Core & RabbitMQ Sample

Yes, yes, this is another example where the [basic guide](#https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html) from the official RabbitMQ website is used.

The original guide contains two console apps to demonstrate the basic functions.

In this sample, I created two Asp.Net Core 3.0 projects as two microservices (SOA), each of them run as independent web-service on their own ports and can do some useful work with some data.

## Requirements

- [Visual Studio 2019](#https://visualstudio.microsoft.com/ru/vs/)
- [RabbitMQ server](#https://www.rabbitmq.com/download.html)
- [.Net Core 3.0 SDK](#https://dotnet.microsoft.com/download/dotnet-core/3.0)

## How it works

As I said above, I have two services:

1) Service_1 - is a message sender, `http://localhost:5001`;
2) Service_2 - is a message receiver, `http://localhost:5002`.

**First service** uses this settings `"Send Queue": "service-1"` as a queue name for publish messages.

**Second service** uses this settings `"ReceiveQueue": "service-1"` as a queue name for subscribing.

These values contained as usually in `appsettings.json`.

Also, for clarity I used SignalR to show when message is posted\received.

## Usage

You need to run both of these services, then open in a browser `http://localhost:5001` and `http://localhost:5002` (you will get two tabs).

So, now enter some text in text area under *RabbitMQ message data* title and click on *Publish event* button, you should see next for first service:

![Service_1](https://github.com/f14shm4n/AspNetCore_RabbitMQ/blob/master/Assets/s1_send_message.png)

and next for second when message is received:

![Service_2](https://github.com/f14shm4n/AspNetCore_RabbitMQ/blob/master/Assets/s2_receive_message.png)

## For what it is?

This is simplest version of using RabbitMQ message broker in a typical SOA project which you can easily download, compile, run and see the results and roughly imagine how it can be done in real project.

In fact, I created a simplest version of [this project](#https://github.com/dotnet-architecture/eShopOnContainers). I implemented only event bus which should demostrate the RabbitMQ functionality.

## Disclaimer

Provided code in this repository does not pretend to absolutely ideal of code writing, it just for demo using (keep it in mind :)).

## License

[MIT](#https://github.com1/f14shm4n/AspNetCore_RabbitMQ/blob/master/LICENSE.md)
