using DotX.Interfaces;

namespace DotX.Xaml.Generation
{
    internal class TemplateVisualGenerator : IVisualTreeGenerator
    {
        public readonly XamlObject _rootDescription;

        public TemplateVisualGenerator(XamlObject root)
        {
            _rootDescription = root;
        }

        public Visual GenerateRootVisual()
        {
            ObjectComposer composer = new ();
            
            return (Visual)composer.Build(_rootDescription);
        }
    }
}