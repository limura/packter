/*
 * カメラを制御するためのclass
 * 
 * memo
 * どこかのサンプルをほぼそのままかっぱらったもののはず。
 * カメラ(視線)の位置とかを保持して、それを座標変換行列として
 * 取り出せるようにしたりする。
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
        //ビュー変換行列
        private Matrix view;
        public Matrix View
        {
            get
            {
                return view;
            }
        }

        //射影変換行列
        private Matrix projection;
        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }

        //カメラ位置
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

        //参照点
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

        //方向
        public Vector3 Direction
        {
            get
            {
                Vector3 direction = look - position;
                direction.Normalize();
                return direction;
            }
        }

        //横位置
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

        //高さ位置
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

        //傾き位置
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

        //コンストラクタ
        public Camera(float aspectRatio)
        {
            //ビュー変換行列
            yaw = 0.0f;
            pitch = 0.0f;
            roll = 0.0f;
            position = new Vector3(0.0f, 0.0f, 3.0f);
            Update();

            //射影変換行列
            PerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f),
                aspectRatio,
                0.005f,
                1000.0f
            );
        }

        //ビュー変換行列の更新
        private void Update()
        {
            Vector3 vec = new Vector3(0, 0, -1);
            Matrix trans = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);
            Vector3 look = Vector3.Transform(vec, trans) + position;
            LookAt(position, look, Vector3.Up);
        }

        //ビュー変換行列の設定
        public void LookAt(Vector3 position, Vector3 look, Vector3 up)
        {
            this.position = position;
            this.look = look;
            view = Matrix.CreateLookAt(position, look, up);
        }

        //射影変換行列の設定
        public void PerspectiveFieldOfView(float fieldOfView,
            float aspectRatio, float near, float far)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, near, far);
        }
    }
}
