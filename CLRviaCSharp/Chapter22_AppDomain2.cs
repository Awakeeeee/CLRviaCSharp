using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter22_AppDomain2
    {
        static void Main22_2(string[] args)
        {
            //Check();

            //从AppDomain角度看异常的处理
            //AppDomain ad = AppDomain.CurrentDomain;
            //ad.FirstChanceException += TestCallback;
            //try { throw new ArgumentException(); }
            //catch (ArgumentException e) { Console.WriteLine("catced " + e.Message); }

            //使用自己的AppDomain Manager
            string strongName = MyTestLibrary.LogAssemblyName();
            AppDomainSetup ads = new AppDomainSetup() {
                AppDomainManagerAssembly = strongName,
                AppDomainManagerType = "AppDomainCustomManager"
            };

            AppDomain testAD = AppDomain.CreateDomain("TEST DOMAIN", null, ads);

            Environment.Exit(0);
        }

        static void Check()
        {
            using (new AppDomainMonitor(null))
            {
                List<object> l = new List<object>();
                for (int i = 0; i < 1000; i++) { l.Add(new byte[10000]); }
                for (int i = 0; i < 1000; i++) { new byte[10000].GetType(); }

                int keepTime = Environment.TickCount + 5000;
                while (Environment.TickCount < keepTime)
                    ;
            }
        }
        static void TestCallback(object sender, EventArgs e)
        {
            Console.WriteLine("callback method from default domain");
        }
    }

    #region 监视AppDomain的使用情况
    internal sealed class AppDomainMonitor : IDisposable
    {
        private AppDomain ad;
        private Int64 m_start_survive_mem;
        private Int64 m_start_all_mem;
        private TimeSpan m_ad_cpu;

        static AppDomainMonitor()
        {
            AppDomain.MonitoringIsEnabled = true; //必须显示打开才能查看性能指标属性
        }

        public AppDomainMonitor(AppDomain ad)
        {
            this.ad = ad ?? AppDomain.CurrentDomain;
            m_start_all_mem = this.ad.MonitoringTotalAllocatedMemorySize;
            m_start_survive_mem = this.ad.MonitoringSurvivedMemorySize;
            m_ad_cpu = this.ad.MonitoringTotalProcessorTime;
        }

        public void Dispose()
        {
            GC.Collect();

            Console.WriteLine("During last 2 GCs, AppDomain {0} data change:\r\n" +
                "All allocated memory: {1}\r\n" +
                "Survived objects memory: {2}\r\n" +
                "CPU timespan: {3}", ad.FriendlyName, ad.MonitoringTotalAllocatedMemorySize - m_start_all_mem, ad.MonitoringSurvivedMemorySize - m_start_survive_mem, (ad.MonitoringTotalProcessorTime - m_ad_cpu).TotalMilliseconds);
            Console.WriteLine("Process memory allocated in last GC: " + AppDomain.MonitoringSurvivedProcessMemorySize);
        }
    }
    #endregion
}
