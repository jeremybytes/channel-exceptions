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

---

This code is still in the experimental state. A full write-up is in the works as well.