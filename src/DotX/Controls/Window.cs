using System;
using Cairo;
using DotX.Abstraction;

namespace DotX.Controls
{
    public class Window : Control
    {
        static Window()
        {
            CompositeObjectProperty.OverrideProperty<bool, Window>(IsVisibleProperty,
                                                                   false);
        }

        public event Action<Window> Closed;

        public IWindowImpl WindowImpl { get; }

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
            IsMeasureDirty = true;
            Measure(new Rectangle(0,0, width, height));

            IsArrangeDirty = true;
            Invalidate();
        }

        public void Show()
        {
            if(IsVisible)
                return;

            WindowImpl.Resize(Width, Height);
            WindowImpl.Show();
            IsVisible = true;
        }

        private void WindowDirty(RenderEventArgs args)
        {
            Invalidate(new Rectangle(args.X, args.Y, args.Width, args.Height));
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
    }
}