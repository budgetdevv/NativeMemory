namespace NativeMemory
{
    public interface INativeMemory<T>
        where T: unmanaged
    {
        public MemoryWindow<T> Window { get; }
    }
}