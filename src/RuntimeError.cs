namespace Vyx.src;

public class RuntimeError(Token token, string message) : Exception(message)
{
    private readonly Token Token = token;
}
