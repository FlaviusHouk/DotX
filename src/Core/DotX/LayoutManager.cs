using System;
using Cairo;
using DotX.Extensions;
using DotX.Interfaces;

namespace DotX
{
    public class LayoutManager : ILayoutManager
    {
        private readonly IRenderManager _renderManager; 

        public LayoutManager()
        {
            _renderManager = Services.RenderManager;
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

        public void InvalidateArrange(Visual visual, Rectangle prevRect)
        {
            visual.IsArrangeDirty = true;

            visual.Arrange(prevRect);
        }

        public void InitiateRender(Visual visual, Rectangle? area)
        {
            var originalVisual = visual;
            var window = visual as IRootVisual;

            if (window is null)
            {
                visual.TraverseTop<IRootVisual>(current =>
                {
                    window = current;
                    return true;
                });
            }

            if(window is null || !window.IsVisible)
                return;

            _renderManager.Invalidate(window, originalVisual, area);
        }
    }
}