using System.Runtime.InteropServices;

namespace NativeMemory.Helpers
{
    internal static unsafe class AllocationHelpers
    {
        public static T[] AllocatePinnedArray<T>(int length, bool zeroed = false)
            where T: unmanaged
        {
            return !zeroed ?
                GC.AllocateArray<T>(length, pinned: true) :
                GC.AllocateUninitializedArray<T>(length, pinned: true);
        }

        // Note that the array reference itself may not point to an aligned address.
        public static T[] AllocatePinnedArray<T>(
            int length,
            out T* alignedPtr,
            bool zeroed = false,
            int alignment = 0)
            where T: unmanaged
        {
            var hasAlignment = alignment != 0;

            var extraAllocs = hasAlignment ?
                (alignment + sizeof(T) - 1) / sizeof(T) :
                0;

            if (alignment % sizeof(T) != 0)
            {
                throw new Exception("Invalid alignment!");
            }

            length += extraAllocs;

            var arr = AllocatePinnedArray<T>(length, zeroed);

            var addr = (nint) arr.PinnedArrayToPointer();

            var offset = hasAlignment ?
                addr % alignment :
                0;

            if (offset != 0)
            {
                addr += (alignment - offset);
            }

            if (addr % alignment != 0)
            {
                throw new Exception("Failed to align the address!");
            }

            alignedPtr = (T*) addr;

            return arr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* PinnedArrayToPointer<T>(this T[] arr) where T: unmanaged
        {
            return (T*) Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(arr));
        }
    }
}