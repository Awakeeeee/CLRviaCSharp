#define QIQIQI //note the position of #define must be in first place of the file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CLRviaCSharp
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "1")]
    class Chapter18_Attribute
    {
        static void Main(string[] args)
        {
            //Test usage
            AttrCooperator ac = new AttrCooperator();
            Console.WriteLine(ac.ToString());
            //Test custom match
            AlwaysLogNumberAttribute alna = new AlwaysLogNumberAttribute();
            alna.Number = 1000;
            AlwaysLogNumberAttribute alna2 = new AlwaysLogNumberAttribute();
            alna2.Number = 999;
            Console.WriteLine("Do they match: " + alna2.Match(alna));
            Console.WriteLine("Do they equal: " + alna2.Equals(alna));

            #region CustomAttributeData -> Get attribute Data, without instancing one
            Console.WriteLine(Environment.NewLine);
            IList<System.Reflection.CustomAttributeData> attList =
            System.Reflection.CustomAttributeData.GetCustomAttributes(typeof(AttrCooperator));

            foreach (System.Reflection.CustomAttributeData data in attList)
            {
                Console.WriteLine(data.Constructor.DeclaringType);
                Console.WriteLine(data.ConstructorArguments.Count);
                if(data.NamedArguments.Count > 0)
                    Console.WriteLine(data.NamedArguments[0].MemberName);
            }
            #endregion
        }
    }

    //1.Define a CLS compatible custom attribute
    //2.Override the Equal and Match method for it
    //3.Applay it, check it, and use it
    //4.Add condition on it

    //create a useless attribute, apply on a class, and its ToString will only log a given number
    //attribute class nommally contain data, not behaviour
    [System.Diagnostics.Conditional("QIQIQI")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class AlwaysLogNumberAttribute : System.Attribute
    {
        public int Number { get; set; }
        public AlwaysLogNumberAttribute() { }

        //as normal class, the rule of equal is exactly same 
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            AlwaysLogNumberAttribute ala = (AlwaysLogNumberAttribute)obj;
            if (ala.Number != this.Number)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return Number;
        }

        //different with Euqual, I can define the rule of Match
        //in this case, I want this matches other, if this number is less than other number
        public override bool Match(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != this.GetType())
                return false;
            AlwaysLogNumberAttribute ala = (AlwaysLogNumberAttribute)obj;
            if (this.Number > ala.Number)
                return false;
            return true;
        }
    }

    //a class use this atrribute(TODO how to apply to every class)
    [AlwaysLogNumber(Number = 100)]
    internal sealed class AttrCooperator
    {
        public override string ToString()
        {
            //there are at least 4 methods to check an attribute, remind them
            //this is one of them
            Attribute att = Attribute.GetCustomAttribute(this.GetType(), typeof(AlwaysLogNumberAttribute), false);
            if (att != null)
            {
                return "To string modified by attribute: " + ((AlwaysLogNumberAttribute)att).Number;
            }
            return base.ToString();
        }
    }
}