﻿namespace Contracts
{
    using System.ServiceModel;

    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string GetData(int value);
    }
}