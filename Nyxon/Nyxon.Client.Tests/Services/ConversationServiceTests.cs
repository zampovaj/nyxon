using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using Xunit;
using Nyxon.Client.Services.Messaging;
using Nyxon.Core.Services;
using Nyxon.Shared.Dtos; // Assuming DTO namespace
using Nyxon.Core.Models.Vaults;

namespace Nyxon.Client.Tests.Services
{

    public class ConversationServiceTests
    {
        // 1. Define Mocks for all dependencies
        private readonly Mock<IConversationRepository> _mockRepo = new();
        private readonly Mock<IX3DHCrypto> _mockX3dh = new();
        private readonly Mock<ICryptoService> _mockCrypto = new();
        private readonly Mock<IUserVaultService> _mockUserVault = new();
        private readonly Mock<IApiService> _mockApi = new();
        private readonly Mock<AuthenticationStateProvider> _mockAuth = new();
        private readonly Mock<IActiveConversationService> _mockActiveConv = new();
        private readonly Mock<IInboxService> _mockInbox = new();
        private readonly Mock<IHandshakeService> _mockHandshake = new();

        private readonly ConversationService _service;

        public ConversationServiceTests()
        {
            // 2. Initialize the service with the mocked dependencies
            _service = new ConversationService(
                _mockRepo.Object,
                _mockX3dh.Object,
                _mockCrypto.Object,
                _mockUserVault.Object,
                _mockApi.Object,
                _mockAuth.Object,
                _mockActiveConv.Object,
                _mockInbox.Object,
                _mockHandshake.Object
            );
        }

        [Fact]
        public async Task CreateConversationAsync_WithOpk_PerformsX3dhAndSendsRequest()
        {
            // --- ARRANGE (Setup the "Happy Path") ---

            // 1. Bypass Auth Check
            var userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "mock"));

            _mockAuth.Setup(x => x.GetAuthenticationStateAsync())
                .ReturnsAsync(new AuthenticationState(user));

            // 2. Bypass Vault Check
            _mockUserVault.Setup(x => x.IsUnlocked).Returns(true);

            // 3. Setup Prekey Data (The other user)
            var prekeyBundle = new PrekeyBundleDto
            {
                UserId = Guid.NewGuid(),
                PublicIdentityKey = new byte[] { 0x01 },
                SpkPublic = new byte[] { 0x02 },
                SpkSignature = new byte[] { 0x03 },
                OpkPublic = new byte[] { 0x04 }, // OPK Exists
                SpkId = 1,
                OpkId = 10
            };
            _mockRepo.Setup(x => x.GetPrekeyBundle()).ReturnsAsync(prekeyBundle);

            // 4. Setup Crypto Verification (Signature)
            _mockCrypto.Setup(x => x.VerifySignatureAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .ReturnsAsync(true);

            // 5. Setup X3DH Calculation (The Core Logic)
            var sharedSecret = new byte[] { 0xAA, 0xBB }; // Fake secret
            var ephemeralKey = new byte[] { 0xCC };
            var x3dhResult = new X3DHResult(sharedSecret, ephemeralKey);

            _mockX3dh.Setup(x => x.CalculateInitiatorSecretAsync(
                prekeyBundle.PublicIdentityKey,
                prekeyBundle.SpkPublic,
                prekeyBundle.OpkPublic)) // Ensure OPK is passed!
                .ReturnsAsync(x3dhResult);

            // 6. Setup Key Splitting
            var chainKey1 = new byte[] { 0x11 };
            var chainKey2 = new byte[] { 0x22 };
            _mockCrypto.Setup(x => x.SplitRootKey(sharedSecret, "nyxon_initiator_v1", "nyxon_responder_v1"))
                .Returns((chainKey1, chainKey2));

            // 7. Setup Encryption (Mock it returning the input as "encrypted" for simplicity)
            _mockUserVault.Setup(x => x.EncryptAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .ReturnsAsync((byte[] data, byte[] aad) => data); // Return input bytes

            // 8. Setup API Response
            var newConvId = Guid.NewGuid();
            _mockApi.Setup(x => x.PostAsync<CreateConversationResponse, CreateConversationRequest>(
                "api/conversation", It.IsAny<CreateConversationRequest>()))
                .ReturnsAsync(new CreateConversationResponse { ConversationId = newConvId, AlreadyExisted = false });

            // --- ACT ---
            var result = await _service.CreateConversationAsync("some_username");

            // --- ASSERT ---

            // 1. Verify X3DH was called with the OPK (since we provided one)
            _mockX3dh.Verify(x => x.CalculateInitiatorSecretAsync(
                prekeyBundle.PublicIdentityKey,
                prekeyBundle.SpkPublic,
                prekeyBundle.OpkPublic), Times.Once, "X3DH should use OPK when available");

            // 2. Verify Root Key Split happened
            _mockCrypto.Verify(x => x.SplitRootKey(sharedSecret, It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            // 3. Verify API called with correct Vault Data structure
            _mockApi.Verify(x => x.PostAsync<CreateConversationResponse, CreateConversationRequest>(
                "api/conversation",
                It.Is<CreateConversationRequest>(req =>
                    req.OpkPublicId == 10 && // Check OPK ID was sent
                    req.VaultData.EncryptedRootKey == sharedSecret // In our mock, encrypted == plaintext
                )), Times.Once);

            // 4. Verify Active Conversation was initialized
            _mockActiveConv.Verify(x => x.InitializeNewAsync(newConvId, It.IsAny<ConversationVaultData>()), Times.Once);

            Assert.Equal(newConvId, result);
        }
    }
}