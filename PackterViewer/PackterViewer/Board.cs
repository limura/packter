/*
 * 板状のテクスチャを表示するためのclass
 * 
 * 表示されるテクスチャイメージ(texture)
 * 絶対座標ひとつ(position)
 * 表示される四角形のモデルひとつ(model)
 * をもっていて、Draw() method でもって表示される
 */
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Packter_viewer
{
    class Board
    {
        Texture2D texture;
        Vector3 position = new Vector3(0, 0, 0);
        Model model = null;

        public Board(Model Model)
        {
            model = Model;
        }

        public Vector3 Position
        {
            set { position = value; }
            get { return position; }
        }

        public float RotationP = 0;
        public float RotationY = 0;
        public float RotationR = 0;
        public string Text = null;
        public float Alpha = 1.0f;
        public float Scale = 1.0f;

        public Texture2D Texture
        {
            set { texture = value; }
            get { return texture; }
        }

        public void Draw(Matrix view, Viewport viewport, Matrix projection)
        {
            Matrix transform = Matrix.Identity;
            transform *= Matrix.CreateScale(Scale);
            transform *= Matrix.CreateRotationY(RotationY);
            transform *= Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.LightingEnabled = false;
                    effect.World = transform;
                    effect.View = view;
                    if (texture != null)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = texture;
                    }
                    effect.Alpha = Alpha;
                    effect.Projection = projection;
                    effect.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                }
                mesh.Draw();
            }
        }
    }
}
