using BenScr.Random;

public static class Program
{
    public static void Main()
    {
        RandomCS randomCS = new RandomCS();

        int count = 0;

        for (int i = 0; i < 1000000; i++)
        {
            count += randomCS.Next<int>(1, 7);
        }

        int avg = count /= 1000000;

        Console.WriteLine(avg);

        Console.WriteLine(randomCS.Next<int>(0, 5));
        Console.WriteLine(randomCS.Next<bool>());
        Console.WriteLine(randomCS.Next<string>());
        Console.WriteLine(RandomHandler.Next<byte>());
    }
}