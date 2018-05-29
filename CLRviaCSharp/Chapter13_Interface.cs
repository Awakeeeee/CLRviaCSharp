using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter13_Interface
    {
        static void Main13(string[] args)
        {
            Console.Beep(261, 200);
            Console.Beep(293, 200);
            Console.Beep(330, 200);
            Console.Beep(349, 200);
            Console.Beep(392, 200);
            Console.Beep(440, 200);
            Console.Beep(494, 200);
            Console.Beep(523, 200);

            ContractBase cb = new ContractDerive();
            cb.Log(); //base实现的接口, 只属于base
            cb.BLog();  //base中被重写的方法, 现在父子类都一样
            ((ILog)cb).Log();   //derive实现的接口, 只属于derive

            #region EIMI带来的困惑
            //Int32实现了IConvertible接口, 该接口定义方法ToSingle, 看起来Int32也能用这个方法
            //但因为是EIMI实现, 所以没有
            //这就是为什么会看到把一个类型实例转换成一个借口 --> 为了使用EIMI定义的接口方法
            Int32 x = 5;
            //float f = x.ToSingle();
            float f = ((IConvertible)x).ToSingle(null);
            #endregion         
        }
    }

    internal interface IFirstTry
    {
        //void int i; //No fields
        //static void SMethod(); //No static
        int IProperty { get; set; }
        void IMethod();
        event EventHandler IEvent;
    }

    #region 重写和重新继承接口
    internal interface ILog
    {
        void Log();
    }
    internal class ContractBase : ILog
    {
        public void Log()   //这个方法用子类自己继承接口的方法重写
        {
            Console.WriteLine("Base interface log");
        }
        public virtual void BLog()   //这个方法正常重写
        {
            Console.WriteLine("Base self log");
        }
    }
    internal class ContractDerive : ContractBase, ILog
    {
        new public void Log()
        {
            Console.WriteLine("Derive interface Log");
        }
        public override void BLog()
        {
            Console.WriteLine("Derived self log");
        }
    }
    #endregion

    #region IIMI和EIMI
    internal interface IPleaseImplement
    {
        void One();
        void Two();
    }
    internal class GO : IPleaseImplement
    {
        //One方法的隐式实现(IIMI implicit interface method implementation)
        //现在GO的类型对象和IPleaseImplement接口的类型对象中两个entry方法记录都指向这同一个方法
        public void One()
        { }

        //Go class中的一个叫Two的方法, 和接口无关, 对应Go的类型对象中的方法记录
        public void Two()
        {
            Console.WriteLine("Method of GO class");
        }

        //显式实现(EIMI explicit interface method implementation)
        //是private的, 只能通过IPleaseImplement类型调用
        //在Go class中实现了接口的Two方法, 对应接口类型对象中的方法记录
        void IPleaseImplement.Two()
        {
            Console.WriteLine("Method of IPleaseImplement interface");
        }
    }
    #endregion
}
