namespace DecoMaker.Generation
{
    internal class DecoratorPropertyInformation
    {
        public DecoratorClassInformation ParentInformation { get; }

        public string PropertyName { get; }

        public string PropertyType { get; }

        public bool HasSetter { get; }

    }
}
