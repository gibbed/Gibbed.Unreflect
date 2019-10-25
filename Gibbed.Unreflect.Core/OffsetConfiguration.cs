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
using Newtonsoft.Json;

namespace Gibbed.Unreflect.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct OffsetConfiguration : ICloneable
    {
        [JsonProperty("Core.Object:Outer", Required = Required.Always)]
        public int CoreObjectOuter { get; set; }

        [JsonProperty("Core.Object:Name", Required = Required.Always)]
        public int CoreObjectName { get; set; }

        [JsonProperty("Core.Object:Class", Required = Required.Always)]
        public int CoreObjectClass { get; set; }

        [JsonProperty("Core.Struct:SuperStruct", Required = Required.Always)]
        public int CoreStructSuperStruct { get; set; }

        [JsonProperty("Core.Struct:Children", Required = Required.Always)]
        public int CoreStructChildren { get; set; }

        [JsonProperty("Core.Field:Next", Required = Required.Always)]
        public int CoreFieldNext { get; set; }

        [JsonProperty("Core.Class:ClassDefaultObject", Required = Required.Always)]
        public int CoreClassClassDefaultObject { get; set; }

        [JsonProperty("Core.Property:ArrayCount", Required = Required.Always)]
        public int CorePropertyArrayCount { get; set; }

        [JsonProperty("Core.Property:Size", Required = Required.Always)]
        public int CorePropertySize { get; set; }

        [JsonProperty("Core.Property:Offset", Required = Required.Always)]
        public int CorePropertyOffset { get; set; }

        [JsonProperty("Core.ArrayProperty:Inner", Required = Required.Always)]
        public int CoreArrayPropertyInner { get; set; }

        [JsonProperty("Core.BoolProperty:FieldSize", Required = Required.Always)]
        public int CoreBoolPropertyFieldSize { get; set; }

        [JsonProperty("Core.BoolProperty:ByteOffset", Required = Required.Always)]
        public int CoreBoolPropertyByteOffset { get; set; }

        [JsonProperty("Core.BoolProperty:ByteMask", Required = Required.Always)]
        public int CoreBoolPropertyByteMask { get; set; }

        [JsonProperty("Core.BoolProperty:FieldMask", Required = Required.Always)]
        public int CoreBoolPropertyFieldMask { get; set; }

        [JsonProperty("Core.ObjectProperty:PropertyClass", Required = Required.Always)]
        public int CoreObjectPropertyPropertyClass { get; set; }

        [JsonProperty("Core.StructProperty:Struct", Required = Required.Always)]
        public int CoreStructPropertyStruct { get; set; }

        [JsonProperty("Core.EnumProperty:UnderlyingProperty", Required = Required.Always)]
        public int CoreEnumPropertyUnderlyingProperty { get; set; }

        [JsonProperty("Core.EnumProperty:Enum", Required = Required.Always)]
        public int CoreEnumPropertyEnum { get; set; }

        [JsonProperty("Core.Enum:TypeName", Required = Required.Always)]
        public int CoreEnumTypeName { get; set; }

        [JsonProperty("Core.Enum:ValueNames", Required = Required.Always)]
        public int CoreEnumValueNames { get; set; }

        public object Clone()
        {
            return new OffsetConfiguration()
            {
                CoreObjectOuter = this.CoreObjectOuter,
                CoreObjectName = this.CoreObjectName,
                CoreObjectClass = this.CoreObjectClass,
                CoreStructSuperStruct = this.CoreStructSuperStruct,
                CoreStructChildren = this.CoreStructChildren,
                CorePropertyArrayCount = this.CorePropertyArrayCount,
                CorePropertySize = this.CorePropertySize,
                CorePropertyOffset = this.CorePropertyOffset,
                CoreFieldNext = this.CoreFieldNext,
                CoreClassClassDefaultObject = this.CoreClassClassDefaultObject,
                CoreArrayPropertyInner = this.CoreArrayPropertyInner,
                CoreBoolPropertyFieldSize = this.CoreBoolPropertyFieldSize,
                CoreBoolPropertyByteOffset = this.CoreBoolPropertyByteOffset,
                CoreBoolPropertyByteMask = this.CoreBoolPropertyByteMask,
                CoreBoolPropertyFieldMask = this.CoreBoolPropertyFieldMask,
                CoreObjectPropertyPropertyClass = this.CoreObjectPropertyPropertyClass,
                CoreStructPropertyStruct = this.CoreStructPropertyStruct,
                CoreEnumPropertyUnderlyingProperty = this.CoreEnumPropertyUnderlyingProperty,
                CoreEnumPropertyEnum = this.CoreEnumPropertyEnum,
                CoreEnumTypeName = this.CoreEnumTypeName,
                CoreEnumValueNames = this.CoreEnumValueNames,
            };
        }
    }
}
