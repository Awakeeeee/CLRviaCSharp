using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Reflection;

namespace CLRviaCSharp
{
    class Chapter24_Serialization
    {
        static void Main_1(string[] args)
        {
            //UseFormatterToSerialize();

            //ControlSerializationByAttribute();

            SerializeCustomType();
        }

        //使用StreamingContext的特性写一个用序列化技术深拷贝对象的方法
        static object SerializationStyleClone(object origin)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            //key
            bf.Context = new StreamingContext(StreamingContextStates.Clone, null);
            bf.Serialize(ms, bf);

            ms.Position = 0;
            object clone = bf.Deserialize(ms);
            return clone;
        }

        //序列化反序列化自己实现接口的类型
        static void SerializeCustomType()
        {
            ISDerive isd = new ISDerive();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            bf.Serialize(ms, isd); //其中会调用自己写的GetObjectData
            isd = null;
            ms.Position = 0;
            isd = (ISDerive)bf.Deserialize(ms); //其中调用自己写的特殊ctor
        }

        static void LookIntoSerialization()
        {
            //1
            MemberInfo[] members = FormatterServices.GetSerializableMembers(typeof(SClass));
            //2
            SClass sc = new SClass(1, 1);
            object[] values = FormatterServices.GetObjectData(sc, members);
            //3 将对象的程序集表示序列化
            //4 将字段值序列化
        }

        static void LookIntoDeserialization()
        {
            //1
            Assembly asm = Assembly.GetEntryAssembly();
            Type t = FormatterServices.GetTypeFromAssembly(asm, "SClass");
            //2 分配内存,拿到一个没有任何初始化的实例
            object o = FormatterServices.GetUninitializedObject(t);
            //3
            MemberInfo[] members = FormatterServices.GetSerializableMembers(t);
            //4
            object[] values = null; //=序列化流中的数据
            //5
            object deserializeResult = FormatterServices.PopulateObjectMembers(o, members, values);
        }

        //使用attribute控制格式化流程
        static void ControlSerializationByAttribute()
        {
            BinaryFormatter bf = new BinaryFormatter();
            SClass sc = new SClass(1, 1);
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, sc);

            sc = null;
            ms.Position = 0;
            sc = (SClass)bf.Deserialize(ms);
        }

        //使用格式化器
        static void UseFormatterToSerialize()
        {
            MyRefType s = new MyRefType(10);
            string ss = "blalala";
            MemoryStream ms = SerializeToMemory(s, ss);
            //我可以随意使用这个序列化信息
            File.WriteAllBytes("D:\\serialize_result_bytes.qiqiqi", ms.GetBuffer());

            s = null;
            ss = string.Empty;
            ms.Position = 0;

            s = DeserializeFromMemory(ms) as MyRefType;
            ss = DeserializeFromMemory(ms) as string;
            Console.WriteLine(s);
            Console.WriteLine(ss);
        }
        static System.IO.MemoryStream SerializeToMemory(params object[] objGraph)
        {
            //byte stream container
            MemoryStream ms = new MemoryStream();
            //formatter, 序列化和反序列化的方法已经由.Net提供在这个类型里了
            BinaryFormatter formatter = new BinaryFormatter();
            foreach (object o in objGraph)
            {
                //现在ms中存放这传入对象图的序列化信息
                formatter.Serialize(ms, o);
            }
            return ms;
        }
        static object DeserializeFromMemory(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

    }

    [Serializable]
    public class SClass
    {
        private int a;
        private int b;

        [NonSerialized] //只能应用于字段
        private int c;

        [System.Runtime.Serialization.OptionalField] //如果序列化和反序列化之间某个时候新添了一个字段
        private int d;

        public SClass(int _a, int _b)
        {
            a = _a;
            b = _b;
            c = a + b;
            d = 100;
        }

        public void Log()
        {
            Console.WriteLine("SClass: a = {0} b = {1} c = {2}", a, b, c);
        }

        [System.Runtime.Serialization.OnSerializing]
        private void OnSerializing(System.Runtime.Serialization.StreamingContext context)
        {
            Console.WriteLine("SClass before serialization");
        }

        [System.Runtime.Serialization.OnSerialized]
        private void OnSerialized(System.Runtime.Serialization.StreamingContext context)
        {
            Console.WriteLine("SClass serialization finishes");
        }

        [System.Runtime.Serialization.OnDeserializing]
        private void OnDeserializing(System.Runtime.Serialization.StreamingContext context)
        {
            Console.WriteLine("SClass before deserialization");
        }

        [System.Runtime.Serialization.OnDeserialized]
        private void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            Console.WriteLine("SClass after normal deserialization, recalculate the non-serialized fields");
            Console.WriteLine("before: c = " + c);
            c = a + b;
            Console.WriteLine("after: c = " + c);
        }
    }

    #region 通过实现接口的方式让自己的类型可序列化
    [Serializable] //实现接口的序列化类型也需要有这个
    class ISBase //一个没实现序列化接口的父类
    {
        private int p;
        public ISBase() { }
    }

    [Serializable]
    sealed class ISDerive : ISBase, ISerializable //父类没实现接口, 子类就要处理父类的字段序列化
    {
        private int c;

        public ISDerive() { c = 10; }

        //控制反序列化
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter = true)]//这个attribute确保即使方法公开都高, 也不会被非formatter的人调用
        private ISDerive(SerializationInfo info, StreamingContext context) //必要的, 用于反序列化的特殊ctor
        {
            Console.WriteLine("Custom serialization special ctor is called!");
            //处理父类, 如果父级有实现, 调用base就可以了
            Type parent = this.GetType().BaseType;
            MemberInfo[] parentFields = FormatterServices.GetSerializableMembers(parent);
            foreach (var m in parentFields)
            {
                FieldInfo i = (FieldInfo)m;
                i.SetValue(this, info.GetValue(i.Name, i.FieldType)); //反射调用字段来设置字段
            }

            //自己的字段
            c = info.GetInt32("c"); //这里的Get和上面的Set完全不是一回事, 这是从serializationInfo拿序列化的值, 上面是反射字段
        }

        //控制序列化
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Console.WriteLine("Custom serialization get field data is called!");
            Type parent = this.GetType().BaseType;
            MemberInfo[] parentFields = FormatterServices.GetSerializableMembers(parent);
            foreach (var m in parentFields)
            {
                FieldInfo i = (FieldInfo)m;
                info.AddValue(i.Name, i.GetValue(this));
            }

            info.AddValue("c", c);
        }
    }
    #endregion

    #region 一个序列化返回单例而不是新对象的类型
    [Serializable]
    class SingletonSerialization : ISerializable
    {
        private static SingletonSerialization instance;
        public static SingletonSerialization Instance { get { return instance; } }

        private SingletonSerialization()
        { }

        //不需要特殊ctor
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //SetType --- AddValue
            //序列化为一个单例中介类型(实现了IObjectReference)
            info.SetType(typeof(SingletonSerializationAgent));
        }
    }
    class SingletonSerializationAgent : IObjectReference
    {
        public object GetRealObject(StreamingContext context)
        {
            return SingletonSerialization.Instance;
        }
    }
    #endregion
}
