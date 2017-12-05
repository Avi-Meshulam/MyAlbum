using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;


namespace MyAlbum.BL
{
    public static class Extensions
    {
        private const int MUST_BE_LESS_THAN = 100000000; // 8 zeros

        public static int GetStableHash(this string s)
        {
            uint hash = 0;

            foreach (byte b in Encoding.Unicode.GetBytes(s))
            {
                hash += b;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            // Final avalanche
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);

            // Truncate result
            return (int)(hash % MUST_BE_LESS_THAN);
        }
    }
}
