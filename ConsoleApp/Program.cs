namespace ConsoleApp;

public static partial class Program
{
    private static readonly MyDisposable MyDisposable = new MyDisposable();
    
    private static void Main(string[] args)
    {
        HelloFrom("Generated Code");
        var f = new MyImplementation();
        Console.WriteLine(f.ToString());
    }

    static partial void HelloFrom(string name);
}