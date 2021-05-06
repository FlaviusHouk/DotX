using Pango;
using Cairo;
using System.Linq;
using System;

namespace DotX.Controls
{
    public class TextBlock : Widget
    {
        private static Pango.Context _defaultContext;

        static TextBlock()
        {
            using var surf = new ImageSurface(Format.RGB24, 1, 1);
            using var ctx = new Cairo.Context(surf);
            _defaultContext = Pango.CairoHelper.CreateContext(ctx); 
        }

        public static readonly CompositeObjectProperty TextProperty =
            CompositeObjectProperty.RegisterProperty<string,TextBlock>(nameof(Text),
                                                                       PropertyOptions.Inherits);

        public static readonly CompositeObjectProperty FontSizeProperty =
            CompositeObjectProperty.RegisterProperty<int, TextBlock>(nameof(FontSize),
                                                                     PropertyOptions.Inherits,
                                                                     changeValueFunc: OnFontSizePropertyChanged);

        private static void OnFontSizePropertyChanged(TextBlock textBlock, int oldValue, int newValue)
        {
            if(textBlock._font is not null)
                textBlock._font.Dispose();

            textBlock._font = new FontDescription()
            {
                Family = textBlock.FontFamily,
                Size = (int)(newValue * Pango.Scale.PangoScale)
            };
        }

        public static readonly CompositeObjectProperty FontFamilyProperty =
            CompositeObjectProperty.RegisterProperty<string, TextBlock>(nameof(FontFamily),
                                                                        PropertyOptions.Inherits,
                                                                        changeValueFunc: OnFontFamilyPropertyChanged);

        private static void OnFontFamilyPropertyChanged(TextBlock textBlock, string oldValue, string newValue)
        {
            if(textBlock._font is not null)
                textBlock._font.Dispose();

            textBlock._font = new FontDescription()
            {
                Family = newValue,
                Size = (int)(textBlock.FontSize * Pango.Scale.PangoScale),
            };
        }

        public static readonly CompositeObjectProperty FontWeightProperty =
            CompositeObjectProperty.RegisterProperty<FontWeight, TextBlock>(nameof(FontWeight),
                                                                            PropertyOptions.Inherits);

        public static readonly CompositeObjectProperty TextAlignmentProperty =
            CompositeObjectProperty.RegisterProperty<Alignment, TextBlock>(nameof(TextAlignment),
                                                                           PropertyOptions.Inherits);

        private FontDescription _font;

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

        public override void Render(Cairo.Context context)
        {
            base.Render(context);

            using Layout pangoLayout = CairoHelper.CreateLayout(context);
            
            pangoLayout.SetText(Text);
            Foreground.ApplyTo(context);
            context.MoveTo(RenderSize.X + Padding.Left, 
                           RenderSize.Y + Padding.Top);

            pangoLayout.FontDescription = _font;
            pangoLayout.Alignment = TextAlignment;
            
            CairoHelper.ShowLayout(context, pangoLayout);
        }

        protected override Cairo.Rectangle MeasureCore(Cairo.Rectangle size)
        {
            using var layout = new Layout(_defaultContext); 

            if(!_defaultContext.Families.Any(font => font.Name == FontFamily))
            {
                _defaultContext.LoadFont(_font);
            }

            layout.FontDescription = _font;

            layout.SetText(Text);
            layout.GetSize(out int width, out int height);
            
            return new Cairo.Rectangle(size.X, 
                                       size.Y, 
                                       size.Width, 
                                       height / Pango.Scale.PangoScale + Padding.Top + Padding.Bottom);
        }

        protected override Cairo.Rectangle ArrangeCore(Cairo.Rectangle size)
        {
            return DesiredSize;
        }
    }
}