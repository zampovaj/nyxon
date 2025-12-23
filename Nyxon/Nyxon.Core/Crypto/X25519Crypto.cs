using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Nyxon.Core.Crypto
{
    public class X25519Crypto : IX25519Crypto
    {
        private readonly SecureRandom _secureRandom = new SecureRandom();
        public (byte[] PublicKey, byte[] PrivateKey) GenerateKeyPair()
        {
            var generator = new X25519KeyPairGenerator();
            generator.Init(new X25519KeyGenerationParameters(_secureRandom));

            var pair = generator.GenerateKeyPair();
            var privateKeyParams = (X25519PrivateKeyParameters)pair.Private;
            var publicKeyParams = (X25519PublicKeyParameters)pair.Public;

            return (publicKeyParams.GetEncoded(), privateKeyParams.GetEncoded());
        }

        public byte[] DeriveSharedSecret(byte[] localPrivateKey, byte[] remotePublicKey)
        {
            var privParam = new X25519PrivateKeyParameters(localPrivateKey, 0);
            var pubParam = new X25519PublicKeyParameters(remotePublicKey, 0);

            var agreement = new Org.BouncyCastle.Crypto.Agreement.X25519Agreement();
            agreement.Init(privParam);

            var secret = new byte[agreement.AgreementSize];
            agreement.CalculateAgreement(pubParam, secret, 0);

            return secret;
        }

    }
}