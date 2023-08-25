using System.Threading.Channels;

public class Program
{
    private static Random Randomizer { get; } = new Random();

    static async Task Main(string[] args)
    {
        try
        {
            await ProducerConsumerWithExceptions();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("Done");
        Console.ReadLine();
    }

    static async Task ProducerConsumerWithExceptions()
    {
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(10));

        Task producer = Producer(channel.Writer);
        Task consumer = Consumer(channel.Reader);

        await Task.WhenAll(producer, consumer);
    }

    static async Task Producer(ChannelWriter<int> writer)
    {
        for (int i = 0; i < 20; i++)
        {
            try
            {
                MightThrowExceptionForProducer();
                Console.WriteLine($"Producing something");
                await Task.Delay(10);
                await writer.WriteAsync(i);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logged: {ex.Message}");
            }
        }

        writer.Complete();
    }

    static async Task Consumer(ChannelReader<int> reader)
    {
        await foreach (var item in reader.ReadAllAsync())
        {
            try
            {
                MightThrowExceptionForConsumer();
                Console.WriteLine($"Consuming object: {item}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logged: {ex.Message}");
            }
        }
    }

    private static void MightThrowExceptionForProducer()
    {
        if (Randomizer.Next(11) % 3 == 0)
            throw new Exception("Bad thing happened in Producer");
    }

    private static void MightThrowExceptionForConsumer()
    {
        if (Randomizer.Next(6) % 3 == 0)
            throw new Exception("Bad thing happened in Consumer");
    }
}