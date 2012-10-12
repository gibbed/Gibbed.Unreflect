using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class IntField : UnrealField
    {
        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.ArrayCount != 1)
            {
                throw new NotSupportedException();
            }

            if (this.Size != 4)
            {
                throw new InvalidOperationException();
            }

            return engine.Runtime.ReadValueS32(objectAddress + this.Offset);
        }
    }
}
