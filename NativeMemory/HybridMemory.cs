using System.Runtime.InteropServices;
using NoParamlessCtor.Shared.Attributes;

namespace NativeMemory
{
    [NoParamlessCtor]
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe partial struct HybridMemory<T>:
        INativeMemory<T>,
        IDisposable
        where T: unmanaged
    {
        public readonly MemoryWindow<T> Window;

        MemoryWindow<T> INativeMemory<T>.Window => Window;

        private readonly bool UsesExistingWindow;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HybridMemory(
            MemoryWindow<T> existingWindow,
            nuint requiredLength,
            bool requiredZeroing = false,
            nuint requiredAlignment = 0)
        {
            var usesExistingWindow = existingWindow.Length >= requiredLength;

            #if DEBUG
            System.Diagnostics.Debug.Assert(
                (nuint) existingWindow.Ptr % requiredAlignment == 0,
                "Existing memory window does not meet the required alignment."
            );
            #endif

            if (usesExistingWindow)
            {
                Window = existingWindow;
            }

            else
            {
                Window = new NativeMemory<T>(
                    requiredLength,
                    zeroed: requiredZeroing,
                    alignment: requiredAlignment
                ).Window;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (!UsesExistingWindow)
            {
                // If we did not use an existing window, we need to free the memory
                NativeMemory<T>.FreeWithPtrUnsafely(Window.Ptr);
            }
        }
    }
}