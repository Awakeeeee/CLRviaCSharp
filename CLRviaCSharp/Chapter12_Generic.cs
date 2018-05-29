using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CLRviaCSharp
{
    class Chapter12_Generic
    {
        static void Main12(string[] args)
        {
            //int iteration = 1000000;
            //PerformanceBtwGenericAndInheritance(iteration);

            //int[]是System.Array类的子类
            int[] iarray = { 1, 2, 10, 4, 5 };
            //System.Array基类中的静态泛型方法
            Array.Sort<Int32>(iarray);
            int idx = Array.BinarySearch<Int32>(iarray, 1);
            Console.WriteLine(idx);

            OpenCloseTypes();

            MyGenericType<MyRefType> mi = new MyGenericType<MyRefType>();
            MyGenericType<String> ms = new MyGenericType<string>();
            MyGenericType<MyRefType>.count += 100;
            //他们都有定义的static成员, 但像不同类型一样, 不共用, 因为static成员的内存是在类型对象中的
            Console.WriteLine(MyGenericType<MyRefType>.count);
            Console.WriteLine(MyGenericType<String>.count);

            //每个节点都是一个Node, 其数据的类型各不相同
            Node tail = new NodeWithData<int>(50, null);
            tail = new NodeWithData<string>("blaaa", tail);
            tail = new NodeWithData<MyValue>(new MyValue(), tail);
        }

        #region Generic相比Inheritance节省了对值类型boxing的消耗
        static void PerformanceBtwGenericAndInheritance(Int32 iteration)
        {
            List<Int32> genericList = new List<int>();
            ArrayList objList = new ArrayList();

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < iteration; i++)
            {
                genericList.Add(1);
                Int32 x = genericList[i];
            }
            Console.WriteLine("Generic time cost: " + sw.ElapsedMilliseconds);
            sw.Stop();

            sw = Stopwatch.StartNew();
            for (int i = 0; i < iteration; i++)
            {
                objList.Add(1);
                Int32 x = (Int32)objList[i];
            }
            Console.WriteLine("Inheritance time cost: " + sw.ElapsedMilliseconds);
            sw.Stop();
        }
        #endregion

        #region 开放类型/封闭类型

        static void OpenCloseTypes()
        {
            //List<>指的是一个未指定类型参数T的List类型, 叫做Open Type
            //ot为该类型的类型对象, 由于CLR不允许open type有对象, 不能真正为List<>生成一个对象
            Type ot = typeof(List<>);
            Console.WriteLine(ot.GetType());
            TryCreateInstanceFromType(ot);

            //一旦全部T都给定, 类型变为Closed Type, 为Closed Type创建对象就像平常的new Class()
            Type ct = typeof(List<Int32>);
            Console.WriteLine(ct.GetType());
            TryCreateInstanceFromType(ct);
        }

        static void TryCreateInstanceFromType(Type t)
        {
            Object o;
            try
            {
                o = Activator.CreateInstance(t); //用这个方法, 由一个信息未知的类型对象创建实际对象
                Console.WriteLine("Instace from Type is: " + o.GetType());
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message + "\r\n");
            }
        }

        #endregion
    }

    #region 通过自定义generic type来理解不同T将对应不同的类型对象
    internal sealed class MyGenericType<T>
    {
        //类型构造器只在类型第一次用到时调用, 但对于Generic type来说, 不同的T相当于不同类型, 也就会调用cctor
        //对于Generic type来说, cctor的意义在于判断T的范围, 比内建功能where T : xxx更强大
        static MyGenericType()
        {
            Console.WriteLine("Generic type cctor is called, type is " + typeof(T));
            count = 0;
            if (!typeof(T).IsEnum)
            {
                Console.WriteLine("ERROR detected in generic type cctor: T is not an enum type.");
            }
        }

        public static int count;
    }
    #endregion

    #region 包含不同类型数据的链表节点定义
    //直接Node<T>的缺点: 你可以定义不同类型的Node<int>, Node<string>
    //但无法把他们连起来: Node<int>.head必须得是一个Node<int>

    internal abstract class Node
    {
        public Node parent;
        public Node(Node p)
        {
            parent = p;
        }
    }
    internal class NodeWithData<T> : Node
    {
        public T data;
        public NodeWithData(T data, Node p) : base(p)
        {
            this.data = data;
        }
    }
    #endregion

    #region TODO Covariant&Contravariant类型参数
    internal sealed class CovariantAndContravariant
    {
        //1.只有委托和接口的泛型类型参数有逆变和协变的说法(???)
        //2.逆变 -> in -> 用于参数 -> 使用中原定义的类型T可变为他的子类,看上去这是有危险的
        //3.协变 -> out -> 用于返回类型 -> 使用中原定义的类型T可变为他的父类
        public delegate TReturn CoContraDel<in TParam, out TReturn>(TParam p);
        public CoContraDel<Object, String> delInstance;

        private void TryAdd()
        {
            delInstance += ExampleMethod;
        }

        private String ExampleMethod(Object p)
        {
            return "Pass in Obejct<-String, return String<-Object";
        }

        private void GenericMethod<T>(T p)
        {
            //泛型方法不需要在泛型类中
        }
    }
    #endregion

    #region 约束性
    internal partial class C1<T1, T2> { }
    //internal class C1<Tr, Ta> { } //类型参数名字不同不能区分同名class
    //internal class C1<T1, T2> where T1 : class where T2 : struct { } //不同的约束也不能区分同名class
    internal class C1<T1, T2, T3> { } //OK 虽然都叫C1, 但是C1`2和C1`3是两个不同的类

    internal partial class C1<T1, T2>
    {
        protected virtual void CMethod<TM>() where TM : class
        { }
    }
    internal class C11<T1, T2> : C1<T1, T2>
    {
        protected override void CMethod<TMC>() //类型变量的名字可以改TM->TMC
            //where TMC : struct //错误, virtual方法的约束只能继承不能修改
        {
            base.CMethod<TMC>();
        }
    }

    //主要约束
    internal class CZ<T1, T2, T3> 
        where T1 : class
        where T2 : struct
        where T3 : MyRefType
    { }
    //次要约束
    internal class CZ<T1, T2>
        where T1 : IComparable<T1>
        where T2 : T1
    { }
    //构造器约束
    internal class CC<T1>
        where T1 : new() //任何有无参构造器的类型都可以(目前只支持无参构造器)
    {
        //如果没有指明T是引用类型还是值类型, 用default关键字来初始化他(value -> 0, ref -> null)
        T1 i = default(T1);

        private void SomeMethod()
        {
            //T不能用作操作数(被* / + -等操作), C#只知道如何把基元类型作为操作数, 这是CLR目前的缺陷
            //T1 j = i * i;
        }
    }

    //将T约束为一个值类型是没有意义的
    //internal class CV<T> where T : Int32 { }
    #endregion
}
