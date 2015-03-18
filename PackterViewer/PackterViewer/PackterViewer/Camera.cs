/*
 * �J�����𐧌䂷�邽�߂�class
 * 
 * memo
 * �ǂ����̃T���v�����قڂ��̂܂܂����ς�������̂̂͂��B
 * �J����(����)�̈ʒu�Ƃ���ێ����āA��������W�ϊ��s��Ƃ���
 * ���o����悤�ɂ����肷��B
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PackterViewer
{
    class Camera
    {
        //�r���[�ϊ��s��
        private Matrix view;
        public Matrix View
        {
            get
            {
                return view;
            }
        }

        //�ˉe�ϊ��s��
        private Matrix projection;
        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }

        //�J�����ʒu
        private Vector3 position;
        public Vector3 Position
        {
            set
            {
                position = value;
                Update();
            }
            get
            {
                return position;
            }
        }
        public float PositionX
        {
            set
            {
                position.X = value;
                Update();
            }
            get
            {
                return position.X;
            }
        }
        public float PositionY
        {
            set
            {
                position.Y = value;
                Update();
            }
            get
            {
                return position.Y;
            }
        }
        public float PositionZ
        {
            set
            {
                position.Z = value;
                Update();
            }
            get
            {
                return position.Z;
            }
        }

        //�Q�Ɠ_
        private Vector3 look;
        public Vector3 Look
        {
            set
            {
                look = value;
                LookAt(position, look, Vector3.Up);
            }
            get
            {
                return look;
            }
        }

        //����
        public Vector3 Direction
        {
            get
            {
                Vector3 direction = look - position;
                direction.Normalize();
                return direction;
            }
        }

        //���ʒu
        private float yaw;
        public float Yaw
        {
            set
            {
                yaw = value;
                Update();
            }
            get
            {
                return yaw;
            }
        }

        //�����ʒu
        private float pitch;
        public float Pitch
        {
            set
            {
                pitch = value;
                Update();
            }
            get
            {
                return pitch;
            }
        }

        //�X���ʒu
        private float roll;
        public float Roll
        {
            set
            {
                roll = value;
                Update();
            }
            get
            {
                return roll;
            }
        }

        //�R���X�g���N�^
        public Camera(float aspectRatio)
        {
            //�r���[�ϊ��s��
            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;
            position = new Vector3(0.0f, 0.0f, 3.0f);
            Update();

            //�ˉe�ϊ��s��
            PerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                aspectRatio,
                0.005f,
                1000.0f
            );
        }

        //�r���[�ϊ��s��̍X�V
        private void Update()
        {
            Vector3 vec = new Vector3(0, 0, -1);
            Matrix trans = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Vector3 look = Vector3.Transform(vec, trans) + position;
            LookAt(position, look, Vector3.Up);
        }

        //�r���[�ϊ��s��̐ݒ�
        public void LookAt(Vector3 position, Vector3 look, Vector3 up)
        {
            this.position = position;
            this.look = look;
            view = Matrix.CreateLookAt(position, look, up);
        }

        //�ˉe�ϊ��s��̐ݒ�
        public void PerspectiveFieldOfView(float fieldOfView,
            float aspectRatio, float near, float far)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, near, far);
        }
    }
}
