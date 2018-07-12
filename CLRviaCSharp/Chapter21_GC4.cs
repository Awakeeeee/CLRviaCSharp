using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter21_GC4
    {
        static void Main_21_4(string[] args)
        {
            GCNotification.GCDone += ActionInGC;

            string @in;

            while (true)
            {
                @in = Console.ReadLine();
                if (@in == "exit")
                    break;

                if (@in == "gc")
                    GC.Collect(0);
            }
        }

        static void ActionInGC(int v)
        {
            Console.WriteLine("recycle object is in generation " + v.ToString());
            Console.Beep();
            Console.WriteLine("gc has happened: " + GC.CollectionCount(0) + " times");
            Console.WriteLine("total memory allocated in bytes: " + GC.GetTotalMemory(false));
        }
    }

    #region GC Notifier
    internal static class GCNotification
    {
        public delegate void MyAction(int i); //prototype, 或直接使用Action<int>
        public static MyAction m_del;   //delegate instance
        public static event MyAction GCDone //instance setter
        {
            add {
                if (m_del == null)
                {
                    new GCObject(0);
                    new GCObject(2);
                }
                m_del += value;
            }
            remove {
                m_del -= value;
            }
        }

        private sealed class GCObject
        {
            private int expected_GC_Generation;
            public GCObject(int e) { expected_GC_Generation = e; }

            ~GCObject()
            {
                if (GC.GetGeneration(this) >= expected_GC_Generation) //在希望的代执行通知方法
                {
                    if (m_del != null)
                    {
                        MyAction temp = System.Threading.Interlocked.CompareExchange(ref m_del, null, null);
                        if (temp != null)
                        {
                            temp(expected_GC_Generation);
                        }
                    }
                }

                if (m_del != null && !AppDomain.CurrentDomain.IsFinalizingForUnload() && !Environment.HasShutdownStarted)  //如果没有给定通知方法, 就不用再不断延续GCObject了
                {
                    if (expected_GC_Generation == 0)
                        new GCObject(0);
                    else
                        GC.ReRegisterForFinalize(this); //resurrection
                }
            }
        }
    }
    #endregion
}
