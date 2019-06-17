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

        [JsonProperty("Core.Struct:Children", Required = Required.Always)]
        public int CoreStructChildren { get; set; }

        [JsonProperty("Core.Field:ArrayCount", Required = Required.Always)]
        public int CoreFieldArrayCount { get; set; }

        [JsonProperty("Core.Field:Size", Required = Required.Always)]
        public int CoreFieldSize { get; set; }

        [JsonProperty("Core.Field:Offset", Required = Required.Always)]
        public int CoreFieldOffset { get; set; }

        [JsonProperty("Core.Field:Next", Required = Required.Always)]
        public int CoreFieldNext { get; set; }

        [JsonProperty("Core.ArrayProperty:Inner", Required = Required.Always)]
        public int CoreArrayPropertyInner { get; set; }

        [JsonProperty("Core.BoolProperty:BitFlag", Required = Required.Always)]
        public int CoreBoolPropertyBitFlag { get; set; }

        [JsonProperty("Core.ObjectProperty:PropertyClass", Required = Required.Always)]
        public int CoreObjectPropertyPropertyClass { get; set; }

        [JsonProperty("Core.StructProperty:Struct", Required = Required.Always)]
        public int CoreStructPropertyStruct { get; set; }

        public object Clone()
        {
            return new OffsetConfiguration()
            {
                CoreObjectOuter = this.CoreObjectOuter,
                CoreObjectName = this.CoreObjectName,
                CoreObjectClass = this.CoreObjectClass,
                CoreStructChildren = this.CoreStructChildren,
                CoreFieldArrayCount = this.CoreFieldArrayCount,
                CoreFieldSize = this.CoreFieldSize,
                CoreFieldOffset = this.CoreFieldOffset,
                CoreFieldNext = this.CoreFieldNext,
                CoreArrayPropertyInner = this.CoreArrayPropertyInner,
                CoreBoolPropertyBitFlag = this.CoreBoolPropertyBitFlag,
                CoreObjectPropertyPropertyClass = this.CoreObjectPropertyPropertyClass,
                CoreStructPropertyStruct = this.CoreStructPropertyStruct,
            };
        }
    }
}
