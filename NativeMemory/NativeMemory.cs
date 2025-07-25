﻿using System.Runtime.InteropServices;
using NoParamlessCtor.Shared.Attributes;
using NM = System.Runtime.InteropServices.NativeMemory;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct NativeMemory<T>:
        INativeMemory<T>,
        IDisposable
        where T: unmanaged
    {
        #if DEBUG
        // Ptr to sizeof(T)
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<nint, nuint> LIVE_ALLOCATIONS = new();

        public static int LiveAllocationsCount => LIVE_ALLOCATIONS.Count;
        #endif

        public readonly MemoryWindow<T> Window;

        MemoryWindow<T> INativeMemory<T>.Window => Window;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NativeMemory(nuint length, bool zeroed = false, nuint alignment = 0)
        {
            var sizeOfT = (nuint) sizeof(T);

            T* ptr;

            if (alignment != 0)
            {
                var byteCount = length * sizeOfT;

                ptr = (T*) NM.AlignedAlloc(byteCount, alignment);
            }

            else
            {
                if (!zeroed)
                {
                    ptr = (T*) NM.Alloc(length, sizeOfT);
                }

                else
                {
                    ptr = (T*) NM.AllocZeroed(length, sizeOfT);
                }
            }

            Window = new(ptr, length);

            #if DEBUG
            System.Diagnostics.Debug.Assert(LIVE_ALLOCATIONS.TryAdd((nint) ptr, sizeOfT));
            #endif
        }

        // Wrap existing memory
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private NativeMemory(MemoryWindow<T> window)
        {
            Window = window;
        }

        // This is unsafe, as it is possible for it to wrap any memory window,
        // even if not allocated via NativeMemory.XXXX APIss
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeMemory<T> WrapBufferUnsafely(MemoryWindow<T> window)
        {
            return new(window);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FreeWithPtrUnsafely(T* ptr)
        {
            #if DEBUG
            // ReSharper disable once UnusedVariable
            // We keep the size variable even though it is useless,
            // so that we can view the size via the debugger!
            System.Diagnostics.Debug.Assert(LIVE_ALLOCATIONS.TryRemove((nint) ptr, out var sizeOfT));
            #endif

            NM.Free(ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            FreeWithPtrUnsafely(Window.Ptr);
        }
    }
}