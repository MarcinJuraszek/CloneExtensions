namespace CloneExtensions.Benchmarks
{
    public class SimpleClassBase
    {
        public int BaseInt { get; set; }
    }

    public class SimpleClass : SimpleClassBase
    {
        public int Int { get; set; }
        public uint UInt { get; set; }
        public long Long { get; set; }
        public ulong ULong { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public string String { get; set; }
    }
}
