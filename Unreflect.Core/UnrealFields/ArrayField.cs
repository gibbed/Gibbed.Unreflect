using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unreflect.Core.UnrealFields
{
    internal class ArrayField : UnrealField
    {
        public UnrealField Inner { get; internal set; }

        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.Inner is StructField)
            {
                var array = engine.Runtime.ReadStructure<UnrealNatives.Array>(objectAddress + this.Offset);
                if (array.Data == IntPtr.Zero)
                {
                    return new UnrealObject[0];
                }

                var structField = (StructField)this.Inner;

                var item = array.Data;
                var items = new object[array.Count];
                for (int i = 0; i < array.Count; i++)
                {
                    items[i] = structField.Read(engine, item);
                    item += structField.Size;
                }
                return items;
            }
            else if (this.Inner is ObjectField)
            {
                var array = engine.ReadPointerArray(objectAddress + this.Offset);

                return array.Select(i =>
                {
                    var obj = engine.GetObject(i);
                    if (obj == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return obj;
                }).ToArray();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
