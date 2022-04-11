using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Newtonsoft.Json.Linq;
using Microsoft.Azure.Cosmos.Table;

namespace TableStorage
{
  class Program
  {
    static JObject azureConfig = JObject.Parse(File.ReadAllText("azureConfig.json"));

    // Batch Account credentials
    static String storageAccountName = azureConfig.GetValue("StorageAccountName").ToString();
    static String storageAccountKey = azureConfig.GetValue("StorageAccountKey").ToString();
    static String tableName = azureConfig.GetValue("TableName").ToString();

    static String firstNamesPath = @"firstnames.csv";
    static String lastNamesPath = @"lastnames.csv";

    static int soManyItems = 100000;

    static TableSettings settings = new TableSettings(
        storageAccountName,
        storageAccountKey,
        tableName);

    static void Main(string[] args)
    {
      // Create Table
      CloudTable table = GetTableAsync().GetAwaiter().GetResult();

      Console.WriteLine(InsertAllTheThings(table).GetAwaiter().GetResult());
      
      Console.ReadLine();
      // Delete Table
      DeleteTableAsync().GetAwaiter().GetResult();
    }

    // Creates the table if it doesn't already exist
    private static async Task<CloudTable> GetTableAsync()
    {
      //Account
      CloudStorageAccount storageAccount = new CloudStorageAccount(
          new StorageCredentials(settings.StorageAccount, settings.StorageKey), true);

      //Client
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

      //Table
      CloudTable table = tableClient.GetTableReference(settings.TableName); 
      await table.CreateIfNotExistsAsync();

      return table;
    }

    // Deletes the table if it does exist
    private static async Task<CloudTable> DeleteTableAsync()
    {
      //Account
      CloudStorageAccount storageAccount = new CloudStorageAccount(
          new StorageCredentials(settings.StorageAccount, settings.StorageKey), true);

      //Client
      CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

      //Table
      CloudTable table = tableClient.GetTableReference(settings.TableName);
      await table.DeleteIfExistsAsync();

      return table;
    }

    private static async Task<String> InsertAllTheThings(CloudTable _table)
    {
      var taskCount = 0;
      var taskThreshold = 200; // Seems to be a good value to start with
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      var items = new List<MyTableEntity>();
      // Create a new customer entity.

      List<String> firstnames = new List<String>();
      List<String> lastnames = new List<String>();

      Console.WriteLine("Reading CSV files...");
      using (var reader = new StreamReader(firstNamesPath))
      {
        var line = reader.ReadLine();
        var values = line.Split(',');
        firstnames = values.ToList<String>();
      }

      using (var reader = new StreamReader(lastNamesPath))
      {
        var line = reader.ReadLine();
        var values = line.Split(',');
        lastnames = values.ToList<String>();
      }

      Console.WriteLine("Reading firstnames and lastnames finished by: " + stopwatch.Elapsed.ToString());

      Console.WriteLine("Creating random lists from firstnames and lastnames...");
      while (items.Count < soManyItems)
      {
        var partitionKey = "placeholder";

        var rnd = new Random();
        var firstname = firstnames[rnd.Next(0, firstnames.Count - 1)];
        var lastname = lastnames[rnd.Next(0, lastnames.Count - 1)];

        MyTableEntity entity1 = new MyTableEntity(partitionKey);
        entity1.Email = String.Format("{0}@{1}.com", firstname, lastname);
        entity1.PhoneNumber = String.Format("{0}-{1}-{2}", rnd.Next(1, 999), rnd.Next(1, 999), rnd.Next(1, 9999));
        entity1.Firstname = firstname;
        entity1.Lastname = lastname;

        items.Add(entity1);
      }

      Console.WriteLine("Creating random lists finished by: " + stopwatch.Elapsed.ToString());

      Console.WriteLine("Starting batch operations for " + soManyItems + " items...");
      Console.WriteLine("200 tasks with 100 inserts running parallel...");
      Console.WriteLine("for a total of 20.000 inserts per operation...");

      var batchTasks = new List<Task<TableBatchResult>>();

      var count = 1;
      var maxParallel = 100; // 100 is max

      for (var i = 0; i < items.Count; i += maxParallel)
      {
        taskCount++;
        var partitionkey = "" + i + maxParallel;
        var batchItems = items.Skip(i).Take(maxParallel).ToList();

        var batch = new TableBatchOperation();

        foreach (var item in batchItems)
        {
          item.PartitionKey = "partition" + count;
          batch.InsertOrMerge(item);
        }

        var task = _table.ExecuteBatchAsync(batch);
        batchTasks.Add(task);

        if (taskCount >= taskThreshold)
        {
          await Task.WhenAll(batchTasks);
          Console.WriteLine("Finished batch " + count + " by: " + stopwatch.Elapsed.ToString());
          count++;
          taskCount = 0;
        }
      }
      //Console.WriteLine("Finished by: " + stopwatch.Elapsed.ToString());

      //Task.WhenAll(batchTasks);

      //Console.WriteLine("Finished all by: " + stopwatch.Elapsed.ToString());
      stopwatch.Stop();
      
      return "Finished " + soManyItems + " inserts by " + stopwatch.Elapsed.ToString();
    }
  }
}
