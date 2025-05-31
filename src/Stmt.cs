namespace Vyx.src;

public abstract class Stmt
{
    public interface IVisitor<R>
    {
        R VisitExpressionStmt(Expression stmt);
        R VisitPrintStmt(Print stmt);
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
}
