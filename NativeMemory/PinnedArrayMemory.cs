using System.Runtime.InteropServices;
using NoParamlessCtor.Shared.Attributes;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct PinnedArrayMemory<T>:
        IEquatable<PinnedArrayMemory<T>>
        where T: unmanaged
    {
        public readonly MemoryWindow<T> Window;

        // POH arrays may still be collected by the GC, so keep a reference.
        public readonly T[] PinnedArr;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public PinnedArrayMemory(nuint length, bool zeroed = true)
        {
            var intLength = unchecked((int) length);

            var pinnedArr = PinnedArr = !zeroed ?
                GC.AllocateArray<T>(intLength, pinned: true) :
                GC.AllocateUninitializedArray<T>(intLength, pinned: true);

            var ptr = (T*) Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(pinnedArr));

            Window = new(ptr, length);
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