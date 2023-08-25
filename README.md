# Channel Exceptions  

Exploring exception handling with C# channels. This came from a questions from somone who attended my presentation ([https://github.com/jeremybytes/csharp-channels-presentation](https://github.com/jeremybytes/csharp-channels-presentation)) and was exploring code with other programmers.  

*The original "toy sample" was provided by Edington Watt and the WTW ICT Technology team.*  

## Projects

* original-code  
The original toy sample. One concern is that if the consumer throws an exception, then the application hangs. Also, an exception thrown in the producer or consumer is short-circuiting, meaning that all processing stops.  

* refactored  
I refactored the code a bit by changing local methods to class methods and extracting some code into additional methods. This separation was mainly for my own benefit (it's more along the line of the way my brain likes to work). The behavior is the same. When the consumer throws an exception, then the application hangs.  

* separate-await  
This code uses separate "await"s for the consumer and producer. This raises an exception where "WhenAll" does not.  
*Note: WhenAll does not raise an exception because the producer has not yet finished.*  
By experimenting with the order of the "await"s and which exceptions are thrown, you can see how order can be important.

* unbounded-channel  
This code uses an unbounded channel instead of a bounded channel. When the consumer throws an exception, this allows the producer to complete so that the application does not hang. An exception will still short-circuit the producer or consumer.  

* doesnt-stop  
This code uses try/catch blocks inside the loops of the producer and consumer. This allows the process to continue even if individual operations fail. Exceptions are randomly thrown in both the producer and consumer and are "logged" to the console.  

## Behavior

### original-code
Output with no exceptions:  

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

Output with Producer exception:  

```
Producing something
System.Exception: Bad thing happened in Producer
   at Program.DoSomethingThatMightThrowExceptionForProducer() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 58
   at Program.<>c__DisplayClass4_0.<<ProducerConsumerWithExceptions>g__Producer|0>d.MoveNext() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 28
--- End of stack trace from previous location ---
   at Program.ProducerConsumerWithExceptions() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 46
   at Program.Main(String[] args) in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 10
```

Output with Consumer exception:  

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

Output with both Consumer/Producer exceptions:  

```
Producing something
System.Exception: Bad thing happened in Producer
   at Program.DoSomethingThatMightThrowExceptionForProducer() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 58
   at Program.<>c__DisplayClass4_0.<<ProducerConsumerWithExceptions>g__Producer|0>d.MoveNext() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 28
--- End of stack trace from previous location ---
   at Program.ProducerConsumerWithExceptions() in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 46
   at Program.Main(String[] args) in C:\Development\Articles\04-ChannelExceptions\SampleCode\ChannelExceptions\original-code\Program.cs:line 10
```

### refactored
Output with no exceptions:  

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

Output with Producer exception:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

Output with Consumer exception:  

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

Output with both Consumer/Producer exceptions:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

### separate-await
Output with no exceptions:  

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

Output with Producer exception:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

Output with Consumer exception:  

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

Output with both Consumer/Producer exceptions:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

### unbounded-channel
Output with no exceptions:  

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

Output with Producer exception:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

Output with Consumer exception:  

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

Output with both Consumer/Producer exceptions:  

```
Producing something: 0
Bad thing happened in Producer (0)
Done
```

### doesnt-stop
Output with no exceptions:  

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

Output with Random Producer exception:  

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

Output with random Consumer exception:  

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

Output with both Consumer/Producer exceptions:  

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

This code is still in the experimental state. A full write-up is in the works as well.