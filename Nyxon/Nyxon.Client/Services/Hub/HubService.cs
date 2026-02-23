using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.Services.Hub
{
    public class HubService : IHubService, IAsyncDisposable
    {
        private readonly HubConnection _hubConnection;

        private readonly HashSet<Guid> _joinedConversations = new();

        public event Action<string>? OnMessageNotification;
        public event Action? OnNewConversationNotification;

        public HubService(NavigationManager nav, IJSRuntime jsRuntime)
        {
            var hubUrl = nav.ToAbsoluteUri("/hubs/chat");
            // var hubUrl = nav.BaseUri.Contains("localhost")
            //     ? new Uri("http://localhost:8000/hubs/chat") // dev only
            //     : nav.ToAbsoluteUri("/hubs/chat");

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

            Console.WriteLine("Registering NewConversationNotification handler");

            // notification
            _hubConnection.On<string>("ReceiveMessageNotification", kvKey =>
            {
                OnMessageNotification?.Invoke(kvKey);
            });

            _hubConnection.On("NewConversationNotification", () =>
            {
                Console.WriteLine($"HubService received NewConversationNotification, invoking event...");
                if (OnNewConversationNotification == null)
                    Console.WriteLine("Warning: No subscribers!");
                OnNewConversationNotification?.Invoke();
            });

        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
                await _hubConnection.StartAsync();
        }
        public async Task DisconnectAsync()
        {
            if (_hubConnection.State != HubConnectionState.Disconnected)
                await _hubConnection.StopAsync();
        }
        public async Task JoinAllConversationsAsync(List<Guid> conversationIds, Guid userId)
        {
            foreach (var conversationId in conversationIds)
            {
                await JoinConversationAsync(conversationId, userId);
            }
        }
        public async Task JoinConversationAsync(Guid conversationId, Guid userId)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                if (_joinedConversations.Add(conversationId))
                    await _hubConnection.InvokeAsync("JoinConversation", conversationId);
            }
        }
        public async Task LeaveConversationAsync(Guid conversationId, Guid userId)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
                await _hubConnection.InvokeAsync("LeaveConversation", conversationId);
        }

        public void CheckHubState()
        {
            Console.WriteLine($"Hub state: {_hubConnection.State}");
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.DisposeAsync();
        }
    }
}