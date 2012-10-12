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

namespace Unreflect.Core
{
    internal abstract class UnrealField
    {
        public IntPtr Address { get; internal set; }
        public IntPtr VfTableObject { get; internal set; }
        public string Name { get; internal set; }
        public UnrealClass Class { get; internal set; }
        public int Offset { get; internal set; }
        public int Size { get; internal set; }
        public int ArrayCount { get; internal set; }

        internal virtual object Read(Engine engine, IntPtr objectAddress)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
