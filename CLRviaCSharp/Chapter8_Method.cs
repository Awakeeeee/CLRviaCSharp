using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CLRviaCSharp
{
    class Chapter8_Method
    {
        static void Main8(string[] args)
        {
            //这里用的是Child的默认实例构造器, 他不是什么也不做而是调用了基类的无参构造器
            //并且用基类的方法看到基类的private成员也没有问题
            Child c = new Child(2);
            c.Log();

            //Test1();
            //Test2();

            Console.WriteLine(~3);

            Parent p = 5;   //call implicit cast operator
            p.Log();
            Int32 x = (Int32)p; //call explicit cast operator
            Console.WriteLine(x.ToString());

            //像single, int等基元类型, F12查看元数据列表, 会发现没有类型转换操作符的定义, 这些基元类型的转换代码是直接生成IL代码的
            System.Single example_float = 0;
            //Decimal是一个很好的样板, 如何来定义自己类型的各种操作符重载, 转换操作符等
            System.Decimal example_decimal = 0;

            //调用定义在CLRviaCSharp中的一个扩展方法
            example_float.LogMe();

            string something = "helloworldy";
            something.PrintList();

            c.ExtentLog();
        }

        #region cctor的调用时机会影响性能
        static void Test1()
        {
            //首次调用beforefieldinit类和precise类
            Stopwatch sw = Stopwatch.StartNew();
            for (Int32 i = 0; i < 1000000000; i++)
            {
                BeforeFieldInit.num = 1;
            }
            Console.WriteLine("First call beforefieldinit: " + sw.Elapsed);

            Stopwatch se = Stopwatch.StartNew();
            for (Int32 i = 0; i < 1000000000; i++)
            {
                Precise.num = 1;
            }
            Console.WriteLine("First call precise: " + sw.Elapsed);
        }
        static void Test2()
        {
            //二次调用beforefieldinit类和precise类, JIT肯定已知cctor都已经调用过
            Stopwatch sw = Stopwatch.StartNew();
            for (Int32 i = 0; i < 1000000000; i++)
            {
                BeforeFieldInit.num = 1;
            }
            Console.WriteLine("2nd call beforefieldinit: " + sw.Elapsed);

            Stopwatch se = Stopwatch.StartNew();
            for (Int32 i = 0; i < 1000000000; i++)
            {
                Precise.num = 1;
            }
            Console.WriteLine("2nd call precise: " + sw.Elapsed);
        }
        #endregion
    }

    #region 父子类的ctor
    class Parent
    {
        private int a;
        protected string s;

        public Parent()
        {
            Console.WriteLine("Parent custom no-arg constructor is called");
            a = 1;
            s = "default";
        }

        public void Log()
        {
            Console.WriteLine(a.ToString() + " , " + s);
        }

        //public static
        //参数必须至少有一个和定义类型相同
        public static Parent operator +(Parent a, Parent b)
        {
            Parent p = new Parent();
            p.a = a.a + b.a;
            return p;
        }

        //public static
        //参数或者返回类型必须有一个和定义类型相同
        //有implict和explict的区分
        //定义int向parent的隐式转换
        public static implicit operator Parent(Int32 other)
        {
            Parent p = new Parent();
            p.a = other;
            return p;
        }
        //定义由parent向int的显式转换
        public static explicit operator Int32(Parent other)
        {
            return other.a;
        }
    }

    class Child : Parent
    {
        private int c;

        public Child(int cc)
        {
            Console.WriteLine("Child custom constructer with int arg is called");
            c = cc;
            //this = new Child(0); //Class中, this实例引用是只读的, 和struct有区别
        }
    }
    #endregion

    #region struct的instance constructor的规则
    struct MyValue
    {
        public int x;
        public int y;
        public static int z = 2;    //static字段是特例, 可以inline
        //public int z = 2; //ERROR:struct中不可以inline初始化字段

        //public MyValue() { } //ERROR:不能为struct定义无参构造器, struct默认也不会生成一个无参构造器
        //public MyValue(int _x) { x = _x; } //ERROR:可以为struct定义有参构造器, 但其中必须将全部字段(非static字段即可)初始化

        public MyValue(int xx, int yy)
        {
            this = new MyValue();   //对struct来说, this实例引用是r/w的, 可以赋值 --> 而class中this引用是只读的
            x = xx;
        }
    }
    #endregion

    #region cctor
    internal sealed class BeforeFieldInit
    {
        //没有显式cctor, 有static field的inline初始化
        public static Int32 num = 100;
    }
    internal sealed class Precise
    {
        //有显式的cctor
        public static Int32 num;
        static Precise()
        {
            num = 100;
        }
    }
    #endregion

    #region 分部方法
    partial struct MyValueBase
    {
        public int value;

        //partial method必须且默认是private访问级, 不可修改
        //partial method必须定义在partial class/struct内, interface也可以是partial, 单其中不能定义partial method, 因为interface中的method都是public
        //可以像abstrct一样无主体, 如果不存在有主体的其他部分, 那么这个分部方法'未实现', 未实现的分部方法会被忽略, 因此运行时可能根本不存在一个BaseMethod()
        //因为可能不存在的原因, partial method必须返回null, 以免有东西以方法的返回值来初始化
        //因为可能不存在的原因, partial method不能有out参数, 因为out要求方法内代码初始化参数, 而方法没有任何代码
        //未实现的partial method不能加入委托

        //除以上之外, 分部方法和普通方法一样, 可以是static, generic, etc
        static partial void BaseMethod<T>(ref int value, T other);
    }

    //通常另一部分写在其他文件里, 可能是某些用户的扩展
    partial struct MyValueBase
    {
        static partial void BaseMethod<T>(ref int value, T other)
        {
            value = 10;
            Console.WriteLine(value + " , " + other.ToString());
        }
    }
    #endregion
}
