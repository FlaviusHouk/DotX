using System;
using Cairo;
using DotX.Interfaces;
using DotX.Data;
using DotX.PropertySystem;
using DotX.Extensions;

namespace DotX.Widgets
{
    public class Window : Control, IRootVisual
    {
        public static readonly CompositeObjectProperty CursorProperty =
            CompositeObjectProperty.RegisterProperty<Cursors, Window>(nameof(Cursor),
                                                                      PropertyOptions.Inherits,
                                                                      Cursors.None,
                                                                      changeValueFunc: OnCursorPropertyChanged);

        private static void OnCursorPropertyChanged(Window window, Cursors oldValue, Cursors newValue)
        {
            window.WindowImpl.SetCursor(newValue);
        }

        public static readonly CompositeObjectProperty TitleProperty =
            CompositeObjectProperty.RegisterProperty<string, Window>(nameof(Title),
                                                                     PropertyOptions.Inherits,
                                                                     "Window",
                                                                     changeValueFunc: OnTitlePropertyChanged);

        private static void OnTitlePropertyChanged(Window w, string oldValue, string newValue)
        {
            w.WindowImpl.SetTitle(newValue);
        }

        static Window()
        {
            CompositeObjectProperty.OverrideProperty<bool, Window>(IsVisibleProperty,
                                                                   false);

            CompositeObjectProperty.OverrideProperty<IBrush, Window>(BackgroundProperty,
                                                                     default,
                                                                     changeValueFunc: (wind, oldVal, newVal) => wind.WindowImpl.UpdateBackground(newVal));
        }

        private readonly IRenderManager _renderManager;

        //It should be complex figure but for now let it be as just rectangle.
        private Rectangle? _dirtyArea;

        public event Action<Window> Closed;

        public IWindowImpl WindowImpl { get; }

        public Rectangle? DirtyArea => _dirtyArea;

        public Cursors Cursor
        {
            get => GetValue<Cursors>(CursorProperty);
            set => SetValue(CursorProperty, value);
        }

        public string Title
        {
            get => GetValue<string>(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Window()
        {
            WindowImpl = Application.CurrentApp.Platform.CreateWindow(Width,
                                                                      Height);
            WindowImpl.Dirty += WindowDirty;
            WindowImpl.Resizing += Resizing;
            WindowImpl.Closed+= OnWindowClosed;

            Application.AddWindow(this);
            _renderManager = Services.RenderManager;
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
            Measure(new (width, height));

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
            //MarkDirtyArea(dirtyRect);

            _renderManager.Expose(this, dirtyRect);            
        }

        protected override Size MeasureCore(Size size)
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
                _dirtyArea = area;
            else
                _dirtyArea = _dirtyArea.Value.Union(area);
        }

        public void CleanDirtyArea()
        {
            //Some class like Area with complex form should be here.
            _dirtyArea = default;
        }
    }
}