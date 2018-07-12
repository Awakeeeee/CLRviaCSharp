using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Diagnostics;

namespace CLRviaCSharp
{
    class Chapter25_Thread
    {
        static void Main_1(string[] args)
        {
            Console.WriteLine("main thread start point");

            Console.WriteLine("Priority check:-------");
            Console.WriteLine("main thread priority: " + Thread.CurrentThread.Priority);
            Console.WriteLine("main thread is pool thread: " + Thread.CurrentThread.IsThreadPoolThread);

            Console.WriteLine(Process.GetCurrentProcess().ProcessName);
            Console.WriteLine(Process.GetCurrentProcess().PriorityClass);
            Console.WriteLine("Priority check END:-------");

            //显示创建一个线程, 用于执行一个可能特别耗时的方法
            Thread dedicateThread = new Thread(ThreadMethod);
            dedicateThread.Priority = ThreadPriority.Highest;
            //Start导致真正开辟一个系统线程, 7是给线程委托用的参数
            dedicateThread.Start(7);

            Console.WriteLine("main thread running...");
            Thread.Sleep(10000);

            dedicateThread.Join(); //d线程插入当前执行代码的线程, 阻塞后者, 直到d线程自己结束运行

            Console.WriteLine("main thread ends");
        }

        private static void ThreadMethod(object obj)
        {
            Console.WriteLine("Dedicate thread running: " + obj.ToString());
            Thread.Sleep(3000);
            Console.WriteLine("Dedicate thread ends");
        }
    }
}
