namespace Contracts
{
    using System.ComponentModel.Composition;
    using System.ServiceModel;

    [Export(typeof(MyService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class MyService : IService, IPartImportsSatisfiedNotification
    {
        [Import(RequiredCreationPolicy = CreationPolicy.NonShared)]
        public ISomeType MyDependency { get; set; }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            ; // Constructor logic goes here...
        }

        string IService.GetData(int value)
        {
            return "Hallo " + value.ToString() + " " + MyDependency.Id.ToString();
        }
    }
}