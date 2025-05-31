namespace Vyx.src;

public class Parser(List<Token> tokens)
{
    private readonly List<Token> Tokens = tokens;
    public uint Position { get; private set; } = 0;

    private class ParseError : Exception {}

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Kind == TokenKind.Semicolon) return;

            switch (Peek().Kind)
            {
                case TokenKind.Let:
                case TokenKind.Print:
                case TokenKind.Fn:
                    return;
            }

            Advance();
        }
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match([TokenKind.Let])) return LetDeclaration();
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null!;
        }
    }

    private Stmt LetDeclaration()
    {
        Token name = Consume(TokenKind.Identifier, "Expected variable name after 'let'.");

        Expr initializer = null!;
        if (Match([TokenKind.Equal]))
        {
            initializer = ParseExpression();
        }

        Consume(TokenKind.Semicolon, "Expected ';' after variable declaration.");
        return new Stmt.Let(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match([TokenKind.Print])) return PrintStatement();
        if (Match([TokenKind.Lbrace])) return BlockStatement();

        return ExpressionStatement();
    }

    private Stmt PrintStatement()
    {
        Consume(TokenKind.Lparen, "Expected '(' after 'print'.");
        Expr value = ParseExpression();
        Consume(TokenKind.Rparen, "Expected ')' after expression.");
        Consume(TokenKind.Semicolon, "Expected ';' after print statement.");
        return new Stmt.Print(value);
    }

    private Stmt BlockStatement()
    {
        var statements = new List<Stmt>();

        while (!Check(TokenKind.Rbrace) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenKind.Rbrace, "Expected '}' after block.");
        return new Stmt.Block(statements);
    }

    private Stmt ExpressionStatement()
    {
        Expr expr = ParseExpression();
        Consume(TokenKind.Semicolon, "Expected ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr ParseExpression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        Expr expr = ParseElvis();

        if (Match([TokenKind.Equal]))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable variable)
            {
                return new Expr.Assign(variable.Name, value);
            }

            Error(equals, "Invalid assignment target. Expected variable.");
        }

        return expr;
    }

    private Expr ParseElvis()
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

    private Expr ParseTernary()
    {
        Expr expr = ParseEquality();

        while (Match([TokenKind.Question]))
        {
            Expr thenExpr = ParseExpression();
            Consume(TokenKind.Colon, "Expected ':' after true expression.");
            Expr elseBranch = ParseExpression();
            expr = new Expr.Ternary(expr, thenExpr, elseBranch);
        }

        return expr;
    }

    private Expr ParseEquality()
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

    private Expr ParseComparison()
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

    private Expr ParseTerm()
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

    private Expr ParseFactor()
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

    private Expr ParseUnary()
    {
        if (Match([TokenKind.Not, TokenKind.Minus]))
        {
            Token op = Previous();
            Expr right = ParsePrimary();

            return new Expr.Unary(op, right);
        }

        return ParsePrimary();
    }

    private Expr ParsePrimary()
    {
        if (Match([TokenKind.False])) return new Expr.Literal(false);
        if (Match([TokenKind.True])) return new Expr.Literal(true);
        if (Match([TokenKind.Null])) return new Expr.Literal(null!);

        if (Match([TokenKind.Number, TokenKind.StringLiteral]))
            return new Expr.Literal(Previous().Literal!);

        if (Match([TokenKind.Identifier]))
            return new Expr.Variable(Previous());

        if (Match([TokenKind.Lparen]))
            {
                Expr expression = ParseExpression();
                Consume(TokenKind.Rparen, "Expected ')' after expression.");
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
