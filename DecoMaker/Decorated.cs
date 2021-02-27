using System;

namespace DecoMaker
{
    public class Decorated
    {
        public class Method
        {
            public class Param
            {
                public class Any
                {
                }

                public class None
                {
                }

                public class Of<T>
                {
                }
            }

            public class Return
            {
                public class Any
                {
                }

                public class Of<T>
                {
                }

                public class Void
                {
                }
            }

            public class AsyncReturn
            {
                public class Any
                {
                }

                public class Of<T>
                {
                }

                public class Task
                {
                }
            }

            public static Return.Any Invoke()
            {
                throw new NotImplementedException("This should not be run.");
            }

            public static Return.Of<T> Invoke<T>()
            {
                throw new NotImplementedException("This should not be run.");
            }

            public static AsyncReturn.Any InvokeAsync()
            {
                throw new NotImplementedException("This should not be run.");
            }

            public static AsyncReturn.Of<T> InvokeAsync<T>()
            {
                throw new NotImplementedException("This should not be run.");
            }
        }

        public class Property
        {            
            public class Any
            {
                public static Any Value { get; set; }
            }

            public class Of<T>
            {
                public static Of<T> Value { get; set; }
            }
        }
    }
}
