using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class ObjectField : UnrealField
    {
        public UnrealClass PropertyClass { get; internal set; }

        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.Size != 4)
            {
                throw new InvalidOperationException();
            }

            var actualObjectAddress = engine.ReadPointer(objectAddress + this.Offset);
            if (actualObjectAddress == IntPtr.Zero)
            {
                return (UnrealObject)null;
            }

            var obj = engine.GetObject(actualObjectAddress);
            if (obj == null)
            {
                throw new InvalidOperationException();
            }
            return obj;
        }
    }
}
