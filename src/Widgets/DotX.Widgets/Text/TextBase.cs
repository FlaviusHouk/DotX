using Cairo;
using DotX.PropertySystem;
using Pango;

namespace DotX.Widgets.Text
{
    public abstract class TextBase : Widget
    {
        protected static Pango.Context _defaultContext;

        static TextBase()
        {
            using var surf = new ImageSurface(Format.RGB24, 1, 1);
            using var ctx = new Cairo.Context(surf);
            _defaultContext = Pango.CairoHelper.CreateContext(ctx); 
        }

        public static readonly CompositeObjectProperty TextProperty =
            CompositeObjectProperty.RegisterProperty<string,TextBase>(nameof(Text),
                                                                       PropertyOptions.Inherits);

        public static readonly CompositeObjectProperty FontSizeProperty =
            CompositeObjectProperty.RegisterProperty<int, TextBase>(nameof(FontSize),
                                                                     PropertyOptions.Inherits,
                                                                     changeValueFunc: OnFontSizePropertyChanged);

        private static void OnFontSizePropertyChanged(TextBase textBlock, int oldValue, int newValue)
        {
            textBlock.RebuildFontDescription();
        }

        public static readonly CompositeObjectProperty FontFamilyProperty =
            CompositeObjectProperty.RegisterProperty<string, TextBase>(nameof(FontFamily),
                                                                        PropertyOptions.Inherits,
                                                                        changeValueFunc: OnFontFamilyPropertyChanged);

        private static void OnFontFamilyPropertyChanged(TextBase textBlock, string oldValue, string newValue)
        {
            textBlock.RebuildFontDescription();
        }

        public static readonly CompositeObjectProperty FontWeightProperty =
            CompositeObjectProperty.RegisterProperty<FontWeight, TextBase>(nameof(FontWeight),
                                                                            PropertyOptions.Inherits);

        public static readonly CompositeObjectProperty TextAlignmentProperty =
            CompositeObjectProperty.RegisterProperty<Alignment, TextBase>(nameof(TextAlignment),
                                                                           PropertyOptions.Inherits);

        protected FontDescription _font;

        public string Text
        {
            get => GetValue<string>(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public int FontSize
        {
            get => GetValue<int>(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public string FontFamily
        {
            get => GetValue<string>(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public FontWeight FontWeight
        {
            get => GetValue<FontWeight>(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public Alignment TextAlignment
        {
            get => GetValue<Alignment>(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        protected void RebuildFontDescription()
        {
            if(_font is not null)
                _font.Dispose();

            _font = new FontDescription()
            {
                Family = FontFamily,
                Size = (int)(FontSize * Pango.Scale.PangoScale),
            };
        }
    }
}