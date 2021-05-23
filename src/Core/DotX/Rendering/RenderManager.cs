using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cairo;
using DotX.Threading;
using DotX.Interfaces;
using DotX.Extensions;
using System;

namespace DotX.Rendering
{
    internal class RenderManager
    {
        //Add pool?
        private record SurfaceWrapper(ImageSurface Surface,
                                      object Locker) : IDisposable
        {
            public void Dispose()
            {
                lock(Locker)
                {
                    Surface.Dispose();
                }
            }
        }

        private readonly Dispatcher _mainDispatcher;
        private readonly Thread _renderThread;

        private readonly ManualResetEventSlim _threadLocker =
            new ManualResetEventSlim();
        private readonly Dictionary<IRootVisual, SurfaceWrapper> _windowBuffers =
            new();

        private readonly ConcurrentBag<RenderRequest> _renderQueue =
            new ();

        private readonly ConcurrentQueue<RenderRequest> _pendingRequests =
            new ();

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
            
            (windowBuffer, locker) = InvalidateWindowBuffer(root);

            Services.Logger.LogRender("Buffer surface has size {0}x{1}.", 
                                      windowBuffer.Width, 
                                      windowBuffer.Height);

            var newRequest = new RenderRequest(visualToInvalidate, 
                                               root, 
                                               area.Value,
                                               root.DirtyArea is null);

            if(newRequest.Redraw)
            {
                while(_pendingRequests.TryPeek(out var request) &&
                      request is not null &&
                      !request.Redraw)
                {
                    request.Cancel();

                    _pendingRequests.TryDequeue(out var _);
                }
            }

            _renderQueue.Add(newRequest);

            _threadLocker.Set();
        }

        private (ImageSurface, object) InvalidateWindowBuffer(IRootVisual root)
        {
            ImageSurface bufferSurface;
            object locker;
            var renderSize = ((Visual)root).RenderSize;

            if(!_windowBuffers.TryGetValue(root, out var pair))
            {
                Services.Logger.LogRender("Creating buffer surface for new root...");

                bufferSurface = new ImageSurface(Format.Argb32, 
                                                 (int)renderSize.Width,
                                                 (int)renderSize.Height);

                Services.Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                          bufferSurface.Width,
                                          bufferSurface.Height);
                
                locker = new object();

                _windowBuffers.Add(root,
                                   new SurfaceWrapper(bufferSurface, locker));

                return (bufferSurface, locker);
            }

            (bufferSurface, locker) = pair;

            if(bufferSurface.Width < renderSize.Width ||
               bufferSurface.Height < renderSize.Height)
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

                _windowBuffers.Add(root, new SurfaceWrapper(bufferSurface, locker));
            }

            return (bufferSurface, locker);
        }

        private void RenderLoop(object obj)
        {
            while(true)
            {
                if(!_renderQueue.TryTake(out var renderRequest))
                {
                    Services.Logger.LogRender("No elements to render. Blocking...");
                    _threadLocker.Reset();
                    _threadLocker.Wait();
                }

                if(renderRequest is null)
                    continue;

                if(!_windowBuffers.TryGetValue(renderRequest.Root, out var bufferSurface))
                    _mainDispatcher.Invoke(() => throw new Exception());

                if (renderRequest.Redraw)
                {
                    lock (bufferSurface.Locker)
                    {
                        Services.Logger.LogRender("Buffer locked. Starting draw cycle...");

                        using (var context = new Context(bufferSurface.Surface))
                        {
                            context.Rectangle(renderRequest.VisualToInvalidate.RenderSize);
                            context.Clip();

                            renderRequest.VisualToInvalidate.Render(context);
                        }
                    }
                }
                else
                {
                    Services.Logger.LogRender("Skip redraw. Renew exposed area.");
                }

                Services.Logger.LogRender("Querying buffer swap...");
                _pendingRequests.Enqueue(renderRequest);
                _mainDispatcher.BeginInvoke(() => FlushToActualSurface(renderRequest), 
                                            OperationPriority.Render);
            }
        }

        private void FlushToActualSurface(RenderRequest request)
        {
            if(request.IsCanceled)
            {
                Services.Logger.LogRender("Request is canceled. Skipping.");
                return;
            }

            Services.Logger.LogRender("Swapping buffers...");

            if (!_windowBuffers.TryGetValue(request.Root, out var bufferSurface))
                throw new Exception();

            using var context = new Context(request.Root.WindowImpl.WindowSurface);
            
            lock(bufferSurface.Locker)
            {
                context.SetSource(bufferSurface.Surface);
                context.Rectangle(request.AreaToUpdate);
                context.Fill();
            }

            _pendingRequests.TryDequeue(out var _);
        }
    }
}