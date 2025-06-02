namespace Vyx.Vyx.Core;

public class VyxFunction(Stmt.Function declaration) : IVyxCallable
{
    private Stmt.Function Declaration { get; } = declaration;

    public int Arity()
    {
        return Declaration.Params.Count;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        InterpreterEnvironment environment = new(interpreter.Globals);
        for (int i = 0; i < Declaration.Params.Count; i++)
        {
            environment.Define(Declaration.Params.ElementAt(i).Lexeme, arguments.ElementAt(i));
        }

        interpreter.ExecuteBlock(Declaration.Body, environment);
        return null!;
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme()}>";
    }
}
