
namespace TeleBufet.NET.Server
{
    internal sealed class IdentificatorGenerator
    {
        private static readonly int maxBitsValue = 255;
        private static readonly byte bitsLength = 8;

        public uint IndentificationHolder { get; private set; } = 0;

        public int GenerateId(byte shiftNumber) 
        {
            int shifter = (shiftNumber * bitsLength);
            int maxShiftValue = maxBitsValue << shifter;

            int lastId = (int)((IndentificationHolder & (uint)maxShiftValue) >> shifter);
            uint index = (uint)((1) << shifter);

            int intHolder = (int)IndentificationHolder;
            IndentificationHolder = unchecked((uint)(Interlocked.Add(ref intHolder, (int)index)));

            return lastId + 1;
        }
    }
}
