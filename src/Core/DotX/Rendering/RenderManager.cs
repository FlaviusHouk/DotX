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
    internal class RenderManager : IRenderManager
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
        private readonly ILogger _logger;

        private readonly ManualResetEventSlim _threadLocker =
            new ManualResetEventSlim();
        private readonly Dictionary<IRootVisual, SurfaceWrapper> _windowBuffers =
            new();

        private readonly ConcurrentQueue<RenderRequest> _renderQueue =
            new ();

        private readonly ConcurrentQueue<RenderRequest> _pendingRequests =
            new ();

        public RenderManager(Dispatcher dispatcher)
        {
            _logger = Services.Logger;
            _mainDispatcher = dispatcher;

            _renderThread = new Thread(RenderLoop);
            _renderThread.Name = "Render thread";
            
            _renderThread.Start();
        }

        public void Invalidate(IRootVisual root,
                               Visual visualToInvalidate,
                               Rectangle? area)
        {
            area ??= visualToInvalidate.RenderSize;

            _logger.LogRender("Received request to redraw area {0}.", area);

            ImageSurface windowBuffer;
            object locker;
            
            (windowBuffer, locker) = InvalidateWindowBuffer(root);

            _logger.LogRender("Buffer surface has size {0}x{1}.", 
                              windowBuffer.Width, 
                              windowBuffer.Height);

            var newRequest = new RenderRequest(visualToInvalidate, 
                                               root, 
                                               area.Value);

            while (_pendingRequests.TryPeek(out var oldRequest) &&
                   oldRequest is not null &&
                   !oldRequest.Redraw &&
                   newRequest.AreaToUpdate.Contains(oldRequest.AreaToUpdate))
            {
                oldRequest.Cancel();

                _pendingRequests.TryDequeue(out var _);
            }

            EnqueueRenderRequest(newRequest);
        }

        public void Expose(IRootVisual root,
                           Rectangle area)
        {
            if(!_windowBuffers.TryGetValue(root, out var surface))
                return;
            
            var surfaceRect = new Rectangle(0, 0, 
                                            surface.Surface.Width, 
                                            surface.Surface.Height);

            if(!surfaceRect.Contains(area))
                _logger.LogRender("Exposing area is bigger than surface");

            var newRequest = new RenderRequest(null, 
                                               root, 
                                               area,
                                               false);

            EnqueueRenderRequest(newRequest);
        }

        private void EnqueueRenderRequest(RenderRequest request)
        {
            _renderQueue.Enqueue(request);

            _threadLocker.Set();
        }

        private (ImageSurface, object) InvalidateWindowBuffer(IRootVisual root)
        {
            ImageSurface bufferSurface;
            object locker;
            var renderSize = ((Visual)root).RenderSize;

            if(!_windowBuffers.TryGetValue(root, out var pair))
            {
                _logger.LogRender("Creating buffer surface for new root...");

                bufferSurface = new ImageSurface(Format.Argb32, 
                                                 (int)renderSize.Width,
                                                 (int)renderSize.Height);

                _logger.LogRender("Size of the created surface is {0}x{1}.", 
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
                _logger.LogRender("Creating bigger surface for root visual...");

                int newWidth = (int)Math.Max(bufferSurface.Width * 2, renderSize.Width);
                int newHeight = (int)Math.Max(bufferSurface.Height * 2, renderSize.Height);

                _logger.LogRender("Size of the created surface is {0}x{1}.", 
                                  newWidth,
                                  newHeight);

                lock(locker)
                {
                    bufferSurface.Dispose();

                    bufferSurface = new ImageSurface(Format.Argb32, 
                                                     newWidth,
                                                     newHeight);
                }

                _windowBuffers[root] = new SurfaceWrapper(bufferSurface, locker);
            }

            return (bufferSurface, locker);
        }

        private void RenderLoop(object obj)
        {
            while(true)
            {
                if(!_renderQueue.TryDequeue(out var renderRequest))
                {
                    _logger.LogRender("No elements to render. Blocking...");
                    _threadLocker.Reset();
                    _threadLocker.Wait();
                }

                if(renderRequest is null)
                    continue;

                if(!_windowBuffers.TryGetValue(renderRequest.Root, out var bufferSurface))
                    _mainDispatcher.Invoke(() => throw new Exception());

                if (renderRequest.Redraw)
                {
                    using var dispatcherLocker = _mainDispatcher.BlockProcessing();
                    lock (bufferSurface.Locker)
                    {
                        _logger.LogRender("Buffer locked. Starting draw cycle...");

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
                    _logger.LogRender("Skip redraw. Renew exposed area.");
                }

                _logger.LogRender("Querying buffer swap...");
                _pendingRequests.Enqueue(renderRequest);
                _mainDispatcher.BeginInvoke(() => FlushToActualSurface(renderRequest), 
                                            OperationPriority.Render);
            }
        }

        private void FlushToActualSurface(RenderRequest request)
        {
            if(request.IsCanceled)
            {
                _logger.LogRender("Request is canceled. Skipping.");
                return;
            }

            _logger.LogRender("Swapping buffers...");

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