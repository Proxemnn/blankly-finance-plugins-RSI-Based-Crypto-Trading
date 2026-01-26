using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substitution
{
    internal class ShiftByNCipher : SubstitutionCipher, ICrypt
    {
        int shift;
        public void Decrypt()
        {
            plainText = Substitution(decryptionKey, CryptoOperation.Decrypt);
        }

        public void Encrypt()
        {
            cipherText = Substitution(encryptionKey, CryptoOperation.Encrypt);
        }

        public void GenerateEncryptionKey()
        {
            for (int i = 0; i < 26; i++)
            {
                encryptionKey[(char)('A' + i)] = (char)('A' + (i + shift)%26);
            }
        }

        public void GenerateDecryptionKey()
        {
            for (int i = 0; i < 26; i++)
            {
                decryptionKey[(char)('A' + i)] = (char)('A' + (i - (shift % 26) + 26) % 26);
            }
        }

        public ShiftByNCipher(int n)
        {
            shift = n;
            GenerateEncryptionKey();
            GenerateDecryptionKey();
        }
    }
}
