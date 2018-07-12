using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CLRviaCSharp
{
    class Chapter22_AppDomain
    {
        static void Main22_1(string[] args)
        {
            Marshalling();
        }

        static void Marshalling()
        {
            //the AppDomain of current executing thread
            AppDomain currentThreadDomain = System.Threading.Thread.GetDomain();
            //currentThreadDomain = System.AppDomain.CurrentDomain; //same as above
            Console.WriteLine("Current AppDomain friendly name: " + currentThreadDomain.FriendlyName); //CLRviaCsharp.exe
            Console.WriteLine(Environment.NewLine);

            //Assembly.GetEntryAssembly而不是AppDomain.xxx
            string exeAssemblyName = System.Reflection.Assembly.GetEntryAssembly().FullName;
            Console.WriteLine("Assembly that has Main method in current AppDomain：" + exeAssemblyName);
            Console.WriteLine(Environment.NewLine);

            //the log from other assembly in this appdomain
            MyTestLibrary.DllMethod();
            Console.WriteLine(Environment.NewLine);

            //Marshal by ref
            Console.WriteLine("#1 : Cross AppDomain boundary by proxy ref");
            AppDomain ad2 = null;
            ad2 = AppDomain.CreateDomain("AppDomain #2", null, null); //null -> 继承当前domain的安全性和配置设定
            //通过string参数指定assembly和类型名
            //线程跳转到AppDomain 2, 加载assembly, 创建对象
            //回到AppDomain 1, 根据指定创建代理类型, 创建代理对象, 返回代理对象引用至ors
            ObjRefSend ors = null;
            //ors = new ObjRefSend();
            //ors.Method_Void_NoParams();
            ors = (ObjRefSend)ad2.CreateInstanceAndUnwrap(exeAssemblyName, "ObjRefSend"); //TODO ?
            Console.WriteLine("CLR says ors type is " + ors.GetType());
            Console.WriteLine("Actually is it a Proxy instance? " + System.Runtime.Remoting.RemotingServices.IsTransparentProxy(ors)); //RemoteingService is not in Intellisense
            Console.WriteLine("proxy obj 'release manager' life time: " + ors.GetLifetimeService());
            ors.Method_Void_NoParams(); //call method on proxy instance, execute back in AD2
            AppDomain.Unload(ad2);
            try
            {
                ors.Method_Void_NoParams();
            }
            catch (AppDomainUnloadedException e)
            {
                Console.WriteLine("Call Fails because " + e.Message);
            }
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("#2 : Cross AppDomain boundary by value");
            ad2 = AppDomain.CreateDomain("AppDomain #2", null, null);
            ors = (ObjRefSend)ad2.CreateInstanceAndUnwrap(exeAssemblyName, "ObjRefSend");
            ObjValueSend ovs = ors.Method_Return_NoParams(); //返回类型是可序列化类型, Ad2中创建一个对象, 其序列化后的bytes回到Ad1, 反序列化得到完全复制的另一个对象
            ovs.MemberMethod();
            AppDomain.Unload(ad2);
            try
            {
                ovs.MemberMethod();
            } catch (AppDomainUnloadedException e)
            {
                Console.WriteLine("Call Fails because " + e.Message);
            } finally
            {
                Console.WriteLine("Call Succeeds");
            }
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("#3 : Object Cannot cross AppDomain boundary");
            ad2 = AppDomain.CreateDomain("AppDomain #2", null, null);
            ors = (ObjRefSend)ad2.CreateInstanceAndUnwrap(exeAssemblyName, "ObjRefSend");
            try
            {
                ObjCannotSend ocs = ors.Method_Return_Params("pass in string");
            } catch (System.Runtime.Serialization.SerializationException e)
            {
                Console.WriteLine("Call Fails because " + e.Message);
            }
        }
    }

    //引用封送需要继承自System.MarshaByRefObject
    public sealed class ObjRefSend : MarshalByRefObject
    {
        public ObjRefSend()
        {
            Console.WriteLine("{0} object ctor is called in {1}", this.GetType().ToString(), Thread.GetDomain().FriendlyName);
        }

        public void Method_Void_NoParams()
        {
            Console.WriteLine("Member method code is executed in " + Thread.GetDomain().FriendlyName);
        }

        public ObjValueSend Method_Return_NoParams()
        {
            ObjValueSend ovs = new ObjValueSend();
            return ovs; //如果方法返回一个类型, 也必须符合隔离, 这里是值封送
        }

        public ObjCannotSend Method_Return_Params(string s) //参数也必须符合隔离, string是值封送而且有特殊处理
        {
            ObjCannotSend ocs = new ObjCannotSend();
            return ocs;
        }
    }

    //值封送需要标记为[Serializable]
    [Serializable]
    public sealed class ObjValueSend
    {
        DateTime dt = DateTime.Now;
        public ObjValueSend()
        {
            Console.WriteLine("{0} object ctor is called in {1}", this.GetType().ToString(), Thread.GetDomain().FriendlyName);
            Console.WriteLine("Created at " + dt.ToString("F"));
        }

        public void MemberMethod()
        {
            Console.WriteLine("Member method of value send object is being executed in " + Thread.GetDomain());
        }
    }

    //如果不是以上两种标记, 那么obj无法跨越AppDomain边界
    public sealed class ObjCannotSend
    {
        public ObjCannotSend()
        {
            Console.WriteLine("{0} object ctor is called in {1}", this.GetType().ToString(), Thread.GetDomain().FriendlyName);
        }
    }
}
