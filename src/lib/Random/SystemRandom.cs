namespace Piot.Random
{
    public class SystemRandom : IRandom
    {
        private readonly System.Random rand = new();

        public int Random(int max)
        {
            return rand.Next(max);
        }
    }
}