namespace Contracts.Common
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    public class MEFSingletonServiceHostFactory<TServiceImpl> : ServiceHostFactory
    {
        public CompositionContainer CompositionContainer { get; private set; }

        public MEFSingletonServiceHostFactory()
        {
            this.CompositionContainer = MEFConfigurationElement.ConfiguredCompositionContainer;
        }

        public MEFSingletonServiceHostFactory(CompositionContainer compositionContainer)
        {
            if (compositionContainer == null) throw new ArgumentNullException("compositionContainer");

            this.CompositionContainer = compositionContainer;
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var instance = this.CompositionContainer.GetExportedValue<TServiceImpl>();

            var serviceHost = new ServiceHost(instance, baseAddresses);

            serviceHost.Description.Behaviors.Add(new MEFServiceBehavior<TServiceImpl>(this.CompositionContainer));

            return serviceHost;
        }

        public ServiceHost CreateServiceHost(Uri[] baseAddresses)
        {
            return this.CreateServiceHost(typeof(TServiceImpl), baseAddresses);
        }
    }
}
