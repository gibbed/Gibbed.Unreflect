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
using System.Collections.Generic;
using System.Dynamic;

namespace Gibbed.Unreflect.Core
{
    public class UnrealObject : DynamicObject
    {
        private readonly UnrealObjectShim _Shim;

        public bool IsA(UnrealClass uclass)
        {
            if (this._Shim.Class == null)
            {
                return false;
            }

            return this._Shim.Class == uclass ||
                   this._Shim.Class.IsA(uclass) == true;
        }

        public string GetName()
        {
            return this._Shim.Name;
        }

        public string GetPath()
        {
            return this._Shim.Path;
        }

        public UnrealClass GetClass()
        {
            return this._Shim.Class;
        }

        internal UnrealObject(UnrealObjectShim shim)
        {
            if (shim == null)
            {
                throw new ArgumentNullException("shim");
            }

            this._Shim = shim;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this._Shim.GetDynamicMemberNames();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return this._Shim.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return this._Shim.TrySetMember(binder, value);
        }

        public override string ToString()
        {
            return this._Shim.Path;
        }
    }
}
