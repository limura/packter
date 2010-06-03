/*
 * ��̃e�N�X�`����\�����邽�߂�class
 * 
 * �\�������e�N�X�`���C���[�W(texture)
 * ��΍��W�ЂƂ�(position)
 * �\�������l�p�`�̃��f���ЂƂ�(model)
 * �������Ă��āADraw() method �ł����ĕ\�������
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
        bool lightingEnabled = false;

        public Board(Model Model)
        {
            model = Model;
        }

        public bool LightingEnabled
        {
            get { return lightingEnabled; }
            set { lightingEnabled = value; }
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

        public void Draw(Matrix view, Viewport viewport, Matrix projection, float scale)
        {
            Matrix transform = Matrix.Identity;
            transform *= Matrix.CreateScale(Scale * scale);
            transform *= Matrix.CreateRotationY(RotationY);
            transform *= Matrix.CreateTranslation(position);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    if (lightingEnabled)
                    {
                        effect.EnableDefaultLighting();
                        effect.LightingEnabled = true;
                    }
                    else
                    {
                        //effect.EnableDefaultLighting();
                        effect.LightingEnabled = false;
                    }
                    effect.World = transform;
                    effect.View = view;
                    if (texture != null)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = texture;
                    }
                    else
                    {
                        effect.TextureEnabled = false;
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
