namespace NativeMemory.Helpers
{
    public static unsafe class NativeMemoryExtensions
    {
        // TODO: Wait for next SDK version, possibly.
        // See: https://github.com/dotnet/roslyn/pull/78758

        // extension<MemoryT, MemoryItemT>(MemoryT source)
        //     where MemoryT: INativeMemory<MemoryItemT>
        //     where MemoryItemT: unmanaged
        // {
        //     public static void Yes()
        //     {
        //
        //     }
        // }

        // extension<MemoryT>(MemoryT)
        //     where MemoryT: INativeMemory<byte>
        // {
        //     public static implicit operator MemoryWindow<byte>(MemoryT memory)
        //     {
        //         return memory.Window;
        //     }
        // }
    }
}