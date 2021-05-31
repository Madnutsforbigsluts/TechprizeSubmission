using System;
using System.Collections.Generic;

namespace Glyph.Utilities
{
    public static class ArraySwap
    {
        public static void Swap<T>(IList<T> list, int a, int b)
        {
            T temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }
    }
}
