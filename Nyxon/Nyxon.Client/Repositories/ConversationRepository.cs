using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly IApiService _apiService;

        public ConversationRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<PrekeyBundleResponse?> GetPrekeyBundleAsync(string username)
        {
            try
            {
                return await _apiService.GetAsync<PrekeyBundleResponse>($"api/prekeys/{username}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<ConversationVaultDto?> FetchVaultAsync(Guid conversationId)
        {
            try
            {
                return await _apiService.GetAsync<ConversationVaultDto>($"api/conversation/vault/{conversationId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<InboxDto?> FetchInboxAsync()
        {
            try
            {
                return await _apiService.GetAsync<InboxDto>($"api/conversation/inbox");
            }
            catch
            {
                return null;
            }
        }
        public async Task<CreateConversationResponse?> CreateConversationAsync(CreateConversationRequest conversationRequest)
        {
            try
            {
                return await _apiService.PostAsync<CreateConversationResponse, CreateConversationRequest>("api/conversation", conversationRequest);
            }
            catch
            {
                return null;
            }
        }

        public async Task<SendMessageResponse?> SendMessageAsync(SendMessageRequest messageRequest)
        {
            try
            {
                return await _apiService.PostAsync<SendMessageResponse, SendMessageRequest>("api/message/send", messageRequest);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateConversationVaultAsync(Guid conversationId, ConversationVaultData vaultData)
        {
            try
            {
                await _apiService.PostAsync<ConversationVaultData>($"/api/conversation/vault/{conversationId}", vaultData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during saving conversation vault: {ex.Message}");
                return false;
            }
        }

        public async Task<MessageResponse?> GetMessageAsync(string kvKey)
        {
            try
            {
                return await _apiService.GetAsync<MessageResponse>($"api/message/{kvKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fetching message: {ex.Message}");
                return null;
            }
        }
        public async Task<MessageReceivedStateUpdateResponse?> ReceiveMessageServerUpdateAsync(MessageReceivedStateUpdateRequest request)
        {
            try
            {
                return await _apiService.PatchAsync<MessageReceivedStateUpdateResponse, MessageReceivedStateUpdateRequest>("api/message/receive", request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during receive server update: {ex.Message}");
                return null;
            }
        }

        public async Task<MessagesBundleDto?> FetchHistoryAsync(Guid conversationId, int count, int lastSequenceNumber)
        {
            try
            {
                return await _apiService.GetAsync<MessagesBundleDto>($"api/message/{conversationId}/bundle/{count}/{lastSequenceNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fetching messages bundle: {ex.Message}");
                return null;
            }
        }

        public async Task<MessagesBundleDto?> FetchRecentAsync(Guid conversationId)
        {
            try
            {
                return await _apiService.GetAsync<MessagesBundleDto>($"api/message/{conversationId}/recent");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fetching recent messages bundle: {ex.Message}");
                return null;
            }
        }

        public async Task<List<string>> GenerateInviteCodesAsync(int count = 1)
        {
            try
            {
                var dto = new NewInviteCodesRequest(count);
                var response = await _apiService.PostAsync<NewInviteCodesResponse, NewInviteCodesRequest>("api/invites", dto);
                return response.Codes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during creating new invite codes: {ex.Message}");
                return null;
            }
        }
        public async Task UpdateReadAsync(Guid conversationId)
        {
            try
            {
                await _apiService.PostAsync($"api/conversation/{conversationId}/read");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during updating read status: {ex.Message}");
            }
        }

    }
}