using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter15_Enum
    {
        static void Main15(string[] args)
        {
            Console.WriteLine(Enum.GetUnderlyingType(typeof(MyEnum)));

            #region Enum <-> number / Enum -> string
            //MyEnum的实例其实就是一个byte
            MyEnum me = (MyEnum)23;
            
            //查看一个enum实例的方法
            Console.WriteLine(me.ToString()); //Enum重写了Object.Tostring, 虽然enum实例是个数字, 但返回的是对应的变量名A
            Console.WriteLine(me.ToString("D")); //查看常量值的十进制
            Console.WriteLine(me.ToString("X")); //查看常量值的十六进制
            Console.WriteLine(Enum.Format(typeof(MyEnum), (Byte)23, "G")); //静态方法, 不通过实例

            Console.WriteLine("\r\nDisplay the values, 因为ToString被重写, 打印出来的是常量数值的符号名");
            Array arr = Enum.GetValues(typeof(MyEnum)); //Array是个抽象基类, GetValue返回子类, MyEnum[]
            foreach (var b in arr)
            {
                Console.WriteLine(b);
            }
            Console.WriteLine("\r\nDisplay the names");
            string[] names = Enum.GetNames(typeof(MyEnum));
            foreach (string s in names)
            {
                Console.WriteLine(s);
            }
            #endregion

            #region string -> enum
            Console.WriteLine("\r\n\r\n");
            //Parse方法, 本质是string->object
            MyEnum e = (MyEnum)Enum.Parse(typeof(MyEnum), "A");
            Console.WriteLine(e.ToString("D"));

            //检查是否有定义
            if (Enum.IsDefined(typeof(MyEnum), "s"))
            {
                Console.WriteLine("Name is defined");
            }
            else {
                Console.WriteLine("Name is not defined");
            }
            #endregion

            #region Bit Flag
            //展示内置bit flag的一个有关文件操作的内容
            //获得.exe的位置
            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            Console.WriteLine(path);
            System.IO.FileAttributes fa = System.IO.File.GetAttributes(path);
            Console.WriteLine("Is the file read only?");
            Console.WriteLine(fa);
            Console.WriteLine((fa&System.IO.FileAttributes.ReadOnly) != 0);

            //Bit flag的特点是查找时要考虑几个选项组合的结果
            //bit flag -> string
            Console.WriteLine("\r\n" + (MyBitFlag)3); //3 -> 1|2 -> ToString -> Layer1,Layer2
            Console.WriteLine(((MyBitFlag)0x3F).ToString("F")); //"F"格式和[Flags]类似

            //string -> bit flag
            MyBitFlag mb = (MyBitFlag)Enum.Parse(typeof(MyBitFlag), "Layer1 , Layer2 "); //内部空格会被切掉
            Console.WriteLine(mb.ToString("D"));
            //Parse有报错可能 - 无法找到时
            #endregion

            #region 通过extention method向enum中添加方法
            MyBitFlag m = MyBitFlag.Layer6 | MyBitFlag.Layer3;
            m.ForeachDoaction((MyBitFlag mf) => { Console.WriteLine("Foreach Print: " + mf.ToString()); });
            #endregion
        }
    }

    //基础类型byte就像是MyEnum的父类
    //enum的定义与class同级, 而不是嵌套进去
    internal enum MyEnum : byte
    {
        A = 0x0001,
        B = 0x0002
    }

    [Flags]
    internal enum MyBitFlag
    {
        Layer1 = 0x0001,
        Layer2 = 0x0002,
        Layer3 = 0x0004,
        Layer4 = 0x0008,
        Layer5 = 0x0010,
        Layer6 = 0x0020,
    }
}
