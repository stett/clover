using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace clover
{
    class Gene
    {
        public Vector2 position;
        public Vector2 size;
        public float angle;
        //public float azimuth;
        //public float pitch;
        //public float roll;

        public static Gene Rand()
        {
            Gene gene = new Gene();
            gene.position = new Vector2(Evolver.REFERENCE_SIZE.X * Utils.rand(), Evolver.REFERENCE_SIZE.Y * Utils.rand());
            gene.size = new Vector2(Evolver.TEXTURE_SIZE.X * Utils.rand(), Evolver.TEXTURE_SIZE.Y * Utils.rand());
            gene.angle = Utils.rand((float)Math.PI);
            //gene.azimuth = Utils.rand((float)Math.PI);
            //gene.pitch = Utils.rand((float)Math.PI);
            //gene.roll = Utils.rand((float)Math.PI);
            return gene;
        }
    }
}