using Generators.Sql;
using System;
using GeneratedDependencyInjection;
using GeneratedNamespace;
using Generators.STEvent;
using DoIt.Decorators;
using System.Threading.Tasks;

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


            //DIService.AddTransient<TestStuff, TestStuff>();
            //DIService.AddSingleton<TestJunk, TestJunk>();
            //DIService.AddTransient<ITestThing, TestThing>();

            //var a = DIService.Get<ITestThing>();
            //var b = DIService.Get<TestStuff>();
            //var c = DIService.Get<TestJunk>();

            //STEvent.Register<string, long, int, Program>(MyHandler);
            //STEvent.Notify("hello", 44, 3, new Program());

            //STEvent.Register<string, Program>((a, b) => { });

            var a = new MyGreatClassDecorator<string>(new MyGreatClass<string>("hello"));

            try
            {
                a.DoSomething(3, new Aa());
            }
            catch
            {
            }

            try
            { 
                a.GetIt();
            }
            catch
            {
            }

            try
            { 
                a.GetNumber();
            }
            catch
            {
            }

            var b = a.DoSomethingNew("", () => 4);

        }

        private static void MyHandler(string arg1, long arg2, int arg3, Program arg4)
        {
            Console.WriteLine("im a litte fat girl");
        }
    }

    [Decorate]
    public class MyGreatClass<T>
    {
        private T _A;

        public MyGreatClass(T a)
        {
            _A = a;
        }

        public TAbc DoSomething<TAbc>(int i, TAbc xyz) where TAbc : class, IAa
        {
            throw new Exception("pants");
        }

        public void GetIt()
        {
            throw new Exception("fun");
        }

        public int GetNumber()
        {
            throw new Exception("cheese");
        }

        public async Task<Aa> DoSomethingNew(string action, Func<int> something)
        {
            return await Task.FromResult(new Aa());
        }
    }

    public class Aa : IAa
    {

    }

    public interface IAa
    {

    }
}
