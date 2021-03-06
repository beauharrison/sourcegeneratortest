using DecoMaker;
using System;
using System.Threading.Tasks;
using DoIt2.Decorators;

namespace DoIt2
{
    class Program
    {
        static void Main(string[] args)
        {
            //new Class1ErrorDecorator(new Class1(), "", 3);

            //new Class1ErrorDecorator(new Class1(), "jimmy", 44);

            var a = new IServiceLoggerDecorator(new MyService());
        }
    }

    public interface IThing
    {
    }

    [Decorate("Logger", typeof(MyTemplate))]
    public interface IService
    {
        int Run(string a);

        long PpP { get; set; }
    }

    public class MyService : IService
    {
        public long PpP { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public int Run(string a)
        {
            throw new NotImplementedException();
        }
    }

    [Decorate("Error", typeof(MyTemplate))]
    [Decorate("Something", typeof(MyTemplate))]
    public class Class1
    {
        public string Method1()
        {
            return null;
        }

        public Task<string> Method2()
        {
            return Task.FromResult("hello");
        }

        public async Task<string> Method3()
        {
            return await Task.FromResult("hello");
        }

        public int Num { get; set; }

        //public string Method3(int a)
        //{
        //    return "hello";
        //}

        //public int Method4(string a)
        //{
        //    return 44;
        //}

        //public long Method5(int a, int b)
        //{
        //    return (long) a + b;
        //}

        //public long Method6(string s)
        //{
        //    return 4;
        //}
    }

    public class MyTemplate
    {
        //public MyTemplate(string name, int timeout)
        //{

        //}

        //public string _AnyString(Decorated.Method.Params.Any _)
        //{
        //    // do stuff
        //    return Decorated.Method.Invoke<string>();
        //}

        public void _idk()
        {
            // hello
            Decorated.Method.Invoke();
        }

        public Decorated.Method.Return.Any _somthfk(Decorated.Method.Params.Any _)
        {
            // there
            return Decorated.Method.Invoke();
        }

        //public Task<string> _AnyTaskString(Decorated.Method.Params.Any _)
        //{
        //    // do things in a task
        //    return Decorated.Method.Invoke<Task<string>>();
        //}

        //public async Task<string> _AnyStringAsync(Decorated.Method.Params.Any _)
        //{
        //    // do things async
        //    return await Decorated.Method.Invoke<Task<string>>();
        //}

        //public Decorated.Property.Any _AnyP
        //{
        //    get
        //    {
        //        // something
        //        return Decorated.Property.Any.Value;
        //    }
        //    set
        //    {
        //        // idk
        //        Decorated.Property.Any.Value = value;
        //    }
        //}

        public Decorated.Property.Any _AnyP2
        {
            get
            {
                // pie
                return Decorated.Property.Any.Value;
            }
            set
            {
                // adkdk
                Decorated.Property.Any.Value = value;
            }
        }
    }
}
