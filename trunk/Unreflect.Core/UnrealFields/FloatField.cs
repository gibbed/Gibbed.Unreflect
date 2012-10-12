using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class FloatField : UnrealField
    {
        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.Size != 4)
            {
                throw new InvalidOperationException();
            }

            return engine.Runtime.ReadValueF32(objectAddress + this.Offset);
        }
    }
}
