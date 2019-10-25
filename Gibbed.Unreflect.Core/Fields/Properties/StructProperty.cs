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

namespace Gibbed.Unreflect.Core.Fields
{
    public class StructProperty : UnrealProperty
    {
        internal StructProperty()
        {
        }

        public UnrealClass Structure { get; internal set; }

        public override object ReadInstance(Engine engine, IntPtr objectAddress)
        {
            var fieldAddress = objectAddress + this.Offset;

            if (this.ArrayCount != 1)
            {
                if (fieldAddress == IntPtr.Zero)
                {
                    throw new InvalidOperationException();
                }

                var items = new object[this.ArrayCount];
                for (int i = 0; i < this.ArrayCount; i++)
                {
                    items[i] = this.ReadInternal(engine, fieldAddress);
                    fieldAddress += this.Size;
                }
                return items;
            }

            if (this.ArrayCount == 0)
            {
                throw new InvalidOperationException();
            }

            if (fieldAddress == IntPtr.Zero)
            {
                return (UnrealObject)null;
            }

            return this.ReadInternal(engine, fieldAddress);
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
