using System;
using Cairo;
using DotX.Extensions;
using DotX.Interfaces;

namespace DotX
{
    internal class LayoutManager
    {
        private static readonly Lazy<LayoutManager> _value =
            new Lazy<LayoutManager>(() => new LayoutManager());

        public static LayoutManager Instance => _value.Value;

        private readonly IRenderManager _renderManager; 

        private LayoutManager()
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

        public void InvalidateArrange(Visual visual)
        {
            visual.IsArrangeDirty = true;
            while(visual.VisualParent is not null)
            {
                visual = visual.VisualParent;
                visual.IsArrangeDirty = true;
            }

            Rectangle arrangeRect = 
                visual.DesiredSize.ToRectangle(visual.RenderSize.X, 
                                               visual.RenderSize.Y);

            visual.Arrange(arrangeRect);
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