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

            var fieldAddress = objectAddress + this.Offset;

            if (this.ArrayCount != 1)
            {
                var items = new object[this.ArrayCount];
                for (int i = 0; i < this.ArrayCount; i++)
                {
                    items[i] = this.ReadInternal(engine, fieldAddress);
                    fieldAddress += this.Size;
                }
                return items;
            }

            return this.ReadInternal(engine, fieldAddress);
        }

        private object ReadInternal(Engine engine, IntPtr objectAddress)
        {
            var actualObjectAddress = engine.ReadPointer(objectAddress);
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
