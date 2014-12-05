﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMQ.zmq.Patterns.Utils
{
    static class ArrayExtensions
    {
        public static T[] Resize<T>(this T[] src, int size, bool ended)
        {
            T[] dest;

            if (size > src.Length)
            {
                dest = new T[size];
                if (ended)
                    Array.Copy(src, 0, dest, 0, src.Length);
                else
                    Array.Copy(src, 0, dest, size - src.Length, src.Length);
            }
            else if (size < src.Length)
            {
                dest = new T[size];
                if (ended)
                    Array.Copy(src, 0, dest, 0, size);
                else
                    Array.Copy(src, src.Length - size, dest, 0, size);

            }
            else
            {
                dest = src;
            }
            return dest;
        }

        public static void Swap<T>(this List<T> items, int index1, int index2) where T : class
        {
            if (index1 == index2)
                return;

            T item1 = items[index1];
            T item2 = items[index2];
            if (item1 != null)
                items[index2] = item1;
            if (item2 != null)
                items[index1] = item2;
        }
    }
}
