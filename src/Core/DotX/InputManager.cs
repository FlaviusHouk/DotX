using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Interfaces;
using DotX.Data;

namespace DotX
{
    internal class InputManager : IInputManager
    {
        private readonly List<Visual> _currentlyHoveredVisuals = 
            new();

        //TODO: Use Focus manager for this?
        private IFocusable _focusedElement;

        public void DispatchPointerMove(Visual visualToTest, PointerMoveEventArgs pointerMoveEventArgs)
        {
            var hitTest = new HitTestResult(pointerMoveEventArgs.X, 
                                            pointerMoveEventArgs.Y);
            
            visualToTest.HitTest(hitTest);
            Visual[] notHoveredAnymore = _currentlyHoveredVisuals.Except(hitTest.Result)
                                                                 .ToArray();

            foreach(var found in hitTest.Result)
            {
                var foundWidget = found as IInputElement;

                if(!_currentlyHoveredVisuals.Contains(found))
                {
                    _currentlyHoveredVisuals.Add(found);

                    foundWidget?.OnPointerEnter(pointerMoveEventArgs);                    
                }

                foundWidget?.OnPointerMove(pointerMoveEventArgs);
            }

            foreach(var visual in notHoveredAnymore)
            {
                var foundWidget = visual as IInputElement;

                _currentlyHoveredVisuals.Remove(visual);
                foundWidget?.OnPointerLeave(pointerMoveEventArgs);
            }
        }

        public void DispatchKeyEvent(IRootVisual root, KeyEventArgs args)
        {
            if(_focusedElement is IInputElement inputElement)
                inputElement.OnKeyboardEvent(args);
        }

        public void DispatchPointerEvent(IRootVisual windowControl, PointerButtonEvent args)
        {
            var hitTest = new HitTestResult(args.X, 
                                            args.Y);

            var actualVisual = (Visual)windowControl;
            actualVisual.HitTest(hitTest);

            bool focusSet = false;
            foreach(Visual v in hitTest.Result)
            {
                if(v is IFocusable focusable && !focusSet)
                {
                    if(focusable.Focus())
                    {
                        _focusedElement = focusable;
                        focusSet = true;
                    }
                }

                if(v is IInputElement inputElement)
                    inputElement.OnPointerButton(args);
            }
        }

        public virtual string MapKeyboarKeyValue(KeyEventArgs keyEventArgs)
        {
            return Application.CurrentApp.Platform.MapKeyToInput(keyEventArgs);
        }
    }
}