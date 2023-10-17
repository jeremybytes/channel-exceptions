using System.Threading.Channels;

public class Program
{
    private static Random Randomizer { get; } = new Random();
    private static int TotalProduced = 0;
    private static int TotalConsumed = 0;

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

        Console.WriteLine($"Total Produced: {TotalProduced}");
        Console.WriteLine($"Total Consumed: {TotalConsumed}");
        Console.WriteLine("Done");
    }

    static async Task ProducerConsumerWithExceptions()
    {
        var channel = Channel.CreateBounded<int>(10);
        var errorChannel = Channel.CreateBounded<int>(10);

        Task producer = Producer(channel.Writer);
        Task consumer = Consumer(channel.Reader, errorChannel.Writer);
        Task errorProcessor = ProcessErrors(errorChannel.Reader);

        await Task.WhenAll(producer, consumer, errorProcessor);
    }

    static async Task Producer(ChannelWriter<int> writer)
    {
        for (int i = 0; i < 100; i++)
        {
            try
            {
                Console.WriteLine($"Producing something: {i}");
                MightThrowExceptionForProducer(i);
                await Task.Delay(10);
                TotalProduced++;
                await writer.WriteAsync(i);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logged: {ex.Message}");
            }
        }

        writer.Complete();
    }

    static async Task Consumer(ChannelReader<int> reader, ChannelWriter<int> errorWriter)
    {
        try
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                try
                {
                    Console.WriteLine($"Consuming object: {item}");
                    MightThrowExceptionForConsumer(item);
                    TotalConsumed++;
                }
                catch (Exception ex)
                {
                    _ = errorWriter.WriteAsync(item);
                    Console.WriteLine($"Logged: {ex.Message}");
                }
            }
        }
        finally
        {
            errorWriter.Complete();
        }
    }

    static async Task ProcessErrors(ChannelReader<int> errorReader)
    {
        await foreach(var item in errorReader.ReadAllAsync())
        {
            for (int iteration = 0; iteration < 3; iteration++)
            {
                try
                {
                    Console.WriteLine($"Retrying object ({iteration}): {item}");
                    MightThrowExceptionForErrorProcessor(item);
                    TotalConsumed++;
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logged ({iteration}): {ex.Message}");
                    if (iteration == 2) // failed on last chance
                        Console.WriteLine($"Logged ({iteration}): Failed to processes item");
                }
            }
        }
    }

    private static void MightThrowExceptionForProducer(int item)
    {
        //if (Randomizer.Next() % 3 == 0)
        //    throw new Exception($"Bad thing happened in Producer ({item})");
    }

    private static void MightThrowExceptionForConsumer(int item)
    {
        if (Randomizer.Next() % 10 == 0)
            throw new Exception($"Bad thing happened in Consumer ({item})");
    }

    private static void MightThrowExceptionForErrorProcessor(int item)
    {
        if (Randomizer.Next() % 5 == 0)
            throw new Exception($"Bad thing happened in Error Processor ({item})");
    }
}