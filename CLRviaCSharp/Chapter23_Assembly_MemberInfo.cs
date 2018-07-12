using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace CLRviaCSharp
{
    class Chapter23_Assembly_MemberInfo
    {
        static void Main_23_memberInfo(string[] args)
        {
            //分析查看Specimen类型:
            //不同情况下, 也许需要先遍历appdomain中的assembly, 再遍历其中type从而拿到一个type
            Type specimen = typeof(Specimen);

            BindingFlags bf = BindingFlags.Instance | BindingFlags.Public;
            Console.WriteLine("Type in Namespcae: " + specimen.Namespace);
            Console.WriteLine("Type interfaces:");
            Type[] interfaces = specimen.GetInterfaces();
            foreach (Type it in interfaces)
            {
                Console.WriteLine(it.ToString());
                InterfaceMapping im = specimen.GetInterfaceMap(it);
                foreach (MethodInfo mmm in im.TargetMethods)
                    Console.WriteLine(mmm.ToString());
            }
            Console.WriteLine("********************");

            foreach (MemberInfo info in specimen.GetMembers(bf))
            {
                Console.WriteLine("MemberType: {0}, Var Name: {1}", info.MemberType.ToString(), info.Name);
                Console.WriteLine("DeclaringType: " + info.DeclaringType);
                Console.WriteLine("ReflectedType: " + info.ReflectedType);
                Console.WriteLine("Module: " + info.Module);
                Console.WriteLine("Metadata Token: " + info.MetadataToken);
                Console.WriteLine("Attributes on this: " + info.GetCustomAttributes().Count());

                if (info.MemberType == MemberTypes.Method)
                {
                    MethodInfo mi = (MethodInfo)info;
                    Console.WriteLine("Generic method params: " + mi.ContainsGenericParameters);
                    ParameterInfo[] @params = mi.GetParameters();
                    foreach (ParameterInfo pi in @params)
                    {
                        Console.WriteLine("A Parameter info-----");
                        Console.WriteLine("Parameter Name:" + pi.Name);
                        Console.WriteLine("Parameter Type: " + pi.ParameterType);
                        Console.WriteLine("Parameter Position: " + pi.Position);
                        Console.WriteLine("Out param?: " + pi.IsOut);
                    }
                }
                Console.WriteLine("********************");
            }

            //用反射查看了Specimen类型后, 用反射调用它的方法
            //Binding -> Call
            Console.WriteLine("********************");
            Console.WriteLine("********************");
            BindingFlags bfc = BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance; //call ctor
            object ins = specimen.InvokeMember(null, bfc, null, null, null);
            Console.WriteLine(ins.GetType());

            BindingFlags bfi = BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod; //call method
            object @return = specimen.InvokeMember("Log", bfi, null, ins, new object[] { 5, "ahh" });

            BindingFlags bff = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField; //get private field
            object fi = specimen.InvokeMember("m_id", bff, null, ins, null);
            Console.WriteLine(fi.GetType() + " --- " + fi.ToString());

            BindingFlags bfip = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod; //call private method
            specimen.InvokeMember("PriLog", bfip, null, ins, null);

            MethodInfo bind = specimen.GetMethod("PriLog", bfip);
            bind.Invoke(ins, null);

            RuntimeMethodHandle rmh = bind.MethodHandle;    //Memberinfo类占用较多内存, 通过句柄引用他
            Console.WriteLine(MethodInfo.GetMethodFromHandle(rmh).Name);

            Console.WriteLine(GC.GetTotalMemory(true));
        }
    }

    class Specimen : IDisposable
    {
        private int m_id; //field
        public int M_id { get { return m_id; } } //property

        public event EventHandler M_Event; //event

        public Specimen() { m_id = 0; } //ctor

        public void Log(int i, string s) //method
        {
            Console.WriteLine("Speciman Log method is called!!!!!! " + i.ToString() + "," + s);
        }

        private void PriLog()
        {
            Console.WriteLine("Private method is called");
        }

        public void Dispose() { }
    }
}
