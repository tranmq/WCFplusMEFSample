namespace OtherDependency
{
    using System.ComponentModel.Composition;
    using System;
    using Contracts;

    [Export(typeof(ISomeType))]
    public class SomeDependency : ISomeType
    {
        private readonly Guid _id = Guid.NewGuid();

        Guid ISomeType.Id
        {
            get { return _id; }
        }
    }
}