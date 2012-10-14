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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gibbed.Unreflect.Runtime;

namespace Gibbed.Unreflect.Core
{
    public class Engine
    {
        internal readonly Configuration Configuration;
        internal readonly RuntimeBase Runtime;

        private readonly IntPtr[] _NameAddresses;
        private readonly IntPtr[] _ObjectAddresses;
        private readonly Dictionary<IntPtr, UnrealObjectShim> _ObjectShims = new Dictionary<IntPtr, UnrealObjectShim>();

        private readonly Dictionary<int, string> _CachedNames = new Dictionary<int, string>();
        private readonly Dictionary<IntPtr, UnrealClass> _CachedClasses = new Dictionary<IntPtr, UnrealClass>();
        private readonly Dictionary<IntPtr, UnrealField> _CachedFields = new Dictionary<IntPtr, UnrealField>();
        private readonly Dictionary<IntPtr, string> _CachedPaths = new Dictionary<IntPtr, string>();

        public IEnumerable<UnrealObject> Objects
        {
            get { return this._ObjectShims.Values.Select(s => s.Object); }
        }

        public UnrealObject GetObject(IntPtr address)
        {
            return this._ObjectShims[address].Object;
        }

        public UnrealObject GetObject(string path)
        {
            var shim = this._ObjectShims.Values.SingleOrDefault(o => o.Path == path);
            if (shim == null)
            {
                return null;
            }
            return shim.Object;
        }

        public UnrealClass GetClass(IntPtr address)
        {
            return this.ReadClass(address);
        }

        public UnrealClass GetClass(string path)
        {
            var uclass = this._CachedClasses.Values.SingleOrDefault(o => o.Path == path);
            if (uclass == null)
            {
                return null;
            }
            return uclass;
        }

        public Engine(Configuration configuration, RuntimeBase runtime)
        {
            if (runtime == null)
            {
                throw new ArgumentNullException("runtime");
            }

            this.Configuration = (Configuration)configuration.Clone();
            this.Runtime = runtime;
            this._NameAddresses = this.ReadPointerArray(configuration.GlobalNameArrayAddress);
            this._ObjectAddresses = this.ReadPointerArray(configuration.GlobalObjectArrayAddress);

            foreach (var objectAddress in this._ObjectAddresses.Where(oa => oa != IntPtr.Zero))
            {
                var objectClassPointer = this.ReadPointer(objectAddress + this.Configuration.ObjectClassOffset);
                var objectClass = this.ReadClass(objectClassPointer);
                var objectName = this.ReadName(objectAddress + this.Configuration.ObjectNameOffset);
                var objectPath = this.ReadPath(objectAddress);
                this._ObjectShims.Add(objectAddress,
                                      new UnrealObjectShim(this, objectAddress, objectClass, objectName, objectPath));
            }
        }

        internal string ReadString(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                throw new ArgumentNullException("address");
            }

            var str = this.Runtime.ReadStructure<UnrealNatives.String>(address);
            return this.ReadString(str);
        }

        internal string ReadString(UnrealNatives.String str)
        {
            return this.Runtime.ReadString(str.Data, str.Length, Encoding.Unicode);
        }

        internal string ReadName(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                throw new ArgumentNullException("address");
            }

            var name = this.Runtime.ReadStructure<UnrealNatives.Name>(address);

            string value;

            if (this._CachedNames.ContainsKey(name.Id) == true)
            {
                value = this._CachedNames[name.Id];
            }
            else
            {
                var dataAddress = this._NameAddresses[name.Id];
                value = this.Runtime.ReadStringZ(dataAddress + 16, Encoding.ASCII);
                this._CachedNames.Add(name.Id, value);
            }

            if (name.Index != 0)
            {
                value += "_" + name.Index.ToString(CultureInfo.InvariantCulture);
            }

