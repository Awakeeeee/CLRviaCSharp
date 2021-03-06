﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter20_Exception
    {
        static void Main20(string[] args)
        {
            //CallStack:
            //---start
            //Main
            //Main - OuterCall
            //Main - OuterCall - MidleCall
            //Main - OuterCall - MidleCall - InnerCall  [在这一步innercall抛出的异常没被自己处理,于是上去找OuterCall的catch]
            //Main - OuterCall - MidleCall
            //Main - OuterCall
            //Main
            //---end
            //OuterCall();

            //CheckException();

            //HeWakesUp();

            //Environment.FailFast("i just try this");
            //string s = "fsdf";
            //char[] arr = s.Where(c => c == 'f').ToArray();
            //Console.WriteLine(arr.Length);

            //LogInnerException();
            //AboutDynamic();

            TryCER(); // TODO ????????????????????????????????????????????????????????????????????????
        }

        #region 内层finally
        //- try抛出的exception如果没有在本层找到对应的catch,就回溯callstack
        //- 回溯匹配到catch后,从抛出异常的try到捕捉异常的catch之间所有的finally为内层finally,他们依次全部执行
        //- 最终到达catch的method, 执行他的catch和finally
        static void OuterCall()
        {
            try { MidleCall(); }
            catch (InvalidCastException e)
            {
                Console.WriteLine("catch block in catch method");
                //stack trace property
                Console.WriteLine("StackTrace Property from Exception: " + e.StackTrace);
                //StackTrace class
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(e, 0, false);
                Console.WriteLine("StackTrace Class instance info: " + st.ToString());
            }  //8. match and come in
            finally { Console.WriteLine("the finally block in catch method"); } //9. 'internal finally' ends, this finally calls
        }
        static void MidleCall()
        {
            try { InnerCall(); }
            catch (ArgumentException e) { } //5. not matching
            finally { Console.WriteLine("the finally block in midle method(internal finally)"); } //6. so finally is called
            //7. check callstack, go to OuterCall()
        }
        static void InnerCall()
        {
            try
            {
                //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                //Console.WriteLine("StackTrace Class instance info: " + st.ToString());
                throw new InvalidCastException();
            } //1. throw exception
            catch (ArgumentException e) { } //2. not matching
            finally { Console.WriteLine("the finally block in exception method(internal finally)"); } //3. so finally is called
            //4. (实际是方法带着未处理的问题返回了)check callstack, go to MidleCall()
        }
        #endregion

        #region Check Exception Info
        [System.Runtime.CompilerServices.MethodImpl]
        static void CheckException()
        {
            Console.WriteLine(Environment.NewLine);
            try
            {
                FieldAccessException fe = new FieldAccessException("Reason: TEST EXCEPTION");
                Console.WriteLine("Stack Trace when instantiate: " + fe.StackTrace); //at this time null
                throw fe;
                //throw new ArgumentNullException();
                //throw new Exception(); //!!!永远都不要抛出代表所有错误的Exception本身
            }
            catch (Exception e) //CLR记录的异常
            {
                Console.WriteLine("Source: " + e.Source);
                Console.WriteLine("Message: " + e.Message);
                Console.WriteLine("Data: " + e.Data.Count);
                Console.WriteLine("Strack Trace:\r\n" + e.StackTrace);
                Console.WriteLine("Target Site: " + e.TargetSite);
                Console.WriteLine("Help Link: " + e.HelpLink);
                Console.WriteLine("Inner Exception: " + e.InnerException);

                throw new InvalidCastException(); //CLR记录的异常改为这个, 总是记录最新的异常
                throw;  //如果仅仅写throw关键字, 这叫重新抛出当前异常, CLR不会修改当前记录
            }
        }
        #endregion

        #region Custom Exception class - generic prototype
        //一切的目的是, 方便的打印出有用的错误信息
        //Exception特殊点是exception类必须序列化(使用attribute, 写序列化方法, 反序列化构造器)
        //因此, 写好一个原型, 所谓自定义类只是自定义错误信息, 以ExceptionArgs的方式包裹在泛型原型里, 打印错误信息时, 带上自定义信息
        [Serializable]
        public sealed class Exception<T> : Exception, System.Runtime.Serialization.ISerializable
            where T : ExceptionArgs
        {
            private const string c_args = "Args"; //const, 统一叫Args, 用来在序列化信息中对应名称
            private readonly T m_args;  //readonly, 只用来读取构造时提供的TArgs信息
            public T Args { get { return m_args; } }

            //构造custom基类多传一个TArgs
            public Exception(T tArgs, string msg, Exception inner) : base(msg, inner) { m_args = tArgs; }
            //也兼容原构造
            public Exception(string msg, Exception inner) : this(null, msg, inner) { }

            //deserialization
            //[TODO 这里有一些不明白的安全性attribute]
            private Exception(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
                //通过string名和类型, 在序列化信息中找到对应obj(???)
                m_args = (T)info.GetValue(c_args, typeof(T));
            }
            //serialization
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                //这里基本上是调用原来的序列化方法, 只是把额外的自定义args添加进去
                info.AddValue(c_args, m_args);
                base.GetObjectData(info, context);
            }

            //重载了Exception类获得原因的属性
            public override string Message
            {
                get
                {
                    string bm = base.Message;
                    return m_args == null ? bm : bm + "(" + m_args.Msg + ")";
                }
            }

            //重载Equals方法
        }

        //本质就是包含一条信息的封装类
        [Serializable]
        public abstract class ExceptionArgs
        {
            public virtual string Msg { get { return string.Empty; } }
        }

        //实际使用时继承ExceptionArgs, 并用泛型基类给包裹起来, 重要的是除了这个异常能传递出有用的错误信息
        [Serializable]
        public class LaptopNotHappyException : ExceptionArgs
        {
            private readonly string notHappyReason;
            public string GetConfession { get { return notHappyReason; } }

            public LaptopNotHappyException(string extraInfo)
            {
                notHappyReason = extraInfo;
            }

            public override string Msg
            {
                get
                {
                    return notHappyReason;
                }
            }
        }

        //使用
        static void HeWakesUp()
        {
            int time = 13;
            try
            {
                if (time > 8)
                {
                    LaptopNotHappyException args = new LaptopNotHappyException("My dad is so weak and LAZY.");
                    //第一个参数额外ExceptionArgs信息, 第二个是原本的Exception信息
                    throw new Exception<LaptopNotHappyException>(args, "异常:本电脑现在不高兴", null);
                }
            }
            catch (Exception e) //我catch所有类型异常, 但我只是测试, 并不是在说我能恢复所有类型的异常
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        #region 反射和dynamic调用时异常重抛的区别
        internal sealed class ExClass
        {
            public void ErrorMethod()
            {
                throw new InvalidCastException();
            }
        }
        static void LogInnerException()
        {
            object o = new ExClass();
            try
            {
                System.Reflection.MethodInfo m = o.GetType().GetMethod("ErrorMethod");
                //CLR在内部捕捉这个调用的所有错误,并转为TargetInvocation重抛
                //相当于真正的原错误没能顺利往上层传递
                m.Invoke(o, null);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Console.WriteLine("CLR reflection:\r\n\r\ncatch: " + e.Message + "\r\n\r\nactual: " + e.InnerException.Message);
                //throw e.InnerException;
            }
        }

        static void AboutDynamic()
        {
            dynamic d = new ExClass();
            try
            {
                //CLR在这里就不会去捕捉错误并重新抛出, 于是原错误正常向上传递
                d.ErrorMethod();
            }
            catch (Exception e)
            {
                Console.WriteLine("\r\nDynamic call: catch: " + e.Message);
                //throw;
            }
        }
        #endregion

        #region 限制执行区域 Constained Exception Region CER
        //catch和finally中处理异常时又抛出异常是不理想的情况, 说明程序'对错误没有好的适应力'
        //CER让catch和finally先于try执行, 如果有异常, 则try块不会执行
        static void TryCER()
        {
            //PrepareConstrainedRegions必须在try上面调用, 让CER机制生效 (???)
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Console.WriteLine("try block is called");
            }
            finally
            {
                ClassCER.MethodOne();
                ClassCER.MethodTwo();
                ClassCER.MethodThree();
            }
        }

        internal sealed class ClassCER
        {
            static ClassCER()
            {
                Console.WriteLine("Type static constructor is called");
            }

            //标记了ReliabilityContractAttribute, 并且Consistency参数是WillNot或者May的方法，JIT才会提前编译 (???)
            //Cer这个参数只是给用户提供信息的
            [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState, System.Runtime.ConstrainedExecution.Cer.Success)]
            public static void MethodOne()
            {
                Console.WriteLine("method that guarantees will not correupt status, and will succeed");
            }

            [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptAppDomain, System.Runtime.ConstrainedExecution.Cer.None)]
            public static void MethodTwo()
            {
                Console.WriteLine("method that may corrupt appdomain status, and no cer info");
            }

            public static void MethodThree()
            {
                Console.WriteLine("method has no ReliabilityContract attribute");
            }
        }
        #endregion
    }
}






















