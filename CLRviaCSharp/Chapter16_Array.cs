using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter16_Array
    {
        static void Main16(string[] args)
        {
            #region Define array:多维数组和交错数组
            //使用数组初始化器{}的话, 可以隐式定义
            var array = new[] { 1, 2 };
            
            //Multi-dimentional array
            //相当于int[,] marray = new int[3, 3] -> [0,0]=1,[0,1]=2,[0,3]=3,[1,0]=1...
            int[,] marray = new int[,] { { 1, 2, 3 }, { 1, 2, 3 }, { 1, 2, 3 } };

            //Jagged-array
            int[][] jarray = new int[3][];
            jarray[0] = null;
            jarray[1] = new int[2];
            jarray[2] = new int[3];
            #endregion

            #region Define array: Anonymous type
            var tuple = new { First = "QI", Last = "ZHOU" };
            Console.WriteLine(string.Format("type:{0}, ele1:{1}, ele2:{2}", tuple.GetType(), tuple.First, tuple.Last));

            //匿名类型数组
            //无法显示显出Anonymouse Type这种类型, 因此声明这个数组需要隐士局部变量和隐士数组声明
            var anonyArray = new[] { tuple, new { First = "Jason", Last = "Brown" } };
            Console.WriteLine("type:" + anonyArray.GetType() + ", " + anonyArray[0].Last);
            #endregion
            
            int[] arr = { 1, 2, 3 };
            int[] arr2 = new int[3];
            System.Buffer.BlockCopy(arr, 0, arr2, 0, arr.Length * 4);
            Console.WriteLine(arr[1]);

            #region IEnumerable, ICollection, IList
            MyRefType[] mr = new MyRefType[2];
            MyRefType[,] mr2D = new MyRefType[1, 2];
            //没有报错, Array没有实现泛型接口, 但[引用类型]MyRefType自动实现了所有自己类型和基类类型的泛型接口
            ExampleMethod(mr);
            //报错: 只有1D, 0基的数组才会自动去实现泛型接口
            //ExampleMethod(mr2D);
            #endregion

            #region 创建非0基数组
            //从lowerBound和length的元素个数看出, 想要创建的是1维数组
            //lowerBound每个元素代表这一维起始的index值, 可设计为非0
            //length每个元素代表这一维的长度
            Int32[] lowerBound = { -5 };
            Int32[] length = { 3 };
            Array a = Array.CreateInstance(typeof(Int32), length, lowerBound);
            a.SetValue(100, -5);
            a.SetValue(200, -4);
            Console.WriteLine("index " + a.GetLowerBound(0) + " : " + a.GetValue(-5));
            #endregion

            //AccessTest();

            UseInlineArray();
            UseStackalloc();
        }

        static void ExampleMethod(ICollection<Object> list)
        {
            return;
        }

        #region 对比多维数组,交错数组,Unsafe数组的访问速度
        static void AccessTest()
        {
            int count = 100;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
                SafeAcessMD();
            sw.Stop();
            Console.WriteLine("safe acess multi dimention array: " + sw.ElapsedMilliseconds);

            sw.Restart();
            for (int i = 0; i < count; i++)
                SafeAcessJA();
            sw.Stop();
            Console.WriteLine("safe acess jagged array: " + sw.ElapsedMilliseconds);

            sw.Restart();
            for (int i = 0; i < count; i++)
                UnsafeAcessMD();
            sw.Stop();
            Console.WriteLine("unsafe acess multi dimentian array: " + sw.ElapsedMilliseconds);
        }

        static void SafeAcessMD() //访问慢, 无优化
        {
            int[,] md = new int[1000, 1000];
            for (int i = 0; i < 1000; i++)
                for (int j = 0; j < 1000; j++)
                    md[i, j] = 1;
        }
        static void SafeAcessJA()
        {
            int[][] ja = new int[1000][];
            for (int i = 0; i < 1000; i++)
                ja[i] = new int[1000];  //创建慢, 需要创建很多string[]对象

            for (int i = 0; i < 1000; i++)
                for (int j = 0; j < 1000; j++)
                    ja[i][j] = 1;
        }
        unsafe static void UnsafeAcessMD() //两方面都很快, 但是危险
        {
            int[,] umd = new int[1000, 1000];

            fixed (int* p = umd)
            {
                for (int i = 0; i < 1000; i++)
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        int pos = 1000 * i + j; //数组内存串的一个位置, 这里没有范围检查, 超出范围将导致未知错误
                        p[pos] = 1;
                    }
                }
            }
        }
        #endregion

        #region 在stack上分配数组内存的方法(都得是unsafe)
        //1. struct inline array
        static void UseInlineArray()
        {
            unsafe
            {
                InlineArray iarr = new InlineArray();
                int size = sizeof(InlineArray); //unsafe中, 可以用sizeof查看一个类型的bytes大小
                Console.WriteLine(size); //这个struct似乎没有别的隐藏的东西,char[20]就得到40bytes
                int length = size / 2; //一个char 16bits占2个bytes

                string raw = "GIMME LUCK";
                for (int i = 0; i < length; i++)
                {
                    int pos = length - i - 1;
                    iarr.inlineArr[i] = pos > (raw.Length - 1) ? '.' : raw[pos];
                }
                string result = new string(iarr.inlineArr, 0, length); //char array, start index, total char number
                Console.WriteLine(result);
            }
        }
        //2. stackalloc
        static void UseStackalloc()
        {
            unsafe
            {
                char* sarr = stackalloc char[20]; //用stackallock代替new, stackalloc不会自动补全
                string raw = "GIMME LUCK";
                for (int i = 0; i < 20; i++)
                {
                    int pos = 20 - i - 1;
                    sarr[i] = pos > raw.Length - 1 ? '.' : raw[pos];
                }
                Console.WriteLine(new string(sarr, 0, 20));
            }
        }
        #endregion
    }

    internal unsafe struct InlineArray
    {
        public fixed char inlineArr[20]; //struct中内嵌数组,像C++的语法,这样写就会把数组分配到stack上
    }

    internal sealed class TestClass : ICollection
    {
        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        //ICollection : IEnumerable
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
