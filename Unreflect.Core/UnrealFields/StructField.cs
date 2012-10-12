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

            if (this.ArrayCount != 1)
            {
                if (actualObjectAddress == IntPtr.Zero)
                {
                    throw new InvalidOperationException();
                }

                var items = new object[this.ArrayCount];
                for (int i = 0; i < this.ArrayCount; i++)
                {
                    items[i] = this.ReadInternal(engine, actualObjectAddress);
                    actualObjectAddress += this.Size;
                }
                return items;
            }

            if (this.ArrayCount == 0)
            {
                throw new InvalidOperationException();
            }

            if (actualObjectAddress == IntPtr.Zero)
            {
                return (UnrealObject)null;
            }

            return this.ReadInternal(engine, actualObjectAddress);
        }

        private object ReadInternal(Engine engine, IntPtr objectAddress)
        {
            if (this.Structure.Path == "Core.Object.Pointer")
            {
                return engine.ReadPointer(objectAddress);
            }

            if (this.Structure.Path == "Core.Object.QWord")
            {
                return engine.Runtime.ReadValueU64(objectAddress);
            }

            if (this.Structure.Path == "Core.Object.Double")
            {
                return engine.Runtime.ReadValueF64(objectAddress);
            }

            return new UnrealObjectShim(engine, objectAddress, this.Structure, null, null).Object;
        }
    }
}
