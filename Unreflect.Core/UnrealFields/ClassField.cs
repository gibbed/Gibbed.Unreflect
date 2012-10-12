using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class ClassField : UnrealField
    {
        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            var classAddress = engine.ReadPointer(objectAddress + this.Offset);
            if (classAddress == IntPtr.Zero)
            {
                return (UnrealObject)null;
            }

            var uclass = engine.GetClass(classAddress);
            if (uclass == null)
            {
                throw new InvalidOperationException();
            }
            return uclass;
        }
    }
}
