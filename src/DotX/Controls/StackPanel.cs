using System.Collections.Generic;
using System.Linq;
using Cairo;

namespace DotX.Controls
{
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

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

        public Orientation Orientation { get; set; }

        protected override Rectangle MeasureCore(Rectangle size)
        {
            if(Children.Count < 1 || !IsVisible)
                return new Rectangle(size.X, size.Y, 0, 0);

            var firstChild = Children.First();
            bool wasDirty = firstChild.IsMeasureDirty;
            firstChild.Measure(size);
            size = GetRestSize(firstChild.DesiredSize, size, Orientation);

            foreach(var child in Children.Skip(1))
            {
                wasDirty |= child.IsMeasureDirty;
                child.IsMeasureDirty |= wasDirty;

                child.Measure(size);
                size = GetRestSize(child.DesiredSize, size, Orientation);
            }

            return Merge(Children.Select(c => c.DesiredSize), Orientation);
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            if(Children.Count < 1 || !IsVisible)
                return new Rectangle(size.X, size.Y, 0,0);

            var firstChild = Children.First();
            bool wasDirty = firstChild.IsArrangeDirty;
            firstChild.Arrange(size);
            size = GetRestSize(firstChild.RenderSize, size, Orientation);

            foreach(var child in Children.Skip(1))
            {
                wasDirty |= child.IsArrangeDirty;
                child.IsArrangeDirty |= wasDirty;

                child.Arrange(size);
                size = GetRestSize(child.RenderSize, size, Orientation);
            }

            return Merge(Children.Select(c => c.RenderSize), Orientation);
        }
    }
}