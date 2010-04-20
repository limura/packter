using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Packter_viewer
{
    class SquareTexture
    {
        Vector3 position;
        Vector3 rotation;
        float size;
        
        BasicEffect basicEffect = null;
        VertexBuffer vertexBuffer = null;
        GraphicsDevice gd;

        public SquareTexture(GraphicsDevice Gd, Vector3 Position, float Size, Vector3 Rotation, Texture2D Texture)
        {
            gd = Gd;
            basicEffect = new BasicEffect(gd, null);
            size = Size;
            rotation = Rotation;
            position = Position;
            basicEffect.Texture = Texture;
            basicEffect.TextureEnabled = true;

            Update();
        }

        // 
        private void Update()
        {
            // ���_�o�b�t�@�쐬
            this.vertexBuffer = new VertexBuffer(gd,
                VertexPositionTexture.SizeInBytes * 4, BufferUsage.None);

            Matrix world =
                Matrix.CreateRotationX(rotation.X)
                * Matrix.CreateRotationY(rotation.Y)
                * Matrix.CreateRotationZ(rotation.Z)
                * Matrix.CreateTranslation(position);
            Vector3 center = world.Translation;
            
            // ���_�f�[�^���쐬����
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0] = new VertexPositionTexture(new Vector3(center.X - size, center.Y + size, center.Z + 0.0f),
                                                    new Vector2(0.0f, 0.0f));
            vertices[1] = new VertexPositionTexture(new Vector3(center.X + size, center.Y + size, center.Z + 0.0f),
                                                    new Vector2(1.0f, 0.0f));
            vertices[2] = new VertexPositionTexture(new Vector3(center.X - size, center.Y - size, center.Z + 0.0f),
                                                    new Vector2(0.0f, 1.0f));
            vertices[3] = new VertexPositionTexture(new Vector3(center.X + size, center.Y - size, center.Z + 0.0f),
                                                    new Vector2(1.0f, 1.0f));

            // ���_�f�[�^�𒸓_�o�b�t�@�ɏ�������
            this.vertexBuffer.SetData<VertexPositionTexture>(vertices);
        }

        public float Size
        {
            set { size = value; Update(); }
            get { return size; }
        }

        public Vector3 Rotation
        {
            set { rotation = value; Update(); }
            get { return rotation; }
        }

        public Vector3 Position
        {
            set { position = value; Update(); }
            get { return position; }
        }

        public Texture2D Texture
        {
            set { basicEffect.Texture = value; }
            get { return basicEffect.Texture; }
        }
        
        public void Draw(GraphicsDevice gd, int number)
        {
            // �`�悷�钸�_�f�[�^�̒�`��ݒ�
            //gd.VertexDeclaration = vertexDeclaration;
            // ���_�o�b�t�@���Z�b�g���܂�
            gd.Vertices[number].SetSource(
                vertexBuffer, 0, VertexPositionTexture.SizeInBytes);

            // �G�t�F�N�g�̎g�p���J�n���܂�
            this.basicEffect.Begin();
            // �p�X�̐������J��ւ����`��
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                // �p�X�̊J�n
                pass.Begin();
                // �|���S���`�悷��
                gd.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
                // �p�X�̏I��
                pass.End();
            }
            // �G�t�F�N�g�̎g�p���I������
            this.basicEffect.End();
        }
    }
}
