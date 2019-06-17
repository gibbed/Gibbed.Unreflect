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
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gibbed.Unreflect.Runtime
{
    public class RuntimeProcess : RuntimeBase
    {
        private static readonly string _ObjectName = "RuntimeProcess";
        private bool _Disposed;
        protected Process Process;
        protected IntPtr Handle = IntPtr.Zero;

        public RuntimeProcess()
            : base(_ObjectName)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (this._Disposed == false)
            {
                if (this.Handle != IntPtr.Zero)
                {
                    this.CloseProcess();
                }

                this._Disposed = true;
            }

            base.Dispose(disposing);
        }

        public bool OpenProcess(Process process)
        {
            if (this._Disposed == true)
            {
                throw new ObjectDisposedException(_ObjectName);
            }

            if (this.Handle != IntPtr.Zero)
            {
                Win32.CloseHandle(this.Handle);
                this.Process = null;
                this.Handle = IntPtr.Zero;
            }

            var handle = Win32.OpenProcess(Win32.ProcessAllAccess, false, (uint)process.Id);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            this.Process = process;
            this.Handle = handle;
            return true;
        }

        public bool CloseProcess()
        {
            if (this._Disposed == true)
            {
                throw new ObjectDisposedException(_ObjectName);
            }

            if (this.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("process handle is invalid");
            }

            var handle = this.Handle;
            this.Process = null;
            this.Handle = IntPtr.Zero;
            return Win32.CloseHandle(handle);
        }

        private bool SuspendThread(int id)
        {
            var handle = Win32.OpenThread(Win32.ThreadAllAccess, false, (uint)id);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            var result = Win32.SuspendThread(handle) == 0xFFFFFFFF;
            Win32.CloseHandle(handle);
            return result;
        }

        private bool ResumeThread(int id)
        {
            var handle = Win32.OpenThread(Win32.ThreadAllAccess, false, (uint)id);
            if (handle == IntPtr.Zero)
            {
                return false;
            }

            var result = Win32.ResumeThread(handle) == 0xFFFFFFFF;
            Win32.CloseHandle(handle);
            return result;
        }

        public bool SuspendThreads()
        {
            bool result = true;
            foreach (var thread in this.Process.Threads.Cast<ProcessThread>())
            {
                if (this.SuspendThread(thread.Id) == false)
                {
                    result = false;
                }
            }
            return result;
        }

        public bool ResumeThreads()
        {
            bool result = true;
            foreach (var thread in this.Process.Threads.Cast<ProcessThread>())
            {
                if (this.ResumeThread(thread.Id) == false)
                {
                    result = false;
                }
            }
            return result;
        }

        public override int Read(IntPtr address, byte[] buffer, int offset, int length)
        {
            if (this._Disposed == true)
            {
                throw new ObjectDisposedException(_ObjectName);
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (length < 0 || offset + length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (this.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("process handle is invalid");
            }

            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            uint size = (uint)length;
            uint read;
            var result = Win32.ReadProcessMemory(
                this.Handle,
                address,
                bufferHandle.AddrOfPinnedObject() + offset,
                size,
                out read);

            bufferHandle.Free();

            if (result == false)
            {
                throw new Win32Exception();
            }

            if (read != size)
            {
                throw new InvalidOperationException(string.Format("only read {0} instead of {1}", read, size));
            }

            return (int)read;
        }

        public override int Write(IntPtr address, byte[] buffer, int offset, int length)
        {
            if (this._Disposed == true)
            {
                throw new ObjectDisposedException(_ObjectName);
            }

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (length < 0 || offset + length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            uint size = (uint)length;
            uint written;
            var result = Win32.WriteProcessMemory(
                this.Handle,
                address,
                bufferHandle.AddrOfPinnedObject() + offset,
                size,
                out written);

            bufferHandle.Free();

            if (result == false)
            {
                throw new Win32Exception();
            }

            if (written != size)
            {
                throw new InvalidOperationException(string.Format("only wrote {0} instead of {1}", written, size));
            }

            return (int)written;
        }
    }
}
