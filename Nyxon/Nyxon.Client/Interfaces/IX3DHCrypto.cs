using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces.Crypto
{
    public interface IX3DHCrypto
    {
        X3DHResult CalculateInitiatorSecret(
            byte[] IK_A_priv,
            byte[] IK_B_pub,
            byte[] SPK_B_pub,
            byte[] OPK_B_pub
        );
        X3DHResult CalculateInitiatorSecret(
            byte[] IK_A_priv,
            byte[] IK_B_pub,
            byte[] SPK_B_pub
        );

        byte[] CalculateReceiverSecret(
            byte[] IK_B_priv,
            byte[] SPK_B_priv,
            byte[] OPK_B_priv,
            byte[] IK_A_pub,
            byte[] EK_A_pub
        );
        byte[] CalculateReceiverSecret(
            byte[] IK_B_priv,
            byte[] SPK_B_priv,
            byte[] IK_A_pub,
            byte[] EK_A_pub
        );
    }
}