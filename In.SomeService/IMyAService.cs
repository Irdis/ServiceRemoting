using System;
using System.ComponentModel;
using In.ServiceCommon.Client;
using In.ServiceCommon.Interface;

namespace In.SomeService
{
    [Serializable]
    public class MyCStreamingData
    {
        public int Key { get; set; }
        public string Data { get; set; }
    }
    
    
    public interface IMyAService
    {
        int Add(int a, int b);
    }
}