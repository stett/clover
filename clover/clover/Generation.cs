using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clover
{
    class Generation
    {
        public List<Genome> individuals = new List<Genome>();
        public int fittest_individual = 0;
        public float total_fitness = 0.0f;

        public static Generation Rand(int genes)
        {
            Generation generation = new Generation();
            for (int i = 0; i < Evolver.POOL_SIZE; i++) generation.individuals.Add(Genome.Rand(genes));
            return generation;
        }

        public Genome get_fittest_individual()
        {
            return individuals[fittest_individual];
        }
    }
}
