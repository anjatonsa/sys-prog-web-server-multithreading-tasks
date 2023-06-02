using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DrugiProjekat
{
    internal class Cache
    {
        static object locker = new object();
        static Dictionary<string, byte[]> ImageCache;
        static LinkedList<string> fifo = new LinkedList<string>();
        private int capcity = 2;

        public Cache()
        {
            ImageCache = new Dictionary<string, byte[]>();
        }

        public void AddImageToCache(string imageName, byte[] buf)
        {
            lock (locker)
            {
                if (fifo.Count == capcity)
                {
                    DeleteFromCache(fifo.Last.Value);
                }
                ImageCache.Add(imageName, buf);
                fifo.AddFirst(imageName);
            }
        }

        public bool GetImageFromCache(string imageName, out byte[] buf)
        {
            bool hit = false;
            lock (locker)
            {
                hit = ImageCache.TryGetValue(imageName, out buf);

            }
            return hit;
        }

        private void DeleteFromCache(string imageName)
        {
            lock (locker)
            {
                ImageCache.Remove(imageName);
            }
        }
    }
}
