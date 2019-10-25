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
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Gibbed.Unreflect.Runtime;

namespace Gibbed.Unreflect.Core
{
    public class Engine
    {
        private readonly Dictionary<int, string> _CachedNames;
        private readonly Dictionary<IntPtr, UnrealClass> _CachedClasses;
        private readonly Dictionary<IntPtr, UnrealField> _CachedFields;
        private readonly Dictionary<IntPtr, string> _CachedPaths;

        internal readonly Configuration Configuration;
        internal OffsetConfiguration Offsets => this.Configuration.Offsets;

        internal readonly RuntimeBase Runtime;

        private readonly IntPtr[,] _NameAddresses;
        private readonly IntPtr[] _ObjectAddresses;
        private readonly Dictionary<IntPtr, UnrealObjectShim> _ObjectShims;

        public Engine(Configuration configuration, RuntimeBase runtime)
        {
            this.Runtime = runtime ?? throw new ArgumentNullException("runtime");

            this._CachedNames = new Dictionary<int, string>();
            this._CachedClasses = new Dictionary<IntPtr, UnrealClass>();
            this._CachedFields = new Dictionary<IntPtr, UnrealField>();
            this._CachedPaths = new Dictionary<IntPtr, string>();

            this.Configuration = (Configuration)configuration.Clone();

            var nameTableAddress = this.ReadPointer(configuration.GlobalNameTableAddressAddress);
            var nameTable = this.Runtime.ReadStructure<UnrealNatives.NameTable>(nameTableAddress);

            this._NameAddresses = new IntPtr[nameTable.ChunkCount, UnrealNatives.NameTable.ItemsPerChunk];
            for (int i = 0; i < nameTable.ChunkCount; i++)
            {
                var nameTableChunk = this.Runtime.ReadStructure<UnrealNatives.NameTableChunk>(nameTable.ChunkPointers[i]);
                for (int j = 0; j < UnrealNatives.NameTable.ItemsPerChunk; j++)
                {
                    this._NameAddresses[i, j] = nameTableChunk.ItemPointers[j];
                }
            }

            var objectTable = this.Runtime.ReadStructure<UnrealNatives.ObjectTable>(configuration.GlobalObjectTableAddress);
            var objectChunkPointers = this.ReadStaticPointerArray(objectTable.ChunkPointers, objectTable.ChunkCount);
            var objectAddresses = new List<IntPtr>();
            //this._ObjectAddresses = new IntPtr[objectTable.ChunkCount, UnrealNatives.ObjectTable.ItemsPerChunk];
            for (int i = 0; i < objectTable.ChunkCount; i++)
            {
                var objectTableChunk = this.Runtime.ReadStructure<UnrealNatives.ObjectTableChunk>(objectChunkPointers[i]);
                /*
                 * for (int j = 0; j < UnrealNatives.NameTable.ItemsPerChunk; j++)
                {
                    this._ObjectAddresses[i, j] = objectTableChunk.ItemPointers[j].ObjectPointer;
                }
                */
                objectAddresses.AddRange(objectTableChunk.Items.Where(it => it.ObjectPointer != IntPtr.Zero).Select(it => it.ObjectPointer));
            }

            var classDefaultObjectPointers = new List<IntPtr>();
            foreach (var objectAddress in objectAddresses)
            {
                var objectClassPointer = this.ReadPointer(objectAddress + this.Offsets.CoreObjectClass);
                var classDefaultObjectPointer = this.ReadPointer(objectClassPointer + this.Offsets.CoreClassClassDefaultObject);
                if (classDefaultObjectPointer != IntPtr.Zero)
                {
                    classDefaultObjectPointers.Add(classDefaultObjectPointer);
                }
            }

            this._ObjectAddresses = objectAddresses.Concat(classDefaultObjectPointers).Distinct().ToArray();

            this._ObjectShims = new Dictionary<IntPtr, UnrealObjectShim>();
            foreach (var objectAddress in this._ObjectAddresses)
            {
                var objectName = this.ReadName(objectAddress + this.Offsets.CoreObjectName);
                var objectPath = this.ReadPath(objectAddress);
                var objectClassPointer = this.ReadPointer(objectAddress + this.Offsets.CoreObjectClass);
                var objectClass = this.ReadClass(objectClassPointer);
                var objectShim = new UnrealObjectShim(this, objectAddress, objectClass, objectName, objectPath);
                this._ObjectShims.Add(objectAddress, objectShim);
            }
        }

        public IEnumerable<UnrealObject> Objects
        {
            get { return this._ObjectShims.Values.Select(s => s.Object); }
        }

