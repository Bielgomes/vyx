namespace Vyx.src;

public enum TokenKind
{
    Identifier,
    Number,
    StringLiteral,

    Plus,
    Minus,
    Star,
    Slash,

    Or,
    And,
    Null,
    Let,
    Fn,
    True,
    False,

    Equal,
    Not,
    GreaterThan,
    LessThan,
    EqualsEqual,
    NotEqual,
    GreaterThanEqual,
    LessThanEqual,

    Lparen,
    Rparen,
    Colon,
    Question,

    EOF
}

public class Position(uint line, uint column)
{
    public uint Line { get; } = line;
    public uint Column { get; } = column;

    public override string ToString()
    {
        return $"[Line: {Line}, Column: {Column}]";
    }
}

public class Token(TokenKind kind, object? literal, Position position)
{
    public TokenKind Kind { get; } = kind;
    public object? Literal { get; } = literal;
    public Position Position { get; } = position;

    public string Lexeme()
    {
        return Kind switch
        {

            TokenKind.Plus => "+",
            TokenKind.Minus => "-",
            TokenKind.Star => "*",
            TokenKind.Slash => "/",
            TokenKind.Equal => "=",
            TokenKind.Not => "!",
            TokenKind.GreaterThan => ">",
            TokenKind.LessThan => "<",
            TokenKind.EqualsEqual => "==",
            TokenKind.NotEqual => "!=",
            TokenKind.GreaterThanEqual => ">=",
            TokenKind.LessThanEqual => "<=",
            _ => ""
        };
    }
}
