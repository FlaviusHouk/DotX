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
    internal class MultiThreadRenderManager : DoubleBufferedManager, IRenderManager, IDisposable
    {
        private readonly Dispatcher _mainDispatcher;
        private readonly Thread _renderThread;

        private readonly ManualResetEventSlim _threadLocker =
            new();

        private readonly ConcurrentQueue<RenderRequest> _renderQueue =
            new ();

        private readonly ConcurrentQueue<RenderRequest> _pendingRequests =
            new ();

        private bool _isDisposed;

        public MultiThreadRenderManager(ILogger logger,
                                        Dispatcher dispatcher)  : 
            base(logger,
                 Services.BackBufferFactory)
        {
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

            Logger.LogRender("Received request to redraw area {0}.", area);
            
            (Surface windowBuffer, object locker) = 
                InvalidateWindowBuffer(root);

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
            if(!WindowBuffers.TryGetValue(root, out var surface))
                return;
            
            var surfaceRect = new Rectangle(0, 0, 
                                            surface.Width, 
                                            surface.Height);

            if(!surfaceRect.Contains(area))
                Logger.LogRender("Exposing area is bigger than surface");

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

        private void RenderLoop(object obj)
        {
            while(!_isDisposed)
            {
                if(!_renderQueue.TryDequeue(out var renderRequest))
                {
                    if(_isDisposed)
                        return;

                    Logger.LogRender("No elements to render. Blocking...");
                    _threadLocker.Reset();
                    _threadLocker.Wait();
                }

                if(renderRequest is null || _isDisposed)
                    continue;

                if(!WindowBuffers.TryGetValue(renderRequest.Root, out var bufferSurface))
                    _mainDispatcher.Invoke(() => throw new Exception());

                if (renderRequest.Redraw)
                {
                    using var dispatcherLocker = _mainDispatcher.BlockProcessing();
                    lock (bufferSurface.Locker)
                    {
                        Logger.LogRender("Buffer locked. Starting draw cycle...");

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
                    Logger.LogRender("Skip redraw. Renew exposed area.");
                }

                Logger.LogRender("Querying buffer swap...");
                _pendingRequests.Enqueue(renderRequest);
                _mainDispatcher.BeginInvoke(() => FlushToActualSurface(renderRequest), 
                                            OperationPriority.Render);
            }
        }

        private void FlushToActualSurface(RenderRequest request)
        {
            if(request.IsCanceled)
            {
                Logger.LogRender("Request is canceled. Skipping.");
                return;
            }

            Logger.LogRender("Swapping buffers...");

            if (!WindowBuffers.TryGetValue(request.Root, out var bufferSurface))
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

        public void Dispose()
        {
            _isDisposed = true;
        }
    }
}