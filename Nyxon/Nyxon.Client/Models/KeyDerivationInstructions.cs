using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSec.Cryptography;

namespace Nyxon.Client.Models
{
    public class KeyDerivationInstructions
    {
        public int RatchetRotations { get; set; } = 0;
        public int MessageKeyRounds { get; set; } = 0;
        // exxpresses the difference between current recvCounter state and recvCounter state after decryption operation
        public int Jump { get; set; } = 0;

        public KeyDerivationInstructions(int ratchetRotations, int messageKeyRounds, int jump)
        {
            RatchetRotations = ratchetRotations;
            MessageKeyRounds = messageKeyRounds;
            Jump = jump;
        }
    }
}