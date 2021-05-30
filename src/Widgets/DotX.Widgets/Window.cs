using System;
using Cairo;
using DotX.Interfaces;
using DotX.Data;
using DotX.PropertySystem;

namespace DotX.Widgets
{
    public class Window : Control, IRootVisual
    {
        static Window()
        {
            CompositeObjectProperty.OverrideProperty<bool, Window>(IsVisibleProperty,
                                                                   false);

            CompositeObjectProperty.OverrideProperty<IBrush, Window>(BackgroundProperty,
                                                                     default,
                                                                     changeValueFunc: (wind, oldVal, newVal) => wind.WindowImpl.UpdateBackground(newVal));
        }

        //It should be complex figure but for now let it be as just rectangle.
        private Rectangle? _dirtyArea;

        public event Action<Window> Closed;

        public IWindowImpl WindowImpl { get; }

        public Rectangle? DirtyArea => _dirtyArea;

        public Window()
        {
            WindowImpl = Application.CurrentApp.Platform.CreateWindow(Width,
                                                                      Height);
            WindowImpl.Dirty += WindowDirty;
            WindowImpl.Resizing += Resizing;
            WindowImpl.Closed+= OnWindowClosed;

            Application.AddWindow(this);
        }

        private void OnWindowClosed()
        {
            WindowImpl.Dirty -= WindowDirty;
            WindowImpl.Resizing -= Resizing;
            WindowImpl.Closed -= OnWindowClosed;

            Closed?.Invoke(this);
        }

        private void Resizing(int width, int height)
        {
            _dirtyArea = default;
            Measure(new Rectangle(0,0, width, height));

            InvalidateArrange(true);
            Invalidate();
        }

        public void Show()
        {
            if(IsVisible)
                return;

            if(!IsInitialized)
                Initialize();

            WindowImpl.Resize(Width, Height);
            WindowImpl.Show();
            IsVisible = true;
        }

        private void WindowDirty(RenderEventArgs args)
        {
            var dirtyRect = new Rectangle(args.X, args.Y, args.Width, args.Height);
            MarkDirtyArea(dirtyRect);

            Invalidate(dirtyRect);
        }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            base.MeasureCore(size);

            return size;
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            base.ArrangeCore(size);

            return size;
        }

        public void Close()
        {
            WindowImpl.Close();
        }

        public void MarkDirtyArea(Rectangle area)
        {
            if(_dirtyArea is null)
            {
                _dirtyArea = area;
            }
            else
            {
                double left = Math.Min(_dirtyArea.Value.X, area.X);
                double top =  Math.Min(_dirtyArea.Value.Y, area.Y);

                double right = Math.Max(_dirtyArea.Value.X + _dirtyArea.Value.Width,
                                        area.X + area.Width);

                double bottom = Math.Max(_dirtyArea.Value.Y + _dirtyArea.Value.Height,
                                        area.Y + area.Height);

                _dirtyArea = new Rectangle(left,
                                           top,
                                           right - left,
                                           bottom - top);
            }
        }

        public void CleanDirtyArea()
        {
            //Some class like Area with complex form should be here.
            _dirtyArea = default;
        }
    }
}