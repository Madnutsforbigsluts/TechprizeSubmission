namespace Glyph.Utilities
{
    public static class Initialize2DArray
    {
        public static T[,] Populate2DArray<T>(int Cols, int Rows) where T : new()
        {
            T[,] array = new T[Cols, Rows];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    array[i, j] = new T();
                }
            }
            return array;
        }
    }
}
