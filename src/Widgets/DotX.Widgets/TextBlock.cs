using Pango;
using Cairo;
using System.Linq;
using System;
using DotX.PropertySystem;
using DotX.Widgets.Text;
using DotX.Data;
using DotX.Extensions;

namespace DotX.Widgets
{
    public class TextBlock : TextBase
    {
        protected override void OnRender(Cairo.Context context)
        {
            base.OnRender(context);

            using Layout pangoLayout = CairoHelper.CreateLayout(context);
            
            pangoLayout.SetText(Text);
            Foreground.ApplyTo(context);
            context.MoveTo(RenderSize.X + Padding.Left, 
                           RenderSize.Y + Padding.Top);

            pangoLayout.FontDescription = _font;
            pangoLayout.Alignment = TextAlignment;
            
            CairoHelper.ShowLayout(context, pangoLayout);
        }

        protected override Size MeasureCore(Size size)
        {
            using var layout = new Layout(_defaultContext); 
            
            if(!_defaultContext.Families.Any(font => font.Name == FontFamily))
            {
                _defaultContext.LoadFont(_font);
            }

            layout.FontDescription = _font;

            layout.SetText(Text);
            layout.GetSize(out int width, out int height);

            return base.MeasureCore(
                new (width / Pango.Scale.PangoScale + Padding.Left + Padding.Right, 
                     height / Pango.Scale.PangoScale + Padding.Top + Padding.Bottom));
        }

        protected override Cairo.Rectangle ArrangeCore(Cairo.Rectangle size)
        {
            if(Stretch == StretchBehavior.Stretch)
                return size;

            return DesiredSize.ToRectangle(size.X,
                                           size.Y);
        }

        protected override void OnStyleApplied(Styling.Style s)
        {
            if(s.Setters.Select(s => s.Property).Any(p => p == nameof(FontSize) || p == nameof(FontFamily)))
                RebuildFontDescription();
        }
    }
}