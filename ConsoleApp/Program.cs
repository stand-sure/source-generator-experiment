namespace ConsoleApp;

public static partial class Program
{
    private static void Main(string[] args)
    {
        HelloFrom("Generated Code");
        Console.WriteLine("end");
    }

    static partial void HelloFrom(string name);
}