using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    //本地资源是非托管的, 但是这不意味着GC完全不管他们
    //CLR眼中位图只是一个handle, 但可能指向10MB的图片, 他们存在于"进程的内存"
    class Chapter21_GC5
    {
        static void Main21_5(string[] args)
        {
            //CreateLotsOfLargeResources(10, 0);
            //Console.WriteLine(Environment.NewLine);
            //CreateLotsOfLargeResources(15, 100 * 1024 * 1024);

            //for (int i = 0; i < 10; i++)
            //{
            //    new ObjTrackedByHandleCollector();
            //}

            Console.WriteLine("Is GC in server mode : " + System.Runtime.GCSettings.IsServerGC);
            //CreateLargeObject();

            Console.WriteLine("current heap memory use: " + GC.GetTotalMemory(false));
            byte[] obj = new byte[100];
            //CreateLargeObject();
            Console.WriteLine("current heap memory use: " + GC.GetTotalMemory(false));
            GC.Collect();
            Console.WriteLine("current heap memory use: " + GC.GetTotalMemory(true)); //true - 等待gc结束后从新获取读数

            Console.WriteLine("GC collect gen 0: " + GC.CollectionCount(0));
            Console.WriteLine("GC collect gen 1: " + GC.CollectionCount(1));
            Console.WriteLine("GC collect gen 2: " + GC.CollectionCount(2));
        }

        static void CreateLotsOfLargeResources(int num, int size)
        {
            for (int i = 0; i < num; i++)
            {
                new ObjLinkToLargeResource(size);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        static void CreateLargeObject()
        {
            byte[] big = new byte[85001];
            Console.WriteLine(GC.GetGeneration(big));
        }
    }

    internal sealed class ObjLinkToLargeResource
    {
        private int resourceSize;
        public ObjLinkToLargeResource(int size)
        {
            resourceSize = size;

            if (resourceSize > 100) //greater than a threshold
            {
                GC.AddMemoryPressure(resourceSize);
            }
            Console.WriteLine("Create obj with resource size " + resourceSize.ToString());
        }

        ~ObjLinkToLargeResource()
        {
            if (resourceSize > 100)
            {
                GC.RemoveMemoryPressure(resourceSize);
            }
            Console.WriteLine("Collect obj with resource size " + resourceSize.ToString());
        }
    }

    internal sealed class ObjTrackedByHandleCollector
    {
        //监控这个类型的实例数量, 大于设定值时则强行GC.Collect回收该类型对象
        //3是index, 3代表可以有0123共4个实例
        private static System.Runtime.InteropServices.HandleCollector hc = new System.Runtime.InteropServices.HandleCollector("Tracker", 3);

        public ObjTrackedByHandleCollector()
        {
            hc.Add();
            Console.WriteLine("A new instance of Obj has created, current count: " + hc.Count);
        }

        ~ObjTrackedByHandleCollector()
        {
            hc.Remove();
            Console.WriteLine("An instance of Obj has been removed from handle tracker, current count: " + hc.Count);
        }
    }
}
