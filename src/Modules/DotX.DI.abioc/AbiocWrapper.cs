using DotX.Interfaces;

using System;
using System.Linq;
using System.Reflection;

using Abioc;
using Abioc.Registration;
using System.Collections.Generic;

namespace DotX.DI.Abioc
{
    public class AbiocWrapper : IServiceContainer
    {
        private IContainer _container;
        private readonly List<Assembly> _assemblies = new(); 
        private readonly RegistrationSetup _setup;
        private bool _isUpdated;

        public AbiocWrapper()
        {
            _setup = new();
        }

        public object GetService(Type serviceType)
        {
            if(_isUpdated)
            {
                _container = _setup.Construct(_assemblies.ToArray());
                _isUpdated = false;
            }

            //Because GetService(serviceType)
            //throw exception if service was not found.
            return _container.GetServices(serviceType).FirstOrDefault();
        }

        public void RegisterSingleton<TInt, TImpl>(TImpl value = default) 
            where TInt : class
            where TImpl : class, TInt
        {
            if(IsRegistered<TInt>())
                throw new InvalidOperationException();

            SaveAssemblies(typeof(TInt).Assembly, typeof(TImpl).Assembly);

            if(value == default)
                _setup.RegisterSingleton<TInt, TImpl>();
            else
                _setup.RegisterFixed<TInt, TImpl>(value);

            _isUpdated = true;
        }

        public void RegisterSingleton<TImpl>(TImpl value = default) 
            where TImpl : class
        {
            if(IsRegistered<TImpl>())
                throw new InvalidOperationException();

            SaveAssemblies(typeof(TImpl).Assembly);

            if(value == default)
                _setup.RegisterSingleton<TImpl>();
            else
                _setup.RegisterFixed<TImpl>(value);

            _isUpdated = true;
        }

        public void RegisterTransient<TInt, TImpl>() 
            where TInt : class
            where TImpl : class, TInt
        {
            if(IsRegistered<TInt>())
                throw new InvalidOperationException();

            SaveAssemblies(typeof(TInt).Assembly, typeof(TImpl).Assembly); 
            
            _setup.Register<TInt, TImpl>();

            _isUpdated = true;
        }

        public void RegisterTransient<TImpl>() 
            where TImpl : class
        {
            if(IsRegistered<TImpl>())
                throw new InvalidOperationException();

            SaveAssemblies(typeof(TImpl).Assembly); 
            
            _setup.Register<TImpl>();

            _isUpdated = true;
        }

        public bool IsRegistered<TServ>()
        {
            return _setup.Registrations.ContainsKey(typeof(TServ));
        }

        private void SaveAssemblies(params Assembly[] assemblies)
        {
            foreach(var assembly in assemblies)
            {
                if(!_assemblies.Contains(assembly))
                    _assemblies.Add(assembly);
            }
        }
    }
}
