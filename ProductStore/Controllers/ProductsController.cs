﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ProductStore.Models;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace ProductStore.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductsController : ApiController
    {
        private const String partitionName = "Products_Partition_1";

        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;
        /// <summary>
        /// 
        /// </summary>
        public ProductsController()
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ToString());
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("Products");
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns></returns>
        // GET: api/Products
        public IEnumerable<Product> Get()
        {
            TableQuery<ProductEntity> query = new TableQuery<ProductEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));
            List<ProductEntity> entityList = new List<ProductEntity>(table.ExecuteQuery(query));

            // Basically create a list of Product from the list of ProductEntity with a 1:1 object relationship, filtering data as needed
            IEnumerable<Product> productList = from e in entityList
                                               select new Product()
                                               {
                                                   ProductID = e.RowKey,
                                                   Name = e.Name,
                                                   Price = e.Price,
                                                   Category = e.Category
                                               };
            return productList;
        }

        // GET: api/Products/5
        /// <summary>
        /// Get a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(string id)
        {
            // Create a retrieve operation that takes a product entity.
            TableOperation getOperation = TableOperation.Retrieve<ProductEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult getOperationResult = table.Execute(getOperation);

            // Construct response including a new DTO as apprporiatte
            if (getOperationResult.Result == null) return NotFound();
            else
            {
                ProductEntity productEntity = (ProductEntity)getOperationResult.Result;
                Product p = new Product()
                {
                    ProductID = productEntity.RowKey,
                    Name = productEntity.Name,
                    Price = productEntity.Price,
                    Category = productEntity.Category
                };
                return Ok(p);
            }
        }

        // POST: api/Products
        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created)]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            ProductEntity productEntity = new ProductEntity()
            {
                RowKey = getNewMaxRowKeyValue(),
                PartitionKey = partitionName,
                Name = product.Name,
                Price = product.Price,
                Category = product.Category
            };

            // Create the TableOperation that inserts the product entity.
            var insertOperation = TableOperation.Insert(productEntity);

            // Execute the insert operation.
            table.Execute(insertOperation);

            return CreatedAtRoute("DefaultApi", new { id = productEntity.RowKey }, productEntity);
        }

        // PUT: api/Products/5
        /// <summary>
        /// Update a product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(string id, Product product)
        {
            if (id != product.ProductID)
            {
                return BadRequest();
            }

            // Create a retrieve operation that takes a product entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<ProductEntity>(partitionName, id);

            // Execute the operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);

            // Assign the result to a ProductEntity object.
            ProductEntity updateEntity = (ProductEntity)retrievedResult.Result;

            updateEntity.Name = product.Name;
            updateEntity.Price = product.Price;
            updateEntity.Category = product.Category;

            // Create the TableOperation that inserts the product entity.
            // Note semantics of InsertOrReplace() which are consistent with PUT
            // See: https://stackoverflow.com/questions/14685907/difference-between-insert-or-merge-entity-and-insert-or-replace-entity
            var updateOperation = TableOperation.InsertOrReplace(updateEntity);

            // Execute the insert operation.
            table.Execute(updateOperation);

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Products/5
        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(string id)
        {
            // Create a retrieve operation that takes a product entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<ProductEntity>(partitionName, id);

            // Execute the retrieve operation.
            TableResult retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result == null) return NotFound();
            else
            {
                ProductEntity deleteEntity = (ProductEntity)retrievedResult.Result;
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                table.Execute(deleteOperation);

                return Ok(retrievedResult.Result);
            }
        }

        private String getNewMaxRowKeyValue()
        {
            TableQuery<ProductEntity> query = new TableQuery<ProductEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionName));

            int maxRowKeyValue = 0;
            foreach (ProductEntity entity in table.ExecuteQuery(query))
            {
                int entityRowKeyValue = Int32.Parse(entity.RowKey);
                if (entityRowKeyValue > maxRowKeyValue) maxRowKeyValue = entityRowKeyValue;
            }
            maxRowKeyValue++;
            return maxRowKeyValue.ToString();
        }


    }
}
