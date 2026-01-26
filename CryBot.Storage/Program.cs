using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substitution
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ShiftByNCipher cipher = new ShiftByNCipher(1);
            //cipher.PlainText = "Hello World";
            //cipher.Encrypt();
            //Console.WriteLine(cipher.CipherText);

            //cipher.CipherText = "IFMMP XPSME";
            //cipher.Decrypt();
            //Console.WriteLine(cipher.PlainText);

            //CaesarCipher c = new CaesarCipher();
            //c.PlainText = "Caesar is here!";
            //c.Encrypt();
            //Console.WriteLine(c.CipherText);

            //c.CipherText = "FDHVDU LV KHUH!";
            //c.Decrypt();
            //Console.WriteLine(c.PlainText);

            //ROT13 rot13 = new ROT13();

            MonoalphabeticSubstitution ms = new MonoalphabeticSubstitution();
            ms.PlainText = "Hello World!";
            ms.Encrypt();
            Console.WriteLine(ms.CipherText);

            Console.WriteLine(string.Join(" ", "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()));
            Console.WriteLine(ms.Key);
            
        }
    }
}
