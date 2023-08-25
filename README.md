# Channel Exceptions  

Exploring exception handling with C# channels. This came from a questions from somone who attended my presentation ([Better Parallel Code with C# Channels](https://github.com/jeremybytes/csharp-channels-presentation)) and was exploring code with other programmers.  

*The original sample is provided by Edington Watt and the WTW ICT Technology team.*  

**This code is still in the experimental state, and the notes below are still rough. A full write-up is in the works.**  

If you want more information on Channels, you can look at the code, slides, and articles that are part of the presentation mentioned above: [Better Parallel Code with C# Channels](https://github.com/jeremybytes/csharp-channels-presentation)

## Projects

* **original-code**  
[original-code/Program.cs](./ChannelExceptions/original-code/Program.cs)  
The original sample provided by Edington Watt and the WTW ITC Technology team. They had a few concerns with the code. One concern is that if the consumer throws an exception, then the application hangs. Another concern: an exception thrown in the producer or consumer is short-circuiting, meaning that all processing stops.  

* **refactored**  
[refactored/Program.cs](./ChannelExceptions/refactored/Program.cs)  
I refactored the code a bit by changing local methods to class methods and extracting some code into additional methods. This separation was mainly for my own benefit (it's more along the line of the way my brain likes to work). The behavior is the same. When the consumer throws an exception, then the application hangs. An exception will cause processing to stop.  

* **separate-await**  
[separate-await/Program.cs](./ChannelExceptions/separate-await/Program.cs)  
This code uses separate "await"s for the consumer and producer. This raises an exception where "WhenAll" does not.  
*Note: WhenAll does not raise an exception because the producer has not yet finished.*  
By experimenting with the order of the "await"s and which exceptions are thrown, you can see how order can be important.

* **unbounded-channel**  
[unbounded-channel/Program.cs](./ChannelExceptions/unbounded-channel/Program.cs)  
This code uses an unbounded channel instead of a bounded channel. When the consumer throws an exception, this allows the producer to complete so that the application does not hang. An exception will still short-circuit the producer or consumer.  

* **doesnt-stop**  
[doesnt-stop/Program.cs](./ChannelExceptions/doesnt-stop/Program.cs)  
This code uses try/catch blocks inside the loops of the producer and consumer. This allows the process to continue even if individual operations fail. Exceptions are randomly thrown in both the producer and consumer and are "logged" to the console.  

## Behavior & Analysis

### original-code
[original-code/Program.cs](./ChannelExceptions/original-code/Program.cs)  

Relevant code:
```c#
var channel = Channel.CreateBounded<object>(new BoundedChannelOptions(10));
```
```c#
await channel.Writer.WriteAsync(i);
```
The bounded channel limits the channel to only holding 10 items. When the channel is full, calls to "await WriteAsync()" will wait until there is space available in the channel.  

```c#
await Task.WhenAll(Producer(), Consumer());
```
The "await WhenAll()" will wait until both the Producer task and the Consumer task have completed. Since the Producer ends up waiting indefinitely when the channel is full, the application will hang on this line.

```c#
finally
{
   channel.Writer.Complete();
}
```
The producer marks the channel writer as "Complete" in a finally block. This means that if the producer throws an exception, the channel is closed. The consumer will stop waiting for items and the consumer task will finish.

**Output with no exceptions**  
Produces and consumes 100 items.

```
Producing something
Consuming object: 0
Producing something
Consuming object: 1
Producing something
Consuming object: 2
Producing something
Consuming object: 3
...
Consuming object: 97
Producing something
Consuming object: 98
Producing something
Consuming object: 99
```

**Output with Producer exception**  
All processing stops when the producer throws an exception on the first item.  

```
Producing something
System.Exception: Bad thing happened in Producer
   at Program.DoSomethingThatMightThrowExceptionForProducer() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 58
   at Program.<>c__DisplayClass4_0.<<ProducerConsumerWithExceptions>g__Producer|0>d.MoveNext() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 28
--- End of stack trace from previous location ---
   at Program.ProducerConsumerWithExceptions() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 46
   at Program.Main(String[] args) in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 10
```

The producer task finishes when the exception is thrown. Since the producer marks the channel as "Complete", the consumer task also finishes. The "await WhenAll" will throw the exception from the producer.

**Output with Consumer exception**  
Application hangs when the consumer throws an exception on the first item.

```
Producing something
Consuming object: 0
Producing something
Producing something
Producing something
Producing something
Producing something
Producing something
Producing something
Producing something
Producing something
Producing something
Producing something
```

Then channel can hold 10 items. When it is full, the producer waits until there is space available. Since the consumer fails (and no items are removed from the channel), the producer ends up waiting indefinitely. The "await WhenAll" will hang since the producer task does not complete.

**Output with both Consumer/Producer exceptions**  
All processing stops when the producer throws an exception on the first item.  

```
Producing something
System.Exception: Bad thing happened in Producer
   at Program.DoSomethingThatMightThrowExceptionForProducer() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 58
   at Program.<>c__DisplayClass4_0.<<ProducerConsumerWithExceptions>g__Producer|0>d.MoveNext() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 28
--- End of stack trace from previous location ---
   at Program.ProducerConsumerWithExceptions() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 46
   at Program.Main(String[] args) in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 10
```

The producer task finishes when the exception is thrown. Since the producer marks the channel as "Complete", the consumer task also finishes. Since there are no items put onto the channel, the consumer does not have a chance to read an item (or throw an exception). The "await WhenAll" will throw the exception from the producer.

### refactored
[refactored/Program.cs](./ChannelExceptions/refactored/Program.cs)  

Relevant code:
```c#
var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(10));
```
```c#
await writer.WriteAsync(i);
```
The bounded channel limits the channel to only holding 10 items. When the channel is full, calls to "await WriteAsync()" will wait until there is space available in the channel.  

```c#
await Task.WhenAll(producer, consumer);
```
The "await WhenAll()" will wait until both the Producer task and the Consumer task have completed. Since the Producer ends up waiting indefinitely when the channel is full, the application will hang on this line.

```c#
finally
{
   writer.Complete();
}
```
The producer marks the channel writer as "Complete" in a finally block. This means that if the producer throws an exception, the channel is closed. The consumer will stop waiting for items and the consumer task will finish.

**Output with no exceptions**  
Produces and consumes 20 items.  

*The number of items is reduced from 100 in the original. Also, notice the "Done" at the bottom that indicates the application completed (i.e., did not hang).*

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Consuming object: 1
Producing something: 3
Consuming object: 2
Producing something: 4
Consuming object: 3
Producing something: 5
Consuming object: 4
Producing something: 6
Consuming object: 5
Producing something: 7
Consuming object: 6
Producing something: 8
Consuming object: 7
Producing something: 9
Consuming object: 8
Producing something: 10
Consuming object: 9
Producing something: 11
Consuming object: 10
Producing something: 12
Consuming object: 11
Producing something: 13
Consuming object: 12
Producing something: 14
Consuming object: 13
Consuming object: 14
Producing something: 15
Producing something: 16
Consuming object: 15
Producing something: 17
Consuming object: 16
Producing something: 18
Consuming object: 17
Producing something: 19
Consuming object: 18
Consuming object: 19
Done
```

**Output with Producer exception**  
All processing stops when the producer throws an exception on the first item.  

*Instead of showing the entire exception, just the message is shown.*  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

The producer task finishes when the exception is thrown. Since the producer marks the channel as "Complete", the consumer task also finishes. The "await WhenAll" will throw the exception from the producer.

**Output with Consumer exception**  
Application hangs when the consumer throws an exception on the first item.

*Notice that there is no "Done" message. This indicates that the application is hung.*

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Producing something: 3
Producing something: 4
Producing something: 5
Producing something: 6
Producing something: 7
Producing something: 8
Producing something: 9
Producing something: 10
Producing something: 11
```

Then channel can hold 10 items. When it is full, the producer waits until there is space available. Since the consumer fails (and no items are removed from the channel), the producer ends up waiting indefinitely. The "await WhenAll" will hang since the producer task does not complete.

**Output with both Consumer/Producer exceptions**  
All processing stops when the producer throws an exception on the first item.  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

The producer task finishes when the exception is thrown. Since the producer marks the channel as "Complete", the consumer task also finishes. Since there are no items put onto the channel, the consumer does not have a chance to read an item (or throw an exception). The "await WhenAll" will throw the exception from the producer.

### separate-await
[separate-await/Program.cs](./ChannelExceptions/separate-await/Program.cs)  

Relevant code:
```c#
await consumer;
await producer;

//await Task.WhenAll(producer, consumer);
```
Instead of awaiting both tasks at the same time, the consumer is awaited separately from the producer (with the consumer awaited first). If the consumer throws an exception, that exception will be raised without waiting for the producer to finish, so we no longer have to worry about the producer hanging when the channel is full.  

The downside is that the Main method will continue without waiting for the producer task to complete.

**Output with no exceptions**  
Produces and consumes 20 items.  

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Consuming object: 1
Producing something: 3
Consuming object: 2
Producing something: 4
Consuming object: 3
Producing something: 5
Consuming object: 4
Producing something: 6
Consuming object: 5
Producing something: 7
Consuming object: 6
Producing something: 8
Consuming object: 7
Producing something: 9
Consuming object: 8
Producing something: 10
Consuming object: 9
Producing something: 11
Consuming object: 10
Producing something: 12
Consuming object: 11
Producing something: 13
Consuming object: 12
Producing something: 14
Consuming object: 13
Consuming object: 14
Producing something: 15
Producing something: 16
Consuming object: 15
Producing something: 17
Consuming object: 16
Producing something: 18
Consuming object: 17
Producing something: 19
Consuming object: 18
Consuming object: 19
Done
```

**Output with Producer exception**  
All processing stops when the producer throws an exception on the first item.  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

**Output with Consumer exception**  
The consumer throws an exception on the first item, and the producer continues to fill up the bounded channel.

```
Producing something: 0
Producing something: 1
Consuming object: 0
Bad thing happened in Consumer (0)
Done
Producing something: 2
Producing something: 3
Producing something: 4
Producing something: 5
Producing something: 6
Producing something: 7
Producing something: 8
Producing something: 9
Producing something: 10
Producing something: 11
```
 
Even though the consumer throws an exception, the application completes (does not hang). Notice that the "Done" message is in the middle. Since the code "awaits" the consumer (which throws an exception), it does not "await" the producer. This means processing on the Main method continues (by printing "Done") without waiting for the producer to finish.

**Output with both Consumer/Producer exceptions**  
All processing stops when the producer throws an exception on the first item.  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

### unbounded-channel
[unbounded-channel/Program.cs](./ChannelExceptions/unbounded-channel/Program.cs)  

Relevant code:

```c#
var channel = Channel.CreateUnbounded<int>();
```

The channel is no longer restricted to 10 items.  

```c#
await Task.WhenAll(producer, consumer);
```

We're back to using "await WhenAll" to wait until both the producer and consumer tasks are complete.

**Output with no exceptions**  
Produces and consumes 20 items.  

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Consuming object: 1
Producing something: 3
Consuming object: 2
Producing something: 4
Consuming object: 3
Producing something: 5
Consuming object: 4
Producing something: 6
Consuming object: 5
Producing something: 7
Consuming object: 6
Producing something: 8
Consuming object: 7
Producing something: 9
Consuming object: 8
Producing something: 10
Consuming object: 9
Producing something: 11
Consuming object: 10
Producing something: 12
Consuming object: 11
Producing something: 13
Consuming object: 12
Producing something: 14
Consuming object: 13
Producing something: 15
Consuming object: 14
Producing something: 16
Consuming object: 15
Producing something: 17
Consuming object: 16
Producing something: 18
Consuming object: 17
Producing something: 19
Consuming object: 18
Consuming object: 19
Done
```

**Output with Producer exception**  
All processing stops when the producer throws an exception on the first item.  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

**Output with Consumer exception**  
The consumer throws an exception on the first item, and the application does not hang.

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Producing something: 3
Producing something: 4
Producing something: 5
Producing something: 6
Producing something: 7
Producing something: 8
Producing something: 9
Producing something: 10
Producing something: 11
Producing something: 12
Producing something: 13
Producing something: 14
Producing something: 15
Producing something: 16
Producing something: 17
Producing something: 18
Producing something: 19
Bad thing happened in Consumer (0)
Done
```
Since the channel is unbounded, the producer can continue to put items onto the channel and completes normally.

The "await WhenAll" will throw the exception from the consumer (and the "catch" block in the Main method prints out the message). The application exits normally.

**Output with both Consumer/Producer exceptions**  
All processing stops when the producer throws an exception on the first item.  

Output with both Consumer/Producer exceptions:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

### doesnt-stop
[doesnt-stop/Program.cs](./ChannelExceptions/doesnt-stop/Program.cs)  

Relevant code:

**Producer**
```c#
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
```

The producer has a "catch" inside the for loop. This will allow the processing to continue even when an individual item throws an exception. When the entire producer process is done, the channel is marked as "Complete".  

**Consumer**
```c#
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
      Console.WriteLine($"Logged: {ex.Message}");
   }
}
```

The consumer also has a try/catch block inside the foreach loop. If an individual item throws an exception, processing continues. The foreach loop will exit once the channel is marked "Complete" and all of the items have been read off of the channel.

**Producer Exceptions**
```c#
if (Randomizer.Next() % 3 == 0)
   throw new Exception($"Bad thing happened in Producer ({item})");
