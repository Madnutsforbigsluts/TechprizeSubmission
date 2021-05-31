using System;
namespace Glyph.Utilities
{
    public class InitializeArray
    {
        public static T[] PopulateArray<T>(int size) where T : new()
        {
            T[] array = new T[size];
            for (int i = 0; i < size; i++)
            {
                array[i] = new T();
            }
            return array;
        }
    }
}
