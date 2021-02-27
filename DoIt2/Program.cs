using DecoMaker;
using System;
using System.Threading.Tasks;

namespace DoIt2
{
    class Program
    {
        static void Main(string[] args)
        {
            new Decorators.Class1ErrorDecorator(new Class1());

            Console.WriteLine("Hello World!");
        }
    }

    public interface IThing
    {

    }

    [Decorate("Error", typeof(MyTemplate), typeof(IThing))]
    public class Class1
    {
        public void Method1()
        {
        }

        public string Method2()
        {
            return "hello";
        }

        public string Method3(int a)
        {
            return "hello";
        }

        public int Method4(string a)
        {
            return 44;
        }

        public long Method5(int a, int b)
        {
            return (long) a + b;
        }

        public long Method6(string s)
        {
            return 4;
        }
    }

    public class MyTemplate
    {
        public string sss()
        {
            throw new NotFiniteNumberException();
            return Decorated.Method.Invoke<string>();
        }

        public string sss(Decorated.Method.Params.Any _)
        {
            // do stuff
            return Decorated.Method.Invoke<string>();
        }

        public Decorated.Method.Return.Any Default(Decorated.Method.Params.Any _)
        {
            try
            {
                return Decorated.Method.Invoke();
            }
            catch
            {
                Console.Write("Catch all!");
                throw;
            }
        }


    }
}
