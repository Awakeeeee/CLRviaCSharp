using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CLRviaCSharp
{
    class Chapter23_Assembly
    {
        static void Main_Load(string[] args)
        {
            string tpath = @"C:\Program Files (x86)\Microsoft.NET\Primary Interop Assemblies\Microsoft.MSXML.dll";
            string pathAlt = @"D:\Microsoft.MSXML.dll";

            AssemblyName an = AssemblyName.GetAssemblyName(tpath);
            Console.WriteLine(an.FullName);

            Assembly abl = Assembly.Load(an);
            Console.WriteLine(abl.FullName);

            Assembly ablf = Assembly.LoadFrom(tpath);
            Console.WriteLine(abl.FullName);
            Console.WriteLine(abl == ablf);

            Assembly ablfi = Assembly.LoadFile(pathAlt); //不会自动处理依赖性
            Console.WriteLine(ablfi.FullName); //可重复Load同一个assemly
            Console.WriteLine(ablfi == abl);

            string dpath = @"D:\Projects\VSWorkspace\CLRviaCSharp\TestDll\bin\Debug\TestDll.dll";
            Assembly rolf = Assembly.ReflectionOnlyLoadFrom(dpath);
            try
            {
                Type t = rolf.GetType("MyTestLibrary");
                MethodInfo mi = t.GetMethod("LogAssemblyName", BindingFlags.Public | BindingFlags.Static);
                mi.Invoke(t, null);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }

            try
            {
                Assembly rol = Assembly.ReflectionOnlyLoad("ttt");
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            //Late Binding
            Assembly testDll = Assembly.LoadFrom(dpath);
            Type t2 = testDll.GetType("MyTestLibrary");
            MethodInfo mi2 = t2.GetMethod("Log");
            Console.WriteLine(mi2.Name);
            mi2.Invoke(t2, null);
        }
    }
}
