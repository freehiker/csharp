// <copyright>free to everyone.</copyright>
using System;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;

namespace ApiTester
{
    /// <summary>
    /// A mstest class to test some APIs deployed to https://1ryu4whyek.execute-api.us-west-2.amazonaws.com/dev/.
    /// Dependency: RestSharp nuget package is needed and can be downloaded from https://www.nuget.org/packages/RestSharp.
    /// </summary>
    /// <remarks>
    /// To run these tests, build this solution in Visual Studio 2022 and the tests will show up in Test Explorer.
    /// Alternatively, in a VS command line, CD into the folder that has the DLL built from this project, and then run
    /// vstest.console.exe ApiTester.dll
    /// </remarks>
    [TestClass]
    public class SkuApiTest
    {
        const string ApiBaseUrl = "https://1ryu4whyek.execute-api.us-west-2.amazonaws.com/dev/";

        /// <summary>
        /// This test verifies GET method against all SKUs.
        /// </summary>
        [TestMethod]
        public void GetAllSkus()
        {
            // arrange
            var client = new RestClient(ApiBaseUrl);
            var request = new RestRequest("skus", Method.GET);

            // act
            var response = client.Execute(request);

            // assert
            Assert.IsTrue(response.IsSuccessful);
            Assert.AreEqual(response.ContentType, "application/json");
            var jDoc = JsonDocument.Parse(response.Content);
            var rootElement = jDoc.RootElement;
            Assert.IsNotNull(rootElement);
            Assert.AreEqual(rootElement.ValueKind, JsonValueKind.Array);
            int itemCount = rootElement.GetArrayLength();
            // Note below hard-coded 7 is just for demo. Tester should configure it for a real test.
            Assert.IsTrue(itemCount > 7);
            Sku? sku = JsonSerializer.Deserialize<Sku>(rootElement[0]);
            Assert.IsTrue(sku?.sku != null && sku?.description != null && sku?.price != null);
        }

        /// <summary>
        /// This test verifies GET method works for a given SKU id.
        /// </summary>
        [TestMethod]
        public void GetSpecificSkuById()
        {
            // arrange
            const string id = "item-123-4-567";
            var client = new RestClient(ApiBaseUrl);
            var request = new RestRequest($"skus/{id}", Method.GET);

            // act
            var response = client.Execute(request);

            // assert
            Assert.IsTrue(response.IsSuccessful);
            Assert.IsNotNull(response.Content);
            SkuItem? skuItem = JsonSerializer.Deserialize<SkuItem>(response.Content);
            var sku = skuItem?.Item;
            Assert.IsNotNull(sku?.description);
            Assert.AreEqual(sku.sku, id);
        }

        /// <summary>
        /// This is a basic end-to-end test which has 4 parts:
        /// 1. Create/update a SKU;
        /// 2. Run GET method against that newly posted SKU to verify creation/update works;
        /// 3. Run DELETE method against that newly posted SKU;
        /// 4. Run GET method against that deleted SKU to verify DELETE actually worked in Part 3.
        /// </summary>
        [TestMethod]
        public void PostNewSkuAndThenDelete()
        {
            // arrange
            const string skuId = "berliner";
            Sku newSku = new Sku { sku = skuId, description = "Jelly donut", price = "2.99" };
            var request = new RestRequest("skus", Method.POST);
            request.AddJsonBody(newSku);
            var client = new RestClient(ApiBaseUrl);

            // act
            var response = client.Execute(request);

            // assert
            Assert.IsTrue(
                response.IsSuccessful,
                $"Failed to post new SKU '{skuId}' (status code: {response.StatusCode})");

            // Part 2 - verify newly posted SKUL exists)
            request = new RestRequest($"skus/{skuId}", Method.GET);
            response = client.Execute(request);
            Assert.IsTrue(response.IsSuccessful);
            SkuItem? skuItem = JsonSerializer.Deserialize<SkuItem>(response.Content);
            var sku = skuItem?.Item;
            Assert.IsNotNull(sku);
            Assert.AreEqual(sku.sku, skuId);
            Assert.AreEqual(sku.description, newSku.description);
            Assert.AreEqual(sku.price, newSku.price);

            // Part 3 - delete
            request = new RestRequest($"skus/{skuId}", Method.DELETE);
            response = client.Execute(request);
            Assert.IsTrue(
                response.IsSuccessful,
                $"Failed to delete SKU '{skuId}' (status code: {response.StatusCode})");

            // Part 4 - verify newly deleted SKU is gone)
            request = new RestRequest($"skus/{skuId}", Method.GET);
            response = client.Execute(request);
            Assert.IsTrue(response.IsSuccessful);
            skuItem = JsonSerializer.Deserialize<SkuItem>(response.Content);
            Assert.IsNull(skuItem?.Item);
        }

        /// <summary>
        /// This test verifies GET method against an inexistent SKU returns nothing.
        /// </summary>
        [TestMethod]
        public void CannotGetInexistentSku()
        {
            // arrange
            string randomSkuId = Guid.NewGuid().ToString();
            var request = new RestRequest($"skus/{randomSkuId}", Method.GET);
            var client = new RestClient(ApiBaseUrl);

            // act
            var response = client.Execute(request);

            // assert
            Assert.IsTrue(response.IsSuccessful);
            SkuItem? skuItem = JsonSerializer.Deserialize<SkuItem>(response.Content);
            Assert.IsNull(
                skuItem?.Item,
                $"You can't get a SKU that does not even exist!");
        }
    }
}