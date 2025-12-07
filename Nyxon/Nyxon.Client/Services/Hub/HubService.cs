using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.Services.Hub
{
    public class HubService : IHubService, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        public event Action<string>? OnMessageNotification;

        public HubService(NavigationManager nav, IJSRuntime jsRuntime)
        {
            //var hubUrl = nav.ToAbsoluteUri("/hubs/chat");
            var hubUrl = nav.BaseUri.Contains("localhost")
                ? new Uri("http://localhost:8080/hubs/chat") // dev only
                : nav.ToAbsoluteUri("/hubs/chat");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () =>
                {
                    return await jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                };
                })
                .WithAutomaticReconnect()
                .Build();

            // notification
            _hubConnection.On<Object>("ReceiveMessageNotification", (payload) =>
            {
                OnMessageNotification?.Invoke(payload.ToString() ?? "");
            });
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                await _hubConnection.StartAsync();
        }
        public async Task DisconnectAsync()
        {
            await _hubConnection.StopAsync();
        }
        public async Task JoinConversationAsync(Guid conversationId)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
                await _hubConnection.InvokeAsync("JoinConversation", conversationId.ToString());
        }
        public async Task LeaveConversationAsync(Guid conversationId)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
                await _hubConnection.InvokeAsync("LeaveConversation", conversationId.ToString());
        }
        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
        }
    }
}