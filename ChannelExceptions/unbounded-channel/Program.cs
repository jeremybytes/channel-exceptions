using System.Threading.Channels;

public class Program
{
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
    }

    static async Task ProducerConsumerWithExceptions()
    {
        var channel = Channel.CreateUnbounded<int>();

        Task producer = Producer(channel.Writer);
        Task consumer = Consumer(channel.Reader);

        await Task.WhenAll(producer, consumer);
    }

    static async Task Producer(ChannelWriter<int> writer)
    {
        try
        {
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine($"Producing something: {i}");
                MightThrowExceptionForProducer(i);
                await Task.Delay(10);
                await writer.WriteAsync(i);
            }
        }
        finally
        {
            writer.Complete();
        }
    }

    static async Task Consumer(ChannelReader<int> reader)
    {
        await foreach (var item in reader.ReadAllAsync())
        {
            Console.WriteLine($"Consuming object: {item}");
            MightThrowExceptionForConsumer(item);
        }
    }

    private static void MightThrowExceptionForProducer(int item)
    {
        //throw new Exception($"Bad thing happened in Producer ({item})");
    }

    private static void MightThrowExceptionForConsumer(int item)
    {
        throw new Exception($"Bad thing happened in Consumer ({item})");
    }
}