```
The producer exceptions are randomized (approximaly 1 in 3 items should fail).  

**Consumer Exceptions**
```c#
if (Randomizer.Next() % 50 == 0)
   throw new Exception($"Bad thing happened in Consumer ({item})");
```
The consumer exceptions are randomized (approximaly 1 in 50 items should fail).  

**Output with no exceptions**  
Produces and consumes 100 items. The total number of items produced and consumed is shown at the bottom.  

*Because we will be using random numbers for exceptions, we are using 100 items once again.*

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Consuming object: 1
Producing something: 3
Consuming object: 2
Producing something: 4
Consuming object: 3
...
Producing something: 97
Consuming object: 96
Producing something: 98
Consuming object: 97
Producing something: 99
Consuming object: 98
Consuming object: 99
Total Produced: 100
Total Consumed: 100
Done
```

**Output with Random Producer exception**  
A try/catch block inside the producer loop produces a "log" of the exception and continues processing. Notice that the number of items produced and consumed are equal since the consumer does not throw any exceptions (all produced items are consumed).  

*Exceptions are thrown based on random numbers (should be about 1 in 3).*

```
Producing something: 0
Logged: Bad thing happened in Producer (0)
Producing something: 1
Logged: Bad thing happened in Producer (1)
Producing something: 2
Logged: Bad thing happened in Producer (2)
Producing something: 3
Producing something: 4
Consuming object: 3
Producing something: 5
Consuming object: 4
Consuming object: 5
Producing something: 6
Logged: Bad thing happened in Producer (6)
Producing something: 7
Producing something: 8
Consuming object: 7
...
Producing something: 97
Logged: Bad thing happened in Producer (97)
Producing something: 98
Producing something: 99
Consuming object: 98
Consuming object: 99
Total Produced: 61
Total Consumed: 61
Done
```

