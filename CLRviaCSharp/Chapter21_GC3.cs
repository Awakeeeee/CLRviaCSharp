using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CLRviaCSharp
{
    class Chapter21_GC3
    {
        static void Main_21_3(string[] args)
        {
            #region using 语句
            FileManipulator fm = new FileManipulator();
            //for Win txt, initial 
            //UTF -8 BOM is EF BB BF(239 187 191)
            //UTF-16 BOM is FF FE(255 254)
            //用Truncate mode写入时, 记事本被完全清空, 需要把win规定的UTF-16 BOM: FF FE也写入
            //然后是内容的unicode
            byte[] bb = new byte[] { 0xFF, 0xFE, 0x34, 0xF9, 0xFC, 0x72 };
            //fm.AddSomething(bb);
            ////fm.ReadAll();
            //fm.Dispose();
            //fm.AddSomething(bb);
            using (fm)
            {
                fm.AddSomething(bb);
            }
            //用代码里删除一个文件
            //System.IO.File.Delete("D:\\textfile.txt");
            #endregion

            //ManipulateGC();
            //UseFixed();

            //Obj_A a = new Obj_A();
            //a.UseWeakRefIfItsStillAlive();
            //Obj_B b = new Obj_B();
            //b.UseWeakRefIfItsStillAlive();

            Go();
        }

        #region 我对GC有一定的控制权: GCHandle struct
        static void ManipulateGC()
        {
            string ps = "secrets";
            //我手动把这个对象加入了GC观察表, 并告诉GC总是把他作为根, 永远不要回收他的内存, 甚至不要在compact中动他
            GCHandle gch = GCHandle.Alloc(ps, GCHandleType.Pinned);

            Console.WriteLine("ToIntPtr: " + GCHandle.ToIntPtr(gch));
            Console.WriteLine("Pinned addr: " + gch.AddrOfPinnedObject());
            Console.WriteLine("Target: " + gch.Target);
            
            //这就是向非托管传递的正确书信
            IntPtr gonnaPassToUnmanaged = GCHandle.ToIntPtr(gch);
            //GChandle关联的对象引用
            object unpackTheHandle = gch.Target;
        }
        #endregion

        #region Fixed block
        unsafe static void UseFixed()
        {
            //搞一块GC认为有必要compact的垃圾
            for (int i = 0; i < 50000; i++) new object();

            IntPtr ori;
            byte[] arr = new byte[1000];

            fixed (byte* pb = arr)
            {
                ori = (IntPtr)pb; //以当前pb的值创建一个IntPtr结构封装他, 这是一个copy过程
            }

            GC.Collect();

            fixed (byte* pb = arr)
            {
                if (ori == (IntPtr)pb)
                    Console.WriteLine("The memory pos of Arr is not moved during GC");
                else
                    Console.WriteLine("The memory pos of Arr is moved during GC");
            }
        }
        #endregion

        #region Create a Form and Buttons to test WeakDelegate
        static void Go()
        {
            var form = new Form() { Text = "TESTING FORM", FormBorderStyle = FormBorderStyle.FixedSingle };
            var btnTest = new Button() { Text = "Test Btn", Width = form.Width / 2 };
            var btnGC = new Button() { Text = "Force GC", Width = btnTest.Width, Left = btnTest.Width };

            //实例化一个WeakEventHandler并直接赋值他的属性
            //通过隐式转型转为了T->EventHandler类型
            btnTest.Click += new WeakEventHandler(new DoNotLiveJustForEvent().OnButtonClicked) { ClearRealDelOnGC = realDel => { btnTest.Click -= realDel; } };
            btnGC.Click += (sender, e) => { GC.Collect(); MessageBox.Show("Runs GC!"); };

            form.Controls.Add(btnTest);
            form.Controls.Add(btnGC);
            form.ShowDialog();
        }
        #endregion
    }

    #region 自己实现一个应用Dispose模式的类型
    internal sealed class FileManipulator : IDisposable
    {
        public System.IO.FileStream _fs;
        //这里就是可以随意规定扩展名的地方
        //写入bytes, 只要选择的打开器的编码方式和写入一致, 就能看到真实的内容 
        private string path = "D:\\textfile.secretqiqiqi";
        private bool hasDisposed;

        public FileManipulator()
        {
            hasDisposed = false;
        }

        public void AddSomething(byte[] bytes)
        {
            if (hasDisposed)
                throw new System.ObjectDisposedException(this.ToString());
            _fs = System.IO.File.Open(path, System.IO.FileMode.Create);
            _fs.Write(bytes, 0, bytes.Length);
        }

        public void ReadAll()
        {
            //File is a static class
            byte[] result = System.IO.File.ReadAllBytes(path);
            foreach (byte b in result)
            {
                Console.WriteLine(b);
            }
        }

        ~FileManipulator()
        {
            Dispose(false);
        }
        public void Close()
        {
            Dispose(true);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        //the actual dispose method
        private void Dispose(bool disposing)
        {
            if (hasDisposed)
            {
                return; //可多次终结而没有异常
            }

            //如果这是在确定性的显式终结
            if (disposing)
            {
                if (_fs != null)
                {
                    _fs.Close();
                    Console.WriteLine("Read fs while dispoing: ");
                    ReadAll();
                }
            }

            if (_fs != null)
            {
                _fs.Close();
                hasDisposed = true;
            }
        }
    }
    #endregion

    #region WeakReference
    //WeakReference意为A对B的引用关系不会导致如果A是一个根, 那么B就不会被回收
    //这是因为A对B的引用不是直接引用, 而是引用B在GC handle table里的对应项来做到的

    //用GCHandle的原理手动实现weak ref
    internal class Obj_A
    {
        GCHandle weakRef;

        public Obj_A()
        {
            weakRef = GCHandle.Alloc(new Obj_Ref(), GCHandleType.Weak);
        }

        public void UseWeakRefIfItsStillAlive()
        {
            if (weakRef.Target != null)
                ((Obj_Ref)weakRef.Target).Callback();
            else
                weakRef.Free();
        }
    }

    //用内置的WeakReference class
    internal sealed class Obj_B
    {
        WeakReference wr;

        public Obj_B()
        {
            wr = new WeakReference(new Obj_Ref());
        }
        public void UseWeakRefIfItsStillAlive()
        {
            if (wr.IsAlive)
                ((Obj_Ref)wr.Target).Callback();
            else
                wr = null; //this is the bug that WeakReference does not implement Dispose
        }
    }

    internal class Obj_Ref
    {
        public void Callback()
        {
            Console.WriteLine("I am the object of weak ref points to. I am alive now.");
        }
    }
    #endregion

    #region WeakDelegate
    //A Generic wrapper of WeakReference
    internal struct WeakReference<T> : IDisposable
        where T : class
    {
        private WeakReference m_wref;

        public WeakReference(T obj)
        {
            m_wref = new WeakReference(obj);
        }

        public T Target { get { return (T)m_wref.Target; } }

        public void Dispose()
        {
            m_wref = null;
        }
    }

    //A normal listener class that provide a callback method
    internal sealed class DoNotLiveJustForEvent
    {
        public void OnButtonClicked(object sender, EventArgs e)
        {
            MessageBox.Show("I am living for my instance, NOT for the event or as delegate memeber!");
        } 
    }

    //WeakDelegate类型的抽象基类
    internal abstract class WeakDelegate<T>
        where T : class //should limit to MultiDelegate but C# donot allow it
    {
        private WeakReference<T> m_weakRef_realDelegate;

        private Action<T> m_clearRealDel_OnGC;
        public Action<T> ClearRealDelOnGC { set { m_clearRealDel_OnGC = value; } }

        public WeakDelegate(T realDelegate)
        {
            MulticastDelegate md = (MulticastDelegate)(object)realDelegate;
            if (md == null)
            {
                throw new ArgumentException("It is only make sense to transfer a Delegate into WeakDelegate!");
            }
            m_weakRef_realDelegate = new WeakReference<T>(realDelegate);
        }

        protected T GetRealDelegate()
        {
            //when real delegate instance is not GCed, return it
            T realDelegate = m_weakRef_realDelegate.Target;
            if (realDelegate != null)
                return realDelegate;

            //if GC has recycled the instance, remove it from user event
            m_weakRef_realDelegate.Dispose();
            if (m_clearRealDel_OnGC != null)
            {
                m_clearRealDel_OnGC(GetDelegate());
                m_clearRealDel_OnGC = null; //这个引用不再引用保存给定的实现'当instance不在时取消事件注册'的方法的委托, 让该委托被下次GC
            }
            return null;
        }

        //这个方法代表Listener方法
        public abstract T GetDelegate();

        //实现隐式转换, 让WeakDelegate的行为像Delegate一样
        public static implicit operator T(WeakDelegate<T> wd)
        {
            return wd.GetDelegate();
        }
    }

    //为delegate EH (object sender, EventArgs e)类型的委托做一个他的弱委托
    internal sealed class WeakEventHandler : WeakDelegate<EventHandler>
    {
        public WeakEventHandler(EventHandler eh) : base(eh) { }

        //这是一个我不常见的返回一个委托的方法
        public override EventHandler GetDelegate()
        {
            return Callback;
        }

        private void Callback(object sender, EventArgs e)
        {
            EventHandler eh = GetRealDelegate();
            if (eh != null)
                eh(sender, e);
        }
    }
    #endregion
}
