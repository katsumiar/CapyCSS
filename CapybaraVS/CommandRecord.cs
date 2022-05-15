using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS
{
    internal class CommandRecord<T>
        where T : class, IComparable
    {
        struct Record
        {
            /// <summary>
            /// 履歴タイトルです。
            /// </summary>
            public string title;
            /// <summary>
            /// 履歴の状態です。
            /// </summary>
            public T data;
        }

        private List<Record> undoStack = new List<Record>();
        private int CurrentPoint = -1;

        /// <summary>
        /// 履歴があるか判定します。
        /// </summary>
        public bool IsStack => undoStack.Count != 0;
        
        /// <summary>
        /// 履歴が初期位置（編集されていない状態）を指しているかを判定します。
        /// </summary>
        public bool IsInitialPoint => CurrentPoint <= 0;

        /// <summary>
        /// 履歴をクリアします。
        /// </summary>
        public void Clear()
        {
            undoStack.Clear();
            CurrentPoint = -1;
        }
        
        /// <summary>
        /// 履歴対象が現在の履歴と比較して変化しているかを判定します。
        /// </summary>
        /// <param name="data"></param>
        /// <returns>true==変化している</returns>
        public bool IsChanges(T data)
        {
            if (!IsStack)
            {
                return true;
            }
            T last = undoStack.Last().data;
            return last.CompareTo(data) != 0;
        }

        /// <summary>
        /// 履歴に現在の状態を積みます。
        /// </summary>
        /// <param name="title">履歴タイトル</param>
        /// <param name="data">状態</param>
        public void Push(string title, T data)
        {
            if (CurrentPoint != -1 && CurrentPoint + 1 < undoStack.Count)
            {
                undoStack.RemoveRange(CurrentPoint + 1, undoStack.Count - 1);
            }
            undoStack.Add(new Record { title = title, data = data });
            CurrentPoint = undoStack.Count - 1;
        }
        
        /// <summary>
        /// 履歴を１つ遡ります。
        /// </summary>
        /// <returns>遡った状態</returns>
        public T Back()
        {
            if (CurrentPoint <= 0)
            {
                return null;
            }
            return undoStack[--CurrentPoint].data;
        }
        
        /// <summary>
        /// 履歴を１つ先に進めます。
        /// </summary>
        /// <returns>進めた状態</returns>
        public T Next()
        {
            if (CurrentPoint >= undoStack.Count - 1)
            {
                return null;
            }
            return undoStack[++CurrentPoint].data;
        }
    }
}
