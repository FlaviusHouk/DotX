using System.Collections.Generic;

//Is it OK here?
using DotX.Controls;
using DotX.Data;

namespace DotX.Abstraction
{
    public class HitTestResult
    {
        private readonly List<Visual> _foundVisuals =
            new List<Visual>();
        public IReadOnlyCollection<Visual> Result => _foundVisuals;

        public Point<int> PointToTest { get; }

        public HitTestResult(int x, int y)
        {
            PointToTest = new Point<int>(x, y);
        }

        public void AddVisual(Visual foundVisual)
        {
            _foundVisuals.Add(foundVisual);
        }
    }
}