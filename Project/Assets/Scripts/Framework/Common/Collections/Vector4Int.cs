using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Collections { 
    public struct Vector4Int
    {
        public int x; 
        public int y; 
        public int z; 
        public int w;
        public Vector4Int(int x, int y, int z, int w)
        {
            this.x = x; 
            this.y = y;
            this.z = z;
            this.w = w;
        }
    }
}
