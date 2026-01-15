using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using NSec.Cryptography;
using Nyxon.Client.Repositories;
using Nyxon.Core.Services;
using Org.BouncyCastle.Asn1.X509;

namespace Nyxon.Client.Services.Messaging
{
    public class ActiveConversationService : IActiveConversationService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly UserContext _userContext;
        private readonly ICryptoService _cryptoService;
        private readonly IUserVaultService _userVaultService;

        private const int SnapshotFrequency = 5;

        public Guid? ConversationId { get; private set; }
        private RuntimeConvVaultData? EncryptedVault { get; set; }
        private int SendingCounter { get; set; }
        private int ReceivingCounter { get; set; }
        public DateTime? UpdatedAt { get; private set; }

        public event Action<List<ChatMessage>>? MessagesDecrypted;

        public ActiveConversationService(IConversationRepository conversationRepository,
            UserContext userContext,
            ICryptoService cryptoService,
            IUserVaultService userVaultService)
        {
            _conversationRepository = conversationRepository;
            _userContext = userContext;
            _cryptoService = cryptoService;
            _userVaultService = userVaultService;
        }

        public async Task InitializeNewAsync(Guid conversationId, ConversationVaultData encryptedVault)
        {
            if (!_userContext.IsAuthenticated) return;

            if (encryptedVault == null)
                throw new InvalidOperationException("Conversation vault cant be null");

            ConversationId = conversationId;
            EncryptedVault = new RuntimeConvVaultData(encryptedVault);
            SendingCounter = 0;
            ReceivingCounter = 0;
        }

        public async Task InitializeAsync(Guid conversationId)
        {
            if (!_userContext.IsAuthenticated) return;

            var conversationVault = await _conversationRepository.FetchVaultAsync(conversationId);
            if (conversationVault == null)
                throw new InvalidOperationException("Failed to fetch conversatoin vault");

            EncryptedVault = new RuntimeConvVaultData(conversationVault.VaultData);
            ConversationId = conversationId;
            SendingCounter = conversationVault.SendCounter;
            ReceivingCounter = conversationVault.RecvCounter;
        }

