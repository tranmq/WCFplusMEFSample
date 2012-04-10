namespace Contracts.Common
{
    using System;
    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;

    public static class CompositionContainerFactory
    {
        public static CompositionContainer Create(ComposablePartCatalog catalog)
        {
            var container = new CompositionContainer(new AggregateCatalog(catalog,
                new TypeCatalog(typeof(CompositionContainer))));

            container.ComposeExportedValue<CompositionContainer>(container);

            return container;
        }
    }
}
