using AnimalAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text;
using System.Net.Http;

#nullable enable

namespace AnimalAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AnimalController : Controller
    {
        private readonly IConfiguration _config;
        public AnimalController(IConfiguration config) 
        {
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<Animal>>> GetAllAnimals()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Animal> animals = await GetAllAnimals(connection);
            return Ok(animals);
        }


        [HttpGet("AnimalId")]
        public async Task<ActionResult<List<Animal>>> GetAnimalById(int AnimalId)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Animal> animal = await GetAnimalById(AnimalId, connection);
            return Ok(animal);
        }

        [HttpGet(template: "/search", Name = "GetAnimal")]
        public async Task<ActionResult> GetAnimal([FromQuery] GetAnimalParameters animal)
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Animal> animals = await GetAnimal(connection, animal);
            return Ok(animals);
        }

        [HttpPost]
        public  async Task<ActionResult<List<Animal>>> CreateAnimal(Animal animal)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                await connection.ExecuteAsync(@"insert into [dbo].[Animal](
                                                [Name]
                                                ,[Breed]
                                                ,[BirthDate]
                                                ,[Sex]
                                                ,[Price]
                                                ,[Status])
                                                VALUES
                                                (@Name
                                                ,@Breed
                                                ,@BirthDate
                                                ,@Sex
                                                ,@Price
                                                ,@Status)", animal);
                return Ok(await GetLastAnimal());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult<List<Animal>>> UpdateAnimal(Animal animal)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                await connection.ExecuteAsync(@"update [dbo].[Animal] set
                                                    Name = @Name,
                                                    Breed = @Breed,
                                                    BirthDate = @BirthDate,
                                                    Sex = @Sex,
                                                    Price = @Price,
                                                    Status = @Status
                                                where AnimalId = @AnimalId"
                                                    , animal);
                return Ok(await GetAnimalById(animal.AnimalId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{AnimalId}")]
        public async Task<ActionResult<List<Animal>>> DeleteAnimal(int AnimalId)
        {
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                await connection.ExecuteAsync(@"delete from [dbo].[Animal]
                                            where AnimalId = @Id", new { Id = AnimalId });
                return Ok(await GetAnimalById(AnimalId));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private static async Task<IEnumerable<Animal>> GetAllAnimals(SqlConnection connection)
        {
            return await connection.QueryAsync<Animal>("select TOP(10) * from animal order by AnimalId desc");
        }

        private async Task<ActionResult> GetLastAnimal()
        {
            using var connection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            IEnumerable<Animal> animal  = await GetLastAnimal(connection);
            return Ok(animal);
        }

        private static async Task<IEnumerable<Animal>> GetLastAnimal(SqlConnection connection)
        {
            return await connection.QueryAsync<Animal>("select TOP(1) * from animal order by AnimalId desc");
        }

        private static async Task<IEnumerable<Animal>> GetAnimal(SqlConnection connection, GetAnimalParameters animal)
        {
            StringBuilder query = new StringBuilder();
            query.Append("select * from animal ");

            StringBuilder whereClauseQuery = new StringBuilder();

            if (!string.IsNullOrEmpty(animal.AnimalId)) whereClauseQuery = AddParameterToQuery(whereClauseQuery, "AnimalId", animal.AnimalId.ToString());
            if (!string.IsNullOrEmpty(animal.Name)) whereClauseQuery = AddParameterToQuery(whereClauseQuery, "Name", animal.Name);
            if (!string.IsNullOrEmpty(animal.Sex)) whereClauseQuery = AddParameterToQuery(whereClauseQuery, "Sex", animal.Sex);
            if (!string.IsNullOrEmpty(animal.Status)) whereClauseQuery = AddParameterToQuery(whereClauseQuery, "Status", animal.Status);

            query.Append(whereClauseQuery);
            

            return await connection.QueryAsync<Animal>(query.ToString());
        }

        private static StringBuilder AddParameterToQuery(StringBuilder query, string paramName, string value)
        {
            if(query.Length == 0)
            {
                query.Append($"where {paramName} = '{value}' ");
            }
            else
            {
                query.Append($"and {paramName} = '{value}' ");
            }
            return query;
        }

        private static async Task<IEnumerable<Animal>> GetAnimalById(int AnimalId, SqlConnection connection)
        {
            return await connection.QueryAsync<Animal>($"select * from animal where AnimalId = {AnimalId}");
        }
    }
}
