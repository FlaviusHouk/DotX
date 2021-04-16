using System;
using Cairo;
using DotX.Controls;

namespace DotX
{
    internal class LayoutManager
    {
        private static readonly Lazy<LayoutManager> _value =
            new Lazy<LayoutManager>(() => new LayoutManager());

        public static LayoutManager Instance => _value.Value;

        private LayoutManager()
        {}

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

        public void InitiateRender(Visual visual)
        {
            var originalVisual = visual;

            while(visual is not Window)
                visual = visual.VisualParent;

            using Context c = ((Window)visual).WindowImpl.CreateContext();
            c.Rectangle(originalVisual.RenderSize);
            c.Clip();

            originalVisual.Render(c);
        }
    }
}