using Cairo;
using DotX.Interfaces;
using DotX.Threading;
using DotX.Extensions;

using System;
using System.Collections.Generic;

namespace DotX.Rendering
{
    public class DispatcherRenderManager : DoubleBufferedManager, IRenderManager
    {
        private readonly Dispatcher _mainDispatcher;
        private readonly Queue<RenderRequest> _pendingRequests =
            new ();

        public DispatcherRenderManager(Dispatcher dispatcher) : 
            base(Services.Logger,
                 Services.BackBufferFactory)
        {
            _mainDispatcher = dispatcher;
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

                _pendingRequests.Dequeue();
            }

            _pendingRequests.Enqueue(newRequest);

            _mainDispatcher.BeginInvoke(() => ProcessRenderRequest(), 
                                        OperationPriority.Render);
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

            _pendingRequests.Enqueue(newRequest);

            _mainDispatcher.BeginInvoke(() => ProcessRenderRequest(), 
                                        OperationPriority.Render);
        }

        private void ProcessRenderRequest()
        {
            if(_pendingRequests.Count < 1)
                return;

            RenderRequest request = 
                _pendingRequests.Dequeue();

            if(request.IsCanceled)
            {
                Logger.LogRender("Request is canceled. Skipping.");
                return;
            }

            if(!WindowBuffers.TryGetValue(request.Root, out var bufferSurface))
                throw new Exception();

            if (request.Redraw)
            {
                using var context = new Context(bufferSurface.Surface);
                request.VisualToInvalidate.Render(context);
            }
            else
            {
                Logger.LogRender("Skip redraw. Renew exposed area.");
            }

            Logger.LogRender("Querying buffer swap...");

            _mainDispatcher.BeginInvoke(() => FlushToActualSurface(request), 
                                        OperationPriority.Render);
        }

        private void FlushToActualSurface(RenderRequest request)
        {
            Logger.LogRender("Swapping buffers...");

            if (!WindowBuffers.TryGetValue(request.Root, out var bufferSurface))
                throw new Exception();

            using Context context = 
                new (request.Root.WindowImpl.WindowSurface);
            
            context.SetSource(bufferSurface.Surface);
            context.Rectangle(request.AreaToUpdate);
            context.Fill();
        }
    }
}