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
        }

        #region 内层finally
        //- try抛出的exception如果没有在本层找到对应的catch,就回溯callstack
        //- 回溯匹配到catch后,从抛出异常的try到捕捉异常的catch之间所有的finally为内层finally,他们依次全部执行
        //- 最终到达catch的method, 执行他的catch和finally
        static void OuterCall()
        {
            try { MidleCall(); }
            catch (InvalidCastException e) { Console.WriteLine("catch block in catch method"); }  //8. match and come in
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
            try { throw new InvalidCastException(); } //1. throw exception
            catch (ArgumentException e) { } //2. not matching
            finally { Console.WriteLine("the finally block in exception method(internal finally)"); } //3. so finally is called
            //4. (实际是方法带着未处理的问题返回了)check callstack, go to MidleCall()
        }
        #endregion
    }
}
