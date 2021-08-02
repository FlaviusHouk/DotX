using DotX.Interfaces;
using DotX.Extensions;
using Cairo;
using System;
using System.Collections.Generic;

namespace DotX.Rendering
{
    public abstract class DoubleBufferedManager 
    {
        private IBackBufferFactory _bufferFactory;

        protected ILogger Logger { get; }

        protected Dictionary<IRootVisual, SurfaceWrapper> WindowBuffers { get; } =
            new();

        protected DoubleBufferedManager(ILogger logger,
                                        IBackBufferFactory bufferFactory)
        {
            _bufferFactory = bufferFactory;

            Logger = logger;
        }

        protected (Surface, object) InvalidateWindowBuffer(IRootVisual root)
        {
            Surface bufferSurface;
            object locker;

            Rectangle renderSize = ((Visual)root).RenderSize;
            int surfaceWidth = (int)renderSize.Width;
            int surfaceHeight = (int)renderSize.Height;

            if(!WindowBuffers.TryGetValue(root, out var pair))
            {
                Logger.LogRender("Creating buffer surface for new root...");

                bufferSurface = 
                    _bufferFactory.CreateBuffer(surfaceWidth,
                                                surfaceHeight);

                Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                 surfaceWidth,
                                 surfaceHeight);
                
                locker = new object();

                WindowBuffers.Add(root,
                                  new (bufferSurface, 
                                       locker, 
                                       surfaceWidth, 
                                       surfaceHeight));

                return (bufferSurface, locker);
            }

            bufferSurface = pair.Surface;
            locker = pair.Locker;

            if(pair.Width < renderSize.Width ||
               pair.Height < renderSize.Height)
            {
                Logger.LogRender("Creating bigger surface for root visual...");

                int newWidth = (int)Math.Max(pair.Width * 2, renderSize.Width);
                int newHeight = (int)Math.Max(pair.Height * 2, renderSize.Height);

                Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                 newWidth,
                                 newHeight);

                pair.Dispose();
                Surface newSurf = 
                    _bufferFactory.CreateBuffer(newWidth,
                                                newHeight);

                WindowBuffers[root] = new SurfaceWrapper(newSurf, 
                                                         pair.Locker,
                                                         newWidth,
                                                         newHeight);

                bufferSurface = newSurf;
            }

            return (bufferSurface, locker);
        }

        //Add pool?
        protected record SurfaceWrapper(Surface Surface,
                                        object Locker,
                                        int Width,
                                        int Height) : IDisposable
        {
            public void Dispose()
            {
                lock(Locker)
                {
                    Surface.Dispose();
                }
            }
        }
    }
}