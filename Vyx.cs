using Vyx.src;

namespace Vyx;

class Vyx
{
    private static readonly Interpreter Interpreter = new();
    static bool HasError = false;
    static bool HadRuntimeError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
            Console.WriteLine("Run: dotnet run [script]");
        else if (args.Length == 1)
        {
            if (args[0].Split('.').Last() != "vyx")
                Console.WriteLine("Error: File must have a .vyx extension.");
            else if (!File.Exists(args[0]))
                Console.WriteLine($"Error: File '{args[0]}' does not exist.");
            else
                RunFile(args[0]);
        }
        else
            Prompt();
    }

    private static void RunFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        Run(System.Text.Encoding.UTF8.GetString(bytes));
        if (HasError) Environment.Exit(0);
        if (HadRuntimeError) Environment.Exit(1);
    }

    private static void Prompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == "" || line == null) { break; }
            Run(line);
            HasError = false;
            HadRuntimeError = false;
        }
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        if (HasError) return;

        var parser = new Parser(tokens);
        var expr = parser.Parse();

        if (HasError) return;

        Interpreter.Interpret(expr);
    }

    internal static void Error(uint line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(uint line, string where, string message)
    {
        Console.WriteLine($"[Line {line}] Error {where}: {message}");
        HasError = true;
    }

    internal static void Error(Token token, string message)
    {
        if (token.Kind == TokenKind.EOF)
            Report(token.Position.Line, $"at end", message);
        else
            Report(token.Position.Line, $"at '{token.Lexeme()}'", message);
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine($"{error.Token.Position.ToString()} {error.Message}");
        HadRuntimeError = true;
    }
}
