﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LightBDD.Core.Execution.Dependencies
{
    public class SimpleDependencyContainer : IDependencyContainer, IContainerConfigurer
    {
        private readonly ConcurrentQueue<IDisposable> _disposable = new ConcurrentQueue<IDisposable>();
        private readonly ConcurrentDictionary<Type, object> _items = new ConcurrentDictionary<Type, object>();

        public SimpleDependencyContainer(Action<IContainerConfigurer> configuration = null)
        {
            configuration?.Invoke(this);
        }

        public async Task<object> ResolveAsync(Type type)
        {
            if (_items.TryGetValue(type, out var cached))
                return cached;

            return EnlistDisposable(Activator.CreateInstance(type));
        }

        public void Dispose()
        {
            while (_disposable.TryDequeue(out var item))
            {
                try
                {
                    item.Dispose();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Failed to dispose dependency '{item.GetType().Name}': {e.Message}", e);
                }
            }
        }

        public IDependencyContainer BeginScope(Action<IContainerConfigurer> configuration = null)
        {
            return new SimpleDependencyContainer(configuration);
        }

        void IContainerConfigurer.RegisterInstance(object instance, RegistrationOptions options)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            _items.AddOrUpdate(instance.GetType(), _ => instance, (_, __) => instance);
            if (options.TakeOwnership)
                EnlistDisposable(instance);
        }

        private object EnlistDisposable(object item)
        {
            if (item is IDisposable disposable)
                _disposable.Enqueue(disposable);
            return item;
        }
    }


}