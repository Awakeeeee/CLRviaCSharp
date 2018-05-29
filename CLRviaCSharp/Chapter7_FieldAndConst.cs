using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter7_FieldAndConst
    {
        //volatile和static, readonly都是对field的修饰
        //static -> 字段属于类型
        //readonly -> 该字段仅能由构造器设定一次
        //volatile -> 不会对该字段进行'线程不安全的'操作 -> 可变的,易变的
        private volatile Example e = new Example();
        private readonly Example ea;

        public Chapter7_FieldAndConst()
        {
            ea = e;
        }

        void SomeMethod()
        {
            Example ep = new Example();
            //此时的readonly ea是引用不能改变
            //ea = ep;

            //但是eadonly ea指向的对象本身是可以变的
            e.access = 2;
        }

        static void Main7(string[] args)
        {
            Console.WriteLine("Const is : " + MyTestLibrary.SIZE);
            Console.WriteLine("Static num is : " + MyTestLibrary.num);
        }
    }
}
