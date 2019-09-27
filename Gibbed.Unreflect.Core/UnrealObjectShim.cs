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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Gibbed.Unreflect.Core
{
    internal class UnrealObjectShim
    {
        private readonly Engine _Engine;
        public readonly IntPtr Address;
        public readonly UnrealClass Class;
        public readonly string Name;
        public readonly string Path;
        public readonly UnrealObject Object;

        internal UnrealObjectShim(Engine engine, IntPtr address, UnrealClass uclass, string name, string path)
        {
            this._Engine = engine;
            this.Address = address;
            this.Class = uclass;
            this.Name = name;
            this.Path = path;
            this.Object = new UnrealObject(this);
        }

        private readonly Dictionary<string, object> _FieldCache = new Dictionary<string, object>();

        public IEnumerable<string> GetDynamicMemberNames()
        {
            return this.Class.GetFieldNames();
        }

        public bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this._FieldCache.TryGetValue(binder.Name, out result) == true)
            {
                return true;
            }

            var field = this.Class.GetField(binder.Name);
            if (field == null)
            {
                //result = $"*** INVALID FIELD {binder.Name} ***";
                result = null;
                return false;
            }

            try
            {
                result = field.ReadInstance(this._Engine, this.Address);
            }
            catch (NotImplementedException)
            {
                result = $"*** NOT IMPLEMENTED {field.Class.Name} ***";
                return true;
            }
            /*catch (Exception e)
            {
                result = $"*** EXCEPTION {e.Message} ***";
                return true;
            }*/

            this._FieldCache.Add(binder.Name, result);
            return true;
        }

        public bool TrySetMember(SetMemberBinder binder, object value)
        {
            var field = this.Class.Fields.SingleOrDefault(f => f.Name == binder.Name);
            if (field == null)
            {
                return false;
            }

            field.WriteInstance(this._Engine, this.Address, value);
            this._FieldCache.Remove(binder.Name);
            return true;
        }

        public override string ToString()
        {
            return this.Path;
        }
    }
}
