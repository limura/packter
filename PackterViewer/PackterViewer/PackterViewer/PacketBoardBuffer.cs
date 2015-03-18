using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackterViewer
{
    class PacketBoardBuffer
    {
        List<PacketBoard> list = new List<PacketBoard>();
        int maxSize = 0;

        /// <summary>
        /// 指定された長さのリングバッファを作成します。バッファの長さを変えることはできません
        /// </summary>
        /// <param name="size">配列の長さ</param>
        public PacketBoardBuffer(int size)
        {
            list = new List<PacketBoard>();            
            maxSize = size;
        }

        /// <summary>
        /// 指定された時間と同じか前に作られた PacketBoard のうち、一番新しいものへの index を返します
        /// </summary>
        /// <param name="targetTime">検索に用いる時間</param>
        /// <returns>index</returns>
        public int BinarySearchPrev(double targetTime)
        {
            if (list.Count <= 0)
            {
                return 0;
            }

            int head = 0;
            int tail = list.Count - 1;
            while (head <= tail)
            {
                int where = (head + tail) / 2;
                PacketBoard pb = list[where];
                if (pb.CreatedTimeMillisecond <= targetTime)
                {
                    if (where + 1 >= list.Count || list[where + 1].CreatedTimeMillisecond > targetTime)
                    {
                        return where;
                    }
                    head = where + 1;
                }
                else
                {
                    tail = where - 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 指定された時間と同じか後に作られた PacketBoard のうち、一番古いものへの index を返します
        /// </summary>
        /// <param name="targetTime">検索に用いる時間</param>
        /// <returns>index</returns>
        public int BinarySearchNext(double targetTime)
        {
            if (list.Count <= 0)
            {
                return 0;
            }

            int head = 0;
            int tail = list.Count - 1;
            while (head <= tail)
            {
                int where = (head + tail) / 2;
                PacketBoard pb = list[where];
                if (pb.CreatedTimeMillisecond <= targetTime)
                {
                    if (where + 1 >= list.Count){
                        return 0;
                    }else if(list[where + 1].CreatedTimeMillisecond > targetTime)
                    {
                        return where + 1;
                    }
                    head = where + 1;
                }
                else
                {
                    tail = where - 1;
                }
            }
            return 0;
        }
        
        /// <summary>
        /// 要素をひとつ追加します
        /// </summary>
        /// <param name="obj">追加される要素</param>
        public void Add(PacketBoard pb)
        {
            if (pb == null)
            {
                return;
            }
            double targetTime = pb.CreatedTimeMillisecond;
            if (list.Count >= maxSize)
            {
                list.Remove(list[0]);
            }
            if (list.Count == 0)
            {
                list.Add(pb);
            }
            else
            {
                int index = BinarySearchPrev(targetTime);
                list.Insert(index + 1, pb);
            }
        }

        // [] のインデクサ
        public PacketBoard this[int index]
        {
            get
            {
                if (index >= list.Count || index < 0)
                {
                    return null;
                }
                return list[index];
            }
        }

        /// <summary>
        /// 配列の長さを返します
        /// </summary>
        public int Length
        {
            get { return list.Count; }
        }
    }
}
