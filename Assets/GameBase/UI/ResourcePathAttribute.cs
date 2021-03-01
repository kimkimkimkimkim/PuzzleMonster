using System;

namespace GameBase
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ResourcePathAttribute : Attribute
    {
        public ResourcePathAttribute(string resourcePath)
        {
            this.resourcePath = resourcePath;
        }

        public string resourcePath { get; private set; }
    }
}
