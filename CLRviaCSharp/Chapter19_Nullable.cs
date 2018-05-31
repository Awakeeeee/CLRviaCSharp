using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter19_Nullable
    {
        static void Main(string[] args)
        {
            #region 赋值和转型
            Nullable<int> a = 5; //implicit int -> int?
            Console.WriteLine("Normal: " + a.HasValue + " , " + a.Value);
            Nullable<int> b = null;
            Console.WriteLine("Null nullable: " + b.HasValue + " , " + b.GetValueOrDefault());
            int c = (int)a; //explicit int? -> int
            float? d = a;   //implicit cast int? -> float?
            #endregion

            #region Boolean操作符&和|在有一个null值时的特殊情况
            bool? t = true;
            bool? f = false;
            bool? n = null;
            Console.WriteLine("& true null : " + (t & n)); //null
            Console.WriteLine("| true null : " + (t | n)); //true
            Console.WriteLine("& false null : " + (f & n)); //false
            Console.WriteLine("| false null : " + (f | n)); //null
            #endregion

            Console.WriteLine(t.GetType()); //Boolean rather than Nullable<Boolean>
        }
    }
}
