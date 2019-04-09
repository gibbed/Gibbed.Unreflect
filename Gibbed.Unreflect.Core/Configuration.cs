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
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Gibbed.Unreflect.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct Configuration : ICloneable
    {
        [JsonProperty("base_address", Required = Required.Always)]
        [JsonConverter(typeof(JsonPointerConverter))]
        public IntPtr BaseAddress { get; set; }

        [JsonProperty("global_name_array_address", Required = Required.Always)]
        [JsonConverter(typeof(JsonPointerConverter))]
        public IntPtr GlobalNameArrayAddress { get; set; }

        [JsonProperty("global_object_array_address", Required = Required.Always)]
        [JsonConverter(typeof(JsonPointerConverter))]
        public IntPtr GlobalObjectArrayAddress { get; set; }

        [JsonProperty("name_entry_string_offset", Required = Required.Always)]
        public int NameEntryStringOffset { get; set; }

        [JsonProperty("offsets")]
        public OffsetConfiguration Offsets { get; set; }

        private static IntPtr AdjustAddress(IntPtr originalBaseAddress, ProcessModule module, IntPtr address)
        {
            var addressValue = address.ToInt64();
            addressValue -= originalBaseAddress.ToInt64();
            addressValue += module.BaseAddress.ToInt64();
            return new IntPtr(addressValue);
        }

        public void AdjustAddresses(ProcessModule module)
        {
            this.GlobalNameArrayAddress = AdjustAddress(this.BaseAddress, module, this.GlobalNameArrayAddress);
            this.GlobalObjectArrayAddress = AdjustAddress(this.BaseAddress, module, this.GlobalObjectArrayAddress);
        }

        public static Configuration Load(string path)
        {
            string text;
            using (var input = new StreamReader(path))
            {
                text = input.ReadToEnd();
            }
            return Deserialize(text);
        }

        public static Configuration Deserialize(string text)
        {
            var settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
            };
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return JsonConvert.DeserializeObject<Configuration>(text, settings);
        }

        public object Clone()
        {
            return new Configuration()
            {
                BaseAddress = this.BaseAddress,
                GlobalNameArrayAddress = this.GlobalNameArrayAddress,
                GlobalObjectArrayAddress = this.GlobalObjectArrayAddress,
                NameEntryStringOffset = this.NameEntryStringOffset,
                Offsets = (OffsetConfiguration)this.Offsets.Clone(),
            };
        }
    }
}
