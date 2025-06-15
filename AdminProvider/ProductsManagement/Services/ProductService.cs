using AdminProvider.ProductsManagement.Data.Entities;
using AdminProvider.ProductsManagement.Interfaces;
using AdminProvider.ProductsManagement.Models;
using OfficeOpenXml;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace AdminProvider.ProductsManagement.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<IProductService> _logger;
    public ProductService(IProductRepository productRepository, ILogger<IProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ProductsCountResponse> GetProductsCountAsync()
    {
        var count = await _productRepository.GetProductCountAsync();

        return count;
    }

    public async Task ImportProductsFromExcelAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("The uploaded file is empty or null.");
        }

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0; 

        using var package = new ExcelPackage(stream); 
        var worksheet = package.Workbook.Worksheets[0];

        if (worksheet == null)
        {
            throw new InvalidOperationException("The uploaded file does not contain a valid worksheet.");
        }

        var rowCount = worksheet.Dimension?.Rows ?? 0;
        if (rowCount < 2)
        {
            throw new InvalidOperationException("The uploaded file does not contain enough data.");
        }

        var headers = new Dictionary<string, int>();
        for (int col = 1; col <= worksheet.Dimension?.Columns; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(header))
            {
                headers[header] = col;
            }
        }

        var requiredColumns = new List<string>
    {
        "företagsnamn", "organisationsnummer", "adress", "postnummer", "ort", "telefonnummer", "e-post", "bransch (sni-kod)", "omsättning (msek)", "antal anställda", "vd-namn"
    };

        foreach (var column in requiredColumns)
        {
            if (!headers.ContainsKey(column))
            {
                throw new InvalidOperationException($"The required column '{column}' is missing in the uploaded file.");
            }
        }

        var productsToAdd = new List<ProductEntity>();
        var errors = new List<string>();

        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                var companyName = worksheet.Cells[row, headers["företagsnamn"]].Text?.Trim();
                var organizationNumber = worksheet.Cells[row, headers["organisationsnummer"]].Text?.Trim();
                var address = worksheet.Cells[row, headers["adress"]].Text?.Trim();
                var postalCode = worksheet.Cells[row, headers["postnummer"]].Text?.Trim();
                var city = worksheet.Cells[row, headers["ort"]].Text?.Trim();
                var phoneNumber = worksheet.Cells[row, headers["telefonnummer"]].Text?.Trim();
                var email = worksheet.Cells[row, headers["e-post"]].Text?.Trim();
                var businessType = worksheet.Cells[row, headers["bransch (sni-kod)"]].Text?.Trim();
                var revenue = int.TryParse(worksheet.Cells[row, headers["omsättning (msek)"]].Text, out var rev) ? rev : 0;
                var employees = int.TryParse(worksheet.Cells[row, headers["antal anställda"]].Text, out var emp) ? emp : 0;
                var ceo = worksheet.Cells[row, headers["vd-namn"]].Text?.Trim();

                var product = new ProductEntity
                {
                    ProductId = Guid.NewGuid(),
                    CompanyName = companyName!,
                    OrganizationNumber = organizationNumber!,
                    Address = address,
                    PostalCode = postalCode,
                    City = city,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    BusinessType = businessType,
                    Revenue = ConvertToDecimal(revenue),
                    NumberOfEmployees = employees,
                    CEO = ceo,
                    CustomerId = null,
                    ReservedUntil = null,
                    SoldUntil = null
                };

                productsToAdd.Add(product);
            }
            catch (Exception ex)
            {
                errors.Add($"Error parsing row {row}: {ex.Message}");
            }
        }

        if (productsToAdd.Count > 0)
        {
            const int batchSize = 1000;
            var batchStart = 0;
            while (batchStart < productsToAdd.Count)
            {
                var batch = productsToAdd.Skip(batchStart).Take(batchSize).ToList();
                await _productRepository.AddProductsAsync(batch);
                batchStart += batchSize;
            }
        }

        if (errors.Any())
        {
            _logger.LogError("Errors occurred during import:");
            foreach (var error in errors)
            {
                _logger.LogError(error);
            }
        }
    }

    private decimal ConvertToDecimal(object value)
    {
        try
        {
            return value switch
            {
                decimal decimalValue => decimalValue,
                int intValue => (decimal)intValue,
                double doubleValue => (decimal)doubleValue,
                _ => 0m 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting value to decimal.");
            return 0m;
        }
    }
}
