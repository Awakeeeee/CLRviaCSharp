using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter20_Exception
    {
        static void Main(string[] args)
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
            OuterCall();

            CheckException();
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
            catch (ArgumentException e){} //2. not matching
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
    }
}
