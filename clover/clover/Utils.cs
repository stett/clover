using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clover
{
    static class Utils
    {
        static Random r = new Random();

        public static float rand()
        {
            return ((float)r.Next(int.MaxValue)) / ((float)int.MaxValue);
        }

        public static float rand(float mag)
        {
            return rand() * mag;
        }
    }
}
