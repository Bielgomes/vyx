namespace Vyx.Core;

public interface IVyxCallable
{
    public int Arity();
    public object Call(Interpreter interpreter, List<object> arguments);
}
