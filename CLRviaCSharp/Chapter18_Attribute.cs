using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLRviaCSharp
{
    class Chapter18_Attribute
    {
        static void Main(string[] args)
        {
            QiQiQiAttribute qa = new QiQiQiAttribute();
            Console.WriteLine(qa.GetType());
            FlagsAttribute fa = new FlagsAttribute();
            bool b = fa.Match(qa);
        }
    }

    //可隐藏的前缀: 表示这个attribute应用于哪些元素, class,struct,enum,interface,delegate都属于type前缀
    [type : Flags]
    enum AttTestEnum
    {
        A = 0x00,
        B = 0x01
    }

    #region 创建attribute
    //attribute本质是一个继承于System.Attribute的类的实例
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
    public class QiQiQiAttribute : Attribute
    {

    }
    #endregion
}
