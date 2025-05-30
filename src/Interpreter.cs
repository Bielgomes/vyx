namespace Vyx.src;

public class Interpreter(Expr expr) : Expr.IVisitor<Object>
{
    private readonly Expr expr = expr;

    public Object Interpret()
    {
        return Evaluate(expr);
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        Object left = Evaluate(expr.Left);
        Object right = Evaluate(expr.Right);

        switch (expr.Operator.Kind)
        {
            case TokenKind.Plus:
                if (left is double leftNum && right is double rightNum)
                    return leftNum + rightNum;
                if (left is string leftStr && right is string rightStr)
                    return leftStr + rightStr;

                break;
            case TokenKind.Minus:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left - (double)right;
            case TokenKind.Star:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left * (double)right;
            case TokenKind.Slash:
                CheckNumberOperands(expr.Operator, left, right);
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

    public object VisitTernaryExpr(Expr.Ternary expr)
    {
        Object condition = Evaluate(expr.Condition);

        return IsTruthy(condition) 
            ? Evaluate(expr.ThenBranch) 
            : Evaluate(expr.ElseBranch);
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        Object right = Evaluate(expr.Right);

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
        ;
    }

    private Object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private static bool IsTruthy(Object obj)
    {
        if (obj == null) return false;
        if (obj is bool v) return v;
        return true;
    }

    private static bool IsEqual(Object a, Object b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private static void CheckNumberOperand(Token @operator, Object operand)
    {
        if (operand is double) return;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token @operator, Object left, Object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(@operator, "Operands must be numbers.");
    }
}
