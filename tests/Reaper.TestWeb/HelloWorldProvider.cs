namespace Reaper.TestWeb;

public class HelloWorldProvider
{
    public const string HelloWorld = "Hello, World! Counter: ";

    private int counter = 0;
    
    public string GetHelloWorld()
    {
        return HelloWorld + ++counter;
    }
}