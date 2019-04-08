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
        [JsonProperty("global_name_array_address")]
        [JsonConverter(typeof(PointerConverter))]
        public IntPtr GlobalNameArrayAddress;

        [JsonProperty("global_object_array_address")]
        [JsonConverter(typeof(PointerConverter))]
        public IntPtr GlobalObjectArrayAddress;

        [JsonProperty("object_name_offset")]
        public int ObjectNameOffset;

        [JsonProperty("object_outer_offset")]
        public int ObjectOuterOffset;

        [JsonProperty("object_class_offset")]
        public int ObjectClassOffset;

        [JsonProperty("class_first_field_offset")]
        public int ClassFirstFieldOffset;

        [JsonProperty("field_next_field_offset")]
        public int FieldNextFieldOffset;

        private static IntPtr AdjustAddress(ProcessModule module, IntPtr address)
        {
            return module.BaseAddress + (address.ToInt32() - 0x400000);
        }

        public void AdjustAddresses(ProcessModule module)
        {
            this.GlobalNameArrayAddress = AdjustAddress(module, this.GlobalNameArrayAddress);
            this.GlobalObjectArrayAddress = AdjustAddress(module, this.GlobalObjectArrayAddress);
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
                GlobalNameArrayAddress = this.GlobalNameArrayAddress,
                GlobalObjectArrayAddress = this.GlobalObjectArrayAddress,
                ObjectNameOffset = this.ObjectNameOffset,
                ObjectOuterOffset = this.ObjectOuterOffset,
                ObjectClassOffset = this.ObjectClassOffset,
                ClassFirstFieldOffset = this.ClassFirstFieldOffset,
                FieldNextFieldOffset = this.FieldNextFieldOffset,
            };
        }

        internal class PointerConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(int);
            }

            public override object ReadJson(JsonReader reader,
                                            Type objectType,
                                            object existingValue,
                                            JsonSerializer serializer)
            {
                return new IntPtr((long)reader.Value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((IntPtr)value).ToInt32());
            }
        }
    }
}
