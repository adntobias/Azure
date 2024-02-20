using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using StackExchange.Redis;

namespace AzureCacheForRedis
{
  class Program
  {
    static JObject azureConfig = JObject.Parse(File.ReadAllText("azureConfig.json"));

    // Batch Account credentials
    static String redisAccountName = azureConfig.GetValue("RedisAccountName").ToString();
    static String redisKey = azureConfig.GetValue("RedisKey").ToString();
    static String redisConnectionString = azureConfig.GetValue("RedisConnectionString").ToString();
    static void Main(string[] args)
    {
      Console.WriteLine("Azure Cache for Redis!");

      // Connection refers to a property that returns a ConnectionMultiplexer
      IDatabase cache = lazyConnection.Value.GetDatabase();

      // Perform cache operations using the cache object...

      // Simple PING command
      string cacheCommand = "PING";
      Console.WriteLine("\nCache command  : " + cacheCommand);
      Console.WriteLine("Cache response : " + cache.Execute(cacheCommand).ToString());

      // Simple get and put of integral data types into the cache
      cacheCommand = "GET System:Message";
      Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
      Console.WriteLine("Cache response : " + cache.StringGet("System:Message").ToString());

      cacheCommand = "SET System:Message \"AZ204 Kurs 08-12-2023 !!\"";
      Console.WriteLine("\nCache command  : " + cacheCommand + " or StringSet()");
      Console.WriteLine("Cache response : " + cache.StringSet("System:Message", "AZ204 Kurs 08-12-2023 !!").ToString());

      // Demonstrate "SET Message" executed as expected...
      cacheCommand = "GET System:Message";
      Console.WriteLine("\nCache command  : " + cacheCommand + " or StringGet()");
      Console.WriteLine("Cache response : " + cache.StringGet("System:Message").ToString());

      // Get the client list, useful to see if connection list is growing...
      cacheCommand = "CLIENT LIST";
      Console.WriteLine("\nCache command  : " + cacheCommand);
      Console.WriteLine("Cache response : \n" + cache.Execute("CLIENT", "LIST").ToString().Replace("id=", "id="));

      // Store .NET object to cache
      Employee e007 = new Employee("007", "Patrick Star", 100);
      Console.WriteLine("Cache response from storing Employee .NET object : " +
          cache.StringSet("e007", JsonConvert.SerializeObject(e007)));

      // Retrieve .NET object from cache
      Employee e007FromCache = JsonConvert.DeserializeObject<Employee>(cache.StringGet("e007"));
      Console.WriteLine("Deserialized Employee .NET object :\n");
      Console.WriteLine("\tEmployee.Name : " + e007FromCache.Name);
      Console.WriteLine("\tEmployee.Id   : " + e007FromCache.Id);
      Console.WriteLine("\tEmployee.Age  : " + e007FromCache.Age + "\n");

      Console.ReadLine();
      lazyConnection.Value.Dispose();
    }

    private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
    {
      string cacheConnection = redisConnectionString;
      return ConnectionMultiplexer.Connect(cacheConnection);
    });

    public static ConnectionMultiplexer Connection
    {
      get
      {
        return lazyConnection.Value;
      }
    }
  }
}
