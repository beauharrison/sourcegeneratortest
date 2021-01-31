using Generators.Sql;
using System;
using GeneratedDependencyInjection;

namespace DoIt
{
    class Program
    {
        static void Main(string[] args)
        {
            //GeneratedNamespace.GeneratedClass.GeneratedMethod();

            //GeneratedNamespace.XmlPrinter.PrintFirst();
            //GeneratedNamespace.XmlPrinter.PrintAnother();

            //var a = new TestThing<string>("a", 22, null);

            //int? b = null;

            //var something = GeneratedNamespace.Conditional.DoSomething("pants");

            //var out1 = GeneratedNamespace.Conditional.DoSomething("Hello");
            //var out2 = Conditional.DoSomething<string, TestThing<string>>(b);
            //var out3 = GeneratedNamespace.Conditional.DoSomething(a);
            //var out4 = GeneratedNamespace.Conditional.DoSomething(4);

            //TestThing aout = GeneratedNamespace.Conditional.DoSomething(a);


            DIService.AddTransient<TestStuff, TestStuff>();
            DIService.AddSingleton<TestJunk, TestJunk>();
            DIService.AddTransient<ITestThing, TestThing>();

            var a = DIService.Get<ITestThing>();
            var b = DIService.Get<TestStuff>();
            var c = DIService.Get<TestJunk>();
        }
    }

    public interface ITestThing
    {
    }

    public class TestThing : ITestThing
    {
        public TestThing(TestStuff stuff)
        {
            Stuff = stuff;
        }

        public string Name { get; }
        public TestStuff Stuff { get; }
    }

    public class TestStuff
    {
        public TestStuff(TestJunk junk)
        {
            Junk = junk;
        }

        private static Random _R = new Random();

        public int Val { get; } = _R.Next(0, 100);

        public TestJunk Junk { get; }
    }

    public class TestJunk
    {
        private static Random _R = new Random();

        public int Val { get; } = _R.Next(0, 100);
    }
}
