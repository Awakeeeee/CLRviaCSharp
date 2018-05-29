using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; //Console app默认不引用这个,用的时候要去referece的assembly里找到并勾选

namespace CLRviaCSharp
{
    class Chapter14_String
    {
        static void Main14(string[] args)
        {
            //CharNumberCast();
            //StringBasic();
            //UseStringInfo();
            //UseIFormattableFCL();
            //UseCustomIFormattable();
            //UseStringFormat();
            //UseFormatCustomProvider();
            //UseParseMethod();
            //DoEncoding();
            //DoStreamEncoding();
            UseSecureString();
        }

        #region 几个Char结构中的静态方法
        static void CharStaticMethodIllustration()
        {
            //输入字符, 给出该字符的类型Enum UnicodeCategory
            Console.WriteLine(Char.GetUnicodeCategory('&'));
            //输入码位, 如果是个数字就返回数字, 不是就返回-1
            Console.WriteLine(Char.GetNumericValue('\u0039'));
        }
        #endregion

        #region Char和数值的转换
        static void CharNumberCast()
        {
            //1.转型
            char c;
            int i;

            c = (Char)100; //int表示的一律是十进制数字 100 -> 0110 0100 -> U+0064 -> 'd'
            c = unchecked((Char)(65536 + 100)); //same
            i = (Int32)c;
            Console.WriteLine(c);

            //2.用System.Convert类
            try
            {
                c = System.Convert.ToChar(1000000);
            }
            catch (Exception e)
            {
                Console.WriteLine("Convert int to char overflow");
            }

            i = System.Convert.ToInt32('光');
            Console.WriteLine(i);

            //3.用基元类型显式实现的IConvertable方法
            //这些接口虽然实现了, 但是显式实现的, 要用需要接口类型
            c = ((IConvertible)20809).ToChar(null);
            Console.WriteLine(c);
        }
        #endregion

        #region String Basic
        static void StringBasic()
        {
            string s = new String(new char[] { 'h', 'e' });
            Console.WriteLine(s);
            Console.WriteLine(System.Environment.NewLine);
            Console.WriteLine(@"\r\n");

            //CultureInfo影响比较结果
            //不考虑文化因素, 就好像字符串是逐字字符串一样, 写的什么字符就是什么字符
            //考虑文化因素, 像ß这种东西就会被转化为另外的字符, 再去比较
            string s1 = "Strasse";
            string s2 = "Straße";
            bool result1 = String.Equals(s1, s2, StringComparison.Ordinal);
            //System.Globalization.CultureInfo info = new System.Globalization.CultureInfo("en-AU");
            bool result2 = String.Equals(s1, s2, StringComparison.InvariantCulture);
            Console.WriteLine("Ignore culture: " + result1);
            Console.WriteLine("Consider culture: " + result2);

            //字符串留用
            string sa = "wocao";
            string sb = "wocao";
            Console.WriteLine(Object.ReferenceEquals(sa, sb)); //如果当前CLR版本自动进行interning, 就会相同, 否则不同, 问题是你不知道有没有自动进行

            sa = String.Intern(sa); //假如没有自动interning, sa是表中新对象, 复制他到sk, 另sa* = sk*, sa对象现在就是垃圾回收对象, sk存在表中
            sb = String.Intern(sb); //已有"wocao"这个key, 另sb* = sk*, sb现在是垃圾回收对象
            Console.WriteLine(Object.ReferenceEquals(sa, sb)); //现在一定返回True

            char c1 = '\u0041';
            char c2 = '\u030A';
            Console.Write(c1);
            Console.Write(c2);

            string ss = new string(new char[] { c1,c2 });
            MessageBox.Show(ss);
        }
        #endregion

