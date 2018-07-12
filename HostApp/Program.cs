using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Reflection;
using QIQIQI.HostSDK;

namespace HostApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //找到第三方add-in
            //这就是反射在构建可扩展应用程序中的一个应用
            string path = @"D:\Projects\VSWorkspace\CLRviaCSharp\UserAddIn\bin\Debug\netstandard2.0\UserAddIn.dll";
            Assembly asb = Assembly.LoadFrom(path);

            List<Type> all = new List<Type>();
            foreach (Type t in asb.GetExportedTypes())
            {
                if (t.IsClass && typeof(IAddin).IsAssignableFrom(t))
                {
                    all.Add(t);
                }
            }

            Form fr = new Form() { Text = "Test Host App", FormBorderStyle = FormBorderStyle.FixedSingle, Width = 800, Height = 600};
            Button btn = new Button { Text = "Check All Add-ins", Width = fr.Width / 2};
            btn.Click += (sender, e) => { foreach (Type ty in all) { IAddin obj = (IAddin)Activator.CreateInstance(ty); MessageBox.Show(obj.CommunicationMethod(3)); } };
            fr.Controls.Add(btn);

            Application.Run(fr);
        }
    }
}
