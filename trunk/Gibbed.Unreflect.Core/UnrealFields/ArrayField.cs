/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Unreflect.Core.UnrealFields
{
    internal class ArrayField : UnrealField
    {
        public UnrealField Inner { get; internal set; }

        internal override object Read(Engine engine, IntPtr objectAddress)
        {
            if (this.ArrayCount != 1)
            {
                throw new NotSupportedException();
            }

            if (this.Inner is ByteField)
            {
                var array = engine.Runtime.ReadStructure<UnrealNatives.Array>(objectAddress + this.Offset);
                if (array.Data == IntPtr.Zero)
                {
                    return new byte[0];
                }

                return engine.Runtime.ReadBytes(array.Data, array.Count);
            }

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

            if (this.Inner is ObjectField)
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

            if (this.Inner is ClassField)
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

            if (this.Inner is StrField)
            {
                return engine.ReadStringArray(objectAddress + this.Offset);
            }

            if (this.Inner is NameField)
            {
                return engine.ReadNameArray(objectAddress + this.Offset);
            }

            throw new NotSupportedException();
        }
    }
}
