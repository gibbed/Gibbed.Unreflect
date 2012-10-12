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
