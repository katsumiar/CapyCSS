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

        private List<Record> commandStack = new List<Record>();
        private int currentPoint = -1;

        /// <summary>
        /// 履歴があるか判定します。
        /// </summary>
        public bool IsStack => commandStack.Count != 0;
        
        /// <summary>
        /// 履歴が初期位置（編集されていない状態）を指しているかを判定します。
        /// </summary>
        public bool IsInitialPoint => currentPoint <= 0;

        /// <summary>
        /// 履歴をクリアします。
        /// </summary>
        public void Clear()
        {
            commandStack.Clear();
            currentPoint = -1;
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
            T last = commandStack.Last().data;
            return last.CompareTo(data) != 0;
        }

        /// <summary>
        /// 履歴に現在の状態を積みます。
        /// </summary>
        /// <param name="title">履歴タイトル</param>
        /// <param name="data">状態</param>
        public void Push(string title, T data)
        {
            if (currentPoint != -1 && currentPoint + 1 < commandStack.Count)
            {
                commandStack.RemoveRange(currentPoint + 1, commandStack.Count - 1);
            }
            commandStack.Add(new Record { title = title, data = data });
            currentPoint = commandStack.Count - 1;
        }

        /// <summary>
        /// Back()が有効か？
        /// </summary>
        public bool IsBack => currentPoint > 0;
        
        /// <summary>
        /// 履歴を１つ遡ります。
        /// </summary>
        /// <returns>遡った状態</returns>
        public T Back()
        {
            if (!IsBack)
            {
                return null;
            }
            return commandStack[--currentPoint].data;
        }

        /// <summary>
        /// Next()が有効か？
        /// </summary>
        public bool IsNext => currentPoint < commandStack.Count - 1;
        
        /// <summary>
        /// 履歴を１つ先に進めます。
        /// </summary>
        /// <returns>進めた状態</returns>
        public T Next()
        {
            if (!IsNext)
            {
                return null;
            }
            return commandStack[++currentPoint].data;
        }
    }
}
