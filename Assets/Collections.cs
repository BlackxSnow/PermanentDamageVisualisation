using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
    public static class Collections
    {
        public static T[] DequeueMany<T>(this Queue<T> queue, int quantity)
        {
            T[] result = new T[quantity];
            for(int i = 0; i < quantity; i++)
            {
                result[i] = queue.Dequeue();
            }
            return result;
        }
        public static T[] TryDequeueMany<T>(this Queue<T> queue, int quantity)
        {
            quantity = quantity > queue.Count ? queue.Count : quantity;
            T[] result = new T[quantity];
            for (int i = 0; i < quantity; i++)
            {
                result[i] = queue.Dequeue();
            }
            return result;
        }
    }
}
