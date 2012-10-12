using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class NameField : UnrealField
    {
        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.Size != 8)
            {
                throw new InvalidOperationException();
            }

            var fieldAddress = objectAddress + this.Offset;

            if (this.ArrayCount != 1)
            {
                var items = new string[this.ArrayCount];
                for (int o = 0; o < this.ArrayCount; o++)
                {
                    items[o] = engine.ReadName(fieldAddress);
                    fieldAddress += this.Size;
                }
                return items;
            }

            if (this.ArrayCount == 0)
            {
                throw new InvalidOperationException();
            }

            return engine.ReadName(fieldAddress);
        }
    }
}
