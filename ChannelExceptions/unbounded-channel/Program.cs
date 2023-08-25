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
        Console.ReadLine();
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
            for (int i = 0; i < 100; i++)
            {
                MightThrowExceptionForProducer();
                Console.WriteLine($"Producing something");
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
            MightThrowExceptionForConsumer();
            Console.WriteLine($"Consuming object: {item}");
        }
    }

    private static void MightThrowExceptionForProducer()
    {
        //throw new Exception("Bad thing happened in Producer");
    }

    private static void MightThrowExceptionForConsumer()
    {
        throw new Exception("Bad thing happened in Consumer");
    }
}