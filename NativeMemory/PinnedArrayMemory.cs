using System.Runtime.InteropServices;
using NativeMemory.Helpers;
using NoParamlessCtor.Shared.Attributes;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct PinnedArrayMemory<T>:
        INativeMemory<T>,
        IEquatable<PinnedArrayMemory<T>>
        where T: unmanaged
    {
        public readonly MemoryWindow<T> Window;

        MemoryWindow<T> INativeMemory<T>.Window => Window;

        // POH arrays may still be collected by the GC, so keep a reference.
        public readonly T[] PinnedArr;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public PinnedArrayMemory(int length, bool zeroed = true, int alignment = 0)
        {
            PinnedArr = AllocationHelpers.AllocatePinnedArray<T>(
                length,
                out var ptr,
                zeroed: zeroed,
                alignment: alignment
            );

            Window = new(ptr, length.ToNuintUnchecked());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PinnedArrayMemory<T> other)
        {
            return PinnedArr == other.PinnedArr;
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

        public static bool operator ==(PinnedArrayMemory<T> left, PinnedArrayMemory<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PinnedArrayMemory<T> left, PinnedArrayMemory<T> right)
        {
            return !left.Equals(right);
        }
    }
}