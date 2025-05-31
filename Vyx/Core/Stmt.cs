namespace Vyx.Core;

public abstract class Stmt
{
    public interface IVisitor<R>
    {
        R VisitBlockStmt(Block block);
        R VisitExpressionStmt(Expression stmt);
        R VisitPrintStmt(Print stmt);
        R VisitLetStmt(Let stmt);
        R VisitIfStmt(If stmt);
        R VisitWhileStmt(While stmt);
    }

    abstract public R Accept<R>(IVisitor<R> visitor);


    public class Block(List<Stmt> statements) : Stmt
    {
        public List<Stmt> Statements { get; private set; } = statements;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitBlockStmt(this);
        }
    }

    public class Expression(Expr expr) : Stmt
    {
        public Expr Expr { get; private set; } = expr;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitExpressionStmt(this);
        }
    }

    public class Print(Expr expr) : Stmt
    {
        public Expr Expr { get; private set; } = expr;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitPrintStmt(this);
        }
    }

    public class Let(Token name, Expr initializer) : Stmt
    {
        public Token Name { get; private set; } = name;
        public Expr Initializer { get; private set; } = initializer;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitLetStmt(this);
        }
    }

    public class If(Expr conditional, Stmt thenBranch, Stmt elseBranch) : Stmt
    {
        public Expr Conditional { get; private set; } = conditional;
        public Stmt ThenBranch { get; private set; } = thenBranch;
        public Stmt ElseBranch { get; private set; } = elseBranch;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitIfStmt(this);
        }
    }

    public class While(Expr condition, Stmt body) : Stmt
    {
        public Expr Condition { get; private set; } = condition;
        public Stmt Body { get; private set; } = body;

        public override R Accept<R>(IVisitor<R> visitor)
        {
            return visitor.VisitWhileStmt(this);
        }
    }
}
