using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substitution
{
    internal class CaesarCipher : ShiftByNCipher
    {
        public CaesarCipher(): base(3)
        {

        }
    }
}
