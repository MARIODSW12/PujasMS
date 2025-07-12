using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MassTransit;
using MediatR;
using Pujas.Application.Commands;
using Pujas.Application.DTOs;
using Pujas.Domain.Events;
using Pujas.Infrastructure.Commands;
using Pujas.Infrastructure.interfaces;
using Pujas.Infrastructure.Interfaces;
using Pujas.Infrastructure.Queries;
using RestSharp;

namespace Pujas.API.Controllers
{

public class BidHub: IBidHub
{
    private readonly List<WebSocket> _sockets = new();
    private readonly object _lock = new();
    private readonly IRestClient _restClient;
    private readonly IServiceProvider _serviceProvider;
    public BidHub(IRestClient restClient, IServiceProvider serviceProvider)
    {
        _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task HandleWebSocketConnection(WebSocket webSocket)
    {
        //this here is to ensure that only one thread at a time can add a socket, to avoid any issue with concurrency
        lock (_lock)
        {
            _sockets.Add(webSocket);
        }

        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessage(message, webSocket);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }
            }
        }
        finally
        {
            lock (_lock)
            {
                _sockets.Remove(webSocket);
            }
            webSocket.Dispose();
        }
    }

    private async Task ProcessMessage(string message, WebSocket senderSocket)
    {
        try
        {
            var messageObject = JsonSerializer.Deserialize<WebSocketMessage>(message);

            if (messageObject != null)
            {
                switch (messageObject.Event)
                {
                    case "bid":
                        var bidData = JsonSerializer.Deserialize<CreateBidDto>(messageObject.Data.ToString());
                        await HandleBidMade(bidData);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error procesando mensaje: {ex.Message}");
        }
    }

    public async Task HandleBidMade(CreateBidDto bidData)
    {
        var message = new WebSocketMessage();
        if (bidData == null || bidData.Price <= 0)
        {
            message = new WebSocketMessage
            {
                Event = "bidError",
                Data = new
                {
                    UserId = "unknown",
                    Message = "Datos de puja inválidos."
                }
            };
            SendMessageToAll(message);
            return;
        }
        try { 
        
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("SUBASTA_MS_URL") + "/getById/" + bidData.AuctionId, Method.Get);

            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            if (!APIResponse.IsSuccessful)
            {
                message = new WebSocketMessage
                {
                    Event = "bidError",
                    Data = new
                    {
                        UserId = bidData.UserId,
                        Message = "La subasta no existe."
                    }
                };
                SendMessageToAll(message);
                return;
            }

            var auction = JsonDocument.Parse(APIResponse.Content);

            if (auction.RootElement.GetProperty("status").GetString() != "active")
            {
                message = new WebSocketMessage
                {
                    Event = "bidError",
                    Data = new
                    {
                        UserId = bidData.UserId,
                        Message = "No puedes pujar en una subasta no activa."
                    }
                };
                SendMessageToAll(message);
                return;
            }

            if (auction.RootElement.GetProperty("basePrice").GetDecimal() > bidData.Price)
            {
                message = new WebSocketMessage
                {
                    Event = "bidError",
                    Data = new
                    {
                        UserId = bidData.UserId,
                        Message = "No puedes pujar por debajo del precio base."
                    }
                };
                SendMessageToAll(message);
                return;
            }
            using (var scope = _serviceProvider.CreateScope())
            {
                var _mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var cronService = scope.ServiceProvider.GetRequiredService<ICronJobService>();
                var _publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var lastBid = await _mediator.Send(new GetAuctionLastBidQuery(bidData.AuctionId));
                if (lastBid != null && (lastBid.Price + auction.RootElement.GetProperty("minimumIncrease").GetDecimal()) > bidData.Price)
                {
                    message = new WebSocketMessage
                    {
                        Event = "bidError",
                        Data = new
                        {
                            UserId = bidData.UserId,
                            Message = "La puja debe ser mayor que la ultima puja mas el incremento minimo.",
                        }
                    };
                    SendMessageToAll(message);
                    return;
                }
                if (lastBid != null && lastBid.Date.CompareTo(bidData.Date) <= 0)
                {
                    message = new WebSocketMessage
                    {
                        Event = "bidError",
                        Data = new
                        {
                            UserId = bidData.UserId,
                            Message = "La puja debe ser despues de la anterior puja.",
                        }
                    };
                    SendMessageToAll(message);
                    return;
                }

                var bidId = await _mediator.Send(new CreateBidCommand(bidData));
                message = new WebSocketMessage
                {
                    Event = "bidMade",
                    Data = new { Id = bidId, UserId = bidData.UserId, AuctionId = bidData.AuctionId, Price = bidData.Price, Date = bidData.Date}
                };
                cronService.DeleteCronJob(bidData.AuctionId);
                SendMessageToAll(message);
                var autoBids = await _mediator.Send(new BidMadeCommand(bidData.UserId, bidData.Price, bidData.AuctionId, auction.RootElement.GetProperty("minimumIncrease").GetDecimal()));
                cronService.CreateCronJob(bidData.AuctionId, autoBids.LastOrDefault()?.Price ?? bidData.Price, auction.RootElement.GetProperty("minimumIncrease").GetDecimal());
                foreach (var bid in autoBids)
                {
                    message = new WebSocketMessage
                    {
                        Event = "bidMade",
                        Data = new { Id = bid.Id, UserId = bid.UserId, AuctionId = bid.AuctionId, Price = bid.Price, Date = bid.Date}
                    };
                    SendMessageToAll(message);
                }
            }
        } catch (Exception ex)
        {
            message = new WebSocketMessage
            {
                Event = "bidError",
                Data = new
                {
                    UserId = bidData.UserId,
                    Message = ex.Message,
                }
            };
            SendMessageToAll(message);
            return;
        }


    }


    async public Task SendMessageToAll(WebSocketMessage message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(jsonMessage);
        var segment = new ArraySegment<byte>(buffer);
        List<WebSocket> socketsToSend;

        lock (_lock)
        {
            socketsToSend = new List<WebSocket>(_sockets);
        }

        foreach (var socket in socketsToSend)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

}
}
