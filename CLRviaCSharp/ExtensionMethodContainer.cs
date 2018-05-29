
namespace CLRviaCSharp
{
    public static class ExtensionMethodContainer
    {
        public static void LogMe(this System.Single f)
        {
            System.Console.WriteLine(f.ToString());
        }

        //为父类定义一个扩展方法, 他的子类都能使用
        internal static void ExtentLog(this Parent p)
        {
            System.Console.WriteLine("parent extention method is called");
        }

        //可以为接口添加扩展方法
        //IEnumerable接口代表有list性质的类型, 可以为他定义一个列出list的方法
        //任何实现了这个接口的类型都可以使用这个方法
        public static void PrintList<T>(this System.Collections.Generic.IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                System.Console.Write(item + "|");
            System.Console.Write("\r\n");
        }

        //向Enum添加方法
        internal static MyBitFlag Clear(this MyBitFlag e, MyBitFlag flag)
        {
            return e & (~flag);
        }
        internal static void ForeachDoaction(this MyBitFlag e, System.Action<MyBitFlag> action)
        {
            if (action == null)
            {
                System.Console.WriteLine("Fail. Action is null.");
                return;
            }
            //取决于基础类型, int循环32次, 他共有32bits
            for (uint i = 1; i != 0; i <<= 1)
            {
                uint m = (uint)e & i; //如果这一位为open
                if (m != 0)
                {
                    action((MyBitFlag)m);
                }
            }
        }
    }
}
