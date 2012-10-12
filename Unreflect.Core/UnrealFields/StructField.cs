using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class StructField : UnrealField
    {
        public UnrealClass Structure { get; internal set; }

        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            var actualObjectAddress = objectAddress + this.Offset;
            if (actualObjectAddress == IntPtr.Zero)
            {
                return (UnrealObject)null;
            }

            if (this.Structure.Path == "Core.Object.Pointer")
            {
                return engine.ReadPointer(actualObjectAddress);
            }

            if (this.Structure.Path == "Core.Object.QWord")
            {
                return engine.Runtime.ReadValueU64(actualObjectAddress);
            }

            if (this.Structure.Path == "Core.Object.Double")
            {
                return engine.Runtime.ReadValueF64(actualObjectAddress);
            }

            return new UnrealObjectShim(engine, actualObjectAddress, this.Structure, null, null).Object;
        }
    }
}
