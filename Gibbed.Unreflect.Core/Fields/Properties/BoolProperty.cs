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
    public class BoolProperty : UnrealProperty
    {
        internal BoolProperty()
        {
        }

        public byte FieldSize { get; internal set; }
        public byte ByteOffset { get; internal set; }
        public byte ByteMask { get; internal set; }
        public byte FieldMask { get; internal set; }

        public override object ReadInstance(Engine engine, IntPtr objectAddress)
        {
            if (this.ArrayCount == 0)
            {
                throw new InvalidOperationException();
            }

            if (this.ArrayCount != 1)
            {
                throw new NotSupportedException();
            }

            if (objectAddress == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            var fieldAddress = objectAddress + this.Offset + this.ByteOffset;
            var byteValue = engine.Runtime.ReadValueU8(fieldAddress);
            return (byteValue & this.FieldMask) != 0;
        }

        public override void WriteInstance(Engine engine, IntPtr objectAddress, object value)
        {
            if (this.ArrayCount == 0)
            {
                throw new InvalidOperationException();
            }

            if (this.ArrayCount != 1)
            {
                throw new NotSupportedException();
            }

            if (objectAddress == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            var fieldAddress = objectAddress + this.Offset + this.ByteOffset;
            var byteValue = engine.Runtime.ReadValueU8(fieldAddress);

            if (Convert.ToBoolean(value) == true)
            {
                byteValue |= this.ByteMask;
            }
            else
            {
                byteValue &= (byte)~this.FieldMask;
            }

            engine.Runtime.WriteValueU8(fieldAddress, byteValue);
        }
    }
}
