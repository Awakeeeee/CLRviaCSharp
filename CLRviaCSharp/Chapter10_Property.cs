using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter10_Property
    {
        static void Main10(string[] args)
        {
            IPropertyAble pa = new PropertyClassAdvanced();
            PropertyClass pc = new PropertyClassAdvanced();
            PropertyClassAdvanced pca = new PropertyClassAdvanced();

            //对象初始化器: 写在一行里的特殊对象初始化方法, 这不是构造器里的代码, inline初始化Dictionary就是这个原理
            PropertyClassAdvanced ex1 = new PropertyClassAdvanced() { PropA = "bla", PropB = "blabla" };
            //这种写法下甚至可以省略(), 像C++似的
            PropertyClassAdvanced ex2 = new PropertyClassAdvanced { PropA = "fsdf", PropB = "fdf" };

            Console.WriteLine(pca.PropA);

            #region Tuple in AnonymousType
            //用匿名类型AnonymousType(这确实是一种类型)来创建一个 C# Tuple
            float Height = 177.5f;
            //因为名字相同, Height会对应上面的成员变量, 而不是创建新的prop
            var tuple = new { Name = "Qi", Age = 27, Height };
            //...f_AnonymousType[...]
            //某种特殊类型, 查看IL它的定义在这个project之外?TODO
            Console.WriteLine(tuple.GetType());
            #endregion

            #region Tuple of SystemType
            Tuple<string, int, float> stuple = new Tuple<string, int, float>("BLABLA", 32, 16.5f);
            //system tuple的元素规定为Item+数字, 不能改, 阅读上并不方便
            //但这是实名类型, 不限制于方法局部
            int ii = stuple.Item2;
            #endregion

            Tuple<int, int> minmaxlist = FormatMinMax(99, 23);
            ShowLastSevenDaysDocs();

            #region Tuple in ExpandoObject
            dynamic ex = new System.Dynamic.ExpandoObject();
            //ExpandoObject类通过某种手段, 使得可以像这样向ex中添加成员(添加tuple元素)
            //所添加的成员类型是System.Collections.Generic.KeyValuePair`2[System.String,System.Object]
            //NAME是KeyValuePair.Key -> string, "Petter"是KeyValuePair.Value -> object(string in this case)
            ex.NAME = "Petter";
            ex.AGE = 20;
            ex.SKILL = "Sword";
            //查看expando tuple
            foreach (var item in ex)
            {
                Console.WriteLine(item.Key + " : " + item.Value);
            }
            #endregion

            BitArray bitarray = new BitArray(15);
            bitarray[11] = true;
            Console.WriteLine("10 is " + bitarray[10] + " 11 is " + bitarray[11]);
        }

        #region System.Tuple应用
        static Tuple<int, int> FormatMinMax(int a, int b)
        {
            return new Tuple<int, int>(Math.Min(a, b), Math.Max(a, b));
        }
        #endregion

        #region 匿名类型tuple应用
        static void ShowLastSevenDaysDocs()
        {
            string myDoc = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //我的文档位置
            //what is this SQL like stuff in C#???
            var query =
                from pathname in System.IO.Directory.GetFiles(myDoc) //对每一个文件和其路径
                let lastReadTime = System.IO.File.GetLastWriteTime(pathname)   //访问时间赋值给lastReadTime
                where lastReadTime > (DateTime.Now - TimeSpan.FromDays(90)) //如果当前文件访问时间符合在距今x天内
                orderby lastReadTime
                select new { PATH = pathname, TIME = lastReadTime };    //那么选出文件, 并将这个文件的信息以这个匿名类型的格式保存起来

            //query是一个IEnumerable类型, 其元素类型是AnonymousType
            //因此元素可以看作是一个匿名类型所构成的tuple, 可读不可写, tuple元素是自定义名称
            foreach (var element in query)
            {
                Console.WriteLine("file access: " + element.PATH + " at " + element.TIME);
            }
        }
        #endregion
    }

    internal interface IPropertyAble
    {
        //interface中可以定义Property
        String PropA { get; set; }
    }

    internal abstract class PropertyClass
    {
        //虚属性
        public virtual String PropA { get; set; }
        //抽象属性
        public abstract String PropB { get; set; }
    }
    internal class PropertyClassAdvanced : PropertyClass, IPropertyAble
    {
        private int val;

        //AIP 自实现属性
        string IPropertyAble.PropA { get; set; }

        public override string PropA
        {
            get { return "propA"; }
            set { }
        }

        //一个有side effect的属性(方法也同理), 指他除了返回一个值以外, 还改变了类/对象的状态(字段)
        public override string PropB
        {
            get { return "propB"; }
            set { val = 100; }
        }

        //尽管property本质是方法, 但不能应用泛型, 因为property的设计本意拿取和设定一个固定的值
        //public T PropC<T> { get; set; }
        public PropertyClassAdvanced()
        {
            base.PropA = "default";
        }
    }

    #region 定义Indexer(有参属性)
    //通过修改Indexer, 实现对Byte array的Bit级访问和操作
    internal sealed class BitArray
    {
        private Byte[] array; //内部维护一个Byte array, 每个元素有8bits: 0000 0000
        private Int32 bits;

        public BitArray(Int32 bit_number)
        {
            bits = bit_number;
            array = new Byte[(bit_number + 7) / 8]; //例如15bits将分配2bytes空间, 保证足够
        }

        //Indexer
        //C#中, Indexer就是overload [] operator, 这个语法背后为类型生成一个默认为Item的属性, 属性指代实例
        //但有参属性的定义语法在各个语言中是不同的
        //对CLR来说, 完全不区分有参/无参属性
        [System.Runtime.CompilerServices.IndexerName("BitElement")] //对于Indexer, C#中得用这个attribute来修改属性名, 否则就叫Item
        public Boolean this[Int32 bit_pos]
        {
            get
            {
                if (bit_pos < 0 || bit_pos > bits)
                {
                    throw new ArgumentException();
                }
                //1 = 0000 0001
                //if bit_pos = 11, bit_pos is looking at the 3 bit in array[1]
                //move 1 to that bit 0000 1000
                return (array[bit_pos / 8] & (1 << (bit_pos % 8))) != 0;
            }

            set
            {
                if (bit_pos < 0 || bit_pos > bits)
                {
                    throw new ArgumentException();
                }
                if (value)
                {
                    array[bit_pos / 8] = (Byte)(array[bit_pos / 8] | (1 << (bit_pos % 8)));
                }
                else
                {
                    array[bit_pos / 8] = (Byte)(array[bit_pos / 8] & (1 << (bit_pos % 8)));
                }
            }
        }
    }
    #endregion
}
