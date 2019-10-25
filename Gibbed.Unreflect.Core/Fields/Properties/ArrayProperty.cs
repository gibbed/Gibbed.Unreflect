/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Linq;

namespace Gibbed.Unreflect.Core.Fields
{
    public class ArrayProperty : UnrealProperty
    {
        internal ArrayProperty()
        {
        }

        public UnrealField Inner { get; internal set; }

        public override object ReadInstance(Engine engine, IntPtr objectAddress)
        {
            if (this.ArrayCount != 1)
            {
                throw new NotSupportedException();
            }

            if (this.Inner is ByteProperty)
            {
                var array = engine.Runtime.ReadStructure<UnrealNatives.Array>(objectAddress + this.Offset);
                if (array.Data == IntPtr.Zero)
                {
                    return new byte[0];
                }

                return engine.Runtime.ReadBytes(array.Data, array.Count);
            }

            if (this.Inner is IntProperty)
            {
                var array = engine.Runtime.ReadStructure<UnrealNatives.Array>(objectAddress + this.Offset);
                if (array.Data == IntPtr.Zero)
                {
                    return new int[0];
                }

                var intField = (IntProperty)this.Inner;
                var bytes = engine.Runtime.ReadBytes(array.Data, array.Count * 4);

                var item = 0;
                var items = new int[array.Count];
                for (int i = 0; i < array.Count; i++)
                {
                    items[i] = BitConverter.ToInt32(bytes, item);
                    item += intField.Size;
                }
                return items;
            }

            if (this.Inner is StructProperty)
            {
                var array = engine.Runtime.ReadStructure<UnrealNatives.Array>(objectAddress + this.Offset);
                if (array.Data == IntPtr.Zero)
                {
                    return new UnrealObject[0];
                }

                var structField = (StructProperty)this.Inner;

                var item = array.Data;
                var items = new object[array.Count];
                for (int i = 0; i < array.Count; i++)
                {
                    items[i] = structField.ReadInstance(engine, item);
                    item += structField.Size;
                }
                return items;
            }

            if (this.Inner is ObjectProperty)
            {
                var array = engine.ReadPointerArray(objectAddress + this.Offset);

                return array.Select(i =>
                {
                    if (i == IntPtr.Zero)
                    {
                        return null;
                    }

                    var obj = engine.GetObject(i);
                    if (obj == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return obj;
                }).ToArray();
            }

            if (this.Inner is ComponentProperty)
            {
                return "*** COMPONENT NOT IMPLEMENTED ***";
            }

            if (this.Inner is ClassProperty)
            {
                var array = engine.ReadPointerArray(objectAddress + this.Offset);

                return array.Select(i =>
                {
                    if (i == IntPtr.Zero)
                    {
                        return null;
                    }

                    var obj = engine.GetClass(i);
                    if (obj == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return obj;
                }).ToArray();
            }

            if (this.Inner is StringProperty)
            {
                return engine.ReadStringArray(objectAddress + this.Offset);
            }

            if (this.Inner is NameProperty)
            {
                return engine.ReadNameArray(objectAddress + this.Offset);
            }

            throw new NotSupportedException();
        }
    }
}