        public async Task<NewMessageObject?> SendMessageAsync(string message)
        {
            try
            {
                if (!_userContext.IsAuthenticated) return null;
                if (!_userVaultService.IsUnlocked) return null;
                if (EncryptedVault == null) return null;

                var session = EncryptedVault.Sending.Session.Clone();
                session.MessageIndex++;

                SendMessageRequest? requestDto = null;

                var messageBytes = Encoding.UTF8.GetBytes(message);

                // rotate
                if (session.MessageIndex >= session.RotateAfter)
                {
                    Console.WriteLine("Rotating...");
                    session.RotationIndex++;
                    session.MessageIndex = 1;

                    requestDto = await CreateMessageWithRotationAsync(session, messageBytes);

                    session.EncryptedCurrentSessionKey = requestDto.EncryptedCurrentSessionKey;

                    // snapshot
                    if (session.RotationIndex % SnapshotFrequency == 0 && session.RotationIndex != 0)
                    {
                        Console.WriteLine("Creating snapshot...");
                        requestDto.Snapshot = await CreateSnapshotAsync(session);
                        Console.WriteLine("Snapshot finsihed");
                    }
                    Console.WriteLine("Rotation finished");
                }
                else
                {
                    Console.WriteLine("Creating message key...");
                    requestDto = await CreateMessageOnlyAsync(session, messageBytes);
                    Console.WriteLine("Message key finsihed");
                }

                if (requestDto == null)
                    throw new ArgumentNullException("Dto creation failed");

                var responseDto = await _conversationRepository.SendMessageAsync(requestDto);
                if (responseDto == null)
                    throw new Exception("Failed to send the message");

                // save session state
                EncryptedVault.Sending.Session = session;
                // save snapshot if exists
                if (requestDto.Snapshot != null)
                    EncryptedVault.Sending.AddSnapshot(requestDto.Snapshot);
                // increase counter
                ++SendingCounter;
                // update datetime
                UpdatedAt = responseDto.CreatedAt;

                // return info for ui
                return new NewMessageObject()
                {
                    Id = responseDto.Id,
                    SequenceNumber = responseDto.MessageSequence,
                    Content = message,
                    SentAt = responseDto.CreatedAt
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sending message: {ex.Message}");
                throw;
            }
        }

        private async Task<SendMessageRequest> CreateMessageWithRotationAsync(SessionState session, byte[] message)
        {
            byte[]? decryptedSessionKey = null;
            byte[]? decryptedNewSessionKey = null;
            byte[]? messageKey = null;

            try
            {
                decryptedSessionKey = await _userVaultService.DecryptAsync(session.EncryptedCurrentSessionKey, AadFactory.ForSendingSessionKey((Guid)ConversationId, session.RotationIndex - 1));
                Console.WriteLine($"Decrypted session key: {Convert.ToBase64String(decryptedSessionKey)}");
                decryptedNewSessionKey = _cryptoService.AdvanceRatchet(decryptedSessionKey, session.RotationIndex, (Guid)ConversationId);
                Console.WriteLine($"Decrypted new session key [{session.RotationIndex}]: {Convert.ToBase64String(decryptedNewSessionKey)}");

                messageKey = _cryptoService.DeriveMessageKey(decryptedNewSessionKey, session.RotationIndex, 1, (Guid)ConversationId);
                Console.WriteLine($"Decrypted message key [{session.MessageIndex}]: {Convert.ToBase64String(messageKey)}");

                Console.WriteLine($"AAD: {Convert.ToBase64String(AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex))}");

                byte[] encryptedMessage = _cryptoService.EncryptWithKey(message, messageKey, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));
                byte[] encryptedNewSessionKey = await _userVaultService.EncryptAsync(decryptedNewSessionKey, AadFactory.ForSendingSessionKey((Guid)ConversationId, session.RotationIndex));

                return new SendMessageRequest(
                    conversationId: (Guid)ConversationId,
                    sessionIndex: session.RotationIndex,
                    messageIndex: session.MessageIndex,
                    encryptedPayload: encryptedMessage,
                    encryptedCurrentSessionKey: encryptedNewSessionKey
                );
            }
            catch
            {
                throw;
            }
            finally
            {
                if (decryptedSessionKey != null) CryptographicOperations.ZeroMemory(decryptedSessionKey);
                if (decryptedNewSessionKey != null) CryptographicOperations.ZeroMemory(decryptedNewSessionKey);
                if (messageKey != null) CryptographicOperations.ZeroMemory(messageKey);
            }
        }

        private async Task<SendMessageRequest> CreateMessageOnlyAsync(SessionState session, byte[] message)
        {
            byte[]? decryptedSessionKey = null;
            byte[]? messageKey = null;

            try
            {
                decryptedSessionKey = await _userVaultService.DecryptAsync(session.EncryptedCurrentSessionKey, AadFactory.ForSendingSessionKey((Guid)ConversationId, session.RotationIndex));
                Console.WriteLine($"Decrypted session key [{session.RotationIndex}]: {Convert.ToBase64String(decryptedSessionKey)}");
                messageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, session.RotationIndex, 1, (Guid)ConversationId);
                Console.WriteLine($"Decrypted message key [1]: {Convert.ToBase64String(messageKey)}");

                for (int i = 2; i <= session.MessageIndex; i++)
                {
                    messageKey = _cryptoService.DeriveMessageKey(messageKey, session.RotationIndex, i, (Guid)ConversationId);
                    Console.WriteLine($"Decrypted message key [{i}]: {Convert.ToBase64String(messageKey)}");
                }
                Console.WriteLine($"Length: {messageKey.Length}");

                Console.WriteLine($"AAD: {Convert.ToBase64String(AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex))}");

                byte[] encryptedMessage = _cryptoService.EncryptWithKey(message, messageKey, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));

