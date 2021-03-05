namespace DecoMaker.Generation
{
    internal interface IDecoratorFactory<in TInfo, out TCode>
    {
        TCode Create(TInfo factoryInformation);
    }
}
