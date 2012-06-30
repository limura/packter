/*
 * テキトーなRingBufferの実装
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Packter_viewer
{
    class RingBuffer<T>
    {
        T [] array = null;
        int startPoint;
        int endPoint;

        /// <summary>
        /// 指定された長さのリングバッファを作成します。バッファの長さを変えることはできません
        /// </summary>
        /// <param name="size">配列の長さ</param>
        public RingBuffer(int size)
        {
            array = new T[size];
            startPoint = 0;
            endPoint = 0;
            for (int i = 0; i < size; i++)
                array[i] = default(T);
        }

        /// <summary>
        /// 要素をひとつ追加します
        /// </summary>
        /// <param name="obj">追加される要素</param>
        public void Add(T obj)
        {
            array[endPoint % array.Length] = obj;
            endPoint = (endPoint+1) % array.Length;
            if (endPoint == startPoint)
                startPoint = (startPoint+1) % array.Length;
        }

        // [] のインデクサ
        public T this[int index]
        {
            get
            {
                return array[(startPoint + index) % array.Length];
            }
            set
            {
                array[(startPoint + index) % array.Length] = value;
            }
        }

        /// <summary>
        /// 配列の長さを返します
        /// </summary>
        public int Length
        {
            get { return array.Length; }
        }
    }
}
