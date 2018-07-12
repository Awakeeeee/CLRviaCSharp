using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace CLRviaCSharp
{
    class Chapter25_Thread_BackgroundThread
    {
        static void Main(string[] args)
        {
            Thread bt = new Thread(BgFunc);

            bt.IsBackground = false; //前台线程当自己运行完是自然结束
            //bt.IsBackground = true; //后台线程因所有前台线程的结束而结束

            bt.Start(null);

            Console.WriteLine("main thread ends");
            //在这里, 如果bt是后台线程, 那么main的结束使得bt强制结束, 程序结束
            //如果bt是前台线程, main已经结束, 但还要等bt结束, 程序继续
        }

        private static void BgFunc(object obj)
        {
            Thread.Sleep(5000);
            Console.WriteLine("worker thread ends");
        }
    }
}
