﻿using System.Threading.Channels;

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
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(10));

        Task producer = Producer(channel.Writer);
        Task consumer = Consumer(channel.Reader);

        await Task.WhenAll(producer, consumer);
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
                Interlocked.Increment(ref TotalProduced);
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
                Console.WriteLine($"Consuming object: {item}");
                MightThrowExceptionForConsumer(item);
                Interlocked.Increment(ref TotalConsumed);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logged: {ex.Message}");
            }
        }
    }

    private static void MightThrowExceptionForProducer(int item)
    {
        if (Randomizer.Next() % 3 == 0)
            throw new Exception($"Bad thing happened in Producer ({item})");
    }

    private static void MightThrowExceptionForConsumer(int item)
    {
        if (Randomizer.Next() % 50 == 0)
            throw new Exception($"Bad thing happened in Consumer ({item})");
    }
}