**Output with random Consumer exception**  
A try/catch block inside the consumer loop produces a "log" of the exception and continues processing. Notice that the number of items produced and consumed are *not* equal since the consumer fails on some items that have been produced.  

*Exceptions are thrown based on random numbers (should be about 1 in 50).*

```
Producing something: 0
Producing something: 1
Consuming object: 0
Producing something: 2
Consuming object: 1
...
Producing something: 7
Consuming object: 6
Producing something: 8
Consuming object: 7
Logged: Bad thing happened in Consumer (7)
Consuming object: 8
Producing something: 9
Producing something: 10
Consuming object: 9
...
Producing something: 52
Consuming object: 51
Producing something: 53
Consuming object: 52
Logged: Bad thing happened in Consumer (52)
Producing something: 54
...
Producing something: 75
Consuming object: 74
Producing something: 76
Consuming object: 75
Logged: Bad thing happened in Consumer (75)
Producing something: 77
Consuming object: 76
Producing something: 78
...
Producing something: 82
Consuming object: 81
Producing something: 83
Consuming object: 82
Logged: Bad thing happened in Consumer (82)
Producing something: 84
Consuming object: 83
...
Producing something: 99
Consuming object: 98
Consuming object: 99
Total Produced: 100
Total Consumed: 96
Done
```

