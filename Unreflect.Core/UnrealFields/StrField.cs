using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class StrField : UnrealField
    {
        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.Size != 12)
            {
                throw new InvalidOperationException();
            }

            return engine.ReadString(objectAddress + this.Offset);
        }
    }
}
