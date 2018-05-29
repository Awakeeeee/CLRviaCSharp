using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLRviaCSharp
{
    class Chapter11_Event
    {
        static void Main11(string[] args)
        {
            ListenerOne listener = new ListenerOne();
            SenderBase sb = new SenderBase();
            SenderChild sc = new SenderChild();

            listener.Register(sb);
            listener.Register(sc);

            string s = "It comes!";
            int i = 100;

            sb.SomethingHappens(i, s);
            sc.SomethingHappens(i, s);

            listener.Unregister(sb);
            listener.Unregister(sc);


            ManyEventsClass mec = new ManyEventsClass();
            mec.e1 += TempHandleMany_1; //背后的实现是自定义的, 而不是默认自动生成的event+=
            mec.e2 += TempHandleMany_2;
            mec.e3 += TempHandleMany_3;

            mec.Happen1();
            mec.Happen2(i, s);
            //mec.Happen3();
        }

        static void TempHandleMany_1(Object sender, EventArgs e)
        {
            Console.WriteLine("Handling event 1 of many events class");
        }
        static void TempHandleMany_2(Object sender, MyEventArg e)
        {
            Console.WriteLine("Handling event 2 of many events class");
        }
        static void TempHandleMany_3()
        {
            Console.WriteLine("Handling event 3 of many events class");
        }
    }

    #region 把事件额外信息encapsulate
    internal sealed class MyEventArg : EventArgs
    {
        //也许事件需要给listener传递一个int, 或一个string, 或者把所有这些额外信息encap到一个类中
        public Int32 IntInfo { get; set; }
        public String StringInfo { get; set; }

        public MyEventArg(int i, string s)
        {
            IntInfo = i;
            StringInfo = s;
        }
    }
    #endregion

    #region 在sender中定义事件
    internal class SenderBase
    {
        //listener方法的原型 -> delegate容器
        //使用delegate关键字, 背后实际是在这里定义了一个名为MyEventHandler<T>的类, 且该类继承自System.Delegate
        public delegate void MyEventHandler<T>(Object sender, T e);

        //定义事件 -> event关键字,总是public,需要一个原型
        //这是所谓的隐式定义, event关键字知道去做背后的事情:
        //1. 实例化一个上面定义的MyEventHandler<T>类, 定为private, 用于存listener方法
        //2. 写一个add和remove方法, 对应C#的+=,-=
        //3. 其他代表event的元数据信息(TODO) 
        public event MyEventHandler<MyEventArg> myEvent;

        //事件触发公开方法, 别人调用它代表事件准备触发
        public void SomethingHappens(int i, string s)
        {
            MyEventArg args = new MyEventArg(i, s);
            OnEventTrigger(args);
        }

        //事件检查内部方法, 事件触发时机达到后, 检查是否可以真正的告诉各个listener
        //如果有子类, 子类应该有是否真正触发事件的选择权
        protected virtual void OnEventTrigger(MyEventArg args)
        {
            //TODO thrading problem
            if (myEvent != null)
                myEvent.Invoke(this, args);
        }
    }
    internal class SenderChild : SenderBase
    {
        protected override void OnEventTrigger(MyEventArg args)
        {
            //子类选择无论如何都不触发事件
            return;
        }
    }
    #endregion

    #region Lisenters
    internal sealed class ListenerOne
    {
        //Listener提供让自己注册和取消监听事件的方法
        public void Register(SenderBase sb)
        {
            sb.myEvent += EventHandlerOne;
        }
        public void Unregister(SenderBase sb)
        {
            //不用担心删除一个没添加过的方法, 最终调用Delegate.Remove方法在这种情况只是什么也不做
            sb.myEvent -= EventHandlerOne;
        }

        //Listener定义如何处理事件
        private void EventHandlerOne(Object sender, MyEventArg e)
        {
            Console.WriteLine("ListenerOne is handling event from " + sender.GetType() +
                ". Dealing with info of [" + e.IntInfo.ToString() + "] and " + "[" + e.StringInfo + "].");
        }
    }
    #endregion

    #region 显示定义事件 -> 利用EventSet更好的处理含有大量事件的类的思路

    //EventSet类的目的在于, 尽量将与众不同的多个事件统一化, 以减少隐式定义各个事件时, 建立各个具体的delegate造成的内存消耗
    //EventSet维护一个dictionary
    //其中key为event代号(这里简单用string), 用于查找对应的delegate
    //value全部统一为基类System.Delegate, 只有真正触发事件时, 才会使用具体的delegate类型
    public sealed class EventSet
    {
        private Dictionary<String, System.Delegate> event_list = new Dictionary<string, Delegate>();
        public Dictionary<String, System.Delegate> EventList { get { return event_list; } private set { } }

        //显示定义event时, 对add的显示定义就是调用了这个方法
        //相比隐式定义event, 一个不同点是自定义中还多了维护dictionary的内容
        public void Add(string eName, Delegate del)
        {
            Delegate d;
            event_list.TryGetValue(eName, out d);
            //因为value是Delegate而不是通过delegate定义的委托, 因此不能用+=
            //TODO delegate关键字背后做的其他工作
            d = Delegate.Combine(d, del);
            event_list[eName] = d;
        }

        public void Remove(string eName, Delegate del)
        {
            Delegate d;
            if (event_list.TryGetValue(eName, out d))
            {
                d = Delegate.Remove(d, del);
                if (d != null)
                    event_list[eName] = d;
                else
                    event_list.Remove(eName);
            }
        }

        //触发事件时通过DynamicInvoke, 最终获得具体delegate
        public void Raise(string eName, Object sender, EventArgs e)
        {
            Delegate d;
            event_list.TryGetValue(eName, out d);
            if (d != null)
            {
                d.DynamicInvoke(sender, e); //TODO 参数不是object,EventArgs怎么办
            }
        }
    }

    //使用EventSet类
    internal sealed class ManyEventsClass
    {
        private readonly EventSet eventSet = new EventSet();

        //event1
        //显示定义event
        //event关键字的作用之一是定义add remove方法, 这两个方法可以这样显示实现
        public event EventHandler<EventArgs> e1 {
            add { eventSet.Add("Event_1", value); }
            remove { eventSet.Remove("Event_1", value); }
        }
        //这里触发方法和检查方法写在一起了
        public void Happen1()
        {
            eventSet.Raise("Event_1", this, EventArgs.Empty);
        }

        //event2
        public event EventHandler<MyEventArg> e2 {
            add { eventSet.Add("Event_2", value); }
            remove { eventSet.Remove("Event_2", value); }
        }
        public void Happen2(int i, string s)
        {
            eventSet.Raise("Event_2", this, new MyEventArg(i, s));
        }

        public event Action e3 {
            add { eventSet.Add("Event_3", value); }
            remove { eventSet.Remove("Event_3", value); }
        }
        public void Happen3()
        {
            eventSet.Raise("Event_3", this, null);
        }

        //... event n
    }

    #endregion
}