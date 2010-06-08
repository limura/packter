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
        bool billboardEnabled = false;
        bool skyboxEnabled = false;

        public Board(Model Model)
        {
            model = Model;
        }

        public Model Model
        {
            get { return model; }
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

        public bool BillboardEnabled
        {
            get { return billboardEnabled; }
            set { billboardEnabled = value; }
        }

        public bool SkyBoxEnabled
        {
            get { return skyboxEnabled; }
            set { skyboxEnabled = value; }
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

        public void Draw(Matrix view, Viewport viewport, Matrix projection, float scale, Vector3 cameraPosition, Vector3 cameraTarget)
        {
            if (skyboxEnabled)
            {
                DrawSkybox(view, viewport, projection, scale, cameraPosition, cameraTarget);
                return;
            }
            Matrix transform = Matrix.Identity;
            if (billboardEnabled == false)
            {
                transform = Matrix.Identity;
                transform *= Matrix.CreateScale(Scale * scale);
                transform *= Matrix.CreateRotationY(RotationY);
                transform *= Matrix.CreateTranslation(position);
            }
            else
            {
                transform = Matrix.Identity * Matrix.CreateScale(Scale * scale)
                    * Matrix.CreateBillboard(position, cameraPosition, Vector3.Up, cameraTarget);
                    //* Matrix.CreateTranslation(position);
            }

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
        
        /// <summary>
        /// Skybox �Ƃ��ĕ\�����܂��B����method���Ăяo���O�� GraphicsDevice.RenderState.DepthBufferEnable = false; �ɂ��Ă����K�v������܂�
        /// </summary>
        void DrawSkybox(Matrix view, Viewport viewport, Matrix projection, float scale, Vector3 cameraPosition, Vector3 cameraTarget)
        {

            Matrix[] skyModelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(skyModelTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = skyModelTransforms[mesh.ParentBone.Index]
                        * Matrix.CreateTranslation(cameraPosition);
                    currentEffect.World = worldMatrix;
                    currentEffect.View = view;
                    currentEffect.Projection = projection;
                    currentEffect.LightingEnabled = false;
                    currentEffect.TextureEnabled = true;
                    if (texture != null)
                    {
                        currentEffect.Texture = texture;
                    }
                }
                mesh.Draw();
            }
        }
    }
}
