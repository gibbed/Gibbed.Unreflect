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

            return engine.ReadName(objectAddress + this.Offset);
        }
    }
}
