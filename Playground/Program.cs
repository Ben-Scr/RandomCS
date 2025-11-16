using BenScr.Random;

RandomCS randomCS = new RandomCS();
Console.WriteLine(randomCS.Next<int>(0, 5));
Console.WriteLine(randomCS.Next<bool>());
Console.WriteLine(randomCS.Next<string>());
Console.WriteLine(RandomHandler.Next<byte>());