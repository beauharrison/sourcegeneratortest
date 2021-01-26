using System.Collections.Generic;

namespace Generators.ApiDefModel
{
    public class ApiDefinition
    {
        public string Name { get; set; }

        public string ControllerRoute { get; set; }

        public ApiType[] Dependencies { get; set; }

        public ApiMethod[] Methods { get; set; }

        public Dictionary<string, ApiModel> Models { get; set; }
    }

    public class ApiType
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }
    }

    public class ApiMethod
    {
        public string Name { get; set; }

        public string Method { get; set; }

        public string MethodRoute { get; set; }

        public bool Async { get; set; }

        public ApiType[] QueryParams { get; set; }

        public ApiType RequestBodyType { get; set; }

        public ApiType ResponseBodyType { get; set; }
    }

    public class ApiModel
    {
        public ApiType[] Props { get; set; }
    }
}
