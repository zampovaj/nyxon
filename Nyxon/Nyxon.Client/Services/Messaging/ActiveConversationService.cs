using System.Linq.Expressions;
using System.Security.Cryptography;

//  [ ]  send message
//      [X]  encrypt message
//          [X]  load current state from state object into method
//              [X]  do kdf messageindex times to get current key
//          [X]  do the rotations if needed and do kdf to derive next key → keep only as local var for now
//              [X]  if rotation happenned →
//                  [X]  encrpyt the new sesion key and keep in memory as local var
//          [X]  decide if snapshot is needed
//              [X]  if yes → create snapshot
//              [X]  else → continue
//          [X]  encrypt message
//          [X]  create message dto
//          [X]  send current state update and new message to server
//              [X]  state:
//                  [X]  inlcudes current rotationindex if changed and and messageindex (always increments by one so probably dont even need to send it i guess)
//                  [X]  if rotaiton happenned → include new sessionkey as well
//                  [X]  if fail → abort whole thing
//                  [X]  else → continue
//              [X]  message:
//                  [X]  save to postgres
//                  [X]  save to valkey
//                      [X]  if valkey fails → delete postgres row
//                  [X]  if rotation happened
//                      [X]  save new key
//                      [X]  rotationindex++
//                      [X]  messageindex = 0
//                      [X]  if any snapshot got created →
//                          [X]  send post to server
//                  [X]  else
//                      [X]  messageindex++
//                  [ ]  server sends signalr notification - exclude sender
//          [X]  if client receives failed http status response
//              [X]  abort the whole thing
//          [X]  else
//              [X]  save updated ratchet state in memory
//              [X]  save new snapshot in memory
//              [X]  display message in ui
//                  [X]  load into activeconversatoin.messages

//  [ ]  receive message
//      [ ]  receive signalr notification with new message kvkey
//          [X]  get message from server
//              [X]  decrypt message
//                  [X]  clone session state to method
//                  [X]  calculate jump size →
//                      [ ]  if (jump == null) → decrpyt message with history logic - not impelmented yet
//                  [X]  decide if rotation will happen
//                      [X]  if yes → derive the new sesion key and keep in memory as local var
//                          [X]  decrypt session key
//                          [X]  advanceratchet(key) as many times as needed (need to accoutnf or big jumps)
//                              [X]  for each advancement:
//                                  [X]  decide if snapshot is needed
//                                      [X]  if yes → create and add to collection
//                          [X]  derive message key from new session key
//                              [X]  hkdf(key) as many times as needed
//                          [X]  encrypt new session key
//                              [X]  uservaultservice.encrypt(key, aad)
//                          [X]  decryptmessage with new key
//                              [X]  decrypt(msg, key, aad)
//                      [X]  else →
//                          [X]  derive message key from session key
//                              [X]  hkdf(key) as many times as needed
//                          [X]  decryptmessage with new key
//                              [X]  decrypt(msg, key, aad)
//                  [X]  update temporal state:
//                      [X]  recvcounter += jump
//                      [X]  rotationindex = msg.rotation
//                      [X]  msgindex = msg.msgindex
//                      [X]  if snapshots got created → save snasphots
//                  [X]  send to server → if fail, abort all
//                      [X]  if (encryptednewsessionkey ! = null) →
//                          [X]  receiving.session.encrpytedkey = request.newkey
//                      [X]  rotationindex = request.rotation (if rotationindex < request.rotation)
//                      [X]  recvcounter = request.recvcounter (if recvcounter < request.recvcounter
//                      [X]  msgindex = request.msgindex
//                      [X]  conversationuser.lastreadat = now
//                      [X]  if (snapshots ! = null) →
//                          [X]  save new snapshots
//                  [X]  update real state
//              [ ]  display message in ui
//                  [ ]  load into activeconversatoin.messages

