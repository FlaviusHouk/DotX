using System;

namespace DotX.PropertySystem
{
    //TODO: prepare interface for binding and possible integration
    //with 3rd-party library.
    public class CompositeObjectPropertyBinding : IObserver<CompositeObjectProperty>, IDisposable
    {
        private readonly CompositeObject _source;
        private readonly CompositeObject _target;
        private readonly CompositeObjectProperty _sourceProperty;
        private readonly CompositeObjectProperty _targetProperty;
        private readonly IDisposable _subscription;

        public CompositeObjectPropertyBinding(CompositeObject source,
                                              CompositeObject target,
                                              CompositeObjectProperty sourceProperty,
                                              CompositeObjectProperty targetProperty)
        {
            _source = source;
            _subscription = _source.Subscribe(this);

            _target = target;
            _sourceProperty = sourceProperty;
            _targetProperty = targetProperty;

            TransferValue();
        }

        public void Dispose()
        {
            _subscription.Dispose();
        }

        public void OnCompleted()
        {}

        public void OnError(Exception error)
        {}

        public void OnNext(CompositeObjectProperty value)
        {
            if(value != _targetProperty)
                return;

            TransferValue();
        }

        private void TransferValue()
        {
            var propVal = _source.GetValue(_targetProperty);
            _target.SetValue(_targetProperty, propVal);
        }
    }
}