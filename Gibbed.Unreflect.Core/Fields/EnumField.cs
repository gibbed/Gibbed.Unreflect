using System;
using System.Collections.Generic;
using System.Text;

namespace Gibbed.Unreflect.Core.Fields
{
    internal class EnumField : UnrealField
    {
        internal string TypeName;
        internal KeyValuePair<string, long>[] ValueNames;
    }
}
