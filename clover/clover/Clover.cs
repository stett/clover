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
    public class Clover : Microsoft.Xna.Framework.Game
    {
        /*
         * Types
         */

        enum States { Running, Paused };


        /*
         * Members
         */

        GraphicsDeviceManager graphics;
        SpriteBatch sprite_batch;
        SpriteFont sprite_font;
        Evolver evolver;
        States state;
        KeyboardInput keyboard;
        Texture2D pixel;

        /*
         * Constants
         */

        const float SCALE = 3.0f;

        /*
         * Settings
         */

        bool show_reference = true;
        bool show_graph = true;
        bool show_hud = true;
        bool show_intermediates = true;


        /*
         * 'Tors
         */

        public Clover()
        {
            graphics = new GraphicsDeviceManager(this);
            evolver = new Evolver(this);
            state = States.Paused;
            keyboard = new KeyboardInput();
        }


        /*
         * Game Methods
         */

        protected override void Initialize()
        {
            // Set the window size
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = (int)Evolver.REFERENCE_SIZE.X * (int)SCALE;
            graphics.PreferredBackBufferHeight = (int)Evolver.REFERENCE_SIZE.Y * (int)SCALE;
            graphics.ApplyChanges();

            // Set the content root
            Content.RootDirectory = "Content";

            // 
            IsFixedTimeStep = false;

            //
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Load the reference image
            evolver.set_reference(Content.Load<Texture2D>("images/reference"));

            // Load the textures directory info, abort if none
            DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "/images/textures");
            if (!dir.Exists)
                throw new DirectoryNotFoundException();

            // Load all files that matches the file filter
            /*FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                string f_name = Path.GetFileNameWithoutExtension(file.Name);
                evolver.add_texture(Content.Load<Texture2D>("images/textures/" + f_name));
            }*/
            pixel = Content.Load<Texture2D>("images/textures/tex");
            evolver.add_texture(pixel);

            // Create the sprite batch and load the font
            sprite_batch = new SpriteBatch(GraphicsDevice);
            sprite_font = Content.Load<SpriteFont>("fonts/consolas");
        }

        protected override void UnloadContent() { }

        protected override void Update(GameTime gt)
        {
            // Update the keyboard
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            keyboard.update(dt);

            // Exit on ESC
            if (keyboard.key_pressed(Keys.Escape)) this.Exit();

            // Toggle state
            if (keyboard.key_pressed(Keys.Space))
            {
                if (state == States.Paused) state = States.Running;
                else state = States.Paused;
            }

            // Toggle reference image
            if (keyboard.key_pressed(Keys.F1)) show_reference = !show_reference;
            if (keyboard.key_pressed(Keys.F2)) show_graph = !show_graph;
            if (keyboard.key_pressed(Keys.F3)) show_hud = !show_hud;
            if (keyboard.key_pressed(Keys.F4)) show_intermediates = !show_intermediates;

            // Update the evolver
            if (state == States.Running)
            {
                evolver.Update(gt);
            }

            //
            base.Update(gt);
        }

        protected override void Draw(GameTime gt)
        {
            // Corn flower blue? Seriously? I want fucking white.
            GraphicsDevice.Clear(Color.White);

            //
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Draw the state of the evolver
            if (show_intermediates && evolver.get_phase() == Evolver.Phases.CalculateFitnesses)
                sprite_batch.Draw(evolver.get_target(), new Vector2(0), null, Color.White, 0.0f, Vector2.Zero, SCALE, SpriteEffects.None, 0.0f);
            else
                sprite_batch.Draw(evolver.get_fittest_texture(), new Vector2(0), null, Color.White, 0.0f, Vector2.Zero, SCALE, SpriteEffects.None, 0.0f);

            // Draw the reference image
            if (show_reference)
                sprite_batch.Draw(evolver.get_reference(), new Vector2(0), null, Color.White * .3f, 0.0f, Vector2.Zero, SCALE, SpriteEffects.None, 0.0f);

            // Draw hud text
            if (show_hud)
            {
                String str_state = "?";
                if (state == States.Paused) str_state = "PAUSED";
                else if (state == States.Running) str_state = "RUNNING";
                String str_hud = "State:       " + str_state + "\n" +
                                 "Phase:       " + evolver.get_phase_str() + " - " + evolver.get_phase_completion() * 100 + "%\n" +
                                 "Gen:         " + evolver.get_num_generations() + "\n" +
                                 "Fixture:     " + evolver.get_num_fixtures() + "\n" +
                                 "Most Fit:    " + evolver.get_best_fitness() * 100 + "%" + "\n" +
                                 "Time:        " + evolver.get_time() + "\n" +
                                 "Time/gen:    " + evolver.get_time().TotalSeconds / Math.Max(evolver.get_num_generations(), 1.0f) + "s\n" +
                                 "\n" +
                                 "Pool:        " + Evolver.POOL_SIZE + "\n" +
                                 "Genome Len.: " + Evolver.GENOME_LENGTH + "\n" +
                                 "Mut. Prob. (genome, gene): " + Evolver.GENOME_MUTATION_PROB * 100 + "% , " + Evolver.GENE_MUTATION_PROB * 100 + "%\n" +
                                 "";
                sprite_batch.DrawString(sprite_font, str_hud, new Vector2(20.0f, 20.0f), Color.Black);
                sprite_batch.DrawString(sprite_font, str_hud, new Vector2(19.0f, 19.0f), Color.White);
            }

            // Draw evolver fitness history plot
            if (show_graph)
            {
                float graph_scale = 5.0f;
                List<float> fitness_history = evolver.get_fitness_history();
                int x = 0;
                int i_0 = (int)Math.Max(0, fitness_history.Count - Math.Ceiling(graphics.PreferredBackBufferWidth / graph_scale) + 1);
                for (int i = i_0; i < fitness_history.Count; i++)
                {
                    float fitness = fitness_history[i];
                    Vector2 scale = new Vector2(graph_scale / pixel.Width, graph_scale / pixel.Height);
                    sprite_batch.Draw(pixel, new Vector2(graph_scale * x, graphics.PreferredBackBufferHeight - fitness * 100.0f), null, Color.HotPink, 0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
                    x++;
                }
                sprite_batch.Draw(pixel, new Vector2(0, graphics.PreferredBackBufferHeight - 100.0f), null, Color.HotPink, 0, Vector2.Zero, new Vector2(graphics.PreferredBackBufferWidth / (pixel.Width - 1), 1.0f / pixel.Height), SpriteEffects.None, 0.0f);
            }

            sprite_batch.End();

            //
            base.Draw(gt);
        }
    }
}
