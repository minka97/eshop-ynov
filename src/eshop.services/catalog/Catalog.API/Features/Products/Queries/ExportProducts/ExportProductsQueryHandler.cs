using BuildingBlocks.CQRS;
using Catalog.API.Models;
using ClosedXML.Excel;
using Marten;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

public class ExportProductsQueryHandler(IDocumentSession documentSession)
    : IQueryHandler<ExportProductsQuery, ExportProductsQueryResult>
{
    public async Task<ExportProductsQueryResult> Handle(ExportProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await documentSession.Query<Product>()
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Products");

        // En-tête
        worksheet.Cell(1, 1).Value = "Id";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Description";
        worksheet.Cell(1, 4).Value = "Price";
        worksheet.Cell(1, 5).Value = "ImageFile";
        worksheet.Cell(1, 6).Value = "Categories";

        // Style de l'en-tête
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Données
        var row = 2;
        foreach (var product in products)
        {
            worksheet.Cell(row, 1).Value = product.Id.ToString();
            worksheet.Cell(row, 2).Value = product.Name;
            worksheet.Cell(row, 3).Value = product.Description;
            worksheet.Cell(row, 4).Value = product.Price;
            worksheet.Cell(row, 5).Value = product.ImageFile;
            worksheet.Cell(row, 6).Value = string.Join("|", product.Categories);
            row++;
        }

        // Ajuster la largeur des colonnes
        worksheet.Columns().AdjustToContents();

        // Convertir en bytes
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var fileContent = stream.ToArray();
        var fileName = $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

        return new ExportProductsQueryResult(fileContent, fileName);
    }
}
