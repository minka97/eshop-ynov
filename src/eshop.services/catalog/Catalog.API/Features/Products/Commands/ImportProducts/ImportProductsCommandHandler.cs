using BuildingBlocks.CQRS;
using Catalog.API.Models;
using ClosedXML.Excel;
using Marten;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

public class ImportProductsCommandHandler(IDocumentSession documentSession)
    : ICommandHandler<ImportProductsCommand, ImportProductsCommandResult>
{
    public async Task<ImportProductsCommandResult> Handle(ImportProductsCommand request,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;
        var errors = new List<string>();

        using var stream = request.File.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Détecter si la colonne Id est présente (en vérifiant l'en-tête)
        var firstHeader = worksheet.Cell(1, 1).GetString().Trim().ToLowerInvariant();
        var hasIdColumn = firstHeader == "id";

        // Définir les index des colonnes selon la présence de la colonne Id
        int nameCol, descCol, priceCol, imageCol, catCol;
        if (hasIdColumn)
        {
            // Format avec Id: Id | Name | Description | Price | ImageFile | Categories
            nameCol = 2;
            descCol = 3;
            priceCol = 4;
            imageCol = 5;
            catCol = 6;
        }
        else
        {
            // Format sans Id: Name | Description | Price | ImageFile | Categories
            nameCol = 1;
            descCol = 2;
            priceCol = 3;
            imageCol = 4;
            catCol = 5;
        }

        // Commencer à la ligne 2 (après l'en-tête)
        var rowNumber = 2;

        while (!worksheet.Row(rowNumber).IsEmpty())
        {
            try
            {
                // Lire l'Id seulement si la colonne existe
                var idStr = hasIdColumn ? worksheet.Cell(rowNumber, 1).GetString().Trim() : "";
                var name = worksheet.Cell(rowNumber, nameCol).GetString().Trim();
                var description = worksheet.Cell(rowNumber, descCol).GetString().Trim();
                var priceValue = worksheet.Cell(rowNumber, priceCol).Value;
                var imageFile = worksheet.Cell(rowNumber, imageCol).GetString().Trim();
                var categoriesStr = worksheet.Cell(rowNumber, catCol).GetString().Trim();

                // Validation du prix
                decimal price;
                if (priceValue.IsNumber)
                {
                    price = (decimal)priceValue.GetNumber();
                }
                else if (!decimal.TryParse(priceValue.ToString(), out price))
                {
                    errors.Add($"Ligne {rowNumber}: Prix invalide '{priceValue}'");
                    rowNumber++;
                    continue;
                }

                // Validation du nom
                if (string.IsNullOrWhiteSpace(name))
                {
                    errors.Add($"Ligne {rowNumber}: Nom requis");
                    rowNumber++;
                    continue;
                }

                var categories = string.IsNullOrWhiteSpace(categoriesStr)
                    ? new List<string>()
                    : categoriesStr.Split('|').ToList();

                // Upsert : si ID présent → update, sinon → create
                if (!string.IsNullOrWhiteSpace(idStr))
                {
                    if (!Guid.TryParse(idStr, out var id))
                    {
                        errors.Add($"Ligne {rowNumber}: ID invalide '{idStr}'");
                        rowNumber++;
                        continue;
                    }

                    var existingProduct = await documentSession.LoadAsync<Product>(id, cancellationToken);

                    if (existingProduct is null)
                    {
                        errors.Add($"Ligne {rowNumber}: Produit avec ID '{id}' non trouvé");
                        rowNumber++;
                        continue;
                    }

                    // Update
                    existingProduct.Name = name;
                    existingProduct.Description = description;
                    existingProduct.Price = price;
                    existingProduct.ImageFile = imageFile;
                    existingProduct.Categories = categories;

                    documentSession.Update(existingProduct);
                    updated++;
                }
                else
                {
                    // Create
                    var newProduct = new Product
                    {
                        Name = name,
                        Description = description,
                        Price = price,
                        ImageFile = imageFile,
                        Categories = categories
                    };

                    documentSession.Store(newProduct);
                    created++;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Ligne {rowNumber}: {ex.Message}");
            }

            rowNumber++;
        }

        await documentSession.SaveChangesAsync(cancellationToken);

        return new ImportProductsCommandResult(created, updated, errors);
    }
}
