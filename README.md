How to use an inversion of control (IoC) container with WCF
-----------------------------------------------------------

This article describes how to use an inversion-of-control (IoC) container, in the concrete sample using the [Managed Extensibility Framework (MEF)][3], in conjunction with Windows Communication Foundation (WCF). I've successfully used this approach in multiple projects, specifically within the implementation of the [VENUS-C Generic Worker][2], and within a custom application development project in the financial services industry in Germany. 

Why did I need that? Let's say we want to develop a service which interacts with some sort of backend service, using a custom provider. Service calls result in an interaction with the backend. An example of this could be a web service, which exposes (wraps) some legacy system towards a broader audience. Talking to the legacy system happends through some provider (implementing the `ILegacySystem` interface). 

```c#
[ServiceContract]
public class MyService
{
    private ILegacySystem _impl;

    public MyService() 
    {
        this._impl = new LegacySystemImpl();
    }

    [OperationContract]
    public string SomeMethod(string input) 
    {
        ...
    }
}
```

The problem we run into with this approach is that in different settings (during development, during unit tests, and in production), we need to be able to easily swap (inject) other providers (dependencies) into the service, such as mock implementations. 

Out of the box, WCF simply `new`s up a new service instance and dispatches calls against it. Responsible for creating service instances is a class implementing the [`IInstanceProvider` interface][4]. Simply speaking, to hook WCF up to your preferred IoC container, you need to implement your own [`IInstanceProvider`][4] and convince WCF to use it using a custom [`IServiceBehavior`][5]. 



When choosing an [`InstanceContextMode`][6] of Single, the system does not call the GetInstance method of our [`IInstanceProvider`][4], so that we have to run a special ServiceHostFactory.  


[1]: http://msdn.microsoft.com/en-us/library/ms733766.aspx "Host a WCF Service in IIS"
[2]: http://www.venus-c.eu/ "VENUS-C Generic Worker"
[3]: http://msdn.microsoft.com/en-us/library/dd460648.aspx
[4]: http://msdn.microsoft.com/en-us/library/system.servicemodel.dispatcher.iinstanceprovider.aspx 
[5]: http://msdn.microsoft.com/en-us/library/system.servicemodel.description.iservicebehavior.aspx 
[6]: http://msdn.microsoft.com/en-us/library/system.servicemodel.instancecontextmode.aspx