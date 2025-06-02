namespace Vyx.Vyx.Core;

public class Lexer(string source)
{
    public string Source { get; } = source;
    public List<Token> Tokens { get; } = [];
    public uint Current { get; private set; } = 0;
    public uint Column { get; private set; } = 0;
    public uint Line { get; private set; } = 1;
    static readonly Dictionary<string, TokenKind> Keywords = new()
    {
        { "or", TokenKind.Or },
        { "and", TokenKind.And },
        { "null", TokenKind.Null },
        { "let", TokenKind.Let },
        { "fn", TokenKind.Fn },
        { "true", TokenKind.True },
        { "false", TokenKind.False },
        { "print", TokenKind.Print },
        { "if", TokenKind.If },
        { "else", TokenKind.Else },
        { "while", TokenKind.While },
        { "for", TokenKind.For }
    };

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            char lexeme = Advance();
            switch (lexeme)
            {
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    Line++;
                    break;

                case '"':
                    HandleString();
                    break;

                case '+':
                    AddToken(TokenKind.Plus);
                    break;
                case '-':
                    AddToken(TokenKind.Minus);
                    break;
                case '*':
                    AddToken(TokenKind.Star);
                    break;
                case '/':
                    if (Match('/'))
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    else if (Match('*'))
                        while (!IsAtEnd())
                        {
                            if (Peek() == '*' && PeekNext() == '/')
                            {
                                Advance();
                                Advance();
                                break;
                            }

                            if (Peek() == '\n') Line++;
                            Advance();
                        }
                    else
                        AddToken(TokenKind.Slash);
                    break;

                case '!':
                    AddToken(Match('=') ? TokenKind.NotEqual : TokenKind.Not);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenKind.EqualsEqual : TokenKind.Equal);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenKind.GreaterThanEqual : TokenKind.GreaterThan);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenKind.LessThanEqual : TokenKind.LessThan);
                    break;

                case '(':
                    AddToken(TokenKind.Lparen);
                    break;
                case ')':
                    AddToken(TokenKind.Rparen);
                    break;
                case '{':
                    AddToken(TokenKind.Lbrace);
                    break;
                case '}':
                    AddToken(TokenKind.Rbrace);
                    break;
                case ':':
                    AddToken(TokenKind.Colon);
                    break;
                case ';':
                    AddToken(TokenKind.Semicolon);
                    break;
                case ',':
                    AddToken(TokenKind.Comma);
                    break;
                case '?':
                    AddToken(Match(':') ? TokenKind.Elvis : TokenKind.Question);
                    break;

                default:
                    if (IsDigit(lexeme))
                        HandleNumber();
                    else if (IsAlpha(lexeme))
                        HandleIdentifier();
                    else
                        Program.Error(Line, "Unexpected Character,");
                    break;
            }
        }

        AddToken(TokenKind.EOF);
        return Tokens;
    }

    private Position Position()
    {
        return new Position(Line, Column);
    }

    private void AddToken(TokenKind kind)
    {
        AddToken(kind, null);
    }

    private void AddToken(TokenKind kind, object? literal)
    {
        Tokens.Add(new Token(kind, literal, Position()));
    }

    private bool IsAtEnd()
    {
        return Column >= Source.Length;
    }

    private char Advance()
    {
        return Source.ElementAt((int)Column++);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (Source.ElementAt((int)Column) != expected) return false;

        Column++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return Source.ElementAt((int)Column);
    }

    private char PeekNext()
    {
        if (Column + 1 >= Source.Length) return '\0';
        return Source.ElementAt((int)Column + 1);
    }

    private static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    public static bool IsAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    public static bool IsAlphaNumeric(char c)
    {
        return IsAlpha(c) || IsDigit(c);
    }

    private void HandleString()
    {
        uint Start = Column;
        uint Length = 0;

        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') Line++;
            Advance();
            Length++;
        }

        if (IsAtEnd())
        {
            Program.Error(Line, "Unterminated String");
            return;
        }

        Advance();

        string value = Source.Substring((int)Start, (int)Length);
        AddToken(TokenKind.StringLiteral, value);
    }

    private void HandleNumber()
    {
        uint Start = Column;
        uint Length = 0;

        while (IsDigit(Peek()))
        {
            Advance();
            Length++;
        }

        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            Length++;

            while (IsDigit(Peek()))
            {
                Advance();
                Length++;
            }
        }

        double value = double.Parse(Source.Substring((int)Start - 1, (int)Length + 1));
        AddToken(TokenKind.Number, value);
    }

    public void HandleIdentifier()
    {
        uint Start = Column;
        uint Length = 0;

        while (IsAlphaNumeric(Peek()))
        {
            Advance();
            Length++;
        }

        string text = Source.Substring((int)Start - 1, (int)Length + 1);
        TokenKind? type = Keywords.GetValueOrDefault(text);
        type ??= TokenKind.Identifier;
        AddToken((TokenKind) type, type == TokenKind.Identifier ? text : null);
    }
}
