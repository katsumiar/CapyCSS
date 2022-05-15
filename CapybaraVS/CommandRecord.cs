using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapyCSS
{
    internal class CommandRecord
    {
        struct UndoPoint
        {
            public string title;
            public string text;
        }

        private List<UndoPoint> undoStack = new List<UndoPoint>();
        public bool IsStack => undoStack.Count != 0;
        private int CurrentPoint = -1;

        public bool IsInitialPoint => CurrentPoint <= 0;

        public void Clear()
        {
            undoStack.Clear();
            CurrentPoint = -1;
        }
        
        public bool IsChanges(string str)
        {
            if (!IsStack)
            {
                return true;
            }
            string last = undoStack.Last().text;
            return last != str;
        }

        public void Push(string title, string str)
        {
            if (CurrentPoint != -1 && CurrentPoint + 1 < undoStack.Count)
            {
                undoStack.RemoveRange(CurrentPoint + 1, undoStack.Count - 1);
            }
            undoStack.Add(new UndoPoint { title = title, text = str });
            CurrentPoint = undoStack.Count - 1;
        }
        
        public string Back()
        {
            if (CurrentPoint <= 0)
            {
                return null;
            }
            return undoStack[--CurrentPoint].text;
        }
        
        public string Next()
        {
            if (CurrentPoint >= undoStack.Count - 1)
            {
                return null;
            }
            return undoStack[++CurrentPoint].text;
        }
    }
}
