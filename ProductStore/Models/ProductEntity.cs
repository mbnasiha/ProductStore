﻿// Entity class for Azure table
using Microsoft.WindowsAzure.Storage.Table;

namespace ProductStore.Models
{

    public class ProductEntity : TableEntity
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }

        public ProductEntity(string partitionKey, string productID)
        {
            PartitionKey = partitionKey;
            RowKey = productID;
        }

        public ProductEntity() { }

    }
}
