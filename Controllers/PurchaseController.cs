using AnimalAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text;
using System.Net.Http;
using System;

#nullable enable

namespace AnimalAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : Controller
    {
        private readonly IConfiguration _config;

        SqlConnection connection;
        public PurchaseController(IConfiguration config)
        {
            _config = config;
            connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
        }

        /// <summary>
        /// Creates a new purchase
        /// </summary>
        /// <param name="animalIdList" example="1,2,3"></param>
        /// <example>[1,2,3]</example>
        /// <returns>Created purchase</returns>
        [HttpPost]
        public async Task<ActionResult<Purchase>> CreatePurchase(int[] animalIdList)
        {

            // check duplicates
            if (HasDuplicates(animalIdList)) return BadRequest("The array contains duplicated id's.");

            // check if animals exist
            if(GetCount(animalIdList) != animalIdList.Count()) return BadRequest("One or more AnimalIds doesnt exist in the database.");

            // calculate list price
            float listPrice = GetListSum(animalIdList);

            // calculate discount percentage
            byte discountPercentage = CalculateDiscountPercentage(animalIdList.Count());

            // calculate Freight price
            float freightPrice = CalculateFreightPrice(animalIdList.Count());

            // calculate total
            float total = listPrice - (listPrice * discountPercentage) / 100 + freightPrice;

            try
            {
                // insert purchase
                Purchase purchase = await SavePurchase(listPrice, discountPercentage, freightPrice, total);

                try
                {
                    // insert purchase animals
                    SavePurchaseAnimals(animalIdList, purchase.PurchaseId);
                }
                catch (Exception ex)
                {
                    RemovePurchase(purchase.PurchaseId);
                    return BadRequest(ex.Message);
                }

                return purchase;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        private async void SavePurchaseAnimals(int[] animalIdList, int purchaseId)
        {
            StringBuilder query = new StringBuilder();

            foreach (var AnimalId in animalIdList)
            {
                query.AppendLine($@"insert into [dbo].[PurchaseAnimals](
                                    [PurchaseId]
                                    ,[AnimalId])
                                    VALUES
                                    ({purchaseId}
                                    ,{AnimalId})");
            }

            await connection.ExecuteAsync(query.ToString());
        }

        private async void RemovePurchase(int purchaseId)
        {
            StringBuilder query = new StringBuilder();

            query.AppendLine($@"delete from [dbo].[PurchaseAnimals] where purchaseId = {purchaseId}");
            query.AppendLine($@"delete from [dbo].[Purchase] where purchaseId = {purchaseId}");

            await connection.ExecuteAsync(query.ToString());
        }

        private async Task<Purchase> SavePurchase(float listPrice, byte discountPercentage, float freightPrice, float total)
        {
            await connection.ExecuteAsync($@"insert into [dbo].[Purchase](
                                        [ListPrice]
                                        ,[DiscountPercentage]
                                        ,[FreightPrice]
                                        ,[TotalPrice])
                                        VALUES
                                        ({listPrice}
                                        ,{discountPercentage}
                                        ,{freightPrice}
                                        ,{total})");
            return await GetLastPurchase();
        }

        private float CalculateFreightPrice(int count)
        {
            if (count > 300) return 0;
            return 1000;
        }

        private byte CalculateDiscountPercentage(int count)
        {
            if (count > 200) return 8;
            if (count > 50) return 5;
            return 0;
        }

        private int GetCount(int[] animalIdList)
        {
            StringBuilder query = new StringBuilder();
            query.Append("select count(AnimalId) as sum from Animal where AnimalId in (");

            bool first = true;

            foreach (var i in animalIdList)
            {
                if (!first)
                {
                    query.Append(',');
                }
                else first = false;

                query.Append(i.ToString());
            }

            query.Append(")");

            return connection.ExecuteScalar<int>(query.ToString());
        }
        private float GetListSum(int[] animalIdList)
        {
            StringBuilder query = new StringBuilder();
            query.Append("select SUM(price) as sum from Animal where animalId in (");

            bool first = true;

            foreach (var i in animalIdList)
            {
                if (!first)
                {
                    query.Append(',');
                }
                else first = false;

                query.Append(i.ToString());
            }

            query.Append(")");

            return connection.ExecuteScalar<float>(query.ToString());
        }

        private bool HasDuplicates(int[] ids)
        {
            if (ids.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                return true;
            }
            return false;
        }

        private async Task<Purchase> GetLastPurchase()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Purchase> purchase = await connection.QueryAsync<Purchase>("select TOP(1) * from purchase order by PurchaseId desc");
            return purchase.FirstOrDefault();
        }
    }
}
