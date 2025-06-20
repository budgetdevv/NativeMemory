using System.Runtime.InteropServices;
using NativeMemory.Helpers;
using NoParamlessCtor.Shared.Attributes;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Auto)]
    public readonly unsafe partial struct PinnedArrayMemory<T>:
        INativeMemory<T>
        where T: unmanaged
    {
        public readonly MemoryWindow<T> Window;

        MemoryWindow<T> INativeMemory<T>.Window => Window;

        // POH arrays may still be collected by the GC, so keep a reference.
        public readonly T[] PinnedArr;

        public int ArrayLength => PinnedArr.Length;

        public int AlignedLength => unchecked((int) Window.Length);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public PinnedArrayMemory(int length, bool zeroed = true, int alignment = 0)
        {
            PinnedArr = AllocationHelpers.AllocatePinnedArray<T>(
                length,
                out var alignedPtr,
                zeroed: zeroed,
                alignment: alignment
            );

            Window = new(alignedPtr, length.ToNuintUnchecked());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> AsPinnedMemory()
        {
            var alignedLength = AlignedLength;

            var arr = PinnedArr;

            var arrLength = arr.Length;

            var offset = arrLength - alignedLength;

            // We mirror MemoryMarshal.CreateFromPinnedArray() logic here,
            // but elide the safety checks.

            return CreateMemory(
                arr,
                offset | (1 << 31),
                alignedLength
            );

            [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
            static extern Memory<T> CreateMemory(
                object? obj,
                int start,
                int length
            );
        }
    }
}