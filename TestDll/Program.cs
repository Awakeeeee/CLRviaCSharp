using System;
using System.Security.Policy;

public sealed class MyTestLibrary
{
    public const Int32 SIZE = 100;
    public static Int32 num = 20;

    public static void DllMethod()
    {
        Console.WriteLine("Log From custom DLL");
        Console.WriteLine("Dll class AppDomain: " + AppDomain.CurrentDomain.FriendlyName);
        Console.WriteLine("Dll class assembly: " + System.Reflection.Assembly.GetExecutingAssembly());
        Console.WriteLine("Dll entry assembly: " + System.Reflection.Assembly.GetEntryAssembly());
    }

    public static string LogAssemblyName()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().FullName;
    }

    public static void Log()
    {
        Console.WriteLine("LLLOOGOGOGO");
    }
    public void LogIns()
    {
        Console.WriteLine("succeed");
    }
}

public class AppDomainCustomManager : AppDomainManager
{
    public override AppDomain CreateDomain(string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
    {
        Console.WriteLine("AppDomain created by my custom AppDomain Manager.");
        Console.WriteLine("This is a test. No appdomain is actually created.");
        //return base.CreateDomain(friendlyName, securityInfo, appDomainInfo);
        return null;
    }
}
