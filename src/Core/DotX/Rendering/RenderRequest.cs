using Cairo;
using DotX.Interfaces;
using System.Threading;

namespace DotX.Rendering
{
    internal record RenderRequest(Visual VisualToInvalidate, 
                                  IRootVisual Root,
                                  Rectangle AreaToUpdate,
                                  bool Redraw = true)
    {
        private int _isCanceled;

        public bool IsCanceled => (_isCanceled & 1) != 0;

        public void Cancel()
        {
            Interlocked.Exchange(ref _isCanceled, 1);
        }
    }
}