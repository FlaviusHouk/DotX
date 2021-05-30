using Cairo;
using DotX.Brush;
using DotX.Extensions;
using DotX.Widgets.Animations;
using DotX.Widgets.Text;
using Pango;

namespace DotX.Widgets
{
    public class TextBox : TextBase
    {
        private readonly TextPointerVisual _textPointer;
        private readonly VisibilityAnimation _pointerAnimation;
        
        private readonly Layout _layout;
        
        private uint _linePosition = 0;
        private uint _charPosition = 0;

        public TextBox()
        {
            _textPointer = new TextPointerVisual();
            _textPointer.VisualParent = this;
            _textPointer.Foreground = new SolidColorBrush(0,0,0);

            _pointerAnimation = new (_textPointer, 650);

            _layout = new Layout(_defaultContext);
            _layout.SetText(" ");
        }

        protected override Cairo.Rectangle MeasureCore(Cairo.Rectangle size)
        {
            var wholeSize = base.MeasureCore(size);
            var ink_rect = new Pango.Rectangle();
            var logical_rect = new Pango.Rectangle();

            _layout.Lines[_linePosition].GetExtents(ref ink_rect,
                                                    ref logical_rect);

            double height = logical_rect.Height / Pango.Scale.PangoScale;

            _textPointer.Measure(new Cairo.Rectangle(logical_rect.X / Pango.Scale.PangoScale + size.X,
                                                     logical_rect.Y / Pango.Scale.PangoScale + size.Y + height,
                                                     1,
                                                     height));

            return wholeSize;
        }

        protected override Cairo.Rectangle ArrangeCore(Cairo.Rectangle size)
        {
            var wholeSize = base.ArrangeCore(size);
            _textPointer.Arrange(size);
            return wholeSize;
        }

        public override void Render(Cairo.Context context)
        {
            using var renderLayout = _layout.Copy();

            base.Render(context);

            context.MoveTo(RenderSize.X + Padding.Left, 
                           RenderSize.Y + Padding.Top);

            if(_textPointer.IsVisible)
                _textPointer.Render(context);

            CairoHelper.ShowLayout(context, renderLayout);
        }
    }
}