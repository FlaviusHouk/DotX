using System.Linq;
using System.Text;
using DotX.Brush;
using DotX.Data;
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
        }

        protected override Cairo.Rectangle MeasureCore(Cairo.Rectangle size)
        {
            var wholeSize = base.MeasureCore(size);
            var strongRect = new Pango.Rectangle();
            var weakRect = new Pango.Rectangle();

            _layout.GetCursorPos(_charPosition, out strongRect, out weakRect);
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
                return;
            }
            else if(keyEvent.Key == 0xff53 && 
                    _charPosition < _layout.Lines[_linePosition].Length) //right
            {
                _charPosition++;

                if(Text[_charPosition] == '\r' ||
                   Text[_charPosition] == '\n')
                   _linePosition++;

                InvalidateMeasure();
                return;
            }
            else if(keyEvent.Key == 0xff0d)
            {
                _linePosition++;
            }
            else if(keyEvent.Key == 0xff08)
            {
                RemoveChar();
                return;
            }

            string valueToAppend = _inputManager.MapKeyboarKeyValue(keyEvent);

            AppendText(valueToAppend);
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
                currentLine = currentLine.Remove(_charPosition, 1);
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
        }
    }
}