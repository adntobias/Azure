using System;
using System.Collections.Generic;
using System.Text;

namespace TableStorage
{
  class TableSettings
  {
    public TableSettings(string storageAccount, string storageKey, string tableName)
    {
      if (string.IsNullOrEmpty(storageAccount)) { 
          throw new ArgumentNullException("StorageAccount"); }


      if (string.IsNullOrEmpty(storageKey)) { 
          throw new ArgumentNullException("StorageKey"); }


      if (string.IsNullOrEmpty(tableName)) { 
          throw new ArgumentNullException("TableName"); }

      this.StorageAccount = storageAccount;
      this.StorageKey = storageKey;
      this.TableName = tableName;
    }

    public string StorageAccount { get; }
    public string StorageKey { get; }
    public string TableName { get; }
  }
}
