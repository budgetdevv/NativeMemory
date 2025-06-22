using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using NativeMemory.Helpers;
using NoParamlessCtor.Shared.Attributes;
#if DEBUG
using System.Diagnostics;
#endif

namespace NativeMemory
{
    // Initialization
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct MemoryWindow<T>(T* ptr, nuint length):
        IEquatable<MemoryWindow<T>>
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
    }

    // Slicing
    public readonly unsafe partial struct MemoryWindow<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow<T> SliceWithStart(nuint start)
        {
            var length = Length - start;

            #if DEBUG
            Debug.Assert(
                IsInRange(start, length),
                $"{nameof(start)} {start} and {nameof(length)} {length} are out of range for {nameof(MemoryWindow<>)} of {nameof(Length)} {Length}."
            );
            #endif

            return new MemoryWindow<T>(Ptr + start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow<T> SliceWithLength(nuint length)
        {
            #if DEBUG
            Debug.Assert(
                IsInRange(start: 0, length),
                $"{nameof(length)} {length} is out of range for {nameof(MemoryWindow<>)} of {nameof(Length)} {Length}."
            );
            #endif

            return new MemoryWindow<T>(Ptr, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow<T> Slice(nuint start, nuint length)
        {
            #if DEBUG
            Debug.Assert(
                IsInRange(start, length),
                $"{nameof(start)} {start} and {nameof(length)} {length} are out of range for {nameof(MemoryWindow<>)} of {nameof(Length)} {Length}."
            );
            #endif

            return new MemoryWindow<T>(Ptr + start, length);
        }
    }

    // Conversion
    public readonly unsafe partial struct MemoryWindow<T>
    {
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TensorSpan<T> AsTensorSpan()
        {
            return new TensorSpan<T>(Ptr, unchecked((nint) Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TensorSpan<T> AsTensorSpan(ReadOnlySpan<nint> dimensions)
        {
            return new TensorSpan<T>(Ptr, unchecked((nint) Length), lengths: dimensions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan()
        {
            return new ReadOnlyTensorSpan<T>(Ptr, unchecked((nint) Length));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyTensorSpan<T> AsReadOnlyTensorSpan(ReadOnlySpan<nint> dimensions)
        {
            return new ReadOnlyTensorSpan<T>(Ptr, unchecked((nint) Length), lengths: dimensions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryWindow<F> Cast<F>() where F: unmanaged
        {
            return new(
                ptr: (F*) Ptr,
                length: UnsafeHelpers.CalculateCastLength<T, F>(Length)
            );
        }
    }

    // Equality comparison, enumerator and other utilities
    public readonly unsafe partial struct MemoryWindow<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInRange(nuint start, nuint length)
        {
            var localLength = Length;

            return start < localLength &&
                   length <= localLength - start;
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