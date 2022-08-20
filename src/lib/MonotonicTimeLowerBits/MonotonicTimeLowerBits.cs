namespace Piot.Surge.MonotonicTimeLowerBits
{
    /// <summary>
    /// The 16 lower bits of a monotonic <see cref="MonotonicTime.Milliseconds"/>.
    /// </summary>
    public struct MonotonicTimeLowerBits
    {
        public ushort lowerBits;

        public MonotonicTimeLowerBits(ushort lowerBits)
        {
            this.lowerBits = lowerBits;
        }
    }
}