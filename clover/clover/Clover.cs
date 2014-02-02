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

        const float SCALE = 4.0f;


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
            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                string f_name = Path.GetFileNameWithoutExtension(file.Name);
                evolver.add_texture(Content.Load<Texture2D>("images/textures/" + f_name));
            }

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

            // Update the evolver
            if (state == States.Running)
            {
                evolver.Update(gt);
            }

            //
            base.Update(gt);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Corn flower blue? Seriously? I want fucking white.
            GraphicsDevice.Clear(Color.White);

            //
            String str_state = "?";
            if (state == States.Paused) str_state = "PAUSED";
            else if (state == States.Running) str_state = "RUNNING";
            String str_hud = "State: " + str_state + "\n" +
                             "Phase: " + evolver.get_phase_str() + " - " + evolver.get_phase_completion() * 100 + "%\n" +
                             "Generation: " + evolver.get_num_generations() + "\n" +
                             "Best Fitness: " + evolver.get_best_fitness() * 100 + "%";

            //
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            if (evolver.get_phase() == Evolver.Phases.CalculateFitnesses)
                sprite_batch.Draw(evolver.get_target(), new Vector2(0), null, Color.White, 0.0f, Vector2.Zero, SCALE, SpriteEffects.None, 0.0f);
            else
                sprite_batch.Draw(evolver.get_fittest_texture(), new Vector2(0), null, Color.White, 0.0f, Vector2.Zero, SCALE, SpriteEffects.None, 0.0f);
            sprite_batch.Draw(evolver.get_reference(), new Vector2(0), null, Color.White * .2f, 0.0f, Vector2.Zero, SCALE, SpriteEffects.None, 0.0f);
            sprite_batch.DrawString(sprite_font, str_hud, new Vector2(20.0f, 20.0f), Color.Black);
            sprite_batch.DrawString(sprite_font, str_hud, new Vector2(19.0f, 19.0f), Color.White);
            sprite_batch.End();

            //
            base.Draw(gameTime);
        }
    }
}
