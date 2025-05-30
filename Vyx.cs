using Vyx.src;

namespace Vyx;

class Vyx
{
    private static bool HasError = false;

    public static void Main(string[] args)
    {
        if (args.Length > 1)
            Console.WriteLine("Run: dotnet run [script]");
        else if (args.Length == 1) {
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
        }
    }

    private static void Run(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        if (HasError) return;

        var parser = new Parser(tokens);
        Expr ats = parser.Parse();

        if (HasError) return;

        var interpreter = new Interpreter(ats);
        Console.WriteLine(interpreter.Interpret());
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
}
