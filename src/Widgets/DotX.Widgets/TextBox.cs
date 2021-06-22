using System.Linq;
using System.Text;
using DotX.Brush;
using DotX.Data;
using DotX.Extensions;
using DotX.Interfaces;
using DotX.PropertySystem;
using DotX.Widgets.Animations;
using DotX.Widgets.Text;
using Pango;

namespace DotX.Widgets
{
    public class TextBox : TextBase, IFocusable
    {
        static TextBox()
        {
            CompositeObjectProperty.OverrideProperty<string, TextBox>(TextProperty,
                                                                      null,
                                                                      changeValueFunc:OnTextPropertyChanged);
        }

        private static void OnTextPropertyChanged(TextBox textBox, 
                                                  string oldValue, 
                                                  string newValue)
        {
            textBox._layout.SetText(newValue);
        }
        
        private readonly TextPointerVisual _textPointer;
        private readonly VisibilityAnimation _pointerAnimation;

        private readonly IInputManager _inputManager;
        private readonly ITimeline _timeline;
        
        private readonly Layout _layout;
        
        private uint _linePosition = 0;
        private int _charPosition = 0;

        public bool CanFocus => true;

        public TextBox()
        {
            _textPointer = new TextPointerVisual();
            _textPointer.VisualParent = this;
            _textPointer.Foreground = new SolidColorBrush(0,0,0);

            _pointerAnimation = new (_textPointer, 650);

            _layout = new Layout(_defaultContext);

            _inputManager = Services.InputManager;
            _timeline = Services.Timeline;
        }

        protected override Cairo.Rectangle MeasureCore(Cairo.Rectangle size)
        {
            var wholeSize = base.MeasureCore(size);
            var strongRect = new Pango.Rectangle();
            var weakRect = new Pango.Rectangle();

            _layout.GetCursorPos(_layout.Lines[_linePosition].StartIndex + _charPosition, 
                                 out strongRect, 
                                 out weakRect);
            double height = strongRect.Height / Pango.Scale.PangoScale;

            _textPointer.Measure(new Cairo.Rectangle(strongRect.X / Pango.Scale.PangoScale + size.X,
                                                     strongRect.Y / Pango.Scale.PangoScale + size.Y,
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

            CairoHelper.ShowLayout(context, renderLayout);

            if(_textPointer.IsVisible)
            {
                context.Save();
                context.Rectangle(_textPointer.RenderSize);
                context.Clip();

                _textPointer.Render(context);

                context.Restore();
            }
        }

        public bool Focus()
        {
            return true;
        }

        public override void OnKeyboardEvent(KeyEventArgs keyEvent)
        {
            base.OnKeyboardEvent(keyEvent);

            if(!keyEvent.IsPressed)
                return;

            if(keyEvent.Key == 0xff51 && 
               _charPosition > 0) //left
            {
                _charPosition--;

                if(Text[_charPosition] == '\r' ||
                   Text[_charPosition] == '\n')
                   _linePosition--;

                InvalidateMeasure();
                InvalidateTextPointer();
                return;
            }
            else if(keyEvent.Key == 0xff53 && 
                    (_charPosition < _layout.Lines[_linePosition].Length ||
                    _linePosition < _layout.LineCount)) //right
            {
                if(Text[_charPosition] == '\r' ||
                   Text[_charPosition] == '\n')
                {
                   _linePosition++;
                   _charPosition = 0;
                }
                else
                {
                    _charPosition++;
                }

                InvalidateMeasure();
                InvalidateTextPointer();
                return;
            }
            else if(keyEvent.Key == 0xff08)
            {
                RemoveChar();
                return;
            }

            string valueToAppend = _inputManager.MapKeyboarKeyValue(keyEvent);

            AppendText(valueToAppend);

            if(keyEvent.Key == 0xff0d)
            {
                _linePosition++;
                InvalidateMeasure();
            }
        }

        public override void OnPointerEnter(PointerMoveEventArgs eventArgs)
        {
            Window root = default;
            this.TraverseTop<Window>(c => 
            {
                root = c;
                return true;
            });

            root.Cursor = Cursors.Text;
        }

        public override void OnPointerLeave(PointerMoveEventArgs eventArgs)
        {
            Window root = default;
            this.TraverseTop<Window>(c => 
            {
                root = c;
                return true;
            });

            root.Cursor = Cursors.None;
        }

        public override void OnPointerButton(PointerButtonEvent buttonEvent)
        {
            const int LeftButton = 1;
            base.OnPointerButton(buttonEvent);

            if(!buttonEvent.IsPressed)
                return;

            if(buttonEvent.Key == LeftButton)
            {
                int x = (int)((buttonEvent.X - RenderSize.X) * Pango.Scale.PangoScale);
                int y = (int)((buttonEvent.Y - RenderSize.Y) * Pango.Scale.PangoScale);

                bool isInside = 
                    _layout.XyToIndex(x, y,
                                      out int index,
                                      out int trailing);

                if(!isInside)
                    return;

                int i = 0;
                var currentLine = _layout.Lines[0];
                for(;i<_layout.LineCount - 1; i++)
                {
                    if(_layout.Lines[i+1].StartIndex > index)
                        break;

                    currentLine = _layout.Lines[i];
                }

                _linePosition = (uint)i;
                _charPosition = index - currentLine.StartIndex;
                InvalidateMeasure();
                InvalidateTextPointer();
            }
        }

        private void AppendText(string valueToAppend)
        {
            int linesLen = 0;
            if(_linePosition > 0)
            {
                linesLen = _layout.Lines.Take(((int)_linePosition) - 1)
                                        .Select(l => l.Length)
                                        .Sum();
            }
            
            if(Text is not null &&
               valueToAppend != "\r" && valueToAppend != "\n" &&
               _charPosition < _layout.Lines[_linePosition].Length)
            {
                Text = Text.Insert(_charPosition + linesLen, valueToAppend);
            }
            else
            {
                Text = string.Concat(Text, valueToAppend);
            }

            _charPosition += valueToAppend.Length;
            InvalidateMeasure();
            InvalidateTextPointer();
        }

        private void RemoveChar()
        {
            string[] lines = Text.Split('\n');
            string currentLine = lines[_linePosition];
            uint newCurrent = _linePosition;
            
            if(string.IsNullOrEmpty(currentLine))
            {
                if(_linePosition == 0)
                    return;

                newCurrent = _linePosition - 1;
                currentLine = lines[newCurrent];;
            }
            else
            {
                if(currentLine.Length == _charPosition)
                    currentLine = currentLine.Substring(0, currentLine.Length - 1);
                else
                    currentLine = currentLine.Remove(_charPosition - 1, 1);

                _charPosition--;
            }

            var sb = new StringBuilder();
            for(int i = 0; i<lines.Length; i++)
            {
                if(i > 0)
                    sb.AppendLine();

                if(i == newCurrent)
                {
                    if(_linePosition != newCurrent)
                    {
                        i++;
                    }

                    sb.Append(currentLine);
                    continue;
                }

                sb.Append(lines[i]);
            }

            Text = sb.ToString();
            InvalidateMeasure();
            InvalidateTextPointer();
        }

        private void InvalidateTextPointer()
        {
            _timeline.Reset(_pointerAnimation);
            
            if(_textPointer.IsVisible)
                Invalidate();
            else
                _textPointer.IsVisible = true;
        }
    }
}