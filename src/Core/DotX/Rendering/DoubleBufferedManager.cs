using DotX.Interfaces;
using DotX.Extensions;
using Cairo;
using System;
using System.Collections.Generic;

namespace DotX.Rendering
{
    public abstract class DoubleBufferedManager 
    {
        protected ILogger Logger { get; }

        protected Dictionary<IRootVisual, SurfaceWrapper> WindowBuffers { get; } =
            new();

        protected DoubleBufferedManager(ILogger logger)
        {
            Logger = logger;
        }

        protected (ImageSurface, object) InvalidateWindowBuffer(IRootVisual root)
        {
            ImageSurface bufferSurface;
            object locker;
            var renderSize = ((Visual)root).RenderSize;

            if(!WindowBuffers.TryGetValue(root, out var pair))
            {
                Logger.LogRender("Creating buffer surface for new root...");

                bufferSurface = new ImageSurface(Format.Argb32, 
                                                 (int)renderSize.Width,
                                                 (int)renderSize.Height);

                Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                 bufferSurface.Width,
                                 bufferSurface.Height);
                
                locker = new object();

                WindowBuffers.Add(root,
                                  new (bufferSurface, locker));

                return (bufferSurface, locker);
            }

            (bufferSurface, locker) = pair;

            if(bufferSurface.Width < renderSize.Width ||
               bufferSurface.Height < renderSize.Height)
            {
                Logger.LogRender("Creating bigger surface for root visual...");

                int newWidth = (int)Math.Max(bufferSurface.Width * 2, renderSize.Width);
                int newHeight = (int)Math.Max(bufferSurface.Height * 2, renderSize.Height);

                Logger.LogRender("Size of the created surface is {0}x{1}.", 
                                 newWidth,
                                 newHeight);

                lock(locker)
                {
                    bufferSurface.Dispose();

                    bufferSurface = new ImageSurface(Format.Argb32, 
                                                     newWidth,
                                                     newHeight);
                }

                WindowBuffers[root] = new SurfaceWrapper(bufferSurface, locker);
            }

            return (bufferSurface, locker);
        }

        //Add pool?
        protected record SurfaceWrapper(ImageSurface Surface,
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
    }
}