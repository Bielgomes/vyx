namespace Vyx.Vyx.Core;

public interface IVyxCallable
{
    public int Arity();
    public abstract object Call(Interpreter interpreter, List<object> arguments);
}