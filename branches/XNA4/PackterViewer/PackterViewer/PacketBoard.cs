using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PackterViewer
{
    public interface PacketBoard : IComparable<PacketBoard>
    {
        float Scale
        {
            set;
            get;
        }

        /// <summary>
        /// マウスに感知されるかどうかを指定します
        /// </summary>
        bool NoSelection
        {
            get;
            set;
        }

        /// <summary>
        /// 指示された時間に表示されるべきかどうかを調べ、表示されるべきであれば自分の座標を更新します
        /// </summary>
        /// <param name="nowGameTimeMilliseconds">表示される時間</param>
        /// <returns>表示されるべきであった(0)、表示される時間よりも後の時間を指定された(1)、表示される時間よりも前の時間を指定された(-1)</returns>
        int Update(double nowGameTimeMilliseconds);

        void Draw(Matrix view, Viewport viewport, Matrix projection, float scale, Vector3 cameraPosition, Vector3 cameraTarget);

        BoundingSphere BoundingSphere
        {
            get;
        }

        string Description
        {
            get;
        }

        bool LightingEnabled
        {
            get;
            set;
        }

        double CreatedTimeMillisecond
        {
            get;
        }

        float Alpha
        {
            get;
            set;
        }

        bool BillboardEnabled
        {
            get;
            set;
        }

        float PositionZ
        {
            get;
        }

        //int CompareTo(PacketBoard x);
        //static int Compare(Object x, Object y);
    }
}
