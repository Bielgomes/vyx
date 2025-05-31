namespace Vyx.src;

public class InterpreterEnvironment
{
    private readonly Dictionary<string, object> Values = [];

    public void Define(string name, object value)
    {
        Values[name] = value;
    }

    public object Get(Token name)
    {
        if (Values.TryGetValue(name.Lexeme(), out var value))
            return value;

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme()}'.");
    }

    public void Assign(Token name, object value)
    {
        if (Values.ContainsKey(name.Lexeme()))
        {
            Values[name.Lexeme()] = value;
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme()}'.");
    }
}
