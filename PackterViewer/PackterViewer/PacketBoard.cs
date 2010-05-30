/*
 * Board class ���g���Ĕ�̃e�N�X�`�����n�_����I�_�܂Ŕ��ł����̂�\�����邽�߂�class
 * 
 * �R���X�g���N�^�Ŏw�肳�ꂽ�n�_����I�_�܂ŁAStartTime ���� StartTime+FlyTimeMillisecond �̊Ԃł܂��������ł����B
 */
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Packter_viewer
{
    class PacketBoard
    {
        Vector3 startPoint;
        Vector3 endPoint;
        double startTimeMillisecond;
        float flyTimeMillisecond;
        string description;

        Board board;
        
        public PacketBoard(Model model, Texture2D texture
            , Vector3 StartPoint, Vector3 EndPoint, GameTime StartTime, float FlyTimeMillisecond
            , string Description)
        {
            board = new Board(model);
            board.Texture = texture;

            startPoint = StartPoint;
            endPoint = EndPoint;
            startTimeMillisecond = StartTime.TotalGameTime.TotalMilliseconds;
            flyTimeMillisecond = FlyTimeMillisecond;
            board.Position = StartPoint;
            description = Description;
        }

        public float Scale
        {
            set { if (board != null) board.Scale = value; }
            get { if (board != null) return board.Scale; return 0; }
        }

        /// <summary>
        /// �w�����ꂽ���Ԃɕ\�������ׂ����ǂ����𒲂ׁA�\�������ׂ��ł���Ύ����̍��W���X�V���܂�
        /// </summary>
        /// <param name="nowGameTimeMilliseconds">�\������鎞��</param>
        /// <returns>�\�������ׂ��ł�����(0)�A�\������鎞�Ԃ�����̎��Ԃ��w�肳�ꂽ(1)�A�\������鎞�Ԃ����O�̎��Ԃ��w�肳�ꂽ(-1)</returns>
        public int Update(double nowGameTimeMilliseconds)
        {
            double diffTime = nowGameTimeMilliseconds - startTimeMillisecond;
            if (diffTime > flyTimeMillisecond)
                return 1;
            if(diffTime < 0)
                return -1;

            float f = (float)(diffTime / flyTimeMillisecond);
            board.Position = startPoint + (endPoint - startPoint) * f;
            //System.Diagnostics.Debug.WriteLine(f + " = " + diffTime + " / " + flyTimeMillisecond
            //    + " gameTime: " + nowGameTime.TotalGameTime.TotalMilliseconds + " - " + startTimeMillisecond);
            return 0;
        }

        public void Draw(Matrix view, Viewport viewport, Matrix projection, float scale)
        {
            board.Draw(view, viewport, projection, scale);
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                return new BoundingSphere(board.Position, board.Scale);
            }
        }

        public string Description
        {
            get { return description; }
        }
    }
}
