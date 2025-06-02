namespace Vyx.Vyx.Core;

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
    If,
    Else,
    While,
    For,

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
    Comma,
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
            TokenKind.Equal => "=",
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
            TokenKind.Comma => ",",
            TokenKind.Semicolon => ";",
            TokenKind.Question => "?",
            TokenKind.Elvis => "?:",
            TokenKind.EOF => "EOF",
            _ => Kind.ToString().ToLower()
        };
    }
}
