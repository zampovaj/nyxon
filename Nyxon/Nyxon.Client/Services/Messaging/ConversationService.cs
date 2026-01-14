using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using NSec.Cryptography;
using Org.BouncyCastle.Security;
using System.Text.Json;
using System.Text.Encodings;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.Ocsp;
using Nyxon.Core.Services;

namespace Nyxon.Client.Services.Messaging
{
    // [X] fetch prekeys
    // [X] calculate x3dh
    // [X] create vault
    // [X] encrypt vault
    // [X] create requestdto
    // [X] send request to server
    // [X] if response alreadyexisted == true
    // [X]   discard everything
    // [X]   resync
    // [X] else
    // [X]   load the created vault and conversation to active conversation state

    public class ConversationService : IConversationService
    {
        private const string Label1 = "nyxon_initiator_v1";
        private const string Label2 = "nyxon_responder_v1";

        private bool _opkExists = false;

        private readonly IConversationRepository _conversationRepository;
        private readonly IX3DHCrypto _x3dh;
        private readonly ICryptoService _cryptoService;
        private readonly IUserVaultService _userVaultService;
        private readonly UserContext _userContext;
        private readonly IActiveConversationService _activeConversation;
        private readonly IInboxService _inboxService;
        private readonly IHandshakeService _handshakeService;

        public ConversationService(IConversationRepository conversationRepository,
            IX3DHCrypto x3dh,
            ICryptoService cryptoService,
            IUserVaultService userVaultService,
            UserContext userContext,
            IActiveConversationService activeConversation,
            IInboxService inboxService,
            IHandshakeService handshakeService)
        {
            _conversationRepository = conversationRepository;
            _x3dh = x3dh;
            _cryptoService = cryptoService;
            _userVaultService = userVaultService;
            _userContext = userContext;
            _activeConversation = activeConversation;
            _inboxService = inboxService;
            _handshakeService = handshakeService;
        }

        public async Task<Guid?> CreateConversationAsync(string username)
        {
            Console.WriteLine("Conversation service reached");
            if (!_userContext.IsAuthenticated)
                throw new UnauthorizedAccessException("Can't create conversation unless unauthenticated");

            var userId = (Guid)_userContext.UserId;

            X3DHResult? x3dhResult = null;
            byte[]? chainKey1 = null;
            byte[]? chainKey2 = null;

            if (!_userVaultService.IsUnlocked)
                throw new UnauthorizedAccessException("Vault must be unlocked to initiate a conversation");

            try
            {
                var prekeyBundle = await _conversationRepository.GetPrekeyBundleAsync(username);

                if (prekeyBundle == null)
                    throw new InvalidOperationException("Prekey bundle fetch failed");
                _opkExists = prekeyBundle.OpkPublic != null;

                // verify
                if (!await _cryptoService.VerifySignatureAsync(prekeyBundle.SpkPublic, prekeyBundle.SpkSignature, prekeyBundle.PublicIdentityKey))
                    throw new InvalidOperationException("Signature verification of spk failed.");

                // get x3dh result
                if (_opkExists)
                {
                    x3dhResult = await _x3dh.CalculateInitiatorSecretAsync(
                        IK_B_pub: prekeyBundle.PublicAgreementKey,
                        SPK_B_pub: prekeyBundle.SpkPublic,
                        OPK_B_pub: prekeyBundle.OpkPublic
                    );
                }
                else
                {
                    x3dhResult = await _x3dh.CalculateInitiatorSecretAsync(
                        IK_B_pub: prekeyBundle.PublicAgreementKey,
                        SPK_B_pub: prekeyBundle.SpkPublic
                    );
                }

                // vault
                var conversationId = Guid.NewGuid();

                (chainKey1, chainKey2) = _cryptoService.SplitRootKey(x3dhResult.SharedSecret, Label1, Label2);

                var encryptedRootKey = await _userVaultService.EncryptAsync(x3dhResult.SharedSecret, AadFactory.ForRootKey(conversationId, userId));
                var encryptedSendingKey = await _userVaultService.EncryptAsync(chainKey1, AadFactory.ForSendingSessionKey(conversationId, 0));
                var encryptedReceivingKey = await _userVaultService.EncryptAsync(chainKey2, AadFactory.ForReceivingSessionKey(conversationId, 0));

                // constructor handles the initialization on its own
                var encryptedVaultData = new ConversationVaultData(encryptedRootKey, encryptedSendingKey, encryptedReceivingKey);

                // request
                CreateConversationRequest request;
                if (_opkExists)
                {
                    request = new CreateConversationRequest()
                    {
                        ConversationId = conversationId,
                        TargetUserId = prekeyBundle.UserId,
                        VaultData = encryptedVaultData,
                        PublicEphemeralKey = x3dhResult.PublicEphemeralKey,
                        SpkId = prekeyBundle.SpkId,
                        OpkId = prekeyBundle.OpkId
                    };
                }
                else
                {
                    request = new CreateConversationRequest()
                    {
                        ConversationId = conversationId,
                        TargetUserId = prekeyBundle.UserId,
                        VaultData = encryptedVaultData,
                        PublicEphemeralKey = x3dhResult.PublicEphemeralKey,
                        SpkId = prekeyBundle.SpkId
                    };
                }

                // contact server
                var response = await _conversationRepository.CreateConversationAsync(request);

                if (response.AlreadyExisted)
                {
                    await _activeConversation.InitializeAsync(response.ConversationId);
                }
                else
                {
                    await _activeConversation.InitializeNewAsync(response.ConversationId, encryptedVaultData);
                }

                await _inboxService.SyncInboxAsync();

                return conversationId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                Console.WriteLine($"SharedSecret: {Convert.ToBase64String(x3dhResult.SharedSecret)}");
                Console.WriteLine($"Chainkey1: {Convert.ToBase64String(chainKey1)}");
                Console.WriteLine($"Chainkey2: {Convert.ToBase64String(chainKey2)}");
                if (x3dhResult?.SharedSecret != null) CryptographicOperations.ZeroMemory(x3dhResult.SharedSecret);
                if (chainKey1 != null) CryptographicOperations.ZeroMemory(chainKey1);
                if (chainKey2 != null) CryptographicOperations.ZeroMemory(chainKey2);
            }
        }

