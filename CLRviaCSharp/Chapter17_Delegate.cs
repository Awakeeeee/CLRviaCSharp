using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    public delegate void SuperFuncPointer();
    public delegate int CombinedDele();

    class Chapter17_Delegate
    {
        static void Main17(string[] args)
        {
            #region
            //实例代码见D:\MS VisualStudio\VSWorkspace\DelegateExample.cs
            #endregion

            #region delegate内部一日游
            SomeListener sl = new SomeListener();
            SuperFuncPointer sfp = new SuperFuncPointer(sl.OnCalled);
            Console.WriteLine(sfp.Target.GetType()); //_target -> 方法的实例对象
            Console.WriteLine(sfp.Method.Name); //_method -> 方法methodInfo
            #endregion

            #region 显式调用_invocationList
            CombinedDele cd = null;
            cd = Delegate.Combine(cd, new CombinedDele(MethodOne)) as CombinedDele;
            cd = Delegate.Combine(cd, new CombinedDele(MethodTwo)) as CombinedDele;
            Delegate[] list = cd.GetInvocationList();
            //这样就可以loop了
            int sum = 0;
            for (int i = 0; i < list.Length; i++)
            {
                CombinedDele d = list[i] as CombinedDele;
                sum += d.Invoke();
            }
            Console.WriteLine(sum);
            #endregion

            #region C#中委托的简化语法
            GiveMeDele(MethodOne); //1. 方法定义要一个delegate, 但是传入一个符合规范的方法就行
            //2. Lambda
            GiveMeDele(() => { return 5; });
            GiveMeDele(() => { Console.WriteLine("I am a lambda"); return 10; });
            //3. Lambda在一个方法内时, 其主体直接使用方法的局部变量, 实质是背后生成一个辅助类

            #endregion

            #region 运行时确定一个具体的delegate,生产一个实例并调用他
            Type runtimeT = Type.GetType(args[0]);
            //GetMethod需要指定non public否则找不到private方法
            System.Reflection.MethodInfo runtimeM = typeof(Chapter17_Delegate).GetMethod(args[1], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Delegate runtimeD;
            runtimeD = Delegate.CreateDelegate(runtimeT, runtimeM);

            //这里没有传参, 单DynamicInvoke可以接收任意参数, 并在内部判断是否可用
            object r = runtimeD.DynamicInvoke();
            Console.WriteLine(r.ToString());
            #endregion
        }

        static void GiveMeDele(CombinedDele cd)
        {
            return;
        }

        static int MethodOne()
        {
            return 1;
        }
        static int MethodTwo()
        {
            return 2;
        }
    }

    internal class SomeListener
    {
        public void OnCalled()
        {
            Console.WriteLine("SomeListener callback method is called");
        }
    }
}
