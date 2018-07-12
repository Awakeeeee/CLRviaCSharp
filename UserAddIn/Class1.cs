using System;
using QIQIQI.HostSDK;

namespace UserAddIn
{
    public class Skada : IAddin
    {
        public String CommunicationMethod(Int32 i)
        {
            return "Skada report : " + i.ToString();
        }
    }

    public class NPCScan : IAddin
    {
        public String CommunicationMethod(Int32 i)
        {
            return "NPCScan version : " + i;
        }
    }
}