**Output with both Consumer/Producer exceptions**  
Both producer and consumer have try/catch blocks to log any exceptions and continue. The number of items produced and consumed are *not* equal since the consumer fails on some items that have been produced.  

*Exceptions are thrown based on random numbers (1 in 3 for the producer; 1 in 50 for the consumer).*

```
Producing something: 0
Producing something: 1
Consuming object: 0
Consuming object: 1
Producing something: 2
Logged: Bad thing happened in Producer (2)
Producing something: 3
Producing something: 4
...
Consuming object: 14
Producing something: 16
Logged: Bad thing happened in Producer (16)
Consuming object: 15
Logged: Bad thing happened in Consumer (15)
Producing something: 17
Producing something: 18
...
Consuming object: 34
Producing something: 35
Logged: Bad thing happened in Producer (35)
Producing something: 36
Logged: Bad thing happened in Producer (36)
Producing something: 37
Producing something: 38
...
Producing something: 42
Logged: Bad thing happened in Producer (42)
Producing something: 43
Consuming object: 41
Logged: Bad thing happened in Consumer (41)
Producing something: 44
Consuming object: 43
...
Consuming object: 95
Consuming object: 96
Producing something: 97
Logged: Bad thing happened in Producer (97)
Producing something: 98
Logged: Bad thing happened in Producer (98)
Producing something: 99
Consuming object: 99
Total Produced: 77
Total Consumed: 74
Done
```
---
## More to Come
This is an initial write-up and work in progress. A full article is in the works.

---