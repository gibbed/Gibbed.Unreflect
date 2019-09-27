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
using System.Linq;

namespace Gibbed.Unreflect.Core
{
    public class UnrealClass
    {
        public IntPtr Address { get; internal set; }
        public IntPtr VfTableObject { get; internal set; }
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public UnrealClass Class { get; internal set; }
        public UnrealClass Super { get; internal set; }
        internal UnrealField[] Fields { get; set; }

        internal UnrealClass()
        {
        }

        public bool IsA(UnrealClass someBase)
        {
            var klass = this;
            do
            {
                if (klass == someBase)
                {
                    return true;
                }
                klass = klass.Class;
            }
            while (klass != null);
            return false;
        }

        public bool IsChildOf(UnrealClass someBase)
        {
            var klass = this;
            do
            {
                if (klass == someBase)
                {
                    return true;
                }
                klass = klass.Super;
            }
            while (klass != null);
            return false;
        }

        internal IEnumerable<string> GetFieldNames()
        {
            return GetFieldNamesReverse().Reverse();
        }

        private IEnumerable<string> GetFieldNamesReverse()
        {
            var klass = this;
            do
            {
                foreach (var field in klass.Fields.Reverse())
                {
                    yield return field.Name;
                }
                klass = klass.Super;
            }
            while (klass != null);
        }

        internal UnrealField GetField(string name)
        {
            var klass = this;
            do
            {
                var field = klass.Fields.SingleOrDefault(f => f.Name == name);
                if (field != null)
                {
                    return field;
                }
                klass = klass.Super;
            }
            while (klass != null);
            return null;
        }

        public override string ToString()
        {
            return this.Path;
        }
    }
}
