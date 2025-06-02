namespace Vyx.Vyx.Core;

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
            if (Match([TokenKind.Fn])) return Function("function");
            if (Match([TokenKind.Let])) return LetDeclaration();
            return Statement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null!;
        }
    }

    private Stmt.Function Function(string kind)
    {
        Token name = Consume(TokenKind.Identifier, $"Expected {kind} name.");
        Consume(TokenKind.Lparen, "Expected '(' after function");
        var parameters = new List<Token>();

        if (!Check(TokenKind.Rparen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(
                    Consume(TokenKind.Identifier, "Expected parameter name.")
                );
            } while (Match([TokenKind.Comma]));
        }
        Consume(TokenKind.Rparen, "Expected ')' after parameters.");
        Consume(TokenKind.Lbrace, "Expected '{' before " + kind + " body.");
        var body = BlockStatement();
        return new Stmt.Function(name, parameters, body.Statements);
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
        if (Match([TokenKind.For])) return ForStatement();
        if (Match([TokenKind.If])) return IfStatement();
        if (Match([TokenKind.Print])) return PrintStatement();
        if (Match([TokenKind.While])) return WhileStatement();
        if (Match([TokenKind.Lbrace])) return BlockStatement();

        return ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(TokenKind.Lparen, "Expected '(' after 'for'.");

        Stmt initializer;
        if (Match([TokenKind.Semicolon]))
        {
            initializer = null!;
        }
        else if (Match([TokenKind.Let]))
        {
            initializer = LetDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr condition = null!;
        if (!Check(TokenKind.Semicolon))
        {
            condition = ParseExpression();
        }

        Consume(TokenKind.Semicolon, "Expected ';' after loop condition.");

        Expr increment = null!;
        if (!Check(TokenKind.Rparen))
        {
            increment = ParseExpression();
        }
        Consume(TokenKind.Rparen, "Expected ')' after for clauses.");

        Stmt body = Statement();
        if (increment != null)
        {
            body = new Stmt.Block(
                [body, new Stmt.Expression(increment)]
            );
        }

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block(
                [initializer, body]
            );
        }

        return body;
    }

    private Stmt IfStatement()
    {
        Consume(TokenKind.Lparen, "Expected '(' after 'if'.");
        Expr conditional = ParseExpression();
        Consume(TokenKind.Rparen, "Expected ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt elseBranch = null!;
        if (Match([TokenKind.Else]))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(conditional, thenBranch, elseBranch);
    }

    private Stmt PrintStatement()
    {
        Consume(TokenKind.Lparen, "Expected '(' after 'print'.");
        Expr value = ParseExpression();
        Consume(TokenKind.Rparen, "Expected ')' after expression.");
        Consume(TokenKind.Semicolon, "Expected ';' after print statement.");
        return new Stmt.Print(value);
    }

    private Stmt WhileStatement()
    {
        Consume(TokenKind.Lparen, "Expected '(' after 'print'.");
        Expr condition = ParseExpression();
        Consume(TokenKind.Rparen, "Expected ')' after expression.");
        Stmt body = Statement();

        return new Stmt.While(condition, body);
    }

    private Stmt.Block BlockStatement()
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
        Expr expr = ParseOr();

        while (Match([TokenKind.Question]))
        {
            Expr thenExpr = ParseExpression();
            Consume(TokenKind.Colon, "Expected ':' after true expression.");
            Expr elseBranch = ParseExpression();
            expr = new Expr.Ternary(expr, thenExpr, elseBranch);
        }

        return expr;
    }

    private Expr ParseOr()
    {
        Expr expr = ParseAnd();

        if (Match([TokenKind.Or]))
        {
            Token op = Previous();
            Expr right = ParseAnd();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr ParseAnd()
    {
        Expr expr = ParseEquality();

        if (Match([TokenKind.And]))
        {
            Token op = Previous();
            Expr right = ParseAnd();
            expr = new Expr.Logical(expr, op, right);
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

        return ParseCall();
    }

    private Expr ParseCall()
    {
        Expr expr = ParsePrimary();

        while (true)
        {
            if (Match([TokenKind.Lparen]))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();
        if (!Check(TokenKind.Rparen)) {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments");
                }
                arguments.Add(ParseExpression());
            } while (Match([TokenKind.Comma]));
        }

        Token paren = Consume(TokenKind.Rparen, "Expected ')' after arguments.");
        return new Expr.Call(callee, paren, arguments);
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
        Program.Error(token, message);
        return new ParseError();
    }
}
