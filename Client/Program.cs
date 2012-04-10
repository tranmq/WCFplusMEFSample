namespace Client
{
    using System.ServiceModel;
    using Contracts;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();

            var cf = new ChannelFactory<IService>(new BasicHttpBinding(), "http://localhost/MEFandWCF/MyService.svc");
            var proxy = cf.CreateChannel();
            System.Console.WriteLine(proxy.GetData(1));
            System.Console.WriteLine(proxy.GetData(2));
        }
    }
}