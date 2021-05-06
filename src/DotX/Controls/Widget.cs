using System.Collections.Generic;
using Cairo;
using DotX.Brush;
using DotX.Data;
using DotX.Extensions;

namespace DotX.Controls
{
    public class Widget : Visual
    {
        static Widget()
        {}
        
        public static readonly CompositeObjectProperty WidthProperty = 
            CompositeObjectProperty.RegisterProperty<int, Widget>(nameof(Width),
                                                                  PropertyOptions.Inherits,
                                                                  coerceFunc: CoerceWidth);

        private static int CoerceWidth(CompositeObject obj, int value)
        {
            return value >= 0 ? value : 0;
        }

        public static readonly CompositeObjectProperty HeightProperty = 
            CompositeObjectProperty.RegisterProperty<int, Widget>(nameof(Height),
                                                                  PropertyOptions.Inherits,
                                                                  coerceFunc: CoerceWidth);

        public static readonly CompositeObjectProperty BackgroundProperty =
            CompositeObjectProperty.RegisterProperty<IBrush, Widget>(nameof(Background),
                                                                     PropertyOptions.Inherits);

        public static readonly CompositeObjectProperty ForegroundProperty =
            CompositeObjectProperty.RegisterProperty<IBrush, Widget>(nameof(Foreground),
                                                                     PropertyOptions.Inherits);

        public static readonly CompositeObjectProperty IsVisibleProperty =
            CompositeObjectProperty.RegisterProperty<bool, Widget>(nameof(IsVisible),
                                                                   PropertyOptions.Inherits,
                                                                   true);

        public static readonly CompositeObjectProperty MarginProperty =
            CompositeObjectProperty.RegisterProperty<Margin, Widget>(nameof(Margin),
                                                                     PropertyOptions.Inherits,
                                                                     new Margin(),
                                                                     changeValueFunc: (w, o, n) => 
                                                                     {
                                                                         w.InvalidateMeasure();
                                                                         w.Invalidate();
                                                                     });

        public static readonly CompositeObjectProperty PaddingProperty =
            CompositeObjectProperty.RegisterProperty<Margin, Widget>(nameof(Padding),
                                                                     PropertyOptions.Inherits,
                                                                     new Margin(),
                                                                     changeValueFunc: (w, o, n) => 
                                                                     {
                                                                         w.Invalidate();
                                                                     });

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

        public IBrush Foreground
        {
            get => GetValue<IBrush>(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public bool IsVisible
        {
            get => GetValue<bool>(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        public Margin Margin
        {
            get => GetValue<Margin>(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        public Margin Padding
        {
            get => GetValue<Margin>(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(!IsVisible)
                new Rectangle();

            return size.Subtract(Padding);
        }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            if(!IsVisible)
                new Rectangle();

            return size.Subtract(Padding);
        }

        public override void Render(Context context)
        {
            if(Background is null)
                return;

            context.Save();

            Background.ApplyTo(context);
            context.Rectangle(RenderSize);
            context.Fill();

            context.Restore();
        }
    }
}