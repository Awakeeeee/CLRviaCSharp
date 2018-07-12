using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.Runtime;
using System.Runtime.Remoting;

namespace CLRviaCSharp
{
    class Chapter23_Assembly_ObjectInstance
    {
        static void Main_ObjectInstance(string[] args)
        {
            //有了一个类型对象后(Type的派生类实例如RuntimeType实例), 如何用他创建一个对象
            Version v = typeof(System.Object).Assembly.GetName().Version;
            string asb = "System.Messaging, PublicKeyToken=b03f5f7f11d50a3a, Culture=neutral, Version=" + v.ToString();
            Assembly msxml = Assembly.Load(asb);
            Type[] types = msxml.GetExportedTypes();
            Console.WriteLine("All types defined in System.Messaging.dll:");
            foreach (Type ty in types)
            {
                Console.WriteLine(ty.FullName);
            }
            Console.WriteLine(Environment.NewLine);

            Type t = types[types.Length - 5];
            string assemblyName = t.Assembly.FullName;
            string typename = t.FullName;
            Console.WriteLine(typename);
            Console.WriteLine(assemblyName);
            Console.WriteLine(Environment.NewLine);
            
            //-
            object m1 = System.Activator.CreateInstance(t); //Type参数及ctor参数
            Console.WriteLine("m1 : " + m1.GetType());

            //-
            //ObjectHandle oh = Activator.CreateComInstanceFrom(assemblyName, typename); //TODO 怎么传
            //object m2 = oh.Unwarp();

            //-
            AppDomain ad = AppDomain.CurrentDomain;
            object m3 = ad.CreateInstanceAndUnwrap(assemblyName, typename);
            Console.WriteLine("m3 : " + m3.GetType());

            //-
            //object m4 = t.InvokeMember();

            //-
            //object m5 = t.GetConstructor().Invoke(null);

            //- 数组
            int[] arr = (int[])System.Array.CreateInstance(typeof(int), 5);
            Console.WriteLine("Array : " + arr.GetType());
            Console.WriteLine(arr.Length);

            //- 委托
            MyDel md = (MyDel)System.Delegate.CreateDelegate(typeof(MyDel), typeof(Chapter23_Assembly_ObjectInstance).GetMethod("SomeMethod"), true);
            Console.WriteLine("Delegate : " + md.GetType());
            Console.WriteLine(md.GetInvocationList().Length);

            //- 泛型类型
            Type open = typeof(TestGeneraic<,>);
            Type close = open.MakeGenericType(typeof(string), typeof(int));
            object m6 = Activator.CreateInstance(close);
            Console.WriteLine("m6 : " + m6.GetType());
        }

        public delegate void MyDel();
        public static void SomeMethod()
        { }
    }

    public class TestGeneraic<T1, T2>
    { }
}
