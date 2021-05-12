using System;
using System.Collections.Generic;
using System.Linq;
using DotX.Abstraction;
using DotX.Controls;

namespace DotX
{
    public class InputManager
    {
        private static readonly Lazy<InputManager> _value =
            new Lazy<InputManager>(() => new InputManager()); 

        public static InputManager Instance => _value.Value;

        private readonly List<Visual> _currentlyHoveredVisuals = 
            new();

        private InputManager()
        {}

        internal void DispatchPointerMove(Window windowControl, PointerMoveEventArgs pointerMoveEventArgs)
        {
            var hitTest = new HitTestResult(pointerMoveEventArgs.X, 
                                            pointerMoveEventArgs.Y);
            
            windowControl.HitTest(hitTest);
            Visual[] notHoveredAnymore = _currentlyHoveredVisuals.Except(hitTest.Result)
                                                                 .ToArray();

            foreach(var found in hitTest.Result)
            {
                var foundWidget = found as Widget;

                if(!_currentlyHoveredVisuals.Contains(found))
                {
                    _currentlyHoveredVisuals.Add(found);

                    foundWidget?.OnPointerEnter(pointerMoveEventArgs);                    
                }

                foundWidget?.OnPointerMove(pointerMoveEventArgs);
            }

            foreach(var visual in notHoveredAnymore)
            {
                var foundWidget = visual as Widget;

                _currentlyHoveredVisuals.Remove(visual);
                foundWidget?.OnPointerLeave(pointerMoveEventArgs);
            }
        }
    }
}