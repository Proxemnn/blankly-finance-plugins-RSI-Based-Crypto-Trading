using System.Text;
using System;

namespace Substitution
{
    internal class Util
    {
        public static void Shuffle(StringBuilder sb)
        {
            Random rnd = new Random();
            int j;
            for (int i = 25; i > 0; i--)
            {
                j = rnd.Next(i + 1);
                (sb[i], sb[j]) = (sb[j], sb[i]);
            }
        }
    }
}