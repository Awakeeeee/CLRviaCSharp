using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CLRviaCSharp
{
    class Chapter24_Serialization_Surrogate
    {
        static void Main_End(string[] args)
        {
            BinaryFormatter bf = new BinaryFormatter();

            SurrogateSelector ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(StringPlate), bf.Context, new StringClassSurrogate());

            //通过SurrogateSelector实例告诉序列化器使用我的代理实例去序列化StringPlate类型
            bf.SurrogateSelector = ss;

            StringPlate sp = new StringPlate("ahahahplate");
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, sp);
            ms.Position = 0;

            //注意在这里, 一个不是序列化类型的StringPlate类型, 被序列化成了一个不同的类型StringPlateEndWithQIQIQI
            //这是通过序列化Surrogate实现的
            StringPlateEndWithQIQIQI sq = (StringPlateEndWithQIQIQI)bf.Deserialize(ms);
            sq.Log();
        }
    }

    //要序列化的原始类型, 注意最初设计时这个类型不是可序列化类型
    class StringPlate
    {
        public string content;

        public StringPlate(string setContent)
        {
            content = setContent;
        }

        public void Log()
        {
            Console.WriteLine(content);
        }
    }

    //反序列化得到的不同类型的对象
    [Serializable]
    class StringPlateEndWithQIQIQI
    {
        public string content;

        public StringPlateEndWithQIQIQI(string setContent)
        {
            content = setContent + "QIQIQI";
        }

        public void Log()
        {
            Console.WriteLine(content);
        }
    }

    //序列化代理
    class StringClassSurrogate : System.Runtime.Serialization.ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Content", ((StringPlate)obj).content);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            StringPlateEndWithQIQIQI sq = new StringPlateEndWithQIQIQI(info.GetString("Content"));
            return sq;
        }
    }
}
