using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CLRviaCSharp
{
    class Chapter9_Parameter
    {
        public static int val = 0;

        static void Main9(string[] args)
        {
            ParameterTest pt = new ParameterTest();

            //var:叫做隐式类型的局部变量, 可用于向方法传参, 但不能用于定义方法参数
            var p4 = new Decimal(3.5);

            //命名参数和传参顺序:
            //尽管参数位置是i1, i2, s, 但传参计算时总是按照书写顺序的从左--->右顺序, 先算i2再算i1再算s, 最后按i1 i2 s顺序摆放
            pt.ParamMethod(i2: val++, i1: val++, s: val.ToString(), d : p4);

            int op;
            int rp = 1;
            pt.OutMethod(out op);
            pt.RefMethod(ref rp);

            //调用者为引用类型指针分配内存
            FileStream fs = null;
            //被调用者操作这个引用类型指针
            ProcessFile(ref fs);

            MyRefType a = new MyRefType(5);
            MyRefType b = new MyRefType(10);
            WrongSwap(a, b);
            Console.WriteLine(a.Log() + "," + b.Log());
            CorrectSwap(ref a, ref b);
            Console.WriteLine(a.Log() + "," + b.Log());

            pt.MiscMethod("hey", 3, 3.5f, new MyRefType(3));
        }

        #region 对引用类型使用ref(out同理)
        static void ProcessFile(ref FileStream fs)
        {
            string path = @"D:\MS VisualStudio\VSWorkspace\CLRviaCSharp";

            if (fs != null)
                fs.Close();

            int count = 0;
            while (true)
            {
                if (count > 2)
                {
                    fs = null;
                    return;
                }
                else
                {
                    count++;
                    fs = new FileStream(path + @"\t" + count.ToString() + ".txt", FileMode.Open);
                    StreamReader reader = new StreamReader(fs);
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
        }
        //Swap意在改变指针, 而传入引用类型的指针时, 传递的是指针本身的一个副本
        //该副本指向同一个对象, 因此对象内容可以通过副本改变, 而对指针副本的改变不影响原指针
        static void WrongSwap(MyRefType a, MyRefType b)
        {
            //a,b现在是指针的副本, 不影响原指针
            MyRefType t = a;
            a = b;
            b = t;
            //而原指针和原指针副本指向同一个对象, 通过副本可以修改对象内容
            a.idx = 100;
        }
        static void CorrectSwap(ref MyRefType a, ref MyRefType b)
        {
            MyRefType t = a;
            a = b;
            b = t;
        }
        //通用交换方法
        static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }
        #endregion
    }

    class ParameterTest
    {
        //可选参数组必须在实参后面
        public void ParamMethod(int i1, int i2, string s, decimal d, float f = 0.0f)
        {
            Console.WriteLine("{0}, {1}, {2}, {3}, {4}", i1, i2, s, d, f);
        }

        //从编译角度来看, out要求必须在方法内初始化
        public void OutMethod(out int ov)
        {
            ov = 100;
            ov++; //初始化后才可以读取
            Console.WriteLine("out param: " + ov.ToString());
        }

        //从编译角度看, ref要求必须在方法前初始化
        public void RefMethod(ref int rv)
        {
            rv++;
            Console.WriteLine("ref param: " + rv.ToString());
        }
        public void RefMethod(int rv)
        {
            //ref或者out和普通参数, 是区分方法重载的标准, 因为metadata不同
            //而ref和out不能区分重载
        }

        //一个接受任意类型(object), 任意数量(param attribute)参数的方法
        public void MiscMethod(params Object[] o)
        {
            foreach (var item in o)
            {
                Console.WriteLine(item.GetType());
            }
        }
    }

    class MyRefType
    {
        public int idx;
        public MyRefType(int _idx) { idx = _idx; }
        public string Log() { return idx.ToString(); }

        public override string ToString()
        {
            return "HeiHeiHei";
        }
    }
}
