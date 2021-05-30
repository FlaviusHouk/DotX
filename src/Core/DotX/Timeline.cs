using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using DotX.Interfaces;
using DotX.Threading;

namespace DotX
{
    public class Timeline : ITimeline
    {
        private static readonly Lazy<ITimeline> _timelineInstance =
            new(() => new Timeline());

        public static ITimeline Instance => _timelineInstance.Value;

        private readonly IDictionary<IAnimatable, Timer> _timerDictionary
            = new Dictionary<IAnimatable, Timer>();
        private readonly Dispatcher _dispatcher;

        private Timeline()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public void Register(IAnimatable animatable)
        {
            if(_timerDictionary.ContainsKey(animatable))
                throw new Exception();

            _timerDictionary.Add(animatable, new Timer(state => 
            {
                _dispatcher.BeginInvoke(() => ((IAnimatable)state).Tick(),
                                        OperationPriority.Background);

            }, animatable, animatable.Period, animatable.Period));
        }
    }
}