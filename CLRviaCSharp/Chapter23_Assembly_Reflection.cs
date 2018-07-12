using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CLRviaCSharp
{
    class Chapter23_Assembly_Reflection
    {
        static void Main_Reflection(string[] args)
        {
            //理解AppDomain.AssemblyResolve Event
            AppDomain ad = AppDomain.CurrentDomain;
            ad.AssemblyResolve += ResolveMyAssemblyFailProblem;
            try
            {
                Assembly x = Assembly.Load("invliad identification");
            }
            catch (Exception e)
            {
                Console.WriteLine("Further exceptions are swallowed " + e.Message);
            }

            Assembly asb = Assembly.LoadFrom(@"D:\Projects\VSWorkspace\CLRviaCSharp\TestDll\bin\Debug\TestDll.dll");

            //获得一个assembly中定义的类型
            foreach (Type t in asb.GetExportedTypes())
            {
                Console.WriteLine(t.FullName);
            }

            #region 获得一个Type实例的方法
            //-
            MyRefType mrt = new MyRefType(1);
            Type t1 = mrt.GetType();
            Console.WriteLine("GetType on obj instance full name: " + t1.FullName); //class name with namespace
            Console.WriteLine("GetType on obj instance assembly qualified name: " + t1.AssemblyQualifiedName);  //class name with assembly info
            string fullname = t1.FullName;
            string assemblyqualifiedname = t1.AssemblyQualifiedName;
            //-
            Type t2 = Type.GetType("CLRviaCSharp.MyRefType");
            Console.WriteLine("Type class static method: " + t2.FullName);
            //-
            Type t3 = Type.ReflectionOnlyGetType(assemblyqualifiedname, false, false);
            Console.WriteLine("Type class static reflection only: " + t3.FullName);
            //-
            Assembly asbthis = Assembly.GetEntryAssembly();
            Type t4 = asbthis.GetType(fullname);
            Console.WriteLine("Assembly instance method: " + t4.FullName);
            //- *
            Type earlyType = typeof(MyRefType);
            Console.WriteLine(earlyType.FullName);
            #endregion

            #region 获得了一个Type后, 查看他的信息
            Console.WriteLine("\r\n\r\n");
            Console.WriteLine("Full Name: " + earlyType.FullName);
            Console.WriteLine("Assembly Name: " + earlyType.Assembly.GetName());
            Console.WriteLine("IsAbstract: " + earlyType.IsAbstract);
            Console.WriteLine("BaseType: " + earlyType.BaseType);
            Console.WriteLine("GUID: " + earlyType.GUID);
            Console.WriteLine("IsSealed: " + earlyType.IsSealed);
            Console.WriteLine("Namespace: " + earlyType.Namespace);
            #endregion

            #region 一段列出FCL assembly中所有Exception派生类的程序
            Console.WriteLine(Environment.NewLine);
            Console.Write("Test use of C# query keywords:");
            var v = (from i in (new int[] { 1, 2, 3 })
                     where i >= 2
                     select i
                     ).ToArray<int>();
            Console.WriteLine(v.Length);

            Console.WriteLine(Environment.NewLine);
            LoadFCLAssemblies();
            Func<Type, string> GetFamilyName = null;
            GetFamilyName = t => "-" + t.FullName + (t.BaseType == typeof(System.Object) ? string.Empty : GetFamilyName(t.BaseType)); //这只是一个方法定义的方式, "-"只是用来以后分割名字

            var exceptionTree = //在()使用C# Query keywords
                (
                    from ab in AppDomain.CurrentDomain.GetAssemblies() //loop这个AppDomain的所有assembly, 对于每个assembly
                    from ty in ab.GetExportedTypes() //loop这个assembly中所有类型, 对于每个类型
                    where ty.IsClass && ty.IsPublic && typeof(Exception).IsAssignableFrom(ty) //如果ty满足这些条件, 执行以下操作
                    let itemPre = GetFamilyName(ty).Split('-').Reverse().ToArray() //字符串变为[]{BaseName,ChildName}
                    let item = String.Join("-", itemPre, 0, itemPre.Length - 1) //字符串变为"BaseName-ChildName"
                    orderby item
                    select item //输出item
                ).ToArray(); //所有输出形成一个数组

            Console.WriteLine("All Exception Types: ");
            foreach (string item in exceptionTree)
            {
                string[] names = item.Split('-');  //并不是都显示, 对于每一个结果只显示最子类, 前面查找子类的父类仅仅只是为了决定前面有多少个空格缩进
                Console.WriteLine(new string(' ', 3 * (names.Length - 1)) + names[names.Length - 1]);
            }
            #endregion
        }

        //构建一系列FCL Assembly全名并Load
        private static void LoadFCLAssemblies()
        {
            String[] assemblies = {
                "System,    PublicKeyToken={0}",
                "System.Core,    PublicKeyToken={0}",
                "System.Design,    PublicKeyToken={1}", //这个key必须给对
                "System.DirectoryServices,    PublicKeyToken={1}",
                "System.Drawing,    PublicKeyToken={1}",
                "System.Drawing.Design,    PublicKeyToken={1}",
                "System.Management,    PublicKeyToken={1}",
                "System.Messaging,    PublicKeyToken={1}",
                "System.Runtime.Remoting,    PublicKeyToken={0}",
                "System.Security,    PublicKeyToken={1}",
                "System.ServiceProcess,    PublicKeyToken={1}",
                "System.Web,    PublicKeyToken={1}",
                "System.Web.RegularExpressions,    PublicKeyToken={1}",
                "System.Web.Services,    PublicKeyToken={1}",
                "System.Windows.Forms,    PublicKeyToken={0}",
                "System.Xml,    PublicKeyToken={0}"
            };

            //TODO 来源
            string EcmaPublicKeyToken = "b77a5c561934e089";
            string MSPublicKeyToken = "b03f5f7f11d50a3a";

            Version version = typeof(System.Object).Assembly.GetName().Version;

            foreach (string a in assemblies)
            {
                //例: CLRviaCSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
                string fullID = string.Format(a, EcmaPublicKeyToken, MSPublicKeyToken) + ", Culture=neutral, Version=" + version.ToString();
                Assembly.Load(fullID);
            }
        }

        //Prototype : ResolveEventHandler delegate
        private static Assembly ResolveMyAssemblyFailProblem(object sender, EventArgs e)
        {
            Console.WriteLine("Assembly load fail problem is resolved.");
            return null;
        }
    }
}
