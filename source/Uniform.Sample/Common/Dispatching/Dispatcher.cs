﻿using System;
using Microsoft.Practices.ServiceLocation;

namespace Uniform.Common.Dispatching
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message);
    }

    public class Dispatcher
    {
        /// <summary>
        /// Service Locator that is used to create handlers
        /// </summary>
        private readonly IServiceLocator _serviceLocator;

        /// <summary>
        /// Registry of all registered handlers
        /// </summary>
        private readonly DispatcherHandlerRegistry _registry;

        /// <summary>
        /// Number of retries in case exception was logged
        /// </summary>
        private readonly int _maxRetries;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Dispatcher(DispatcherConfiguration configuration)
        {
            if (configuration.ServiceLocator == null)
                throw new ArgumentException("Unity Container is not registered for distributor.");

            if (configuration.DispatcherHandlerRegistry == null)
                throw new ArgumentException("Dispatcher Handler Registry is null in distributor.");

            _serviceLocator = configuration.ServiceLocator;
            _registry = configuration.DispatcherHandlerRegistry;
            _maxRetries = configuration.NumberOfRetries;

            // order handlers 
            _registry.InsureOrderOfHandlers(configuration.Order);
        }

        /// <summary>
        /// Factory method
        /// </summary>
        public static Dispatcher Create(Func<DispatcherConfiguration, DispatcherConfiguration> configurationAction)
        {
            var config = new DispatcherConfiguration();
            configurationAction(config);
            return new Dispatcher(config);
        }

        public void Dispatch(Object message)
        {
            try
            {
                var handlerTypes = _registry.GetHandlersType(message.GetType());

                foreach (var handlerType in handlerTypes)
                {
                    var handler = _serviceLocator.GetInstance(handlerType);

                    var attempt = 0;
                    while (attempt < _maxRetries)
                    {
                        try
                        {
                            var context = new DispatcherInvocationContext(this, handler, message);

                            context.Invoke();

                            // message handled correctly - so that should be 
                            // the final attempt
                            attempt = _maxRetries;
                        }
                        catch (Exception exception)
                        {
                            attempt++;

                            if (attempt == _maxRetries)
                            {
                                throw new Exception(String.Format(
                                    "Exception in the handler {0} for message {1}", handler.GetType().FullName, message.GetType().FullName));

                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error when dispatching message", exception);
            }
        }

        public void InvokeDynamic(Object handler, Object message)
        {
            dynamic dynamicHandler = handler;
            dynamic dynamicMessage = message;

            dynamicHandler.Handle(dynamicMessage);
        }

        public void InvokeByReflection(Object handler, Object message)
        {
            var methodInfo = handler.GetType().GetMethod("Handle", new[] { message.GetType() });
            methodInfo.Invoke(handler, new[] { message });
        }
    }
}