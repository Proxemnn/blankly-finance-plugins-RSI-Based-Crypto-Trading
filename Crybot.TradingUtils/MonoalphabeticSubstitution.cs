using System.Text;

namespace Substitution
{
    internal class MonoalphabeticSubstitution : SubstitutionCipher, ICrypt
    {
        public MonoalphabeticSubstitution()
        {
            GenerateEncryptionKey();
        }

        public void Decrypt()
        {
            plainText = Substitution(decryptionKey, CryptoOperation.Decrypt);
        }

        public void Encrypt()
        {
            cipherText = Substitution(encryptionKey, CryptoOperation.Encrypt);
        }


        public void GenerateDecryptionKey()
        {
            throw new System.NotImplementedException();
        }

        public void GenerateEncryptionKey()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 26; i++)
            {
                sb.Append((char)('A' + i));
            }
            Util.Shuffle(sb);

            for (int i = 0; i < 26; i++)
            {
                encryptionKey[(char)('A' + i)] = sb[i];
                decryptionKey[sb[i]] = (char)('A' + i);
            }

        }
    }
}