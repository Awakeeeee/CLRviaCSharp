using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Microsoft")]

namespace CLRviaCSharp
{
    class Chapter6_ClassAndMember
    {
        static void Main6(string[] args)
        {
            Example e = new Example();
            Nested n = new Nested();

            e.ToString();
            n.ToString();
        }
    }

    internal partial class Example
    {
        //const
        public const int SIZE = 100;

        //field
        public readonly int AGE = 50;
        public static int NUMBER = 10;
        private string desc;
        private int idx;

        //constructor
        static Example()
        {
            NUMBER = 10;
        }
        public Example()
        { }

        //method
        public static void ClassMethod()
        { }
        public void InstanceMethod()
        { }
        public void InstanceMethod(int i)
        { }
        public override String ToString()
        {
            //IL: call
            return base.ToString();
        }

        //property
        public int Prop { get { return idx; } set { idx = value; } }
        public int this[string s]{get{return 0;} set{}} //TODO ?

        //event
        public delegate void MyEventHandler();
        public event MyEventHandler e;

        //other type
        public Nested nested;

        //operator overrides
    }

    internal partial class Example
    {
        //Partial content
        protected internal int access;
    }

    public sealed class Nested
    {
        public override String ToString()
        {
            //IL: callvirt
            return "Nested";
        }
    }
}
