﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace KelpNet.CL.Common
{
    public class ComputeBuffer<T> : ComputeObject, IDisposable where T : unmanaged
    {
        public long Count { get; private set; }

        public long Size { get; protected set; }

        protected unsafe void Init()
        {
            Size = (long)GetInfo<IntPtr, ComputeMemoryInfo, IntPtr>(handle, ComputeMemoryInfo.Size, CL10.GetMemObjectInfo);
            Count = Size / sizeof(T);

#if DEBUG
            Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
#endif
        }

        public void Dispose()
        {
#if DEBUG
            Trace.WriteLine("Dispose " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
#endif
            CL10.ReleaseMemObject(handle);
        }

        public ComputeBuffer(ComputeContext context, ComputeMemoryFlags flags, long count) : this(context, flags, count, IntPtr.Zero) { }

        public unsafe ComputeBuffer(ComputeContext context, ComputeMemoryFlags flags, long count, IntPtr dataPtr)
        {
            handle = CL10.CreateBuffer(context.handle, flags, new IntPtr(sizeof(T) * count), dataPtr, out _);
            Init();
        }

        public unsafe ComputeBuffer(ComputeContext context, ComputeMemoryFlags flags, T[] data)
        {
            GCHandle dataPtr = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                handle = CL10.CreateBuffer(context.handle, flags, new IntPtr(sizeof(T) * data.Length), dataPtr.AddrOfPinnedObject(), out _);
            }
            finally
            {
                dataPtr.Free();
            }

            Init();
        }
    }
}