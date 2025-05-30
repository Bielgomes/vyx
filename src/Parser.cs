namespace Vyx.src;

public class Parser(List<Token> tokens)
{
    private readonly List<Token> Tokens = tokens;
    public uint Position { get; private set; } = 0;

    private class ParseError : Exception {}

    public Expr Parse()
    {
        try
        {
            return ParseExpression();
        }
        catch (ParseError)
        {
            return null!;
        }
    }

    public Expr ParseExpression()
    {
        Expr expr = ParseElvis();
        return expr;
    }

    public Expr ParseElvis()
    {
        Expr expr = ParseTernary();

        if (Match([TokenKind.Elvis]))
        {
            Token op = Previous();
            Expr right = ParseTernary();

            expr = new Expr.Binary(op, expr, right);
        }

        return expr;
    }

    public Expr ParseTernary()
    {
        Expr expr = ParseEquality();

        while (Match([TokenKind.Question]))
        {
            Expr thenExpr = ParseExpression();
            Consume(TokenKind.Colon, "Expected ':' after true expression");
            Expr elseBranch = ParseExpression();
            expr = new Expr.Ternary(expr, thenExpr, elseBranch);
        }

        return expr;
    }

    public Expr ParseEquality()
    {
        Expr expr = ParseComparison();

        while (Match([TokenKind.NotEqual, TokenKind.EqualsEqual]))
        {
            Token op = Previous();
            Expr right = ParseComparison();

            expr = new Expr.Binary(op, expr, right);
        }

        return expr;
    }

    public Expr ParseComparison()
    {
        Expr expr = ParseTerm();

        while (Match([TokenKind.GreaterThan, TokenKind.LessThan, TokenKind.GreaterThanEqual, TokenKind.LessThanEqual]))
        {
            Token op = Previous();
            Expr right = ParseTerm();

            expr = new Expr.Binary(op, expr, right);
        }

        return expr;
    }

    public Expr ParseTerm()
    {
        Expr expr = ParseFactor();

        while (Match([TokenKind.Plus, TokenKind.Minus]))
        {
            Token op = Previous();
            Expr right = ParseFactor();

            expr = new Expr.Binary(op, expr, right);
        }

        return expr;
    }

    public Expr ParseFactor()
    {
        Expr expr = ParseUnary();

        while (Match([TokenKind.Slash, TokenKind.Star]))
        {
            Token op = Previous();
            Expr right = ParseUnary();

            expr = new Expr.Binary(op, expr, right);
        }

        return expr;
    }

    public Expr ParseUnary()
    {
        if (Match([TokenKind.Not, TokenKind.Minus]))
        {
            Token op = Previous();
            Expr right = ParsePrimary();

            return new Expr.Unary(op, right);
        }

        return ParsePrimary();
    }

    public Expr ParsePrimary()
    {
        if (Match([TokenKind.False])) return new Expr.Literal(false);
        if (Match([TokenKind.True])) return new Expr.Literal(true);
        if (Match([TokenKind.Null])) return new Expr.Literal(null!);

        if (Match([TokenKind.Number, TokenKind.StringLiteral]))
            return new Expr.Literal(Previous().Literal!);

        if (Match([TokenKind.Lparen]))
        {
            Expr expression = ParseExpression();
            Consume(TokenKind.Rparen, "Expected ')' after expression");
            return new Expr.Grouping(expression);
        }

        throw Error(Peek(), "Expect expression.");
    }

    private bool IsAtEnd()
    {
        return Peek().Kind == TokenKind.EOF;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) Position++;
        return Previous();
    }

    private Token Peek()
    {
        return Tokens.ElementAt((int)Position);
    }

    private bool Check(TokenKind kind)
    {
        if (IsAtEnd()) return false;
        return Peek().Kind == kind;
    }

    private bool Match(TokenKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            if (Check(kind))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Previous()
    {
        return Tokens.ElementAt((int)(Position - 1));
    }

    private Token Consume(TokenKind kind, string message)
    {
        if (Check(kind)) return Advance();
        throw Error(Peek(), message);
    }

    private static ParseError Error(Token token, string message)
    {
        Vyx.Error(token, message);
        return new ParseError();
    }
}
