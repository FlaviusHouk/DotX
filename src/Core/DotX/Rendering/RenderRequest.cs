using Cairo;

namespace DotX.Rendering
{
    internal record RenderRequest(Visual VisualToInvalidate, 
                                  Surface TargetSurface, 
                                  Surface BufferSurface, 
                                  Rectangle AreaToUpdate, 
                                  object Locker)
    {}
}