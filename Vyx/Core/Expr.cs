namespace Vyx.Vyx.Core;

public abstract class Expr
{
    public interface IVisitor<R>
    {
        R VisitLiteralExpr(Literal expr);
        R VisitCallExpr(Call expr);
        R VisitGroupingExpr(Grouping expr);
        R VisitLogicalExpr(Logical expr);
        R VisitUnaryExpr(Unary expr);
        R VisitAssignExpr(Assign expr);
        R VisitBinaryExpr(Binary expr);
        R VisitTernaryExpr(Ternary expr);
        R VisitVariableExpr(Variable expr);
    }

    abstract public R Accept<R>(IVisitor<R> visitor);

    public class Literal(object value) : Expr
    {
        public object Value { get; private set; } = value;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class Call(Expr callee, Token paren, List<Expr> arguments) : Expr
    {
        public Expr Calle { get; private set; } = callee;
        public Token Paren { get; private set; } = paren;
        public List<Expr> Arguments { get; private set; } = arguments;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitCallExpr(this);
        }
    }

    public class Grouping(Expr expression) : Expr
    {
        public Expr Expression { get; private set; } = expression;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class Logical(Expr left, Token op, Expr right) : Expr
    {
        public Expr Left { get; private set; } = left;
        public Token Operator { get; private set; } = op;
        public Expr Right { get; private set; } = right;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLogicalExpr(this);
        }
    }

    public class Unary(Token op, Expr right) : Expr
    {
        public Token Operator { get; private set; } = op;
        public Expr Right { get; private set; } = right;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }

    public class Assign(Token name, Expr value) : Expr
    {
        public Token Name { get; private set; } = name;
        public Expr Value { get; private set; } = value;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitAssignExpr(this);
        }
    }

    public class Binary(Token op, Expr left, Expr right) : Expr
    {
        public Expr Left { get; private set; } = left;
        public Token Operator { get; private set; } = op;
        public Expr Right { get; private set; } = right;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    public class Ternary(Expr condition, Expr thenBranch, Expr elseBranch) : Expr
    {
        public Expr Condition { get; private set; } = condition;
        public Expr ThenBranch { get; private set; } = thenBranch;
        public Expr ElseBranch { get; private set; } = elseBranch;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitTernaryExpr(this);
        }
    }

    public class Variable(Token name) : Expr
    {
        public Token Name { get; private set; } = name;

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitVariableExpr(this);
        }
    }
}
