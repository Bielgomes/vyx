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
    Print,

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
    Lbrace,
    Rbrace,
    Colon,
    Semicolon,
    Question,
    Elvis,

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
            TokenKind.Identifier => Literal as string ?? string.Empty,
            TokenKind.Number => Literal?.ToString() ?? string.Empty,
            TokenKind.StringLiteral => Literal as string ?? string.Empty,
            TokenKind.Plus => "+",
            TokenKind.Minus => "-",
            TokenKind.Star => "*",
            TokenKind.Slash => "/",
            TokenKind.Or => "or",
            TokenKind.And => "and",
            TokenKind.Null => "null",
            TokenKind.Let => "let",
            TokenKind.Fn => "fn",
            TokenKind.True => "true",
            TokenKind.False => "false",
            TokenKind.Print => "print",
            TokenKind.Equal => "=",
            TokenKind.Not => "not",
            TokenKind.GreaterThan => ">",
            TokenKind.LessThan => "<",
            TokenKind.EqualsEqual => "==",
            TokenKind.NotEqual => "!=",
            TokenKind.GreaterThanEqual => ">=",
            TokenKind.LessThanEqual => "<=",
            TokenKind.Lparen => "(",
            TokenKind.Rparen => ")",
            TokenKind.Lbrace => "{",
            TokenKind.Rbrace => "}",
            TokenKind.Colon => ":",
            TokenKind.Semicolon => ";",
            TokenKind.Question => "?",
            TokenKind.Elvis => "?:",
            TokenKind.EOF => "EOF",
            _ => Kind.ToString()
        };
    }
}
