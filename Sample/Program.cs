using System.Buffers;
using System.Runtime.InteropServices;
using FluentAssertions;
using NativeMemory;

namespace Sample
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var pinnedArr = new PinnedArrayMemory<int>(10, zeroed: false);

            using var nativeMemory = new NativeMemory<int>(10, zeroed: false);

            foreach (var x in pinnedArr.Window)
            {
                Console.WriteLine(x);
            }

            var pinHandle = pinnedArr.AsPinnedMemory().Pin();

            // No GC handle allocated, it is pre-pinned memory.
            GetGCHandle(ref pinHandle).IsAllocated.Should().BeFalse();

            foreach (var x in nativeMemory.Window)
            {
                Console.WriteLine(x);
            }

            return;

            [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_handle")]
            static extern ref GCHandle GetGCHandle(ref MemoryHandle handle);
        }
    }
}