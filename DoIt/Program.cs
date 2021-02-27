using Generators.Sql;
using System;
using GeneratedDependencyInjection;
using GeneratedNamespace;
using Generators.STEvent;
using DoIt.Decorators;
using System.Threading.Tasks;

namespace DoIt
{
    public delegate void MyImportantEventHandler2(DoIt.MyImportantEvent arg0);

    class Program
    {
        static void Main(string[] args)
        {
            Action<MyImportantEvent> act = (e) => { };
            MyImportantEventHandler2 d = new MyImportantEventHandler2(act);

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

            //STEvent.Register<MyImportantEvent>(MyHandler);
            //STEvent.Register<MyImportantEvent>(MyHandler2);
            //STEvent.Notify(new MyImportantEvent { Name = "John" });
            //STEvent.Notify(new MyImportantEvent { Name = "Timmy" });

            //var a = new MyGreatClassDecorator<string>(new MyGreatClass<string>("hello"));
            //var b = new MyOtherGreatClassDecorator(new MyOtherGreatClass());

            //try
            //{
            //    a.DoSomething(3, new Aa());
            //}
            //catch
            //{
            //}

            //try
            //{ 
            //    a.GetIt();
            //}
            //catch
            //{
            //}

            //try
            //{ 
            //    a.GetNumber();
            //}
            //catch
            //{
            //}

        }

        private static void MyHandler(MyImportantEvent eventTohandle)
        {
            Console.WriteLine($"\"im a litte fat girl\" said {eventTohandle.Name}");
        }

        private static void MyHandler2(MyImportantEvent eventTohandle)
        {
            Console.WriteLine($"\"Do it again\" said {eventTohandle.Name}");
        }
    }

    public class MyImportantEvent
    {
        public string Name { get; set; }
    }

    public interface IMyGreatClass
    {
        TAbc DoSomething<TAbc>(int i, TAbc xyz) where TAbc : class, IAa;
        Task<Aa> DoSomethingNew(string action, Func<int> something);
        void GetIt();
        int GetNumber();
    }

    [Decorate]
    public class MyGreatClass<T> : IMyGreatClass
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


    [Generators.STEvent.Decorate]
    public class MyOtherGreatClass : IMyGreatClass
    {
        public Task<Aa> DoSomethingNew(string action, Func<int> something)
        {
            throw new NotImplementedException();
        }

        public void GetIt()
        {
            throw new NotImplementedException();
        }

        public int GetNumber()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// comments
        /// </summary>
        /// <typeparam name="TAbc"></typeparam>
        /// <param name="i"></param>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public TAbc DoSomething<TAbc>(int i, TAbc xyz) where TAbc : class, IAa
        {
            throw new NotImplementedException();
        }

        public int EatPie(string a)
        {
            return 2;
        }
    }
    public class Aa : IAa
    {

    }

    public interface IAa
    {

    }
}