                return new SendMessageRequest(
                    conversationId: (Guid)ConversationId,
                    sessionIndex: session.RotationIndex,
                    messageIndex: session.MessageIndex,
                    encryptedPayload: encryptedMessage
                );
            }
            catch
            {
                throw;
            }
            finally
            {
                if (decryptedSessionKey != null) CryptographicOperations.ZeroMemory(decryptedSessionKey);
                if (messageKey != null) CryptographicOperations.ZeroMemory(messageKey);
            }
        }

        private async Task<Snapshot> CreateSnapshotAsync(SessionState session)
        {
            return new Snapshot(session.RotationIndex, session.EncryptedCurrentSessionKey);
        }
        public async Task<ChatMessage> ReceiveMessageAsync(string kvKey)
        {
            Console.WriteLine("Reached active conversation service method");
            try
            {
                if (!_userContext.IsAuthenticated) return null;
                if (!_userVaultService.IsUnlocked) return null;
                if (EncryptedVault == null) return null;

                // fetch the message
                var message = await _conversationRepository.GetMessageAsync(kvKey);
                if (message == null)
                    throw new Exception("Message fetch failed");

                // session state clone
                var session = EncryptedVault.Receiving.Session.Clone();

                var keyDerivationInstructions = CalculateDerivationInstructions(session, message);
                if (keyDerivationInstructions == null)
                {
                    // this message is form the past
                    // TODO: use history decryption logic to decrypt this message
                }

                MessageReceivedStateUpdateRequest requestDto;
                string content;

                // rotate ratchet
                if (keyDerivationInstructions.RatchetRotations > 0)
                {
                    keyDerivationInstructions.RatchetRotations += session.RotationIndex;
                    Console.WriteLine("Ratchet rotating...");
                    session.MessageIndex = 0;
                    (requestDto, content) = await DecryptMessageWithRotationAsync(
                        session: session,
                        instructions: keyDerivationInstructions,
                        encryptedPayload: message.EncryptedPayload
                    );
                    // update temporal state
                    session.EncryptedCurrentSessionKey = requestDto.EncryptedNewSessionKey;
                    session.RotationIndex = requestDto.SessionIndex;
                    session.MessageIndex = requestDto.MessageIndex;
                    Console.WriteLine("Ratchet rotation done");
                }

                // derive message key
                else
                {
                    Console.WriteLine("Deriving message key...");
                    (requestDto, content) = await DecryptMessageOnlyAsync(
                        session: session,
                        instructions: keyDerivationInstructions,
                        encryptedPayload: message.EncryptedPayload
                    );
                    // update temporal state
                    session.MessageIndex = requestDto.MessageIndex;
                    Console.WriteLine("Message key derivation done");
                }

                // send request to server
                var responseDto = await _conversationRepository.ReceiveMessageServerUpdateAsync(requestDto);
                if (responseDto == null)
                    throw new Exception("Failed to update server about receiving message. Message read aborted.");

                // save session state
                EncryptedVault.Receiving.Session = session;
                // save snapshot if exists
                if (requestDto.Snapshots != null && requestDto.Snapshots.Any())
                    EncryptedVault.Receiving.MergeHistory(requestDto.Snapshots);
                // increase counter
                ReceivingCounter += keyDerivationInstructions.Jump;
                // update datetime
                UpdatedAt = responseDto.UpdatedAt;

                return new ChatMessage(
                    id: message.Id,
                    conversationId: (Guid)ConversationId,
                    senderUsername: message.SenderUsername,
                    sequenceNumber: message.SequenceNumber,
                    content: content,
                    sentAt: message.CreatedAt,
                    isMine: false
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during receiving message: {ex.Message}");
                throw;
            }
        }

        private async Task<(MessageReceivedStateUpdateRequest requestDto, string message)> DecryptMessageOnlyAsync(SessionState session, KeyDerivationInstructions instructions, byte[] encryptedPayload)
        {
            byte[]? decryptedSessionKey = null;
            byte[]? messageKey = null;

            try
            {
                decryptedSessionKey = await _userVaultService.DecryptAsync(session.EncryptedCurrentSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));
                Console.WriteLine($"Decrypted session key [{session.RotationIndex}]: {Convert.ToBase64String(decryptedSessionKey)}");
                messageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, session.RotationIndex, 1, (Guid)ConversationId);
                Console.WriteLine($"Decrypted message key [1]: {Convert.ToBase64String(messageKey)}");

                for (int i = 2; i <= instructions.MessageKeyRounds; i++)
                {
                    messageKey = _cryptoService.DeriveMessageKey(messageKey, session.RotationIndex, i, (Guid)ConversationId);
                    Console.WriteLine($"Decrypted message key [{i}]: {Convert.ToBase64String(messageKey)}");
                }

                // update session
                session.MessageIndex = instructions.MessageKeyRounds;

                Console.WriteLine($"AAD: {Convert.ToBase64String(AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex))}");
                byte[] decryptedMessage = _cryptoService.DecryptWithKey(encryptedPayload, messageKey, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));

                var requestDto = new MessageReceivedStateUpdateRequest(
                    conversationId: (Guid)ConversationId,
                    sessionIndex: session.RotationIndex,
                    messageIndex: session.MessageIndex,
                    recvCounter: ReceivingCounter + instructions.Jump
                );

                return (requestDto, Encoding.UTF8.GetString(decryptedMessage));
            }
            catch
            {
                throw;
            }
            finally
            {
                if (decryptedSessionKey != null) CryptographicOperations.ZeroMemory(decryptedSessionKey);
                if (messageKey != null) CryptographicOperations.ZeroMemory(messageKey);
            }
        }

        private async Task<(MessageReceivedStateUpdateRequest requestDto, string message)> DecryptMessageWithRotationAsync(SessionState session, KeyDerivationInstructions instructions, byte[] encryptedPayload)
        {
            byte[]? decryptedSessionKey = null;
            byte[]? messageKey = null;
            byte[]? decryptedMessage = null;
            byte[]? decryptedNewSessionKey = null;

            try
            {
                var snapshots = new List<Snapshot>();
                // rotate ratchet
                Console.WriteLine("Decrypting session key");
                Console.WriteLine($"Rotation index: {session.RotationIndex}");
                Console.WriteLine($"AAD: {Convert.ToBase64String(AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex))}");
                decryptedSessionKey = await _userVaultService.DecryptAsync(session.EncryptedCurrentSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));
                Console.WriteLine($"Decrypted session key [{session.RotationIndex}]: {Convert.ToBase64String(decryptedSessionKey)}");

                Console.WriteLine("Starting the rotation loop...");
                Console.WriteLine($"Rotation index: {session.RotationIndex}");
                ++session.RotationIndex;
                decryptedNewSessionKey = _cryptoService.AdvanceRatchet(decryptedSessionKey, session.RotationIndex, (Guid)ConversationId);
                Console.WriteLine($"Decrypted new session key [{session.RotationIndex}]: {Convert.ToBase64String(decryptedNewSessionKey)}");

                // if snapshot for the first rotation
                if (session.RotationIndex % SnapshotFrequency == 0 && session.RotationIndex != 0)
                {
                    Console.WriteLine($"Creating snapshot");
                    var encryptedCurrentSessionKey = await _userVaultService.EncryptAsync(decryptedNewSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));
                    session.EncryptedCurrentSessionKey = encryptedCurrentSessionKey;
                    snapshots.Add(await CreateSnapshotAsync(session));
                }

                while (session.RotationIndex < instructions.RatchetRotations)
                {
                    ++session.RotationIndex;
                    decryptedNewSessionKey = _cryptoService.AdvanceRatchet(decryptedNewSessionKey, session.RotationIndex, (Guid)ConversationId);
                    Console.WriteLine($"Decrypted session key [{session.RotationIndex}]: {Convert.ToBase64String(decryptedSessionKey)}");

                    // snapshots
                    if (session.RotationIndex % SnapshotFrequency == 0 && session.RotationIndex != 0)
                    {
                        Console.WriteLine($"Creating snapshot");
                        var encryptedCurrentSessionKey = await _userVaultService.EncryptAsync(decryptedNewSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));
                        session.EncryptedCurrentSessionKey = encryptedCurrentSessionKey;
                        snapshots.Add(await CreateSnapshotAsync(session));
                    }
                }
                Console.WriteLine("Rotation loop finished");
                Console.WriteLine($"Rotation index: {session.RotationIndex}");

                // derive message key
                session.MessageIndex = 1;
                messageKey = _cryptoService.DeriveMessageKey(decryptedNewSessionKey, session.RotationIndex, session.MessageIndex, (Guid)ConversationId);
                Console.WriteLine($"Decrypted message key [1]: {Convert.ToBase64String(messageKey)}");

                Console.WriteLine("Starting the message loop...");
                Console.WriteLine($"Message index: {session.MessageIndex}");
                while (session.MessageIndex < instructions.MessageKeyRounds)
                {
                    ++session.MessageIndex;
                    messageKey = _cryptoService.DeriveMessageKey(messageKey, session.RotationIndex, session.MessageIndex, (Guid)ConversationId);
                    Console.WriteLine($"Decrypted message key [{session.MessageIndex}]: {Convert.ToBase64String(messageKey)}");
                }
                Console.WriteLine("Message loop finished");
                Console.WriteLine($"Message index: {session.MessageIndex}");

                // decrypt message
                Console.WriteLine($"AAD: {Convert.ToBase64String(AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex))}");
                decryptedMessage = _cryptoService.DecryptWithKey(encryptedPayload, messageKey, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));
                byte[] encryptedNewSessionKey = await _userVaultService.EncryptAsync(decryptedNewSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));

                Console.WriteLine($"Snapshots count: {(snapshots.Any() ? snapshots.Count : "null")}");
                var requestDto = new MessageReceivedStateUpdateRequest(
                    conversationId: (Guid)ConversationId,
                    sessionIndex: session.RotationIndex,
                    messageIndex: session.MessageIndex,
                    recvCounter: ReceivingCounter + instructions.Jump,
                    encryptedCurrentSessionKey: encryptedNewSessionKey,
                    snapshots: snapshots.Any() ? snapshots : null
                );

                return (requestDto, Encoding.UTF8.GetString(decryptedMessage));
            }
            catch
            {
                throw;
            }
            finally
            {
                if (decryptedSessionKey != null) CryptographicOperations.ZeroMemory(decryptedSessionKey);
                if (messageKey != null) CryptographicOperations.ZeroMemory(messageKey);
                if (decryptedMessage != null) CryptographicOperations.ZeroMemory(decryptedMessage);
                if (decryptedNewSessionKey != null) CryptographicOperations.ZeroMemory(decryptedNewSessionKey);
            }
        }

        private KeyDerivationInstructions? CalculateDerivationInstructions(SessionState session, MessageResponse message)
        {
            int rotations = 0;
            int msgKeyRounds = 0;
            int jump = 0;

            if (session.RotationIndex > message.SessionIndex)
                return null;

            if (session.RotationIndex == message.SessionIndex)
            {
                if (session.MessageIndex > message.MessageIndex)
                    return null;

                rotations = 0;
                msgKeyRounds = message.MessageIndex;
                jump = message.MessageIndex - session.MessageIndex;
            }

            else
            {
                rotations = message.SessionIndex - session.RotationIndex;
                msgKeyRounds = message.MessageIndex;

                int frequency = session.RotateAfter;
                int sessionTotal = session.RotationIndex * frequency + session.MessageIndex;
                int msgTotal = message.SessionIndex * frequency + message.MessageIndex;
                jump = msgTotal - sessionTotal;
            }

            return new KeyDerivationInstructions(
                ratchetRotations: rotations,
                messageKeyRounds: msgKeyRounds,
                jump: jump
            );
        }

        /* ----------------------------------------- CLEANUP ----------------------------------------- */

        public void Clear()
        {
            EncryptedVault = null;
            ConversationId = null;
            UpdatedAt = null;
            SendingCounter = 0;
            ReceivingCounter = 0;
        }

        public void Dispose()
        {
            Clear();
        }

        /* ----------------------------------------- HISTORY ----------------------------------------- */


        //  [X]  decrypt range:
        //      [X]  fetch messages
        //          [X]  get message (count, lastsequence)
        //          [X]  server finds messages
        //              [X]  ask valkey
        //              [X]  if valkey doesnt deliver all messages → ask postgres
        //              [X]  find needed snapshots
        //          [X]  return snapshots + messages
        //      [X]  prepare data:
        //          [X]  insert snapshots into exisitng snapshots
        //          [X]  sort messages (desc sequence)
        //          [X]  divide messages by sender
        //          [X]  go through each chain
        //              [X]  foreach message:
        //                  [X]  messages are already sorted desc → starting at newest message
        //                  [X]  find the snaphsot for this message → add message to this snapshot group
        //                  [X]  go to next message
        //                  [X]  keep adding until msg.rotationindex < group.snpashotindex
        //                  [X]  then find the correct snapshot for this message and repeat whole process
        //          [X]  join batches from both chain into one collection
        //          [X]  sort collection by max sequence number
        //          [X]  decrypt individual batches → foreach batch (from latest to oldest - desc sequence):
        //              [X]  reverse list of messages (asc sequence)
        //              [X]  calculate the session key for first message → keep in memory as local var
        //              [X]  calculate the message key for this message, decrypt it → add to list of decrypted messages
        //              [X]  if next message.rotation == previous message.rotation
        //                  [X]  use message key to derive next message key
        //              [X]  else
        //                  [X]  use session key to derive next needed session key
        //                  [X]  repeat the whole thing (keep session key in memory, derive message key, if else…)
        //              [X]  fire event with list<chatmessage> → activeconversation insert, ui updates immediately

        public async Task LoadRecentMessagesAsync()
        {
            if (!_userContext.IsAuthenticated) return;
            if (!_userVaultService.IsUnlocked) return;
            if (EncryptedVault == null) return;
            try
            {
                // fetch messages and snapshots from server
                var response = await _conversationRepository.FetchRecentAsync((Guid)ConversationId);
                if (response == null || !response.Messages.Any()) return;

                await HandleMessageBundleAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loading history failed: {ex.Message}");
            }
        }

        public async Task LoadHistoryAsync(int lastSequenceNumber, int count = 50)
        {
            if (!_userContext.IsAuthenticated) return;
            if (!_userVaultService.IsUnlocked) return;
            if (EncryptedVault == null) return;
            try
            {
                // fetch messages and snapshots from server
                var response = await _conversationRepository.FetchHistoryAsync((Guid)ConversationId, count, lastSequenceNumber);
                if (response == null || !response.Messages.Any()) return;

                await HandleMessageBundleAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loading history failed: {ex.Message}");
            }
        }

        private async Task HandleMessageBundleAsync(MessagesBundleDto messagesBundle)
        {
            try
            {
                // fetch messages and snapshots from server
                if (messagesBundle == null || !messagesBundle.Messages.Any()) return;

                // add snapshots to existing collection
                EncryptedVault.Receiving.MergeHistory(messagesBundle.Snapshots.Receiving);
                EncryptedVault.Sending.MergeHistory(messagesBundle.Snapshots.Sending);

                // sort desc
                var messages = messagesBundle.Messages;
                messages.Sort((a, b) => b.SequenceNumber.CompareTo(a.SequenceNumber));

                // divide by sender
                List<MessageResponse> sendingMessages = new();
                List<MessageResponse> receivingMessages = new();

                foreach (var message in messages)
                {
                    if (message.SenderId == _userContext.UserId) sendingMessages.Add(message);
                    else receivingMessages.Add(message);
                }

                // organize into batches
                List<MessagesBatch> sendingBatches = BatchMessages(EncryptedVault.Sending.SnapshotsHistory, sendingMessages, RatchetType.Sending);
                List<MessagesBatch> receivingBatches = BatchMessages(EncryptedVault.Receiving.SnapshotsHistory, receivingMessages, RatchetType.Receiving);

                List<MessagesBatch> messagesBatches = new(sendingBatches.Count + receivingBatches.Count);
                messagesBatches.AddRange(sendingBatches);
                messagesBatches.AddRange(receivingBatches);

                messagesBatches.Sort((a, b) => b.Messages.Max(m => m.SequenceNumber).CompareTo(a.Messages.Max(m => m.SequenceNumber)));

                // decrypt
                foreach (var batch in messagesBatches)
                {
                    await DecryptBatchAsync(batch);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during decrypting messages bundle: {ex.Message}");
            }
        }

        private async Task DecryptBatchAsync(MessagesBatch messagesBatch)
        {
            byte[]? decryptedSessionKey = null;
            byte[]? decryptedMessageKey = null;
            byte[]? decryptedMessageBytes = null;
            try
            {
                List<ChatMessage> decryptedMessages = new();
                // reverse messages to ascending order
                int rotationIndex = messagesBatch.RotationIndex;
                var messages = messagesBatch.Messages;
                messages.Reverse();

                // get snapshot key
                byte[]? encryptedSnapshotKey;
                if (messagesBatch.RatchetType == RatchetType.Sending)
                    EncryptedVault.Sending.SnapshotsHistory.TryGetValue(rotationIndex, out encryptedSnapshotKey);
                else EncryptedVault.Receiving.SnapshotsHistory.TryGetValue(rotationIndex, out encryptedSnapshotKey);

                if (encryptedSnapshotKey == null)
                    throw new ArgumentNullException($"Snapshot damaged: R:{rotationIndex}, T:{messagesBatch.RatchetType}. encrypted key is missing");

                // first message
                var currentMessage = messages[0];
                int rotations = currentMessage.SessionIndex - rotationIndex;
                int messageRounds = 0;
                int messageIndex = 1;

                decryptedSessionKey = await _userVaultService.DecryptAsync(
                    encryptedSnapshotKey,
                    messagesBatch.RatchetType == RatchetType.Sending ?
                        AadFactory.ForSendingSessionKey((Guid)ConversationId, rotationIndex) :
                        AadFactory.ForReceivingSessionKey((Guid)ConversationId, rotationIndex)
                    );

                rotations = currentMessage.SessionIndex - rotationIndex;

                // rotate
                while (rotationIndex < currentMessage.SessionIndex)
                {
                    ++rotationIndex;
                    decryptedSessionKey = _cryptoService.AdvanceRatchet(decryptedSessionKey, rotationIndex, (Guid)ConversationId);
                }

                decryptedMessageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, currentMessage.SessionIndex, messageIndex, (Guid)ConversationId);

                // derive messagekey
                while (messageIndex < currentMessage.MessageIndex)
                {
                    ++messageIndex;
                    decryptedMessageKey = _cryptoService.DeriveMessageKey(decryptedMessageKey, currentMessage.SessionIndex, messageIndex, (Guid)ConversationId);
                }

                // decrypt message
                decryptedMessageBytes = _cryptoService.DecryptWithKey(currentMessage.EncryptedPayload, decryptedMessageKey, AadFactory.ForMessage((Guid)ConversationId, currentMessage.SessionIndex, currentMessage.MessageIndex));
                decryptedMessages.Add(
                    new ChatMessage(
                        id: currentMessage.Id,
                        conversationId: (Guid)ConversationId,
                        senderUsername: currentMessage.SenderUsername,
                        sequenceNumber: currentMessage.SequenceNumber,
                        content: Encoding.UTF8.GetString(decryptedMessageBytes),
                        sentAt: currentMessage.CreatedAt,
                        messagesBatch.RatchetType == RatchetType.Sending
                    )
                );

                // rest of messages
                for (int i = 1; i < messages.Count; i++)
                {
                    currentMessage = messages[i];
                    var previousMessage = messages[i - 1];

                    rotations = currentMessage.SessionIndex - previousMessage.SessionIndex;

                    if (rotations > 0)
                    {
                        // rotate
                        while (rotationIndex < currentMessage.SessionIndex)
                        {
                            ++rotationIndex;
                            decryptedSessionKey = _cryptoService.AdvanceRatchet(decryptedSessionKey, rotationIndex, (Guid)ConversationId);
                        }

                        messageRounds = currentMessage.MessageIndex;
                        messageIndex = 1;
                    }
                    else
                    {
                        messageRounds = currentMessage.MessageIndex - previousMessage.MessageIndex;
                    }

                    // derive messagekey
                    if (rotations > 0) decryptedMessageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, currentMessage.SessionIndex, messageIndex, (Guid)ConversationId);
                    else _cryptoService.DeriveMessageKey(decryptedMessageKey, currentMessage.SessionIndex, messageIndex, (Guid)ConversationId);
                    while (messageIndex < currentMessage.MessageIndex)
                    {
                        ++messageIndex;
                        decryptedMessageKey = _cryptoService.DeriveMessageKey(decryptedMessageKey, currentMessage.SessionIndex, messageIndex, (Guid)ConversationId);
                    }

                    // decrypt message
                    decryptedMessageBytes = _cryptoService.DecryptWithKey(currentMessage.EncryptedPayload, decryptedMessageKey, AadFactory.ForMessage((Guid)ConversationId, currentMessage.SessionIndex, currentMessage.MessageIndex));
                    decryptedMessages.Add(
                        new ChatMessage(
                            id: currentMessage.Id,
                            conversationId: (Guid)ConversationId,
                            senderUsername: currentMessage.SenderUsername,
                            sequenceNumber: currentMessage.SequenceNumber,
                            content: Encoding.UTF8.GetString(decryptedMessageBytes),
                            sentAt: currentMessage.CreatedAt,
                            messagesBatch.RatchetType == RatchetType.Sending
                        )
                    );

                    if (decryptedMessageBytes != null) CryptographicOperations.ZeroMemory(decryptedMessageBytes);
                }

                MessagesDecrypted?.Invoke(decryptedMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decrypt messages batch: R:{messagesBatch.RotationIndex}, T:{messagesBatch.RatchetType}. Error: {ex.Message}");
            }
            finally
            {
                if (decryptedSessionKey != null) CryptographicOperations.ZeroMemory(decryptedSessionKey);
                if (decryptedMessageKey != null) CryptographicOperations.ZeroMemory(decryptedMessageKey);
                if (decryptedMessageBytes != null) CryptographicOperations.ZeroMemory(decryptedMessageBytes);
            }
        }

        private List<MessagesBatch> BatchMessages(SortedList<int, byte[]> snapshots, List<MessageResponse> messages, RatchetType ratchetType)
        {
            if (!messages.Any()) return new List<MessagesBatch>();
            try
            {
                List<MessagesBatch> messageGroups = new();

                int? snapshotIndex = FindSnapshot(snapshots, messages[0].SessionIndex);
                if (snapshotIndex == null)
                    throw new InvalidOperationException($"Couldn't find snapshot able to decrypt this message. Messege: {messages[0].Id}");

                MessagesBatch currentGroup = new((int)snapshotIndex, ratchetType);
                currentGroup.AddMessage(messages[0]);

                for (int i = 1; i < messages.Count; i++)
                {
                    var message = messages[i];
                    if (message.SessionIndex >= currentGroup.RotationIndex)
                    {
                        currentGroup.AddMessage(message);
                    }
                    else
                    {
                        messageGroups.Add(currentGroup);

                        snapshotIndex = FindSnapshot(snapshots, message.SessionIndex);
                        if (snapshotIndex == null)
                            throw new InvalidOperationException($"Couldn't find snapshot able to decrypt this message. Messege: {messages[0].Id}");

                        currentGroup = new((int)snapshotIndex, ratchetType);
                        currentGroup.AddMessage(message);
                    }
                }
                messageGroups.Add(currentGroup);

                return messageGroups;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to batch messages: {ex.Message}");
                throw;
            }
        }

        private int? FindSnapshot(SortedList<int, byte[]> snapshots, int rotationIndex)
        {
            if (!snapshots.Any()) return null;

            int low = 0;
            int high = snapshots.Count - 1;
            int mid;
            int key;

            var keys = snapshots.Keys;

            while (low <= high)
            {
                mid = low + ((high - low) / 2);
                key = keys[mid];

                if (key == rotationIndex)
                    return keys[mid];
                if (key < rotationIndex)
                    low = mid + 1;
                else high = mid - 1;
            }

            if (high >= 0) return keys[high];

            return null;
        }
    }
}