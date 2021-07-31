using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WpfApplication1.GameComponents
{

    public class SpriteFontTest : XNAControlBase
    {
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        TimeSpan ts;
        int frames = 0;
        int fps = 0;


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.spriteFont = this.Content.Load<SpriteFont>("MainFont");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            this.spriteBatch.Dispose();

            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update()
        {
            // TODO: Add your update code here
            base.Update();
        }

        public override void Draw()
        {
            this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

            frames++;

            if (DateTime.Now.TimeOfDay.Subtract(ts).TotalSeconds > 1)
            {
                fps = frames;
                frames = 0;
                ts = DateTime.Now.TimeOfDay;
            }

            this.spriteBatch.Begin();
            this.spriteBatch.DrawString(this.spriteFont, fps.ToString(), Vector2.Zero, Color.Black);
            this.spriteBatch.End();

            base.Draw();
        }
    }
}
