using System;
using System.ComponentModel.Composition.Hosting;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Contracts.Common
{
    /// <summary>
    ///  A <see cref="ServiceHostFactory"/> which injects the <see cref="MEFInstanceProvider{T}"/>
    ///  into all service endppoints. 
    /// </summary>
    /// <typeparam name="TServiceImpl"></typeparam>
    public class MEFServiceHostFactory<TServiceImpl> : ServiceHostFactory
    {
        public CompositionContainer CompositionContainer { get; private set; }

        public MEFServiceHostFactory()
        {
            this.CompositionContainer = MEFConfigurationElement.ConfiguredCompositionContainer;
        }

        public MEFServiceHostFactory(CompositionContainer compositionContainer)
        {
            if (compositionContainer == null) throw new ArgumentNullException("compositionContainer");

            this.CompositionContainer = compositionContainer;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var serviceHost = new ServiceHost(serviceType, baseAddresses);

            serviceHost.Description.Behaviors.Add(new MEFServiceBehavior<TServiceImpl>(this.CompositionContainer));

            return serviceHost;
        }

        public ServiceHost CreateServiceHost(Uri[] baseAddresses)
        {
            return this.CreateServiceHost(typeof(TServiceImpl), baseAddresses);
        }
    }
}