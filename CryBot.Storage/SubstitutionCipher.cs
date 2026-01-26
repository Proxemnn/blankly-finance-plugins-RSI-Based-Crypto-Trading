using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substitution
{
    internal abstract class SubstitutionCipher
    {
        protected string plainText;
        public string PlainText 
        { 
            get
            {
                return plainText;
            }
            set
            {
                plainText = value.ToUpper();
            }
        }
        protected string cipherText;
        public string CipherText
        {
            get
            {
                return cipherText;
            }
            set
            {
                cipherText = value.ToUpper();
            }
        }
        protected Dictionary<char, char> encryptionKey = new Dictionary<char, char>();
        public string Key 
        { 
            get
            {
                return string.Join(" ", encryptionKey.Values);
            }
        }
        protected Dictionary<char, char> decryptionKey = new Dictionary<char, char>();
        protected string Substitution(Dictionary<char, char> key, CryptoOperation operation)
        {
            StringBuilder sb = new StringBuilder();
            string message;
            if (operation == CryptoOperation.Encrypt)
                message = plainText;
            else
                message = cipherText; 
            
            foreach (char c in message) 
            {
                if(char.IsLetter(c))
                    sb.Append(key[c]);
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
