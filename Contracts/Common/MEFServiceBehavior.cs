﻿using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Contracts.Common
{
    /// <summary>
    /// A WCF service behavior to enable MEF inside the <see cref="MEFInstanceProvider{T}"/> instance provider. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MEFServiceBehavior<T> : IServiceBehavior
    {
        /// <summary>
        /// Gets the composition container.
        /// </summary>
        public CompositionContainer CompositionContainer { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="MEFServiceBehavior&lt;T&gt;"/> class from being created.
        /// </summary>
        private MEFServiceBehavior() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MEFServiceBehavior&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public MEFServiceBehavior(CompositionContainer container)
        {
            this.CompositionContainer = container;
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription,
                                          ServiceHostBase serviceHostBase)
        {
            foreach (var dispatcher in serviceHostBase.ChannelDispatchers)
            {
                var channelDispatcher = dispatcher as ChannelDispatcher;
                if (channelDispatcher == null)
                    continue;

                foreach (var e in channelDispatcher.Endpoints.Where(e => e.ContractNamespace != "http://schemas.microsoft.com/2006/04/mex"))
                {
                    e.DispatchRuntime.InstanceProvider = new MEFInstanceProvider<T>(this.CompositionContainer);
                }
            }
        }
    }
}
