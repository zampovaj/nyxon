using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces.Crypto
{
    public interface IX3DHCrypto
    {
        Task<X3DHResult> CalculateInitiatorSecretAsync(
            byte[] IK_B_pub,
            byte[] SPK_B_pub,
            byte[] OPK_B_pub
        );
        Task<X3DHResult> CalculateInitiatorSecretAsync(
            byte[] IK_B_pub,
            byte[] SPK_B_pub
        );

        Task<byte[]> CalculateReceiverSecretAsync(
            byte[] SPK_B_priv,
            byte[] OPK_B_priv,
            byte[] IK_A_pub,
            byte[] EK_A_pub
        );
        Task<byte[]> CalculateReceiverSecretAsync(
            byte[] SPK_B_priv,
            byte[] IK_A_pub,
            byte[] EK_A_pub
        );
    }
}