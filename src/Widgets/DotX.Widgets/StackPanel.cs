using System.Collections.Generic;
using System.Linq;
using Cairo;
using DotX.Extensions;

namespace DotX.Widgets
{
    public class StackPanel : Panel
    {
        private static Rectangle GetRestSize(Rectangle occupiedSize,
                                             Rectangle givenSize,
                                             Orientation orientation)
        {
            double x, y, width, height;

            if(orientation == Orientation.Vertical)
            {
                x = givenSize.X;
                y = givenSize.Y + occupiedSize.Height;
                width = givenSize.Width;
                height = givenSize.Height - occupiedSize.Height;
                
                if(height < 0)
                    height = 0;
            }
            else
            {
                x = givenSize.X + occupiedSize.Width;
                y = givenSize.Y;
                width = givenSize.Width - occupiedSize.Width;
                height = givenSize.Height;
                
                if(width < 0)
                    width = 0;
            }

            return new Rectangle(x, y, width, height);
        }

        private static Rectangle Merge(IEnumerable<Rectangle> rectangles, Orientation orientation)
        {
            double x, y, width, height;

            if(orientation == Orientation.Vertical)
            {
                x = rectangles.Select(rect => rect.X).Min();
                y = rectangles.Select(rect => rect.Y).Min();
                width = rectangles.Select(rect => rect.Width).Max();
                height = rectangles.Select(rect => rect.Height).Sum();
            }
            else
            {
                x = rectangles.Select(rect => rect.X).Min();
                y = rectangles.Select(rect => rect.Y).Min();
                width = rectangles.Select(rect => rect.Width).Sum();
                height = rectangles.Select(rect => rect.Height).Max();
            }

            return new Rectangle(x, y, width, height);
        }

        public Widgets.Orientation Orientation { get; set; }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            if(Children.Count < 1 || !IsVisible)
                return new Rectangle(size.X, size.Y, 0, 0);

            size = size.Subtract(Padding);

            var firstChild = Children.First();

            size = MeasureChild(firstChild, size);

            foreach(var child in Children.Skip(1))
                size = MeasureChild(child, size);

            return Merge(Children.Select(c => c is Widget w ?
                                                w.DesiredSize.Add(w.Margin) :
                                                c.DesiredSize), 
                                        Orientation).Add(Padding);
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(Children.Count < 1 || !IsVisible)
                return new Rectangle(size.X, size.Y, 0,0);

            size = size.Subtract(Padding);

            var firstChild = Children.First();

            size = ArrangeChild(firstChild, size);

            foreach(var child in Children.Skip(1))
                size = ArrangeChild(child, size);

            return Merge(Children.Select(c => c is Widget w ?
                                                w.RenderSize.Add(w.Margin) : 
                                                c.RenderSize), 
                         Orientation).Add(Padding);
        }

        private Rectangle MeasureChild(Visual child, Rectangle size)
        {
            Widget widget = default;
            Rectangle adjustedSize = size;
            
            if(child is Widget)
            {
                widget = (Widget)child;
                adjustedSize = size.Subtract(widget.Margin);
            }

            child.Measure(adjustedSize);
            
            Rectangle desiredSize = child.DesiredSize;
            if(widget is not null)
                desiredSize = desiredSize.Add(widget.Margin);

            return GetRestSize(desiredSize, 
                               size, 
                               Orientation);
        }

        private Rectangle ArrangeChild(Visual child, Rectangle size)
        {
            Widget widget = default;
            Rectangle adjustedSize = size;
            
            if(child is Widget)
            {
                widget = (Widget)child;
                adjustedSize = size.Subtract(widget.Margin);
            }

            child.Arrange(adjustedSize);
            
            Rectangle renderSize = child.RenderSize;
            if(widget is not null)
                renderSize = renderSize.Add(widget.Margin);

            return GetRestSize(renderSize, 
                               size, 
                               Orientation);
        }
    }
}