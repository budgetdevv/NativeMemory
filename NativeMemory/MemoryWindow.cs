using System.Runtime.InteropServices;
using NativeMemory.Helpers;
using NoParamlessCtor.Shared.Attributes;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct MemoryWindow<T>(T* ptr, nuint length): IEquatable<MemoryWindow<T>>
        where T: unmanaged
    {
        public readonly T* Ptr = ptr;

        public readonly nuint Length = length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow(T[] pinnedBuffer, nuint length)
            : this(ref MemoryMarshal.GetArrayDataReference(pinnedBuffer), length) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow(Span<T> pinnedSpan)
            : this(ref MemoryMarshal.GetReference(pinnedSpan), (nuint) pinnedSpan.Length) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow(ref T pinnedStart, nuint length)
            : this((T*) Unsafe.AsPointer(ref pinnedStart), length) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan()
        {
            return MemoryMarshal.CreateSpan(ref *Ptr, (int) Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return MemoryMarshal.CreateReadOnlySpan(ref *Ptr, (int) Length);
        }

        public bool Equals(MemoryWindow<T> other)
        {
            return Ptr == other.Ptr && Length == other.Length;
        }

        public override bool Equals(object? obj)
        {
            return obj is MemoryWindow<T> other && Equals(other);
        }

        public static bool operator ==(MemoryWindow<T> left, MemoryWindow<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MemoryWindow<T> left, MemoryWindow<T> right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return (int) Ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow<F> Cast<F>() where F: unmanaged
        {
            return new(
                ptr: (F*) Ptr,
                length: UnsafeHelpers.CalculateCastLength<T, F>(Length)
            );
        }

        public struct Enumerator
        {
            private T* CurrentPtr;

            private readonly T* LastPtrOffsetByOne;

            public ref T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ref *CurrentPtr;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(T* ptr, nuint length)
            {
                LastPtrOffsetByOne = ptr + length;
                CurrentPtr = ptr - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                return ++CurrentPtr != LastPtrOffsetByOne;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator()
        {
            return new(Ptr, Length);
        }
    }
}