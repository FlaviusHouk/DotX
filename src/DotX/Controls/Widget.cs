using System.Collections.Generic;
using Cairo;
using DotX.Brush;

namespace DotX.Controls
{
    public class Widget : Visual
    {
        static Widget()
        {}
        
        public static readonly CompositeObjectProperty WidthProperty = 
            TypedObjectProperty<int>.RegisterProperty<Widget>(nameof(Width),
                                                              PropertyOptions.Inherits,
                                                              coerceFunc: CoerceWidth);

        private static int CoerceWidth(CompositeObject obj, int value)
        {
            return value >= 0 ? value : 0;
        }

        public static readonly CompositeObjectProperty HeightProperty = 
            TypedObjectProperty<int>.RegisterProperty<Widget>(nameof(Height),
                                                              PropertyOptions.Inherits,
                                                              coerceFunc: CoerceWidth);

        public static readonly CompositeObjectProperty BackgroundProperty =
            TypedObjectProperty<IBrush>.RegisterProperty<Widget>(nameof(Background),
                                                                 PropertyOptions.Inherits);

        public Widget LogicalParent { get; internal set; }
        public ICollection<Visual> VisualChildren { get; }

        public int Width 
        {
            get => GetValue<int>(WidthProperty);
            set => SetValue<int>(WidthProperty, value);
        }

        public int Height 
        {
            get => GetValue<int>(HeightProperty);
            set => SetValue<int>(HeightProperty, value);
        }

        public IBrush Background
        {
            get => GetValue<IBrush>(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        protected override void ArrangeCore(Rectangle size)
        {}

        protected override Rectangle MeasureCore(Rectangle size)
        {
            return size;
        }

        public override void Render(Context context)
        {
            Background?.Render(context, RenderSize.Width, RenderSize.Height);
        }
    }
}