        public async Task<string> OpenConversationAsync(Guid conversationId)
        {
            try
            {
                if (!_userContext.IsAuthenticated)
                    throw new UnauthorizedAccessException("Anuthenticated access prohibited");

                var userId = (Guid)_userContext.UserId;

                if (!_userVaultService.IsUnlocked)
                    throw new UnauthorizedAccessException("Vault must be unlocked to initiate a conversation");

                var conversation = _inboxService.Conversations
                    .Where(c => c.ConversationId == conversationId)
                    .FirstOrDefault();

                if (conversation == null) throw new InvalidOperationException("Conversation doesn't exist");
                if (conversation.IsProcessing) return null;

                if (conversation.HasHandshake)
                {
                    var handshake = _handshakeService.Handshakes
                        .Where(h => h.ConversationId == conversation.ConversationId)
                        .FirstOrDefault();

                    if (handshake == null) throw new InvalidOperationException("Handshake can't be null");

                    handshake.IsProcessing = true;

                    var vaultData = await CalculateX3dhAsync(conversation.ConversationId, userId, handshake);

                    var success = await _conversationRepository.CreateConversationVaultAsync(conversationId, vaultData);
                    if (!success)
                        throw new Exception("Failed to create conversation vault");

                    await _handshakeService.UseAsync(handshake.Id);

                    await _activeConversation.InitializeNewAsync(conversation.ConversationId, vaultData);
                }
                else
                {
                    await _activeConversation.InitializeAsync(conversation.ConversationId);
                }

                return conversation.TargetUsername;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task<ConversationVaultData> CalculateX3dhAsync(Guid conversationId, Guid userId, Handshake handshake)
        {
            byte[]? sharedSecret = null;
            byte[]? chainKey1 = null;
            byte[]? chainKey2 = null;
            byte[]? decryptedPrivateSpk = null;
            byte[]? decryptedPrivateOpk = null;

            _opkExists = handshake.PrivateOpk != null;

            try
            {
                decryptedPrivateSpk = await _userVaultService.DecryptAsync(handshake.PrivateSpk, AadFactory.ForSpk(userId));

                if (_opkExists)
                {
                    decryptedPrivateOpk = await _userVaultService.DecryptAsync(handshake.PrivateOpk, AadFactory.ForOpk(userId));

                    sharedSecret = await _x3dh.CalculateReceiverSecretAsync(
                        SPK_B_priv: decryptedPrivateSpk,
                        OPK_B_priv: decryptedPrivateOpk,
                        IK_A_pub: handshake.PublicAgreementKey,
                        EK_A_pub: handshake.PublicEphemeralKey
                    );
                }
                else
                {
                    sharedSecret = await _x3dh.CalculateReceiverSecretAsync(
                        SPK_B_priv: decryptedPrivateSpk,
                        IK_A_pub: handshake.PublicAgreementKey,
                        EK_A_pub: handshake.PublicEphemeralKey
                    );
                }
                (chainKey1, chainKey2) = _cryptoService.SplitRootKey(sharedSecret, Label1, Label2);

                var encryptedRootKey = await _userVaultService.EncryptAsync(sharedSecret, AadFactory.ForRootKey(conversationId, userId));
                var encryptedSendingKey = await _userVaultService.EncryptAsync(chainKey2, AadFactory.ForSendingSessionKey(conversationId, 0));
                var encryptedReceivingKey = await _userVaultService.EncryptAsync(chainKey1, AadFactory.ForReceivingSessionKey(conversationId, 0));

                return new ConversationVaultData(encryptedRootKey, encryptedSendingKey, encryptedReceivingKey);
            }
            finally
            {
                Console.WriteLine($"SharedSecret: {Convert.ToBase64String(sharedSecret)}");
                Console.WriteLine($"Chainkey1: {Convert.ToBase64String(chainKey1)}");
                Console.WriteLine($"Chainkey2: {Convert.ToBase64String(chainKey2)}");
                if (sharedSecret != null) CryptographicOperations.ZeroMemory(sharedSecret);
                if (chainKey1 != null) CryptographicOperations.ZeroMemory(chainKey1);
                if (chainKey2 != null) CryptographicOperations.ZeroMemory(chainKey2);
                if (decryptedPrivateOpk != null) CryptographicOperations.ZeroMemory(decryptedPrivateOpk);
                if (decryptedPrivateSpk != null) CryptographicOperations.ZeroMemory(decryptedPrivateSpk);
            }
        }
    }
}