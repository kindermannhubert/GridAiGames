namespace GridAiGames
{
    public static class Utils
    {
        public static void Exchange<T>(ref T a, ref T b)
        {
            var c = a;
            a = b;
            b = c;
        }
    }
}
