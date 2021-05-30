using Pango;
using Cairo;
using System.Linq;
using System;
using DotX.PropertySystem;
using DotX.Widgets.Text;

namespace DotX.Widgets
{
    public class TextBlock : TextBase
    {
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

        protected override void OnStyleApplied(Styling.Style s)
        {
            if(s.Setters.Select(s => s.Property).Any(p => p == nameof(FontSize) || p == nameof(FontFamily)))
                RebuildFontDescription();
        }
    }
}