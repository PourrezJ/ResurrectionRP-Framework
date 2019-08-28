using System;
using System.Collections.Generic;
using System.Text;

namespace ResurrectionRP_Server
{
    public struct MenuColor
    {
        public MenuColor(int r, int g, int b)
        {
            red = r;
            green = g;
            blue = b;
            alpha = 255;
        }

        public MenuColor(int r, int g, int b, int a)
        {
            red = r;
            green = g;
            blue = b;
            alpha = a;
        }

        public int red;
        public int green;
        public int blue;
        public int alpha;
    }
}
