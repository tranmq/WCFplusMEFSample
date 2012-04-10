namespace Contracts
{
    using Contracts.Common;

    public class MyServiceHostFactory : MEFServiceHostFactory<MyService> { }

    // public class MyServiceHostFactory : MEFSingletonServiceHostFactory<MyService> { }
}