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
        public Color color;

        public static Gene Rand(float fitness = 0)
        {
            float fit_scale = 2.0f * (1.2f - fitness);

            Gene gene = new Gene();
            gene.position = new Vector2((Evolver.REFERENCE_SIZE.X - Evolver.TEXTURE_SIZE.X) * Utils.rand(), (Evolver.REFERENCE_SIZE.Y - Evolver.TEXTURE_SIZE.Y) * Utils.rand()) + Evolver.TEXTURE_SIZE * .5f;
            gene.size = new Vector2(Evolver.TEXTURE_SIZE.X * Utils.rand(), Evolver.TEXTURE_SIZE.Y * Utils.rand()) * fit_scale;
            gene.angle = Utils.rand((float)Math.PI);
            gene.color = new Color(new Vector3(Utils.rand())) * Utils.rand();
            return gene;
        }
    }
}