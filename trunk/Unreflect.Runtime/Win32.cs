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
using System.Runtime.InteropServices;

namespace Unreflect.Runtime
{
    internal static class Win32
    {
        public static readonly IntPtr InvalidHandleValue = (IntPtr)(-1);

        public static uint Synchronize = 0x00100000u;
        public static uint StandardRightsRequired = 0x000F0000u;
        public static uint ProcessAllAccess = StandardRightsRequired | Synchronize | 0xFFFu;
        public static uint ThreadAllAccess = StandardRightsRequired | Synchronize | 0x3FFu;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint desiredAccess,
                                                [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
                                                uint processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr process,
                                                    IntPtr baseAddress,
                                                    IntPtr buffer,
                                                    uint size,
                                                    out uint numberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(IntPtr process,
                                                     IntPtr baseAddress,
                                                     IntPtr buffer,
                                                     uint size,
                                                     out uint numberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(uint desiredAccess,
                                               [MarshalAs(UnmanagedType.Bool)] bool inheritHandle,
                                               uint threadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SuspendThread(IntPtr thread);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint ResumeThread(IntPtr thread);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr handle);
    }
}
