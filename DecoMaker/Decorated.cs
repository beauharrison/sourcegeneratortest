using System;

namespace DecoMaker
{
    public class Decorated
    {
        public class Method
        {
            public class Params
            {
                public class Any
                {
                }
            }

            public class Return
            {
                public class Any
                {
                }
            }

            public static Return.Any Invoke()
            {
                throw new NotImplementedException("This should not be run.");
            }

            public static T Invoke<T>()
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
                public static T Value { get; set; }
            }
        }
    }
}