        #region StringInfo和TextElement
        static void UseStringInfo()
        {
            string s = new string(new char[] { '\u0041', '\u030A', '\u0625', '\u0650' }); //a combined string
            s = s.Insert(2, "+");
            System.Globalization.StringInfo info = new System.Globalization.StringInfo(s);
            Console.WriteLine(s.Length + " | " + info.LengthInTextElements);

            System.Globalization.TextElementEnumerator enumerator = System.Globalization.StringInfo.GetTextElementEnumerator(s);
            //enumerator开始是在null位置, 需要先movenext
            string output = "Traverse string by Text Element: " + System.Environment.NewLine;
            while (enumerator.MoveNext() == true)
            {
                output += enumerator.GetTextElement() + System.Environment.NewLine;
            }
            output += "Traverse string by Character" + System.Environment.NewLine;
            List<char> cl = new List<char>();
            foreach (char c in s)
            {
                output += c + System.Environment.NewLine;
            }
            output += "Whole string is represented as" + System.Environment.NewLine;
            output += s;
            MessageBox.Show(output);
        }
        #endregion

        #region 理解实现IFormattable的类型
        //大多数需要有给定格式和文化ToString方法的类型都跟数值有关, 用处比如给不同国家的用户显示钱数(钱数的显示方法, 这个国家的货币符号等问题)

        //1. 使用FCL中的IFormattable
        static void UseIFormattableFCL()
        {
            double price = 2350.58;
            //Object.Tostring, double重写了这个方法, 现在单纯显示这个数字
            string defaultDisplay = price.ToString();
            Console.WriteLine(defaultDisplay);
            System.Globalization.CultureInfo exampleCulture = new System.Globalization.CultureInfo("zh-CN");
            //IFormattabl要求的ToString
            //带来的变化有:
            //- 数字显示为2,350.58 这是因为指定了"C", 这个数字的含义是一个货币
            //- 前面加了RMB符号, 因为知道了是货币的情况下, 还知道是中国文化
            string customDisplay = price.ToString("C", exampleCulture);
            Console.WriteLine(customDisplay);

            //*理解第二个参数IFormatProvider
            //要求一个类提供一些内容, 在知道格式的基础上, 用于选择性应用其中某些内容来构建最终的字符串显示
            //如知道是货币格式的基础上, 去IFormatProvider实例中找货币符号
            System.Globalization.NumberFormatInfo exampleNumRelated = new System.Globalization.NumberFormatInfo();
            //NumberFormatInfo一般以CultureInfo成员的方式来用, 由当前Culture来构造完整的信息
            //为了理解, 这里手动新建一个对象并手动设置会用到的内容
            //虽然不能看到源代码, 但可以确定double的IFormattable.ToString的内部实现, 在"C"的格式下, 只需要NumberFormatInfo.CurrecySign
            exampleNumRelated.CurrencySymbol = "%";
            customDisplay = price.ToString("C", exampleNumRelated);
            Console.WriteLine(customDisplay);
        }

        //2. 自定义一个IFormattable类型
        internal sealed class MyFormattable : IFormattable
        {
            public string ToString(string format, IFormatProvider formatProvider)
            {
                string raw = "the string description of MyFormattable Class";
                System.Globalization.NumberFormatInfo needs = formatProvider.GetFormat(typeof(System.Globalization.NumberFormatInfo)) as System.Globalization.NumberFormatInfo;
                if (needs == null)
                    throw new FormatException();

                switch (format)
                {
                    case "QI":
                        string symbolNeeded = needs.CurrencySymbol;
                        if (symbolNeeded == null)
                            throw new FormatException();
                        raw += System.Environment.NewLine + symbolNeeded + "in QI MODE" + symbolNeeded;
                        break;
                    default:
                        throw new FormatException();
                }

                return raw;
            }
        }
        static void UseCustomIFormattable()
        {
            MyFormattable mf = new MyFormattable();
            Console.WriteLine(mf.ToString("QI", System.Threading.Thread.CurrentThread.CurrentCulture));
        }
        #endregion

