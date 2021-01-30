using System.Collections.Generic;

namespace Generators.ApiDefModel
{
    public class ApiDefinition
    {
        public string Name { get; set; } 

        public string Namespace { get; set; }

        public string ControllerRoute { get; set; }

        public ApiMethod[] Methods { get; set; } = new ApiMethod[0];

        public List<ApiModel> Models { get; set; } = new List<ApiModel>();

        public string Description { get; set; }
    }

    public class ApiType
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public bool Required { get; set; }

        public string Description { get; set; }
    }

    public class ApiMethod
    {
        public string Name { get; set; }

        public string Method { get; set; }

        public string MethodRoute { get; set; }

        public bool Async { get; set; }

        public ApiType[] QueryParams { get; set; } = new ApiType[0];

        public ApiType RequestBodyType { get; set; }

        public ApiType ResponseBodyType { get; set; }

        public string Description { get; set; }
    }

    public class ApiModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public ApiType[] Props { get; set; } = new ApiType[0];
    }
}
