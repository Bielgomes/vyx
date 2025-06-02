
namespace Vyx.Vyx.Core;

public class InterpreterEnvironment
{
    private InterpreterEnvironment? Enclosing = null;
    private readonly Dictionary<string, object> Values = [];

    public InterpreterEnvironment()
    {
        Enclosing = null;
    }

    public InterpreterEnvironment(InterpreterEnvironment enclosing)
    {
        Enclosing = enclosing;
    }

    public void Define(Token name, object value)
    {
        if (Values.ContainsKey(name.Lexeme()))
            throw new RuntimeError(name, $"Variable '{name.Lexeme()}' already defined.");

        Values[name.Lexeme()] = value;
    }

    public void Define(string name, object value)
    {
        Values[name] = value;
    }

    public object Get(Token name)
    {
        if (Values.TryGetValue(name.Lexeme(), out var value))
            return value;

        if (Enclosing != null) return Enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme()}'.");
    }

    public void Assign(Token name, object value)
    {
        if (Values.ContainsKey(name.Lexeme()))
        {
            Values[name.Lexeme()] = value;
            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme()}'.");
    }

    internal void Define(Func<string> lexeme, object v)
    {
        throw new NotImplementedException();
    }
}