using System.Text;
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

        public event Action<ChatMessage>? MessageDecrypted;

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

        public async Task<List<ChatMessage>> LoadHistoryAsync(int count = 50, int skip = 0)
        {
            throw new NotImplementedException();
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
                    session.MessageIndex = 0;

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
                decryptedNewSessionKey = _cryptoService.AdvanceRatchet(decryptedSessionKey, session.RotationIndex, (Guid)ConversationId);
                messageKey = _cryptoService.DeriveMessageKey(decryptedNewSessionKey, session.RotationIndex, session.MessageIndex, (Guid)ConversationId);

                byte[] encryptedMessage = _cryptoService.EncryptWithKey(message, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));
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
                messageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, session.RotationIndex, session.MessageIndex, (Guid)ConversationId);

                for (int i = 1; i <= session.MessageIndex; i++)
                {
                    messageKey = _cryptoService.DeriveMessageKey(messageKey, session.RotationIndex, i, (Guid)ConversationId);
                }
                Console.WriteLine($"Length: {messageKey.Length}");

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
                    //  TODO: use history decryption logic to decrypt this message
                }

                MessageReceivedStateUpdateRequest requestDto;
                string content;

                // rotate ratchet
                if (keyDerivationInstructions.RatchetRotations > 0)
                {
                    (requestDto, content) = await DecryptMessageWithRotationAsync(
                        session: session,
                        instructions: keyDerivationInstructions,
                        encryptedPayload: message.EncryptedPayload
                    );
                    // update temporal state
                    session.EncryptedCurrentSessionKey = requestDto.EncryptedNewSessionKey;
                    session.RotationIndex = requestDto.SessionIndex;
                    session.MessageIndex = requestDto.MessageIndex;
                }

                // derive message key
                else
                {
                    (requestDto, content) = await DecryptMessageOnlyAsync(
                        session: session,
                        instructions: keyDerivationInstructions,
                        encryptedPayload: message.EncryptedPayload
                    );
                    // update temporal state
                    session.MessageIndex = requestDto.MessageIndex;
                }

                // send request to server
                var responseDto = await _conversationRepository.ReceiveMessageServerUpdate(requestDto);
                if (responseDto == null)
                    throw new Exception("Failed to update server about receiving message. Message read aborted.");

                // save session state
                EncryptedVault.Sending.Session = session;
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
                messageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, session.RotationIndex, session.MessageIndex, (Guid)ConversationId);

                for (int i = 1; i <= instructions.MessageKeyRounds; i++)
                {
                    messageKey = _cryptoService.DeriveMessageKey(messageKey, session.RotationIndex, i, (Guid)ConversationId);
                }

                // update session
                session.MessageIndex = instructions.MessageKeyRounds;

                byte[] decryptedMessage = _cryptoService.DecryptWithKey(encryptedPayload, messageKey, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));

                var requestDto = new MessageReceivedStateUpdateRequest(
                    conversationId: (Guid)ConversationId,
                    sessionIndex: session.RotationIndex,
                    messageIndex: session.MessageIndex,
                    recvCounter: ReceivingCounter + instructions.Jump
                );

                return (requestDto, Convert.ToBase64String(decryptedMessage));
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

            try
            {
                var snapshots = new List<Snapshot>();
                // rotate ratchet
                decryptedSessionKey = await _userVaultService.DecryptAsync(session.EncryptedCurrentSessionKey, AadFactory.ForSendingSessionKey((Guid)ConversationId, session.RotationIndex - 1));

                for (int i = 1; i <= instructions.RatchetRotations; i++)
                {
                    decryptedSessionKey = _cryptoService.AdvanceRatchet(decryptedSessionKey, session.RotationIndex, (Guid)ConversationId);
                    ++session.RotationIndex;

                    // snapshots
                    if (session.RotationIndex % SnapshotFrequency == 0 && session.RotationIndex != 0)
                    {
                        var encryptedCurrentSessionKey = await _userVaultService.EncryptAsync(decryptedSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));
                        session.EncryptedCurrentSessionKey = encryptedCurrentSessionKey;
                        snapshots.Add(await CreateSnapshotAsync(session));
                    }
                }

                // derive message key

                messageKey = _cryptoService.DeriveMessageKey(decryptedSessionKey, session.RotationIndex, session.MessageIndex, (Guid)ConversationId);

                for (int i = 1; i <= instructions.MessageKeyRounds; i++)
                {
                    messageKey = _cryptoService.DeriveMessageKey(messageKey, session.RotationIndex, i, (Guid)ConversationId);
                }

                // update session msgindex
                session.MessageIndex = instructions.MessageKeyRounds;

                // decrypt message
                decryptedMessage = _cryptoService.DecryptWithKey(encryptedPayload, AadFactory.ForMessage((Guid)ConversationId, session.RotationIndex, session.MessageIndex));
                byte[] encryptedNewSessionKey = await _userVaultService.EncryptAsync(decryptedSessionKey, AadFactory.ForReceivingSessionKey((Guid)ConversationId, session.RotationIndex));

                var requestDto = new MessageReceivedStateUpdateRequest(
                    conversationId: (Guid)ConversationId,
                    sessionIndex: session.RotationIndex,
                    messageIndex: session.MessageIndex,
                    recvCounter: ReceivingCounter + instructions.Jump,
                    encryptedCurrentSessionKey: encryptedNewSessionKey,
                    snapshots: snapshots.Any() ? snapshots : null
                );

                return (requestDto, Convert.ToBase64String(decryptedMessage));
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

        public void Clear()
        {
            EncryptedVault = null;
            ConversationId = null;
            UpdatedAt = null;
            SendingCounter = 0;
            ReceivingCounter = 0;
        }
    }
}