namespace Vyx.Vyx.Core;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
{
    public InterpreterEnvironment Globals = new();
    private InterpreterEnvironment InterpreterEnvironment;

    public Interpreter()
    {
        InterpreterEnvironment = Globals;
        Globals.Define("clock", new ClockCallable());
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
                Execute(statement);
        }
        catch (RuntimeError error)
        {
            Program.RuntimeError(error);
        }
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        object value = Evaluate(expr.Value);
        InterpreterEnvironment.Assign(expr.Name, value);
        return value;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return InterpreterEnvironment.Get(expr.Name);
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        object left = Evaluate(expr.Left);
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Kind)
        {
            case TokenKind.Plus:
                if (left is double leftNum && right is double rightNum)
                    return leftNum + rightNum;
                if (left is string || right is string)
                    return Stringify(left) + Stringify(right);

                throw new RuntimeError(expr.Operator, "Operands must be numbers or strings.");
            case TokenKind.Minus:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left - (double)right;
            case TokenKind.Star:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
            case TokenKind.Slash:
                CheckNumberOperands(expr.Operator, left, right);
                if ((double)right == 0) throw new RuntimeError(expr.Operator, "Division by zero.");
                return (double)left / (double)right;

            case TokenKind.EqualsEqual:
                return IsEqual(left, right);
            case TokenKind.NotEqual:
                return !IsEqual(left, right);
            case TokenKind.GreaterThan:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left > (double)right;
            case TokenKind.GreaterThanEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left >= (double)right;
            case TokenKind.LessThan:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left < (double)right;
            case TokenKind.LessThanEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left <= (double)right;

            case TokenKind.Elvis:
                if (left == null) return right;
                return left;
        }

        return null!;
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        object callee = Evaluate(expr.Calle);

        var arguments = new List<object>();
        foreach (Expr argument in expr.Arguments)
        {
            arguments.Add(Evaluate(argument));
        }

        if (callee is not IVyxCallable)
        {
            throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        }

        IVyxCallable function = (IVyxCallable)callee;
        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
        }

        return function.Call(this, arguments);
    }

    public object VisitTernaryExpr(Expr.Ternary expr)
    {
        object condition = Evaluate(expr.Condition);

        return IsTruthy(condition)
            ? Evaluate(expr.ThenBranch)
            : Evaluate(expr.ElseBranch);
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        object left = Evaluate(expr.Left);

        if (expr.Operator.Kind == TokenKind.Or)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        object right = Evaluate(expr.Right);

        switch (expr.Operator.Kind)
        {
            case TokenKind.Minus:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right;
            case TokenKind.Not:
                return !IsTruthy(right);
            default:
                throw new RuntimeError(expr.Operator, "Unknown unary operator.");
        }
    }

    public object VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new InterpreterEnvironment(InterpreterEnvironment));
        return null!;
    }

    public object VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null!;
    }

    public object VisitPrintStmt(Stmt.Print stmt)
    {
        object value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null!;
    }

    public object VisitLetStmt(Stmt.Let stmt)
    {
        object value = null!;
        if (stmt.Initializer != null)
            value = Evaluate(stmt.Initializer);

        InterpreterEnvironment.Define(stmt.Name, value);
        return null!;
    }

    public object VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Conditional)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }

        return null!;
    }

    public object VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new VyxFunction(stmt);
        InterpreterEnvironment.Define(stmt.Name.Lexeme(), function);
        return null!;
    }

    public object VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }
        return null!;
    }

    private object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public void ExecuteBlock(List<Stmt> statements, InterpreterEnvironment environment)
    {
        InterpreterEnvironment previous = InterpreterEnvironment;

        try
        {
            InterpreterEnvironment = environment;

            foreach (Stmt statement in statements)
                Execute(statement);
        }
        finally
        {
            InterpreterEnvironment = previous;
        }
    }

    private static bool IsTruthy(object obj)
    {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }

    private static bool IsEqual(object a, object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private static string Stringify(object obj)
    {
        if (obj == null) return "null";
        if (obj is double d)
        {
            string text = d.ToString();
            if (text.EndsWith(".0")) text = text.Substring(0, text.Length - 2);
            return text;
        }

        return obj.ToString()!;
    }

    private static void CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token op, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(op, "Operands must be numbers.");
    }
}
