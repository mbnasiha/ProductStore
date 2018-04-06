// Client Conole Application
// Note the methods to call the server have been generated as part of the REST API client and have 
//   application specific names e.g. client.Products.PostProductWithHttpMessagesAsync()

using Microsoft.Rest;
using ProductStoreClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductStoreClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {

            Console.WriteLine("Press <ENTER> after each HTTP request to step through.");
            Console.WriteLine();

            // Pick URL location of service up from metadata
            ProductStore client = new ProductStore(new AnonymousCredential());

            // Alternatively set URL location of service explicitly
            //ProductStore client = new ProductStore(new Uri("http://localhost:50001/"), new AnonymousCredential());

            #region POST 
            // HTTP POST - create a new product and obtain the new resource URL (location)
            Console.WriteLine("Test POST");
            Product gizmo = new Product() { Name = "Gizmo", Price = 100.0, Category = "Widget" };

            // PostProductWithHttpMessagesAsync() generated as part of REST API client.
            HttpOperationResponse<Product> postResponse = await client.Products.PostProductWithHttpMessagesAsync(gizmo);
            Console.WriteLine("POST complete; status code: {0}; location: {1}", postResponse.Response.StatusCode, postResponse.Response.Headers.Location);
            Console.WriteLine();
            Console.WriteLine();
            Console.ReadLine();
            #endregion

            if (postResponse.Response.IsSuccessStatusCode)
            {
                #region GET_specific    
                // HTTP GET -get specific product - here the last one POSTed
                Console.WriteLine("Test GET specific at: " + postResponse.Response.Headers.Location);

                // Get right hand end of URL string i.e. the "id" part
                String id = postResponse.Response.Headers.Location.LocalPath.Split('/').Last();

                // GetProductWithHttpMessagesAsync() generated as part of REST API client.
                HttpOperationResponse<Product> getResponse = await client.Products.GetProductWithHttpMessagesAsync(id);
                Console.WriteLine("GET complete; status code: " + getResponse.Response.StatusCode);
                Console.WriteLine("Product at {0}: ", getResponse.Request.RequestUri);
                Console.WriteLine("{0}\t\t${1}\t\t{2}", getResponse.Body.Name, getResponse.Body.Price, getResponse.Body.Category);
                Console.WriteLine();
                Console.WriteLine();
                Console.ReadLine();
                #endregion

                #region GET_all
                // HTTP GET - get all products
                Console.WriteLine("Test GET all");
                // GetWithHttpMessagesAsync() generated as part of REST API client.
                HttpOperationResponse<IList<Product>> getListResponse = await client.Products.GetWithHttpMessagesAsync();
                Console.WriteLine("GET complete; status code: " + getListResponse.Response.StatusCode);
                Console.WriteLine("Products:");
                foreach (Product product in getListResponse.Body)
                {
                    Console.WriteLine("{0}\t\t{1}\t\t${2}\t\t{3}", product.ProductID, product.Name, product.Price, product.Category);
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.ReadLine();
                #endregion

                #region PUT
                // HTTP PUT - overwrite a product
                Console.WriteLine("Test PUT");
                try
                {
                    getResponse = await client.Products.GetProductWithHttpMessagesAsync(id); // last one POSTed
                }
                catch (Exception e)
                {
                    Console.WriteLine("HTTP error: " + e.Message); // NotFound will throw this...
                }

                if (getResponse.Response.IsSuccessStatusCode)
                {
                    Product putGizmo = new Product() { ProductID = getResponse.Body.ProductID, Name = getResponse.Body.Name, Price = 80.0, Category = getResponse.Body.Category }; // Update price
                    // PutProductWithHttpMessagesAsync() generated as part of REST API client.
                    HttpOperationResponse putResponse = await client.Products.PutProductWithHttpMessagesAsync(id, putGizmo);
                    Console.WriteLine("PUT complete; status code: " + putResponse.Response.StatusCode);
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.ReadLine();
                #endregion

                #region DELETE
                // HTTP DELETE - delete a product
                Console.WriteLine("Test DELETE");
                try
                {
                    // DeleteProductWithHttpMessagesAsync() generated as part of REST API client.
                    HttpOperationResponse deleteResponse = await client.Products.DeleteProductWithHttpMessagesAsync(id);
                    Console.WriteLine("DELETE complete; status code: " + deleteResponse.Response.StatusCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine("HTTP error: " + e.Message); // NotFound will throw this...
                }
                #endregion

                Console.ReadLine();

            }
            else Console.WriteLine("Woops - check POST");
        }


    }
}

