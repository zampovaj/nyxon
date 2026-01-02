using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Models
{
    public class RuntimeConvVaultData
    {
        public byte[] EncryptedRootKey { get; set; }
        public RuntimeRatchet Sending { get; set; }
        public RuntimeRatchet Receiving { get; set; }

        public RuntimeConvVaultData() { }

        public RuntimeConvVaultData(byte[] encryptedRootKey, RatchetState sending, RatchetState receiving)
        {
            EncryptedRootKey = encryptedRootKey;
            Sending = new RuntimeRatchet(sending);
            Receiving = new RuntimeRatchet(receiving);
        }

        public RuntimeConvVaultData(ConversationVaultData vaultData)
        {
            EncryptedRootKey = vaultData.EncryptedRootKey;
            Sending = new RuntimeRatchet(vaultData.Sending);
            Receiving = new RuntimeRatchet (vaultData.Receiving);
        }
    }
}