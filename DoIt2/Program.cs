using DecoMaker;
using System;

namespace DoIt2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    [Decorate("Error", typeof(MyTemplate))]
    public class Class1
    {
        public string DoIt()
        {
            return "hello";
        }
    }

    public static class Names
    {
        public const string Dec = "Audit";
    }


    [Decorate(Names.Dec, typeof(MyTemplate), typeof(Names))]
    public class Class2
    {
        public string DoIt()
        {
            return "hello";
        }
    }

    public class MyTemplate
    {
        public Decorated.Method.Return.Of<string> MethodTemplate(Decorated.Method.Param.Of<string> stringValue)
        {
            try
            {
                return Decorated.Method.Invoke<string>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Decorated.Method.Return.Of<string> MethodTemplate2(Decorated.Method.Param.Of<string> stringValue) => Decorated.Method.Invoke<string>();


        public Decorated.Method.AsyncReturn.Of<int> AsyncMethodTemplate(Decorated.Method.Param.None _)
        {
            try
            {
                return Decorated.Method.InvokeAsync<int>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public Decorated.Property.Any PropertyTemplate
        {
            get
            {
                Console.WriteLine("getting it");
                return Decorated.Property.Any.Value;
            }
            set
            {
                Console.WriteLine("setting it");
                Decorated.Property.Any.Value = value;
            }
        }

        public Decorated.Property.Of<string> StringPropertyTemplate
        {
            get
            {
                Console.WriteLine("getting it");
                return Decorated.Property.Of<string>.Value;
            }
            set
            {
                Console.WriteLine("setting it");
                Decorated.Property.Of<string>.Value = value;
            }
        }

        public Decorated.Property.Of<string> Something => Decorated.Property.Of<string>.Value;
    }
}
