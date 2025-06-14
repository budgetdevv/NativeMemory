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

            foreach (var x in nativeMemory.Window)
            {
                Console.WriteLine(x);
            }
        }
    }
}