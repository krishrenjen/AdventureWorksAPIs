using AdventureWorksAPIs.DTO;
using AdventureWorksAPIs.Identity;
using AdventureWorksAPIs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AdventureWorksContext _context;
    public ProductsController(AdventureWorksContext context) {
        _context = context;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? queryNameId,
        [FromQuery] decimal? listPriceMax,    
        [FromQuery] decimal? listPriceMin = 0,    
        [FromQuery] int? pageNumber = 1, 
        [FromQuery] int? pageSize = 25)  
    {
        
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 30) pageSize = 25;

        var rowParam = new SqlParameter
        {
            ParameterName = "@TotalRows",
            SqlDbType = System.Data.SqlDbType.Int,
            Direction = System.Data.ParameterDirection.Output
        };

        var products = await _context.ProductDTOs
        .FromSqlRaw(
            "EXEC GetProductsList " +
                "@QueryNameID  = {0}, " +
                "@ListPriceMax = {1}, " +
                "@ListPriceMin = {2}, " +
                "@PageNumber = {3}, " +
                "@PageSize = {4}, " +
                "@TotalRows = {5} OUTPUT",
            queryNameId ?? "",
            listPriceMax ?? -1,
            listPriceMin ?? 0,
            pageNumber ?? 1,
            pageSize ?? 25,
            rowParam
        )
        .ToListAsync();

        int totalRows = (int)(rowParam.Value ?? 0);
        int totalPages = (int)Math.Ceiling(totalRows / (double)pageSize);

        return Ok(new
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            totalRows,
            totalPages,
            data = products
        });
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var result = await _context.ProductInfoDTOs
            .FromSqlRaw("EXEC GetProductInfoById @ProductId = {0}", id)
            .ToListAsync();

        var product = result.FirstOrDefault();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [Authorize]
    [HttpGet("{productId}/similar")]
    public async Task<IActionResult> GetSimilarProducts(int productId, [FromQuery] int amount = 5)
    {
        var res1 = await _context.ProductNameDTOs
            .FromSqlRaw("EXEC GetProductNameById @ProductId = {0}", productId)
            .ToListAsync();

        var productName = res1.FirstOrDefault()?.Name;

        if (productName == null)
            return NotFound("Product not found.");

        var keywords = productName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(word => word.ToLower())
            .Where(word => word.Length > 2)
            .ToList();

        var exclusiveProducts = await _context.ProductDTOs
            .FromSqlRaw("EXEC GetProductsOnSaleExclusiveID @ProductId = {0}", productId)
            .ToListAsync();

        var results = exclusiveProducts
            .AsEnumerable()
            .Where(p =>
                p.Name != null &&
                keywords.Any(k => p.Name.Contains(k, StringComparison.OrdinalIgnoreCase))
            )
            .Take(amount)
            .Select(p => new
            {
                p.ProductId,
                p.Name,
                p.ProductNumber,
                p.ListPrice
            })
            .ToList();


        return Ok(results);
    }

    [Authorize]
    [RequiresClaim(IdentityData.EmployeeUserClaimName, "true")]
    [HttpPost("{id}")]
    public async Task<IActionResult> UpsertProduct(int id, [FromBody] ProductUpdateDTO updated)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        updated.SizeUnitMeasureCode = string.IsNullOrWhiteSpace(updated.SizeUnitMeasureCode) ? "CM" : updated.SizeUnitMeasureCode;
        updated.WeightUnitMeasureCode = string.IsNullOrWhiteSpace(updated.WeightUnitMeasureCode) ? "LB" : updated.WeightUnitMeasureCode;

        var productIdParam = new SqlParameter("@ProductId", SqlDbType.Int)
        {
            Direction = ParameterDirection.InputOutput,
            Value = id
        };

        await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC UpsertProductFields
                @Name                  = {updated.Name},
                @ListPrice             = {updated.ListPrice},
                @StandardCost          = {updated.StandardCost},
                @Size                  = {updated.Size ?? ""},
                @SizeUnitMeasureCode   = {updated.SizeUnitMeasureCode ?? "CM"},
                @Weight                = {updated.Weight},
                @WeightUnitMeasureCode = {(updated.Weight == null
                                        ? null
                                        : updated.WeightUnitMeasureCode ?? "LB")},
                @ProductId             = {productIdParam} OUTPUT"
        );

        id = (int)productIdParam.Value!;

        var modelIdParam = new SqlParameter
        {
            ParameterName = "@ProductModelId",
            SqlDbType = System.Data.SqlDbType.Int,
            Direction = System.Data.ParameterDirection.Output
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC EnsureProductModel @ProductId = {0}, @ProductName = {1}, @ProductModelId = @ProductModelId OUTPUT",
            id, updated.Name, modelIdParam
        );

        int productModelId = (int)modelIdParam.Value;

        if (!string.IsNullOrWhiteSpace(updated.Description))
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC UpsertProductDescription @ProductModelId = {0}, @Description = {1}",
                productModelId, updated.Description
            );
        }

        return Ok(new { ProductId = id });
    }

    [Authorize]
    [RequiresClaim(IdentityData.EmployeeUserClaimName, "true")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _context.Database
        .ExecuteSqlRawAsync("EXEC DeleteProduct @ProductId = {0}", id);

        return Ok();
    }


}
