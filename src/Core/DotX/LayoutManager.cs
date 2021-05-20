using System;
using Cairo;
using DotX.Interfaces;
using DotX.Rendering;
using DotX.Threading;

namespace DotX
{
    internal class LayoutManager
    {
        private static readonly Lazy<LayoutManager> _value =
            new Lazy<LayoutManager>(() => new LayoutManager());

        public static LayoutManager Instance => _value.Value;

        private readonly RenderManager _renderManager; 

        private LayoutManager()
        {
            var d = Dispatcher.CurrentDispatcher;
            
            _renderManager = new RenderManager(d);
        }

        public void InvalidateMeasure(Visual visual)
        {
            visual.IsMeasureDirty = true;
            while(visual.VisualParent is not null)
            {
                visual = visual.VisualParent;
                visual.IsMeasureDirty = true;
            }
            
            visual.Measure(visual.DesiredSize);
        }

        public void InvalidateArrange(Visual visual)
        {
            visual.IsArrangeDirty = true;
            while(visual.VisualParent is not null)
            {
                visual = visual.VisualParent;
                visual.IsArrangeDirty = true;
            }

            visual.Arrange(visual.DesiredSize);
        }

        public void InitiateRender(Visual visual, Rectangle? area)
        {
            var originalVisual = visual;

            while(visual is not IRootVisual && visual is not null)
                visual = visual.VisualParent;

            if(visual is null)
                return;

            var window = (IRootVisual)visual;

            if(!window.IsVisible)
                return;

            _renderManager.Invalidate(window, originalVisual, area);
        }
    }
}