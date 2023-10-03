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
            Console.WriteLine(ex.ToString());
        }
    }

    static async Task ProducerConsumerWithExceptions()
    {
        var channel = Channel.CreateBounded<object>(new BoundedChannelOptions(10));
        async Task Producer()
        {
            try
            {
                for (int i = 0; i < 100; i++)
                {
                    DoSomethingThatMightThrowExceptionForProducer();
                    await channel.Writer.WriteAsync(i);
                    await Task.Delay(10);
                }
            }
            finally
            {
                channel.Writer.Complete();
            }
        }
        async Task Consumer()
        {
            await foreach (var anObject in channel.Reader.ReadAllAsync())
            {
                DoSomethingThatMightThrowExceptionForConsumer(anObject);
            }
        }

        await Task.WhenAll(Producer(), Consumer());
    }

    private static void DoSomethingThatMightThrowExceptionForConsumer(object o)
    {
        Console.WriteLine($"Consuming object: {o}");
        throw new Exception("Bad thing happened in Consumer");
    }

    private static void DoSomethingThatMightThrowExceptionForProducer()
    {
        Console.WriteLine($"Producing something");
        //throw new Exception("Bad thing happened in Producer");
    }
}