        #region 格式化多个对象 - String.Format
        static void UseStringFormat()
        {
            //用{0:D}的语法指定格式
            string s = string.Format("Time {0:D}, data {1:X}", DateTime.Now, 230);
            Console.WriteLine(s);
            //用Format的传参指定其中所有对象的文化信息
            string sc = string.Format(new System.Globalization.CultureInfo("en-US"), "Time {0:D}, data {1:C}", DateTime.Now, 1200000);
            Console.WriteLine(sc);

            Console.WriteLine("{0:C}", 100000); //Writeline没有提供有IFormatProvider的重载
        }
        #endregion

        #region 利用Format和AppendFormat方法自定义格式化器
        //我的理解是, 这两个方法内部, 会先尝试查找IFormatProvider(CustomInfo <-> CultureInfo)参数是否能提供一个ICustomFormatter(CustomFormatInfo <-> NumberFormatInfo)类型
        //因为要自定义格式化器
        //1.就自定义一个IFormatProvide
        //2.使他的GetFormat(Type)能返回一个ICustomFormatter
        //3.返回的ICustomFormatter实现了接口Format方法, 而其中就是自定义的格式化方式
        
        //IFormattable要求类型有一个ToString(...)
        //ICustomFormatter要求类型有一个Format(...), 其实也是一个ToString方法, 且内置使用来看ICustomFormtter只被Format,AppendFormat使用
        //以上两个接口方法都要接受一个IFormatProvider参数
        internal sealed class CustomInfoProvider : IFormatProvider, ICustomFormatter
        {
            //一个作用类似于arg.ToString()的方法
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                if (arg is Int32)
                {
                    return "<B>" + arg.ToString() + "</B>";
                }
                else {
                    return arg.ToString();
                }
            }

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                    return this;
                else
                    return null;
            }
        }

        static void UseFormatCustomProvider()
        {
            //Format方法会创建一个ICustomFormmater并尝试通过CustomInfoProvider获取他
            string s = string.Format(new CustomInfoProvider(), "data is {0}, result is {1}", 231, 55);
            Console.WriteLine(s);
        }
        #endregion

        #region Parse - 将string解析为object
        static void UseParseMethod()
        {
            //将一个写成十六进制的字符串转化为Int32类型
            string hex = "C9";
            Int32 deci = Int32.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier);

            hex = "  -3 ";
            deci = Int32.Parse(hex); //-3 可见默认NumberStyles.Integer = AllowLeadingWhite | AllowTrailingWhite | AllowLeadingSign

            //能用上Parse完全体的场合
            hex = "$499";
            //deci = Int32.Parse(hex, System.Globalization.NumberStyles.Currency, new System.Globalization.CultureInfo("zh-CN")); //Wrong, 该文化无法识别$
            deci = Int32.Parse(hex, System.Globalization.NumberStyles.Currency, new System.Globalization.CultureInfo("en-US"));

            Console.WriteLine(deci.ToString());

            ParseClass pc;
            ParseClass.TryParse("  ParseClass_qiqiqi", ParseClassStyles.IgnoreSpace, null, out pc);
            Console.WriteLine(pc.Content);
        }

        //Parse只是一个类型的方法, 提供由String向自己转化的途径
        internal sealed class ParseClass
        {
            public string Content { get; set; }
            public static bool TryParse(string input, ParseClassStyles style, IFormatProvider provider, out ParseClass result)
            {
                switch (style)
                {
                    case ParseClassStyles.None:
                        break;
                    case ParseClassStyles.IgnoreSpace:
                        input = input.Replace(" ", "");
                        break;
                    default:
                        throw new FormatException();
                }

                if (!input.StartsWith("ParseClass_"))
                {
                    result = null;
                    return false;
                }
                else
                {
                    result = new ParseClass();
                    result.Content = input;
                    return true;
                }
           }
        }
        public enum ParseClassStyles
        {
            None = 1,
            IgnoreSpace = 2
        }
        #endregion

        #region Encoding(char -> bits)
        static void DoEncoding()
        {
            //通过基类Encoding的单例属性获得具体encoding
            System.Text.Encoding utf8encoding = System.Text.Encoding.UTF8;
            //也可以直接创建该具体encoding, 但每次创建都要new
            System.Text.UnicodeEncoding alternative = new UnicodeEncoding(false, false);//这时可以指定特定encoding的参数, 如utf16的高低序

            string s = "Aureliano Jose";
            byte[] encode = utf8encoding.GetBytes(s);
            //这个方法把byte array中每个0000 0000以十六进制的数字显示出来
            Console.WriteLine(BitConverter.ToString(encode) + " Count:" + utf8encoding.GetByteCount(s));
            string decode = utf8encoding.GetString(encode);
            Console.WriteLine(decode);

            //用正确的code page指定一个不常用的编码格式
            Encoding e = System.Text.Encoding.GetEncoding("Shift-JIS");
            decode = e.GetString(encode);
            Console.WriteLine(decode);

            //如果用了错误的编码方式就得不到原结果, 这就是文本乱码的来历
            System.Text.Encoding utf16encoding = System.Text.Encoding.Unicode;
            string decode16 = utf16encoding.GetString(encode);
            Console.WriteLine(decode16);
        }
        #endregion

        #region Stream Encoding
        static void DoStreamEncoding()
        {
            //Stream的问题是可能会一股一股的传递数据, 如一波5bytes, 下一波7bytes
            //对与5字节的输入, Encoding的utf16的GetChar会丢掉最后一个字节, 从此往后stream来的信息就都错了
            //因此需要保持状态, 即使最后一字节无法翻译, 也要把它暂存起来
            //Encoding.GetDecoder返回的Decoder的GetChar实现了这一点
            UnicodeEncoding utf16 = new UnicodeEncoding();
            //简单来说对于Stream的编解码, 要用Encoding.GetDe/Encoder
            Decoder decode16 = utf16.GetDecoder();

            //simulate a stream
            byte[] block1 = { 72, 0, 69, 0, 76}; //big endian
            byte[] block2 = { 0, 76, 0 };
            char[] result = new char[128];
            decode16.GetChars(block1, 0, 5, result, 0);
            decode16.GetChars(block2, 0, 3, result, 2);
            Console.WriteLine(new string(result));
        }
        #endregion

        #region SecureString
        //User -> SecureString(instance in Managed Heap) -> raw string(string in Unmanaged Heap)
        static void UseSecureString()
        {
            //using代表强制清理资源
            //()里的类型必须实现IDisposable, 因为using结束后立刻隐式调用dispose方法
            using (System.Security.SecureString ss = new System.Security.SecureString())
            {
                Console.WriteLine("Enter your very secure password:");
                while (true)
                {
                    ConsoleKeyInfo ck = Console.ReadKey(true); //参数 intercept(拦截), true表示读到的key被拦截而不显示在console上
                    if (ck.Key == ConsoleKey.Enter)
                        break;
                    ss.AppendChar(ck.KeyChar);  //SecureString不能显示秘密string, 但提供方法操作秘密string
                    Console.Write("*");
                }
                Console.WriteLine();
                Console.WriteLine(ss.ToString()); //这只是object tostring

                unsafe //unsafe表示下面要手动操作非托管资源了
                {
                    char* raw; //char array
                    //在Security之外另一个命名空间InteropServices, 提供获得秘密string的方法
                    //该方法解密秘密string, 把解密内容储存在非托管内存, 返回指向那里的一个指针(IntPtr)
                    raw = (char*)System.Runtime.InteropServices.Marshal.SecureStringToCoTaskMemUnicode(ss);

                    for (int i = 0; i < ss.Length; i++)
                    {
                        Console.Write(raw[i]);
                    }
                    System.Runtime.InteropServices.Marshal.ZeroFreeCoTaskMemUnicode((IntPtr)raw); //like C++ delete
                }
            }
            Console.WriteLine();
        }
        #endregion
    }
}
