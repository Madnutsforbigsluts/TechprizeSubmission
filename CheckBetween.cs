namespace Glyph.Utilities
{
    public static class CheckBetween
    {
        public static bool IsBetween(int a, int b, int n)
        {
            return a < n && n < b;
        }

        public static bool IsBetween(float a, float b, float n)
        {
            return a < n && n < b;
        }

        public static bool IsBetweenLong(long a, long b, long n)
        {
            return a < n && n < b; 
        }
    }
}
