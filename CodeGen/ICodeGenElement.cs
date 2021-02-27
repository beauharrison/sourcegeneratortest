namespace CodeGen
{
    public interface ICodeGenElement
    {
        string GenerateCode(CodeGenStyle style = null);
    }
}
