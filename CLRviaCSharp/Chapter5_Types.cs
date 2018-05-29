using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLRviaCSharp
{
    class Chapter5_Types
    {
        static void Main_5(string[] args)
        {
            Point p1 = new Point(1, 2);
            Point p2 = new Point(2, 3);

            Console.WriteLine(p1 == p2);
            Console.WriteLine(3 ^ 3);

            //Out of Type Safe: dynamic
            dynamic s = "a string";
            dynamic i = 4;
            RequireArg(s);
            RequireArg(i);

            Console.WriteLine(s.GetType()); //像python一样, GetType是硬写的, 而没有智能感知

            List<Point> plist = new List<Point> { p1, p2 };
            foreach (dynamic item in plist)
            {
                item.x = 1; //like Python

                try
                {
                    item.z = 2; //runtime error
                }
                catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            
        }

        static void RequireArg(String s)
        {
            Console.WriteLine("String: " + s);
        }
        static void RequireArg(Int32 i)
        {
            Console.WriteLine("Int32: " + i.ToString());
        }
    }

    //custom value type
    [StructLayout(LayoutKind.Explicit)]
    struct Point
    {
        [FieldOffset(0)]
        public Int32 x;
        [FieldOffset(1)] //如果这里也是0, 那么xy相当于同占一个位置的内存, 从而只能有共同的值
        public Int32 y;

        public Point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }

        public override string ToString()
        {
            return x + "," + y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)    //1. check null
                return false;

            if (ReferenceEquals(this, obj)) //2. identity check(ref equals)
                return true;

            if (this.GetType() != obj.GetType()) //3. type check
                return false;

            Point other = (Point)obj;   //4. compare contents
            if (other.x == this.x && other.y == this.y)
                return true;
            else
                return false;
        }
        //一些底层方法需求如果对象相等, 那么他们有同样的HashCode, 现在Equals方法被重写了, GetHashCode也希望被正确定义
        public override int GetHashCode()
        {
            return x ^ y;
        }

        //内部类型安全的Equals, 而不是Object的Equals
        public bool Equals(Point other)
        {
            if (other.x == this.x && other.y == this.y)
                return true;
            else
                return false;
        }
        //用安全的Equals方法实现==和!=
        public static bool operator ==(Point self, Point other)
        {
            return self.Equals(other);
        }
        public static bool operator !=(Point self, Point other)
        {
            return !(self == other);
        }

        //值类型不应该定义修改自身字段的成员
        //public void Change(Int32 _x, Int32 _y)
        //{
        //    x = _x;
        //    y = _y;
        //}
    }
}
