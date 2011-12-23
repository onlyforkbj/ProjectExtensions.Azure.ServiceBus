﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using ProjectExtensions.Azure.ServiceBus.Container;

namespace ProjectExtensions.Azure.ServiceBus.Autofac.Container {
    /// <summary>
    /// Autofac support for the azure service bus
    /// </summary>
    public class AutofacAzureBusContainer : IAzureBusContainer {
        IContainer container;
        ContainerBuilder builder = new ContainerBuilder();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Optional autofac container owned by the calling application.</param>
        public AutofacAzureBusContainer(IContainer container = null) {
            this.container = container;
        }

        /// <summary>
        /// Resolve component type of T with optional arguments.
        /// </summary>
        /// <param name="parms">Additional parameters to be passed to the constructor.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>(params KeyValuePair<string, object>[] parms) where T : class {
            if (parms == null || parms.Length == 0) {
                return container.Resolve<T>();
            }
            return container.Resolve<T>((from p in parms select new NamedParameter(p.Key, p.Value)).ToList());
        }

        /// <summary>
        /// Resolve component with optional arguments.
        /// </summary>
        /// <param name="t">The type to resolve.</param>
        /// <param name="parms">Additional parameters to be passed to the constructor.</param>
        /// <returns></returns>
        public object Resolve(Type t, params KeyValuePair<string, object>[] parms) {
            if (parms == null || parms.Length == 0) {
                return container.Resolve(t);
            }
            return container.Resolve(t, (from p in parms select new NamedParameter(p.Key, p.Value)).ToList());
        }

        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        public void Register(Type serviceType, Type implementationType, bool perInstance = false) {
            var reg = builder.RegisterType(implementationType).As(serviceType);
            if (perInstance) {
                reg.InstancePerDependency();
            }
            else {
                reg.SingleInstance();
            }
        }

        /// <summary>
        /// Register the bus
        /// </summary>
        /// <param name="busConfiguration">The configuration instance to use on the bus.</param>
        public void RegisterBus(BusConfiguration busConfiguration) {
            if (!container.IsRegistered(typeof(IBus))) {
                builder.Register(item => new AzureBus(busConfiguration)).As<IBus>().SingleInstance();
            }
        }

        /// <summary>
        /// Build the container if needed.
        /// </summary>
        public void Build() {
            if (container == null) {
                container = builder.Build();
            }
            else {
                builder.Update(container);
            }
            builder = new ContainerBuilder();
        }

        /// <summary>
        /// Return true if the given type is registered with the container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsRegistered(Type type) {
            return container != null && container.IsRegistered(type);
        }
    }
}