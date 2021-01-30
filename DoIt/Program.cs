using Generators.Sql;
using System;

namespace DoIt
{
    class Program
    {
        static void Main(string[] args)
        {
            //GeneratedNamespace.GeneratedClass.GeneratedMethod();

            //GeneratedNamespace.XmlPrinter.PrintFirst();
            //GeneratedNamespace.XmlPrinter.PrintAnother();

            var a = new TestThing();

            int? b = null;

            //var something = GeneratedNamespace.Conditional.DoSomething("pants");

            var out1 = GeneratedNamespace.Conditional.DoSomething("Hello");
            var out2 = GeneratedNamespace.Conditional.DoSomething(b);
            var out3 = GeneratedNamespace.Conditional.DoSomething(a);
            var out4 = GeneratedNamespace.Conditional.DoSomething(4);

            //TestThing aout = GeneratedNamespace.Conditional.DoSomething(a);



        }
    }

    public class TestThing
    {
        public string Name { get; }
    }
}
