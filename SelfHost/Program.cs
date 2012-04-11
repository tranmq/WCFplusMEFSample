namespace SelfHost
{
    using System;
    using System.ServiceModel;
    using System.ComponentModel.Composition.Hosting;

    class Program
    {
        static void Main(string[] args)
        {
            using (var sh = new ServiceHost(typeof(Contracts.MyService), new Uri("http://localhost:8080/selfhost")))
            {
                sh.AddServiceEndpoint(typeof (Contracts.IService), new BasicHttpBinding(), "percall");

                var cc =
                    new CompositionContainer(new TypeCatalog(typeof (Contracts.MyService),
                                                             typeof (OtherDependency.SomeDependency)));
                sh.Description.Behaviors.Add(new Contracts.Common.MEFServiceBehavior<Contracts.MyService>(cc));

                sh.Open();


                var cf = new ChannelFactory<Contracts.IService>(new BasicHttpBinding(),
                                                                "http://localhost:8080/selfhost/percall");
                var proxy = cf.CreateChannel();
                Console.WriteLine(proxy.GetData(1));
                Console.WriteLine(proxy.GetData(2));
            }
        }
    }
}