        public IEnumerable<UnrealClass> Classes
        {
            get { return this._CachedClasses.Values; }
        }

        public UnrealObject GetObject(IntPtr address)
        {
            return this._ObjectShims.TryGetValue(address, out var result) == true
                ? result.Object
                : null;
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

        internal string ReadNameEntry(UnrealNatives.Name name)
        {
            var sb = new StringBuilder();

            string value;
            if (this._CachedNames.TryGetValue(name.Id, out value) == false)
            {
                var dataAddress = this._NameAddresses[name.Id / UnrealNatives.NameTable.ItemsPerChunk, name.Id % UnrealNatives.NameTable.ItemsPerChunk];
                var indexAddress = dataAddress + this.Configuration.NameEntryIndexOffset;
                var index = this.Runtime.ReadValueS32(indexAddress);
                var encoding = (index & 1) == 0
                    ? Encoding.ASCII
                    : Encoding.Unicode;
                var stringAddress = dataAddress + this.Configuration.NameEntryStringOffset;
                value = this.Runtime.ReadStringZ(stringAddress, encoding);
                this._CachedNames.Add(name.Id, value);
            }
            sb.Append(value);

            if (name.Index != 0)
            {
                sb.Append('_');
                sb.Append((name.Index - 1).ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        internal string ReadName(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                throw new ArgumentNullException("address");
            }

            return this.ReadNameEntry(this.Runtime.ReadStructure<UnrealNatives.Name>(address));
        }

        internal string ReadPath(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                throw new ArgumentNullException("address");
            }

            string path;
            if (this._CachedPaths.TryGetValue(address, out path) == true)
            {
                return path;
            }

            path = this.ReadName(address + this.Offsets.CoreObjectName);
            var outer = this.ReadPointer(address + this.Offsets.CoreObjectOuter);
            while (outer != IntPtr.Zero && outer != address)
            {
                var name = this.ReadName(outer + this.Offsets.CoreObjectName);
                path = name + "." + path;
                outer = this.ReadPointer(outer + this.Offsets.CoreObjectOuter);
            }
            this._CachedPaths.Add(address, path);
            return path;
        }

        private UnrealClass ReadClass(IntPtr address)
        {
            UnrealClass instance;
            if (this._CachedClasses.TryGetValue(address, out instance) == true)
            {
                return instance;
            }

            instance = new UnrealClass();
            this._CachedClasses.Add(address, instance);

            instance.Address = address;
            instance.VfTableObject = this.ReadPointer(address);
            instance.Name = this.ReadName(address + this.Offsets.CoreObjectName);
            instance.Path = this.ReadPath(address);

            var superAddress = this.ReadPointer(address + this.Offsets.CoreStructSuperStruct);
            if (superAddress != IntPtr.Zero && superAddress != address)
            {
                instance.Super = this.ReadClass(superAddress);
            }

            var classAddress = this.ReadPointer(address + this.Offsets.CoreObjectClass);
            if (classAddress != IntPtr.Zero && classAddress != address)
            {
                instance.Class = this.ReadClass(classAddress);
            }

            var fieldAddress = this.ReadPointer(address + this.Offsets.CoreStructChildren);
            var fields = new List<UnrealField>();
            while (fieldAddress != IntPtr.Zero)
            {
                fields.Add(this.ReadField(fieldAddress));
                fieldAddress = this.ReadPointer(fieldAddress + this.Offsets.CoreFieldNext);
            }
            instance.Fields = fields.ToArray();

            var classDefaultObjectAddress = this.ReadPointer(address + this.Offsets.CoreClassClassDefaultObject);
            instance.ClassDefaultObject = classDefaultObjectAddress;

            return instance;
        }

        private UnrealField ReadField(IntPtr address)
        {
            UnrealField instance;
            if (this._CachedFields.TryGetValue(address, out instance) == true)
            {
                return instance;
            }

            UnrealClass klass = null;
            var classAddress = this.ReadPointer(address + this.Offsets.CoreObjectClass);
            if (classAddress != IntPtr.Zero && classAddress != address)
            {
                klass = this.ReadClass(classAddress);
            }

            if (this._CachedFields.TryGetValue(address, out instance) == true)
            {
                return instance;
            }

            var name = this.ReadName(address + this.Offsets.CoreObjectName);

            switch (klass.Path)
            {
                case "/Script/CoreUObject.Enum":
                {
                    var enumField = new Fields.EnumField();
                    instance = enumField;
                    this._CachedFields.Add(address, instance);
                    var enumTypeNameAddress = address + this.Offsets.CoreEnumTypeName;
                    var enumValueNamesAddress = address + this.Offsets.CoreEnumValueNames;
                    enumField.TypeName = this.ReadString(enumTypeNameAddress);
                    enumField.ValueNames = this.ReadNameValuePairArray(enumValueNamesAddress);
                    break;
                }

                case "/Script/CoreUObject.ClassProperty":
                {
                    instance = new Fields.ClassProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.ObjectProperty":
                {
                    var objectField = new Fields.ObjectProperty();
                    instance = objectField;
                    this._CachedFields.Add(address, instance);
                    var propertyClassAddress = this.ReadPointer(address + this.Offsets.CoreObjectPropertyPropertyClass);
                    objectField.PropertyClass = this.ReadClass(propertyClassAddress);
                    break;
                }

                case "/Script/CoreUObject.StructProperty":
                {
                    var structField = new Fields.StructProperty();
                    instance = structField;
                    this._CachedFields.Add(address, instance);
                    var structAddress = this.ReadPointer(address + this.Offsets.CoreStructPropertyStruct);
                    structField.Structure = this.ReadClass(structAddress);
                    break;
                }

                case "/Script/CoreUObject.ComponentProperty":
                {
                    instance = new Fields.ComponentProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.ArrayProperty":
                {
                    var arrayField = new Fields.ArrayProperty();
                    instance = arrayField;
                    this._CachedFields.Add(address, instance);
                    var innerAddress = this.ReadPointer(address + this.Offsets.CoreArrayPropertyInner);
                    arrayField.Inner = this.ReadField(innerAddress);
                    break;
                }

                case "/Script/CoreUObject.EnumProperty":
                {
                    var enumField = new Fields.EnumProperty();
                    instance = enumField;
                    this._CachedFields.Add(address, instance);
                    var underlyingPropertyAddress = this.ReadPointer(address + this.Offsets.CoreEnumPropertyUnderlyingProperty);
                    var enumAddress = this.ReadPointer(address + this.Offsets.CoreEnumPropertyEnum);
                    //enumField.UnderlyingProperty = this.ReadClass(underlyingPropertyAddress);
                    enumField.Enum = this.ReadField(enumAddress);
                    break;
                }

                case "/Script/CoreUObject.BoolProperty":
                {
                    var boolField = new Fields.BoolProperty();
                    instance = boolField;
                    this._CachedFields.Add(address, instance);
                    var fieldSizeAddress = address + this.Offsets.CoreBoolPropertyFieldSize;
                    var byteOffsetAddress = address + this.Offsets.CoreBoolPropertyByteOffset;
                    var byteMaskAddress = address + this.Offsets.CoreBoolPropertyByteMask;
                    var fieldMaskAddress = address + this.Offsets.CoreBoolPropertyFieldMask;
                    boolField.FieldSize = this.Runtime.ReadValueU8(fieldSizeAddress);
                    boolField.ByteOffset = this.Runtime.ReadValueU8(byteOffsetAddress);
                    boolField.ByteMask = this.Runtime.ReadValueU8(byteMaskAddress);
                    boolField.FieldMask = this.Runtime.ReadValueU8(fieldMaskAddress);
                    break;
                }

                case "/Script/CoreUObject.ByteProperty":
                {
                    instance = new Fields.ByteProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.Int8Property":
                {
                    instance = new Fields.Int8Property();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.UInt16Property":
                {
                    instance = new Fields.UInt16Property();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.Int16Property":
                {
                    instance = new Fields.Int16Property();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.UInt32Property":
                {
                    instance = new Fields.UInt32Property();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.IntProperty":
                {
                    instance = new Fields.IntProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.UInt64Property":
                {
                    instance = new Fields.UInt64Property();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.Int64Property":
                {
                    instance = new Fields.Int64Property();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.FloatProperty":
                {
                    instance = new Fields.FloatProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.DoubleProperty":
                {
                    instance = new Fields.DoubleProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.StrProperty":
                {
                    instance = new Fields.StringProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.NameProperty":
                {
                    instance = new Fields.NameProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.DelegateProperty":
                case "/Script/CoreUObject.InterfaceProperty":
                case "/Script/CoreUObject.LazyObjectProperty":
                case "/Script/CoreUObject.MapProperty":
                case "/Script/CoreUObject.MulticastDelegateProperty":
                case "/Script/CoreUObject.TextProperty":
                case "/Script/CoreUObject.SetProperty":
                case "/Script/CoreUObject.SoftClassProperty":
                case "/Script/CoreUObject.SoftObjectProperty":
                case "/Script/CoreUObject.WeakObjectProperty":
                {
                    instance = new Fields.DummyProperty();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                case "/Script/CoreUObject.Function":
                case "/Script/CoreUObject.DelegateFunction":
                {
                    instance = new Fields.DummyField();
                    this._CachedFields.Add(address, instance);
                    break;
                }

                default:
                {
                    throw new NotImplementedException();
                }
            }

            var arrayCountAddress = address + this.Offsets.CorePropertyArrayCount;
            var sizeAddress = address + this.Offsets.CorePropertySize;
            var offsetAddress = address + this.Offsets.CorePropertyOffset;

            instance.Address = address;
            instance.VfTableObject = this.ReadPointer(address);
            instance.Name = name;
            instance.Class = klass;

            if (instance is UnrealProperty property)
            {
                property.ArrayCount = this.Runtime.ReadValueS32(arrayCountAddress);
                property.Size = this.Runtime.ReadValueS32(sizeAddress);
                property.Offset = this.Runtime.ReadValueS32(offsetAddress);
            }

            return instance;
        }

        internal IntPtr ReadPointer(IntPtr address)
        {
            return this.Runtime.ReadStructure<UnrealNatives.Pointer>(address).Value;
        }

        internal void WritePointer(IntPtr address, IntPtr value)
        {
            this.Runtime.WriteStructure(address, new UnrealNatives.Pointer(value));
        }

        internal IntPtr[] ReadStaticPointerArray(IntPtr address, int count)
        {
            return this.ReadStructureStaticArray<UnrealNatives.Pointer>(address, count)
                .Select(p => p.Value)
                .ToArray();
        }

        internal IntPtr[] ReadPointerArray(IntPtr address)
        {
            return this.ReadStructureArray<UnrealNatives.Pointer>(address)
                .Select(p => p.Value)
                .ToArray();
        }

        internal void WritePointerArray(IntPtr address, IntPtr[] items)
        {
            this.WriteStructureArray(
                address,
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

            var structureSize = Marshal.SizeOf(typeof(UnrealNatives.String));
            var buffer = this.Runtime.ReadBytes(array.Data, array.Count * structureSize);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            var items = new string[array.Count];
            for (var o = 0; o < array.Count; o++)
            {
                var ustring = (UnrealNatives.String)Marshal.PtrToStructure(current, typeof(UnrealNatives.String));
                items[o] = this.ReadString(ustring);
                current += structureSize;
            }
            handle.Free();
            return items;
        }

        internal string[] ReadNameArray(IntPtr address)
        {
            var array = this.Runtime.ReadStructure<UnrealNatives.Array>(address);
            if (array.Data == IntPtr.Zero)
            {
                return new string[0];
            }

            var structureSize = Marshal.SizeOf(typeof(UnrealNatives.Name));
            var buffer = this.Runtime.ReadBytes(array.Data, array.Count * structureSize);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            var items = new string[array.Count];
            for (var o = 0; o < array.Count; o++)
            {
                var uname = (UnrealNatives.Name)Marshal.PtrToStructure(current, typeof(UnrealNatives.Name));
                items[o] = this.ReadNameEntry(uname);
                current += structureSize;
            }
            handle.Free();
            return items;
        }

        internal TStructure[] ReadStructureStaticArray<TStructure>(IntPtr address, int count)
            where TStructure : struct
        {
            var structureSize = Marshal.SizeOf(typeof(TStructure));
            var buffer = this.Runtime.ReadBytes(address, count * structureSize);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            var items = new TStructure[count];
            for (var o = 0; o < count; o++)
            {
                items[o] = (TStructure)Marshal.PtrToStructure(current, typeof(TStructure));
                current += structureSize;
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
            if (array.Data == IntPtr.Zero || array.Count < items.Length)
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

        internal KeyValuePair<string, long>[] ReadNameValuePairArray(IntPtr address)
        {
            var array = this.Runtime.ReadStructure<UnrealNatives.Array>(address);
            if (array.Data == IntPtr.Zero)
            {
                return new KeyValuePair<string, long>[0];
            }

            var structureSize = Marshal.SizeOf(typeof(UnrealNatives.NameValuePair));
            var buffer = this.Runtime.ReadBytes(array.Data, array.Count * structureSize);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var current = handle.AddrOfPinnedObject();
            var items = new KeyValuePair<string, long>[array.Count];
            for (var o = 0; o < array.Count; o++)
            {
                var pair = (UnrealNatives.NameValuePair)Marshal.PtrToStructure(current, typeof(UnrealNatives.NameValuePair));
                items[o] = new KeyValuePair<string, long>(this.ReadNameEntry(pair.Name), pair.Value);
                current += structureSize;
            }
            handle.Free();
            return items;
        }
    }
}
