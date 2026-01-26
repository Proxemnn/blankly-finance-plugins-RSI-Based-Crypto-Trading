namespace Substitution
{
    internal interface ICrypt
    {
        void Decrypt();
        void Encrypt();
        void GenerateEncryptionKey();
        void GenerateDecryptionKey();
    }
}