using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using DotX.Data;
using DotX.Extensions;

namespace DotX.Widgets
{
    public class StackPanel : Panel
    {
        private static Size GetRestSize(Size occupiedSize,
                                        Size givenSize,
                                        Orientation orientation)
        {
            double width, height;

            if (orientation == Orientation.Vertical)
            {
                width = givenSize.Width;
                height = givenSize.Height - occupiedSize.Height;

                if (height < 0)
                    height = 0;
            }
            else
            {
                width = givenSize.Width - occupiedSize.Width;
                height = givenSize.Height;

                if (width < 0)
                    width = 0;
            }

            return new(width, height);
        }

        private static Rectangle Merge(IEnumerable<Rectangle> rectangles, Orientation orientation)
        {
            double x, y, width, height;

            if (orientation == Orientation.Vertical)
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

        private static Size Merge(IEnumerable<Size> rectangles, Orientation orientation)
        {
            double width, height;

            if (orientation == Orientation.Vertical)
            {
                width = rectangles.Select(rect => rect.Width).Max();
                height = rectangles.Select(rect => rect.Height).Sum();
            }
            else
            {
                width = rectangles.Select(rect => rect.Width).Sum();
                height = rectangles.Select(rect => rect.Height).Max();
            }

            return new(width, height);
        }

        public Widgets.Orientation Orientation { get; set; }

        protected override Size MeasureCore(Size size)
        {
            if (Children.Count < 1)
                return new(0, 0);

            Size adjustedSize = size.Subtract(Padding);

            var rects = new List<Size>(Children.Count);
            foreach (var child in Children)
            {
                Margin margin = Padding;
                if(child is Widget w)
                    margin = margin + w.Margin;

                child.Measure(adjustedSize.Subtract(margin));

                Size accumRect = 
                    child.DesiredSize.Add(margin);

                adjustedSize =
                    GetRestSize(accumRect,
                                adjustedSize,
                                Orientation);

                rects.Add(accumRect);
            }
            
            return base.MeasureCore(Merge(rects, Orientation));
        }

        protected override Rectangle ArrangeCore(Rectangle size)
        {
            Logger.LogLayoutSystemEvent("Arranging {0}. Having x - {1}, y - {2}, w - {3}, h - {4}",
                                        NameToLog,
                                        size.X,
                                        size.Y,
                                        size.Width,
                                        size.Height);
                                        
            if (Children.Count < 1)
                return new Rectangle(size.X, size.Y, 0, 0);

            Rectangle allSize = size;
            bool isHorizontal = Orientation == Orientation.Horizontal;
            double x = size.X + Padding.Left,
                   y = size.Y + Padding.Top,
                   width = size.Width - Padding.Left - Padding.Right,
                   height = size.Height - Padding.Top - Padding.Bottom,
                   maxWidth = 0, maxHeight = 0;

            foreach (var child in Children)
            {
                Margin margin = Padding;
                if(child is Widget w)
                {
                    margin = margin + w.Margin;
                }

                Rectangle givenRect = 
                    new (x, y,
                         isHorizontal ?
                             child.DesiredSize.Width :
                             width,
                         isHorizontal ?
                             height :
                             child.DesiredSize.Height);

                child.Arrange(givenRect.Subtract(margin));

                if (Orientation == Orientation.Vertical)
                {
                    double accumWidth = 
                        child.RenderSize.Width + margin.Left + margin.Right;

                    y += child.RenderSize.Height + margin.Top + margin.Bottom;
                    width = size.Width;
                    height = size.Height - child.RenderSize.Height - margin.Top - margin.Bottom;

                    if(maxWidth < accumWidth)
                        maxWidth = accumWidth;

                    if (height < 0)
                        height = 0;
                }
                else
                {
                    double accumHeight =
                        child.RenderSize.Height + margin.Top + margin.Bottom;

                    x += child.RenderSize.Width + margin.Left + margin.Right;
                    width = size.Width - child.RenderSize.Width - margin.Left - margin.Right;
                    height = size.Height;

                    if(maxHeight < accumHeight)
                        maxHeight = accumHeight;

                    if (width < 0)
                        width = 0;
                }
            }

            if (Orientation == Orientation.Horizontal)
            {
                return new Rectangle(allSize.X,
                                     allSize.Y,
                                     allSize.Width,
                                     maxHeight);
            }
            else
            {
                return new Rectangle(allSize.X,
                                     allSize.Y,
                                     maxWidth,
                                     allSize.Height);
            }
        }
    }
}