using System;
using Abioc;

namespace DotX.DI.Abioc
{
    public class AbiocWrapper : IServiceProvider
    {
        private readonly IContainer _container;

        public AbiocWrapper(IContainer container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            return _container.GetService(serviceType);
        }
    }
}
