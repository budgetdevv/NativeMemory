using System.Runtime.InteropServices;
using NoParamlessCtor.Shared.Attributes;
using NM = System.Runtime.InteropServices.NativeMemory;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct NativeMemory<T>:
        IDisposable,
        IEquatable<NativeMemory<T>>
        where T: unmanaged
    {
        #if DEBUG
        // Ptr to size
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<nint, nuint> LIVE_ALLOCATIONS = new();

        public static int LiveAllocationsCount => LIVE_ALLOCATIONS.Count;
        #endif

        public readonly MemoryWindow<T> Window;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(NativeMemory<T> other)
        {
            var window = Window;

            var otherWindow = other.Window;

            return window == otherWindow;
        }

        public override bool Equals(object? obj)
        {
            return obj is NativeMemory<T> other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Window.GetHashCode();
        }

        public static bool operator ==(NativeMemory<T> left, NativeMemory<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NativeMemory<T> left, NativeMemory<T> right)
        {
            return !left.Equals(right);
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

        public void Dispose()
        {
            FreeWithPtrUnsafely(Window.Ptr);
        }
    }
}