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
using System.Runtime.InteropServices;

namespace Gibbed.Unreflect.Core
{
    internal static class UnrealNatives
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Array
        {
            public IntPtr Data;
            public int Count;
            public int Allocated;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Pointer
        {
            public IntPtr Value;

            public Pointer(IntPtr value)
            {
                this.Value = value;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct String
        {
            public IntPtr Data;
            public int Length;
            public int Allocated;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Name
        {
            public int Id;
            public uint Index;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NameValuePair
        {
            public Name Name;
            public long Value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NameTable
        {
            public const int MaxChunkCount = 0x80;
            public const int ItemsPerChunk = 0x4000;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxChunkCount)]
            public IntPtr[] ChunkPointers;
            public int ItemCount;
            public int ChunkCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NameTableChunk
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NameTable.ItemsPerChunk)]
            public IntPtr[] ItemPointers;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjectTable
        {
            public const int ItemsPerChunk = 0x10000;

            public IntPtr ChunkPointers;
            public IntPtr Unknown;
            public int MaxItemCount;
            public int ItemCount;
            public int MaxChunkCount;
            public int ChunkCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjectTableItem
        {
            public IntPtr ObjectPointer;
            public int Unknown1;
            public int Unknown2;
            public int Unknown3;
            public int Unknown4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjectTableChunk
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = ObjectTable.ItemsPerChunk)]
            public ObjectTableItem[] Items;
        }
    }
}
