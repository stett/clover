using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace clover
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Evolver : Microsoft.Xna.Framework.GameComponent
    {
        /*
         * Constant settings
         */

        public const int POOL_SIZE = 400;
        public const int GENOME_LENGTH = 16;
        public const float GENE_MUTATION_PROB = 0.08f; //.05
        public const float GENOME_MUTATION_PROB = 0.05f; //.001
        public const int POINT_TEST_RESOLUTION = 2;
        public const int GENS_PER_FIXTURE = 5;
        public static Vector2 REFERENCE_SIZE = new Vector2(150, 180) * 2f;
        public static Vector2 TEXTURE_SIZE = new Vector2(50) * 1.5f;
        public static Vector2 FULL_RESOLUTION = new Vector2(2000, 2400);


        /*
         * Types
         */

        public enum Phases { Initialize, RandomizePopulation, GenerateFixture, CalculateFitnesses, MakeBabies, MutateBabies, DiscardParents };


        /*
         *  Members
         */

        Phases phase;
        Texture2D reference;
        Texture2D fixture;
        Texture2D fittest_texture;
        List<Texture2D> textures;
        RenderTarget2D target;
        Game game;
        Generation current_generation;
        Generation next_generation;
        Genome fittest;
        int num_gens;
        int num_fixtures;
        int i;
        int i_max;
        int i_steps;
        Color[] reference_colors;
        Color[] target_colors;
        List<float> fitness_history;
        TimeSpan time;
        DateTime time_of_initialization;


        /*
         * 'Tors
         */

        public Evolver(Game _game) : base(_game)
        {
            textures = new List<Texture2D>();
            game = _game;
            num_gens = 0;
            num_fixtures = 0;
            set_phase(Phases.Initialize);
            fitness_history = new List<float>();
        }


        /*
         * Game Component Methods
         */

        public override void Initialize()
        {
            base.Initialize();
        }
        public override void Update(GameTime gt)
        {
            time += gt.ElapsedGameTime;

            for (int ii = 0; ii < i_steps; ii++)
            {
                if (phase == Phases.Initialize)
                {
                    i_max = 0;
                    time_of_initialization = DateTime.Now;
                    set_phase(Phases.RandomizePopulation);
                    fittest = null;
                }
                else if (phase == Phases.GenerateFixture)
                {
                    // Generate a fixture texture. That is, render the current
                    // fittest individual to a "backdrop" which will be rendered
                    // behiind all subsequent generations. It is these fixtures
                    // which will eventually add up to the final image. Once the new
                    // fixture is rendered, we completely rediversify the population
                    i_max = 0;
                    if (num_gens - num_fixtures * GENS_PER_FIXTURE >= GENS_PER_FIXTURE)
                    {
                        update_fixture();
                        num_fixtures++;
                        set_phase(Phases.RandomizePopulation);
                    } else set_phase(Phases.CalculateFitnesses);
                }
                else if (phase == Phases.RandomizePopulation)
                {
                    // Generate a random initial generation. In this context,
                    // the counter "i" is meaningless.
                    i_max = 0;
                    current_generation = Generation.Rand(GENOME_LENGTH, get_best_fitness());//num_textures());
                    set_phase(Phases.CalculateFitnesses);
                }
                else if (phase == Phases.CalculateFitnesses)
                {
                    // Calculate the fitness of each individual, making sure
                    // to keep track of the fittest individual. In this context,
                    // the counter "i" is the index of the current individual.
                    i_max = POOL_SIZE;
                    i_steps = 10;
                    if (i == 0) current_generation.total_fitness = 0.0f;
                    if (i < i_max)
                    {
                        Genome genome = current_generation.individuals[i];
                        genome.fitness = calculate_fitness(genome);
                        current_generation.total_fitness += genome.fitness;
                        if (genome.fitness > current_generation.get_fittest_individual().fitness)
                            current_generation.fittest_individual = i;
                        i++;
                    }
                    else
                    {
                        Genome new_fittest = current_generation.get_fittest_individual();
                        fitness_history.Add(new_fittest.fitness);
                        if (fittest == null || new_fittest.fitness > fittest.fitness)
                            set_fittest(new_fittest);
                        set_phase(Phases.MakeBabies);
                    }
                }
                else if (phase == Phases.MakeBabies)
                {
                    // In this phase, we create the next generation by selecting
                    // the fittest of the current generation, and forcing them to
                    // have sex until they produce two children, at which point
                    // we kidnap the children and discard of the parents >:)
                    // In this context, the counter "i" is the number of couples
                    // we've harrassed (and the number of babies they've made)
                    i_max = POOL_SIZE;
                    i_steps = 40;
                    if (i == 0) next_generation = new Generation();
                    if (i < i_max)
                    {
                        Genome parent_a = select_individual(current_generation);
                        Genome parent_b = select_individual(current_generation);
                        Genome child_a, child_b;
                        breed(parent_a, parent_b, out child_a, out child_b);
                        next_generation.individuals.Add(child_a);
                        next_generation.individuals.Add(child_b);
                        i++;
                    }
                    else set_phase(Phases.MutateBabies);
                }
                else if (phase == Phases.MutateBabies)
                {
                    // Now that we have some babies, we roll a D20 to see if we
                    // should drop them in radioactive waste. In this context, "i"
                    // is the individual for whose genetics we are currently rolling
                    i_max = POOL_SIZE;
                    i_steps = 40;
                    if (i < i_max)
                    {
                        if (Utils.rand() < GENOME_MUTATION_PROB)
                            mutate(next_generation.individuals[i]);
                        i++;
                    }
                    else set_phase(Phases.DiscardParents);
                }
                else if (phase == Phases.DiscardParents)
                {
                    // Now that our new pool of genetically mutated freaks is all
                    // grown up, we discard their parents and get ready to start
                    // our eugenic process all over again.
                    i_max = 0;
                    current_generation = next_generation;
                    num_gens++;
                    set_phase(Phases.GenerateFixture);
                }
            }

            //
            base.Update(gt);
        }


        /*
         * Class Methods
         */

        #region Calculators

        void render(Genome genome) {

            if (target.Width != reference.Width || target.Height != reference.Height)
                throw new System.Exception("The render target and reference texture must have equal dimensions");

            // Set up and clear the render target
            game.GraphicsDevice.SetRenderTarget(target);
            game.GraphicsDevice.Clear(Color.White);//new Color(new Vector3(.5f)));//Color.White);

            // Begin drawing to the render target
            SpriteBatch sprite_batch = new SpriteBatch(game.GraphicsDevice);
            sprite_batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);//Graphics.MultiplyBlendState());

            // Render the fixture backdrop
            if (num_fixtures > 0)
            {
                sprite_batch.Draw(fixture, Vector2.Zero, Color.White);
            }

            // Calculate the size multiplier... this basically shrinks all parts
            // as the image becomes fitter, so that they can take care of finer
            // details.
            //float fit_scale = 1.25f * (1.5f - get_best_fitness());

            // Draw each gene
            for (int i = 0; i < GENOME_LENGTH; i++)
            {
                Gene gene = genome.genes[i];
                Texture2D texture = textures[0];
                //Vector2 scale = new Vector2(
                //    TEXTURE_SIZE.X / texture.Width * (float)Math.Cos(gene.azimuth),
                //    TEXTURE_SIZE.Y / texture.Height * (float)Math.Cos(gene.pitch));
                Vector2 scale = new Vector2(gene.size.X / texture.Width, gene.size.Y / texture.Height);// *fit_scale;
                sprite_batch.Draw(texture, gene.position, null, gene.color/* * TEXTURE_OPACITY*/, gene.angle, /*TEXTURE_SIZE * scale * 0.5f*/ scale * .5f, scale, SpriteEffects.None, 0.0f);
            }
            sprite_batch.End();

            // Reset the render target
            game.GraphicsDevice.SetRenderTarget(null);
        }

        void update_fixture()
        {
            // Save the rendering of the current fittest individual
            fittest_texture.GetData(target_colors);
            fixture.SetData(target_colors);
            
            // Save the fixture to a file
            String fname = num_fixtures.ToString() + ".jpeg";
            save(fname);
        }

        float calculate_fitness(Genome genome)
        {
            //if (num_textures() != genome.genes.Count)
            //    throw new System.Exception("Genome length must match the number of textures");

            // Render the genome to the target 
            render(genome);

            // Get reference to the pixel color data of the render target
            target.GetData(target_colors);

            // Loop through the pixels, and take the total color differences
            float fitness = 0.0f;
            float max_byte_val = (float)Byte.MaxValue;
            for (int x = 0; x < reference.Width; x += POINT_TEST_RESOLUTION)
            {
                for (int y = 0; y < reference.Height; y += POINT_TEST_RESOLUTION)
                {
                    int i = x + y * reference.Width;
                    float diff = (float)Math.Abs(reference_colors[i].R.CompareTo(target_colors[i].R)) / max_byte_val;
                    fitness += 1.0f - diff;
                }
            }

            //
            return fitness * POINT_TEST_RESOLUTION * POINT_TEST_RESOLUTION / (reference.Width * reference.Height);
        }

        Genome select_individual(Generation generation)
        {
            // I pulled this algorithm from somewhere... 
            float f = current_generation.total_fitness * Utils.rand();
            Genome individual = null;
            for (int i = 0; i < POOL_SIZE; i++)
            {
                individual = current_generation.individuals[i];
                if (f < individual.fitness) break;
                else f -= individual.fitness;
            }
            return individual;
        }

        void breed(Genome parent_a, Genome parent_b, out Genome child_a, out Genome child_b)
        {
            if (parent_a.genes.Count != parent_b.genes.Count)
                throw new System.Exception("Parents with genomes of different sizes are not compatible and cannot be bred");

            // Select a random gene at which to splice the genome
            int num_genes = parent_a.genes.Count;
            int ix = (int)(Utils.rand(num_genes));
            child_a = new Genome();
            child_b = new Genome();
            for (int i = 0; i < ix; i++)
            {
                child_a.genes.Add(parent_a.genes[i]);
                child_b.genes.Add(parent_b.genes[i]);
            }
            for (int i = ix; i < num_genes; i++)
            {
                child_a.genes.Add(parent_b.genes[i]);
                child_b.genes.Add(parent_a.genes[i]);
            }
        }

        void mutate(Genome individual)
        {
            // Loop through each gene, randomly mutating
            for (int i = 0; i < individual.genes.Count; i++)
                if (Utils.rand() < GENE_MUTATION_PROB)
                    individual.genes[i] = Gene.Rand(get_best_fitness());
        }

        void save(String fname, bool full=false)
        {
            // Create the file IO stream
            String dirname = "evolution/" + time_of_initialization.ToString("MM-dd-yy-H-mm-ss") + "/";
            Directory.CreateDirectory(dirname);
            Stream stream = File.Create(dirname + fname);

            // If we need to save in current working dimensions,
            // simply save the fittest texture to a file
            if (full)
            {
                Texture2D full_texture = new Texture2D(game.GraphicsDevice, (int)FULL_RESOLUTION.X, (int)FULL_RESOLUTION.Y);
                render(fittest);
            }
            else
            {
                fittest_texture.SaveAsJpeg(stream, fixture.Width, fixture.Height);
            }
        }

        #endregion

        #region Getters & Setters

        void set_phase(Phases new_phase)
        {
            phase = new_phase;
            i = 0;
            i_max = 0;
            i_steps = 1;
        }
        public void set_reference(Texture2D new_reference)
        {
            int width = (int)REFERENCE_SIZE.X;
            int height = (int)REFERENCE_SIZE.Y;

            // Create the new render target and textures
            reference = new Texture2D(game.GraphicsDevice, width, height);
            target = new RenderTarget2D(game.GraphicsDevice, width, height);
            fittest_texture = new Texture2D(game.GraphicsDevice, width, height);
            fixture = new Texture2D(game.GraphicsDevice, width, height);

            //
            reference_colors = new Color[width * height];
            target_colors = new Color[width * height];
            
            // Resize the new_reference to the proper reference size
            // and save it in reference.
            SpriteBatch batch = new SpriteBatch(game.GraphicsDevice);
            game.GraphicsDevice.SetRenderTarget(target);
            batch.Begin();
            batch.Draw(new_reference, new Rectangle(0, 0, width, height), Color.White);
            batch.End();
            game.GraphicsDevice.SetRenderTarget(null);
            target.GetData(target_colors);
            reference.SetData(target_colors);

            //
            reference.GetData(reference_colors);
            set_phase(Phases.Initialize);
        }
        public Texture2D get_reference()
        {
            return reference;
        }
        public RenderTarget2D get_target()
        {
            return target;
        }
        public void add_texture(Texture2D texture)
        {
            textures.Add(texture);
            set_phase(Phases.Initialize);
        }
        public int num_textures()
        {
            return textures.Count;
        }
        public float get_best_fitness()
        {
            if (fittest != null) return fittest.fitness;
            else return 0.0f;
        }
        public int get_num_generations()
        {
            return num_gens;
        }
        public int get_num_fixtures()
        {
            return num_fixtures;
        }
        public Phases get_phase()
        {
            return phase;
        }
        public float get_phase_completion()
        {
            if (i_max != 0) return (float)i / (float)i_max;
            else return 0.0f;
        }
        public String get_phase_str()
        {
            if (phase == Phases.Initialize) return "Initializing";
            else if (phase == Phases.GenerateFixture) return "Generating Fixture";
            else if (phase == Phases.CalculateFitnesses) return "Calculating Fitness";
            else if (phase == Phases.MakeBabies) return "Making Babies";
            else if (phase == Phases.MutateBabies) return "Mutating Babies";
            else if (phase == Phases.DiscardParents) return "Discarding Old Generation";
            else return "...";
        }
        void set_fittest(Genome new_fittest_individual)
        {
            fittest = new_fittest_individual;
            render(fittest);
            target.GetData(target_colors);
            fittest_texture.SetData(target_colors);
        }
        public Texture2D get_fittest_texture()
        {
            return fittest_texture;
        }
        public List<float> get_fitness_history()
        {
            return fitness_history;
        }
        public TimeSpan get_time()
        {
            return time;
        }

        #endregion
    }
}
