using System.Security.Cryptography.X509Certificates;

namespace Vyx.src;

public abstract class Stmt
{
    public interface IVisitor<R>
    {
        R VisitExpressionStmt(Expression stmt);
        R VisitPrintStmt(Print stmt);
        R VisitLetStmt(Let stmt);
    }

    abstract public R Accept<R>(IVisitor<R> visitor);

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
}
