using System;
using System.Linq;
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
            //Because GetService(serviceType)
            //throw exception if service was not found.
            return _container.GetServices(serviceType).FirstOrDefault();
        }
    }
}
