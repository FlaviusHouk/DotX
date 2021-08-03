using System;

namespace DotX.Interfaces
{
    public interface IServiceContainer : IServiceProvider
    {
        void RegisterSingleton<TInt, TImpl>(TImpl value = default) 
            where TInt : class
            where TImpl : class, TInt;

        void RegisterSingleton<TImpl>(TImpl value = default) 
            where TImpl : class;
            
        void RegisterTransient<TInt, TImpl>() 
            where TInt : class
            where TImpl : class, TInt;

        void RegisterTransient<TImpl>() 
            where TImpl : class;

        bool IsRegistered<TServ>();
    }
}