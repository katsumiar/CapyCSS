using System;
using System.Collections.Generic;
using System.Text;

namespace CbVS
{
    /// <summary>
    /// データ付きの実行を待機するための待ち行列クラスです。
    /// </summary>
    /// <typeparam name="T">識別子の型</typeparam>
    public class HoldActionQueue<T>
        : IDisposable
    {
        /// <summary>
        /// 保留処理リスト
        /// </summary>
        private Dictionary<T, Action> holdActionDic = new Dictionary<T, Action>();
        private List<Action> holdActionList = new List<Action>();
        private bool enabled = false;
        private bool disposedValue;

        /// <summary>
        /// 処理保留モードを参照します。
        /// ※ false にしたとき、登録した処理を一括処理する。
        /// </summary>
        public bool Enabled
        {
            get => enabled;
            set
            {
                bool IsOffFromOn = !value && enabled;
                enabled = value;
                if (IsOffFromOn)
                {
                    // 処理保留モードモードを false にしたとき、一括処理する

                    foreach (var node in holdActionDic.Values)
                    {
                        node();
                    }
                    holdActionDic.Clear();
                    foreach (var node in holdActionList)
                    {
                        node();
                    }
                    holdActionList.Clear();
                }
            }
        }

        /// <summary>
        /// 保留処理リストに登録します。
        /// ※同一の識別子は、上書き保存される。
        /// </summary>
        /// <param name="key">識別子</param>
        /// <param name="action">保留処理</param>
        public void Add(T key, Action action)
        {
            if (holdActionDic.ContainsKey(key))
                holdActionDic[key] = action;
            else
                holdActionDic.Add(key, action);
        }

        /// <summary>
        /// 識別子無しで追加します。
        /// </summary>
        /// <param name="action"></param>
        public void Add(Action action)
        {
            holdActionList.Add(action);
        }

        /// <summary>
        /// 保留の処理数を参照します。
        /// </summary>
        public int Count
        {
            get
            {
                return holdActionDic.Count + holdActionList.Count;
            }
        }

        /// <summary>
        /// 保留している処理を空にします。
        /// </summary>
        public void Clear()
        {
            holdActionDic.Clear();
            holdActionList.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Clear();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
