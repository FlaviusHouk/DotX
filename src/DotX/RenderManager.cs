using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cairo;
using DotX.Controls;
using DotX.Threading;

namespace DotX
{
    internal class RenderManager
    {
        private readonly Dispatcher _mainDispatcher;
        private readonly Thread _renderThread;

        private readonly ManualResetEventSlim _threadLocker =
            new ManualResetEventSlim();
        private readonly Dictionary<Window, (ImageSurface, object)> _windowBuffers =
            new Dictionary<Window, (ImageSurface, object)>();

        //Add class/struct for it?
        private readonly ConcurrentBag<(Visual, Window, Surface, Rectangle, object)> _visualsToUpdate =
            new ConcurrentBag<(Visual, Window, Surface, Rectangle, object)>();

        public RenderManager(Dispatcher mainThread)
        {
            _mainDispatcher = mainThread;

            _renderThread = new Thread(RenderLoop);
            
            _renderThread.Start();
        }

        public void Invalidate(Window window,
                               Visual visualToInvalidate,
                               Rectangle? area)
        {
            area ??= visualToInvalidate.RenderSize;

            ImageSurface windowBuffer;
            object locker;
            
            (windowBuffer, locker) = InvalidateWindowBuffer(window);

            _visualsToUpdate.Add((visualToInvalidate, window, windowBuffer, area.Value, locker));

            _threadLocker.Set();
        }

        private (ImageSurface, object) InvalidateWindowBuffer(Window window)
        {
            ImageSurface bufferSurface;
            object locker;

            if(!_windowBuffers.TryGetValue(window, out var pair))
            {
                bufferSurface = new ImageSurface(Format.Argb32, 
                                                 (int)window.RenderSize.Width,
                                                 (int)window.RenderSize.Height);
                
                locker = new object();

                _windowBuffers.Add(window,
                                   (bufferSurface, locker));

                return (bufferSurface, locker);
            }

            (bufferSurface, locker) = pair;

            if(bufferSurface.Width < window.RenderSize.Width ||
               bufferSurface.Height < window.RenderSize.Height)
            {
                _windowBuffers.Remove(window);

                int newWidth = bufferSurface.Width * 2;
                int newHeight = bufferSurface.Height * 2;

                lock(locker)
                {
                    bufferSurface.Dispose();

                    bufferSurface = new ImageSurface(Format.Argb32, 
                                                     newWidth,
                                                     newHeight);
                }

                _windowBuffers.Add(window, (bufferSurface, locker));
            }

            return (bufferSurface, locker);
        }

        private void RenderLoop(object obj)
        {
            while(true)
            {
                if(!_visualsToUpdate.TryTake(out var drawRequest))
                {
                    _threadLocker.Reset();
                    _threadLocker.Wait();
                }

                Visual visualToRedraw;
                Window owner;
                Surface buffer;
                Rectangle areaToUpdate;
                object locker;

                (visualToRedraw, owner, buffer, areaToUpdate, locker) = drawRequest;

                if(buffer is null)
                    continue;

                lock(locker)
                {
                    using (var context = new Context(buffer))
                    {
                        context.Rectangle(visualToRedraw.RenderSize);
                        context.Clip();

                        visualToRedraw.Render(context);
                    }
                }

                _mainDispatcher.BeginInvoke(() => FlushToActualSurface(areaToUpdate, 
                                                                       buffer, 
                                                                       owner.WindowImpl.WindowSurface,
                                                                       locker), 
                                            OperationPriority.Render);
            }
        }

        private void FlushToActualSurface(Rectangle area, 
                                          Surface bufferSurface,
                                          Surface actualSurface,
                                          object locker)
        {
            using var context = new Context(actualSurface);
            
            lock(locker)
            {
                context.SetSource(bufferSurface);
                context.Rectangle(area);
                context.Fill();
            }
        }
    }
}