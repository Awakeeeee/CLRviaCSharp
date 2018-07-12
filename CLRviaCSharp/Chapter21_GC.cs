using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: System.Diagnostics.Debuggable(System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations)]

namespace CLRviaCSharp
{
    class Chapter21_GC
    {
        static void Main_21_1(string[] args)
        {
            //如果有code optimization, 定义了而没用的对象, 会在下次gc被回收
            //不会等到方法结束
            //release配置的默认带有code optimization, 于是会被回收
            //debug配置避免调试人员疑惑, 默认没有code optimization, 于是JIT会等到方法结束
            System.Threading.Timer timer = new System.Threading.Timer(TimerCallback, null, 0, 500);

            Console.ReadKey();

            //强行让一个未使用的对象不被回收(不管是否有code optimization), 这相当有使用timer引用, 是个根
            timer.Dispose();
        }
        
        static void TimerCallback(object o)
        {
            Console.WriteLine("Timer calls");
            System.GC.Collect(); //强制垃圾回收
        }
    }
}
