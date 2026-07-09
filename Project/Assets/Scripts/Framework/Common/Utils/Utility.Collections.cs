using System;
using System.Collections.Generic;

namespace Framework.Utils
{
    public static partial class Utility
    {
        public static class Collections
        {
            public static void BubbleSortList<T>(List<T> list, Func<T, T, int> compare)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    for (int j = 0; j < list.Count - i - 1; j++)
                    {
                        if (compare(list[j], list[j + 1]) > 0)
                        {
                            T temp = list[j];
                            list[j] = list[j + 1];
                            list[j + 1] = temp;
                        }
                    }
                }
            }

            public static void QuickSort<T>(List<T> array, Func<T, T, int> compare)
            {
                QuickSort(array, 0, array.Count - 1, compare);
            }

            private static int Partition<T>(List<T> array, int left, int right, Func<T, T, int> compare)
            {
                T pivot = array[right];
                int partitionIndex = left;

                for (int i = left; i < right; i++)
                {
                    if (compare(array[i], pivot) > 0)
                    {
                        Swap(array, i, partitionIndex);
                        partitionIndex++;
                    }
                }

                Swap(array, partitionIndex, right);
                return partitionIndex;
            }

            private static void QuickSort<T>(List<T> array, int left, int right, Func<T, T, int> compare)
            {
                if (left < right)
                {
                    int partitionIndex = Partition(array, left, right, compare);
                    QuickSort(array, left, partitionIndex - 1, compare);
                    QuickSort(array, partitionIndex + 1, right, compare);
                }
            }

            private static void Swap<T>(List<T> array, int i, int j)
            {
                T temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }
}