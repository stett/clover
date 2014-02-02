using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clover
{
    class Genome
    {
        public List<Gene> genes = new List<Gene>();
        public float fitness = 0.0f;
        
        public static Genome Rand(int genes)
        {
            Genome genome = new Genome();
            for (int i = 0; i < genes; i++) genome.genes.Add(Gene.Rand());
            return genome;
        }
    }
}