            return value;
        }

        internal string ReadPath(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                throw new ArgumentNullException("address");
            }

            if (this._CachedPaths.ContainsKey(address) == true)
            {
                return this._CachedPaths[address];
            }

            var path = this.ReadName(address + this.Configuration.ObjectNameOffset);
            var outer = this.ReadPointer(address + this.Configuration.ObjectOuterOffset);
            while (outer != IntPtr.Zero && outer != address)
            {
                var name = this.ReadName(outer + this.Configuration.ObjectNameOffset);
                path = name + "." + path;
                outer = this.ReadPointer(outer + this.Configuration.ObjectOuterOffset);
            }

            this._CachedPaths.Add(address, path);
            return path;
        }

        private UnrealClass ReadClass(IntPtr address)
        {
            if (this._CachedClasses.ContainsKey(address) == true)
            {
                return this._CachedClasses[address];
            }

            var uclass = new UnrealClass();
            this._CachedClasses.Add(address, uclass);

            uclass.Address = address;
            uclass.VfTableObject = this.ReadPointer(address + 0);
            uclass.Name = this.ReadName(address + this.Configuration.ObjectNameOffset);
            uclass.Path = this.ReadPath(address);

            var classAddress = this.ReadPointer(address + this.Configuration.ObjectClassOffset);
            if (classAddress != IntPtr.Zero &&
                classAddress != address)
            {
                uclass.Class = this.ReadClass(classAddress);
            }

            var fieldAddress = this.ReadPointer(address + this.Configuration.ClassFirstFieldOffset);
            var fields = new List<UnrealField>();
            while (fieldAddress != IntPtr.Zero)
            {
                fields.Add(this.ReadField(fieldAddress));
                fieldAddress = this.ReadPointer(fieldAddress + this.Configuration.FieldNextFieldOffset);
            }
            uclass.Fields = fields.ToArray();

            return uclass;
        }

        private UnrealField ReadField(IntPtr address)
        {
            if (this._CachedFields.ContainsKey(address) == true)
            {
                return this._CachedFields[address];
            }

            UnrealClass uclass = null;
            var classAddress = this.ReadPointer(address + this.Configuration.ObjectClassOffset);
            if (classAddress != IntPtr.Zero &&
                classAddress != address)
            {
                uclass = this.ReadClass(classAddress);
            }

            if (this._CachedFields.ContainsKey(address) == true)
            {
                return this._CachedFields[address];
            }

            UnrealField field;
            switch (uclass.Path)
            {
                case "Core.ClassProperty":
                {
                    field = new UnrealFields.ClassField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                case "Core.ObjectProperty":
                {
                    var objectField = new UnrealFields.ObjectField();
                    field = objectField;
                    this._CachedFields.Add(address, field);
                    // TODO: move offset to config value
                    objectField.PropertyClass = this.ReadClass(this.ReadPointer(address + 0x80));
                    break;
                }

                case "Core.StructProperty":
                {
                    var structField = new UnrealFields.StructField();
                    field = structField;
                    this._CachedFields.Add(address, field);
                    // TODO: move offset to config value
                    structField.Structure = this.ReadClass(this.ReadPointer(address + 0x80));
                    break;
                }

                case "Core.ArrayProperty":
                {
                    var arrayField = new UnrealFields.ArrayField();
                    field = arrayField;
                    this._CachedFields.Add(address, field);
                    // TODO: move offset to config value
                    arrayField.Inner = this.ReadField(this.ReadPointer(address + 0x80));
                    break;
                }

                case "Core.BoolProperty":
                {
                    var boolField = new UnrealFields.BoolField();
                    field = boolField;
                    this._CachedFields.Add(address, field);
                    // TODO: move offset to config value
                    boolField.BitFlag = this.Runtime.ReadValueS32(address + 0x80);
                    break;
                }

                case "Core.ByteProperty":
                {
                    field = new UnrealFields.ByteField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                case "Core.IntProperty":
                {
                    field = new UnrealFields.IntField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                case "Core.FloatProperty":
                {
                    field = new UnrealFields.FloatField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                case "Core.StrProperty":
                {
                    field = new UnrealFields.StrField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                case "Core.NameProperty":
                {
                    field = new UnrealFields.NameField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                case "Core.ByteAttributeProperty":
                case "Core.IntAttributeProperty":
                case "Core.FloatAttributeProperty":
                case "Core.ComponentProperty":
                case "Core.MapProperty":
                case "Core.DelegateProperty":
                case "Core.InterfaceProperty":
                {
                    field = new UnrealFields.DummyField();
                    this._CachedFields.Add(address, field);
                    break;
                }

                default:
                {
                    throw new NotImplementedException();
                }
            }

            field.Address = address;
            field.VfTableObject = this.ReadPointer(address + 0);
            field.Name = this.ReadName(address + this.Configuration.ObjectNameOffset);
            field.Class = uclass;
            field.ArrayCount = this.Runtime.ReadValueS32(address + 0x40); // TODO: move offset to config value
            field.Size = this.Runtime.ReadValueS32(address + 0x44); // TODO: move offset to config value
            field.Offset = this.Runtime.ReadValueS32(address + 0x60); // TODO: move offset to config value
            return field;
        }

        internal IntPtr ReadPointer(IntPtr address)
        {
            return this.Runtime.ReadStructure<UnrealNatives.Pointer>(address).Value;
        }

        internal void WritePointer(IntPtr address, IntPtr value)
        {
            this.Runtime.WriteStructure(address, new UnrealNatives.Pointer(value));
        }

        internal IntPtr[] ReadPointerArray(IntPtr address)
        {
            return this.ReadStructureArray<UnrealNatives.Pointer>(address)
                .Select(p => p.Value)
                .ToArray();
        }

        internal void WritePointerArray(IntPtr address, IntPtr[] items)
        {
            this.WriteStructureArray(address,
                                     items.Select(i => new UnrealNatives.Pointer(i))
                                         .ToArray());
        }

        internal string[] ReadStringArray(IntPtr address)
        {
            var array = this.Runtime.ReadStructure<UnrealNatives.Array>(address);
            if (array.Data == IntPtr.Zero)
            {
                return new string[0];
            }

            var structureSize = 12;
            var buffer = this.Runtime.ReadBytes(array.Data, array.Count * structureSize);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            var items = new string[array.Count];
            for (var o = 0; o < array.Count; o++)
            {
                items[o] =
                    this.ReadString((UnrealNatives.String)Marshal.PtrToStructure(current, typeof(UnrealNatives.String)));
                current += 12;
            }
            handle.Free();
            return items;
        }

        internal TStructure[] ReadStructureArray<TStructure>(IntPtr address)
            where TStructure : struct
        {
            var array = this.Runtime.ReadStructure<UnrealNatives.Array>(address);
            if (array.Data == IntPtr.Zero)
            {
                return new TStructure[0];
            }

            var structureSize = Marshal.SizeOf(typeof(TStructure));
            var buffer = this.Runtime.ReadBytes(array.Data, array.Count * structureSize);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            var items = new TStructure[array.Count];
            for (var o = 0; o < array.Count; o++)
            {
                items[o] = (TStructure)Marshal.PtrToStructure(current, typeof(TStructure));
                current += structureSize;
            }
            handle.Free();
            return items;
        }

        internal void WriteStructureArray<TStructure>(IntPtr address, TStructure[] items)
            where TStructure : struct
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (items.Length == 0)
            {
                return;
            }

            var array = this.Runtime.ReadStructure<UnrealNatives.Array>(address);
            if (array.Data == IntPtr.Zero ||
                array.Count < items.Length)
            {
                throw new InvalidOperationException();
            }

            var structureSize = Marshal.SizeOf(typeof(TStructure));
            var buffer = new byte[items.Length * structureSize];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            for (var i = 0; i < items.Length; i++)
            {
                Marshal.StructureToPtr(items[i], current, false);
                current += structureSize;
            }
            handle.Free();
            this.Runtime.WriteBytes(array.Data, buffer);
        }
    }
}
