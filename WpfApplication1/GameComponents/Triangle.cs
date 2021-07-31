using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace WpfApplication1.GameComponents
{

    public class Triangle : XNAControlBase
    {
        VertexPositionColor[] vertices;
        SpriteBatch spriteBatch;

        Matrix world;
        Matrix view;
        Matrix projection;
        BasicEffect effect;

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            this.InitEffect();
            this.InitCamera();
            base.Initialize();
        }


        private void InitCamera()
        {
            this.projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, this.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000f);
            this.view = Matrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            this.world = Matrix.Identity;
        }

        private void InitEffect()
        {
            this.effect = new BasicEffect(this.GraphicsDevice);
            this.effect.VertexColorEnabled = true;
        }


        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);

            this.vertices = new VertexPositionColor[3];
            this.vertices[0] = new VertexPositionColor(new Vector3(0, -0.5f, 0), Color.Red);
            this.vertices[1] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0), Color.Green);
            this.vertices[2] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0), Color.Blue);

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
        /// 
        public override void Update()
        {
            this.world = Matrix.CreateRotationY((float)DateTime.Now.TimeOfDay.TotalSeconds);
            base.Update();
        }

        public override void Draw()
        {
            this.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
            this.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            this.effect.World = this.world;
            this.effect.View = this.view;
            this.effect.Projection = this.projection;

            foreach (var pass in this.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip, this.vertices, 0, 1);
            }

            base.Draw();
        }
    }
}
