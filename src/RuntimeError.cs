namespace Vyx.src;

public class RuntimeError(Token token, string message) : Exception(message)
{
    public Token Token { get; private set; } = token;
}
