using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cairo;
using DotX.Threading;
using DotX.Interfaces;
using DotX.Extensions;

namespace DotX
{
    internal class RenderManager
    {
        private readonly Dispatcher _mainDispatcher;
        private readonly Thread _renderThread;

        private readonly ManualResetEventSlim _threadLocker =
            new ManualResetEventSlim();
        private readonly Dictionary<Visual, (ImageSurface, object)> _windowBuffers =
            new Dictionary<Visual, (ImageSurface, object)>();

        //Add class/struct for it?
        private readonly ConcurrentBag<(Visual, Surface, Surface, Rectangle, object)> _visualsToUpdate =
            new ConcurrentBag<(Visual, Surface, Surface, Rectangle, object)>();

        public RenderManager(Dispatcher mainThread)
        {
            _mainDispatcher = mainThread;

            _renderThread = new Thread(RenderLoop);
            
            _renderThread.Start();
        }

        public void Invalidate(IRootVisual root,
                               Visual visualToInvalidate,
                               Rectangle? area)
        {
            area ??= visualToInvalidate.RenderSize;

            Services.Logger.LogRender("Render request received. Area to update is {0}.", area);

            ImageSurface windowBuffer;
            object locker;
            
            (windowBuffer, locker) = InvalidateWindowBuffer((Visual)root);

            Services.Logger.LogRender("Buffer surface has size {0}x{1}.", windowBuffer.Width, windowBuffer.Handle);

            _visualsToUpdate.Add((visualToInvalidate, 
                                  root.WindowImpl.WindowSurface, 
                                  windowBuffer, 
                                  area.Value, 
                                  locker));

            _threadLocker.Set();
        }

        private (ImageSurface, object) InvalidateWindowBuffer(Visual root)
        {
            ImageSurface bufferSurface;
            object locker;

            if(!_windowBuffers.TryGetValue(root, out var pair))
            {
                Services.Logger.LogRender("Creating buffer surface for new root...");

                bufferSurface = new ImageSurface(Format.Argb32, 
                                                 (int)root.RenderSize.Width,
                                                 (int)root.RenderSize.Height);

                Services.Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                          bufferSurface.Width,
                                          bufferSurface.Height);
                
                locker = new object();

                _windowBuffers.Add(root,
                                   (bufferSurface, locker));

                return (bufferSurface, locker);
            }

            (bufferSurface, locker) = pair;

            if(bufferSurface.Width < root.RenderSize.Width ||
               bufferSurface.Height < root.RenderSize.Height)
            {
                Services.Logger.LogRender("Creating bigger surface for root visual...");

                _windowBuffers.Remove(root);

                int newWidth = bufferSurface.Width * 2;
                int newHeight = bufferSurface.Height * 2;

                Services.Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                          newWidth,
                                          newHeight);

                lock(locker)
                {
                    bufferSurface.Dispose();

                    bufferSurface = new ImageSurface(Format.Argb32, 
                                                     newWidth,
                                                     newHeight);
                }

                _windowBuffers.Add(root, (bufferSurface, locker));
            }

            return (bufferSurface, locker);
        }

        private void RenderLoop(object obj)
        {
            while(true)
            {
                if(!_visualsToUpdate.TryTake(out var drawRequest))
                {
                    Services.Logger.LogRender("No elements to render. Blocking...");
                    _threadLocker.Reset();
                    _threadLocker.Wait();
                }

                Visual visualToRedraw;
                Surface targetSurface;
                Surface buffer;
                Rectangle areaToUpdate;
                object locker;

                (visualToRedraw, targetSurface, buffer, areaToUpdate, locker) = drawRequest;

                if(buffer is null)
                    continue;

                lock(locker)
                {
                    Services.Logger.LogRender("Buffer locked. Starting draw cycle...");

                    using (var context = new Context(buffer))
                    {
                        context.Rectangle(visualToRedraw.RenderSize);
                        context.Clip();

                        visualToRedraw.Render(context);
                    }
                }

                Services.Logger.LogRender("Querying buffer swap...");
                _mainDispatcher.BeginInvoke(() => FlushToActualSurface(areaToUpdate, 
                                                                       buffer, 
                                                                       targetSurface,
                                                                       locker), 
                                            OperationPriority.Render);
            }
        }

        private void FlushToActualSurface(Rectangle area, 
                                          Surface bufferSurface,
                                          Surface actualSurface,
                                          object locker)
        {
            Services.Logger.LogRender("Swapping buffers...");
            
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