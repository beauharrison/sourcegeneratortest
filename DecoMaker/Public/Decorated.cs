using System;

namespace DecoMaker
{
    /// <summary>
    /// Placeholder types for use when writing template.
    /// </summary>
    public class Decorated
    {
        /// <summary>
        /// Placeholder types for template methods.
        /// </summary>
        public class Method
        {
            /// <summary>
            /// Placeholder types for template method parameters.
            /// </summary>
            public class Params
            {
                /// <summary>
                /// Placeholder type to indicate a template method can be used when decorating methods with any set of parameters.
                /// </summary>
                public class Any
                {
                }
            }

            /// <summary>
            /// Placeholder types for template method returns.
            /// </summary>
            public class Return
            {
                /// <summary>
                /// Placeholder type to indicate a template methood can be used when decorating methods with any return type.
                /// </summary>
                public class Any
                {
                }
            }

            /// <summary>
            /// Placeholder function representing the call to the decorated method in a decorator template.
            /// </summary>
            public static Return.Any Invoke()
            {
                throw new NotImplementedException("This should not be run.");
            }

            /// <summary>
            /// Placeholder function representing the call to the decorated method in a decorator template.
            /// </summary>
            public static T Invoke<T>()
            {
                throw new NotImplementedException("This should not be run.");
            }
        }

        /// <summary>
        /// Placeholder types for template parameters.
        /// </summary>
        public class Property
        {
            /// <summary>
            /// Placeholder type to indicate a template property can be used when decorating properties of any type.
            /// </summary>
            public class Any
            {
                /// <summary>
                /// The placeholder property's value.
                /// </summary>
                public static Any Value { get; set; }
            }
        }
    }
}
