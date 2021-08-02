using System;
using System.Collections.Generic;
using System.Windows.Input;
using DotX.Data;
using DotX.Interfaces;
using DotX.PropertySystem;
using DotX.Widgets.Data;

namespace DotX.Widgets
{
    public class Button : Control, 
                          IObservable<PressedEventArgs>,
                          IFocusable
    {
        public static readonly CompositeObjectProperty CommandProperty =
            CompositeObjectProperty.RegisterProperty<ICommand, Button>(nameof(Command),
                                                                       PropertyOptions.Inherits);

        
        public ICommand Command
        {
            get => GetValue<ICommand>(CommandProperty);
            set => SetValue(CommandProperty, value);
        }        

        public static readonly CompositeObjectProperty CommandParameterProperty =
            CompositeObjectProperty.RegisterProperty<object, Button>(nameof(CommandParameter),
                                                                     PropertyOptions.Inherits);

        public object CommandParameter
        {
            get => GetValue<object>(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private ICollection<IObserver<PressedEventArgs>> _observers =
            new List<IObserver<PressedEventArgs>>();

        public bool Focusable
        {
            get;
            set;
        }

        bool IFocusable.CanFocus => Focusable;

        public bool Focus()
        {
            return true;
        }

        public IDisposable Subscribe(IObserver<PressedEventArgs> observer)
        {
            if(observer is null)
                throw new ArgumentNullException(nameof(observer));

            _observers.Add(observer);
            return new SubscriptionHolder<PressedEventArgs>(_observers, observer);
        }

        public override void OnPointerButton(PointerButtonEvent buttonEvent)
        {
            base.OnPointerButton(buttonEvent);

            if(!buttonEvent.IsPressed || buttonEvent.Key != 1)
                return;

            RaisePressEvent(new PressedEventArgs());
        }

        public override void OnKeyboardEvent(KeyEventArgs keyEvent)
        {
            base.OnKeyboardEvent(keyEvent);

            if(!keyEvent.IsPressed || keyEvent.Key != 0xff8d)
                return;

            RaisePressEvent(new PressedEventArgs());
        }

        protected void RaisePressEvent(PressedEventArgs args)
        {
            Command?.Execute(CommandParameter);

            foreach(var observer in _observers)
                observer.OnNext(args);
        }
    }
}