using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter21_GC2
    {
        static void Main21_2(string[] args)
        {
            //GetExecute ge = new GetExecute();
            //ge.Test();

            new GCBeep();

            //Microsoft.Win32.SafeHandles.SafeWaitHandle swh = CreateEventGood(IntPtr.Zero, false, false, null);
        }

        #region 使用safe handle
        [System.Runtime.InteropServices.DllImport("Kernel32", CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint ="CreateEvent")]
        private static extern Microsoft.Win32.SafeHandles.SafeWaitHandle CreateEventGood(
            IntPtr handle, bool selfReset, bool init, string name
            );
        #endregion
    }

    #region 确保Finaliza调用的特殊基类CriticalFinalizerObject
    //继承CriticalFinalizaerObject
    //该类的finalize方法在构造时就编译好, 且在其他finalize方法之后执行
    internal class GetExecute : System.Runtime.ConstrainedExecution.CriticalFinalizerObject
    {
        public GetExecute() { }
        ~GetExecute()
        {
            if (fs != null)
                fs.Close();
            Console.WriteLine("Called after all others");
        } //IL: protected override Finalize()

        //FileStream操作的文件属于本地资源, 不是能被gc管理的内存
        //于是其中定义了句柄(handle), 一个IntPtr
        //和终结方法~FileStream
        System.IO.FileStream fs = new System.IO.FileStream("D:\\textfile.txt", System.IO.FileMode.Open);

        public void Test()
        {
            fs.Flush();
        }
    }
    #endregion

    #region GC Beep
    class GCBeep
    {
        public GCBeep() { }
        ~GCBeep()
        {
            Console.Beep();

            //如果是自然情况下的终结(非appdomain卸载和进程结束)
            if(!AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)
                new GCBeep();
        }
    }
    #endregion
}
