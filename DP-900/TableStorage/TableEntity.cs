using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TableStorage
{
  class MyTableEntity : TableEntity
  {
    public MyTableEntity(string partitionKey)
    {
      this.PartitionKey = partitionKey;
      this.RowKey = new Random().Next(0, 9999999) + ":" + new Random().Next(0, 9999999);
    }

    public MyTableEntity() { }

    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
  }
}