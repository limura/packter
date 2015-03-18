/*
 * �e�L�g�[��RingBuffer�̎���
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace PackterViewer
{
    class RingBuffer<T>
    {
        T [] array = null;
        int startPoint;
        int endPoint;

        /// <summary>
        /// �w�肳�ꂽ�����̃����O�o�b�t�@���쐬���܂��B�o�b�t�@�̒�����ς��邱�Ƃ͂ł��܂���
        /// </summary>
        /// <param name="size">�z��̒���</param>
        public RingBuffer(int size)
        {
            array = new T[size];
            startPoint = 0;
            endPoint = 0;
            for (int i = 0; i < size; i++)
                array[i] = default(T);
        }

        /// <summary>
        /// �v�f���ЂƂǉ����܂�
        /// </summary>
        /// <param name="obj">�ǉ������v�f</param>
        public void Add(T obj)
        {
            array[endPoint % array.Length] = obj;
            endPoint = (endPoint+1) % array.Length;
            if (endPoint == startPoint)
                startPoint = (startPoint+1) % array.Length;
        }

        // [] �̃C���f�N�T
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
        /// �z��̒�����Ԃ��܂�
        /// </summary>
        public int Length
        {
            get { return array.Length; }
        }
    }
}
