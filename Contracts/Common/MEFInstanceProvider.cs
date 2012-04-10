using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Contracts.Common
{
    /// <summary>
    /// WCF Instance Provider which uses MEF to create a service instance. 
    /// The service instance is dynamically created to dispatch calls against. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MEFInstanceProvider<T> : IInstanceProvider
    {
        /// <summary>
        /// Gets the composition container.
        /// </summary>
        public CompositionContainer CompositionContainer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MEFInstanceProvider&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public MEFInstanceProvider(CompositionContainer container)
        {
            this.CompositionContainer = container;
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <param name="message">The message that triggered the creation of a service object.</param>
        /// <returns>
        /// The service object.
        /// </returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            object instance;
            try
            {
                var type = typeof(T);
                Trace.TraceInformation("Creating MEF instance of {0}", type.FullName);

                instance = this.CompositionContainer.GetExportedValue<T>();
            }
            catch (Exception ex)
            {
                for (var e = ex; e != null; e = e.InnerException)
                {
                    Trace.TraceError(e.Message);
                }

                this.CompositionContainer.Catalog.Parts.ToList()
                    .ForEach(p => Trace.TraceError(p.ToString()));

                throw;
            }

            return instance;
        }

        /// <summary>
        /// Returns a service object given the specified <see cref="T:System.ServiceModel.InstanceContext"/> object.
        /// </summary>
        /// <param name="instanceContext">The current <see cref="T:System.ServiceModel.InstanceContext"/> object.</param>
        /// <returns>
        /// A user-defined service object.
        /// </returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return this.GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Called when an <see cref="T:System.ServiceModel.InstanceContext"/> object recycles a service object.
        /// </summary>
        /// <param name="instanceContext">The service's instance context.</param>
        /// <param name="instance">The service object to be recycled.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}