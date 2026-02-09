using FluentValidation;

namespace Catalog.API.Features.Products.Commands.ImportProducts;

/// <summary>
/// Validates the ImportProductsCommand to ensure that the uploaded file meets the required constraints.
/// </summary>
public class ImportProductsCommandValidator : AbstractValidator<ImportProductsCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = [".xlsx", ".xls"];
    private static readonly string[] AllowedContentTypes =
    [
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-excel" // .xls
    ];

    public ImportProductsCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Le fichier est requis");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(MaxFileSize)
            .When(x => x.File != null)
            .WithMessage($"La taille du fichier ne doit pas dépasser {MaxFileSize / (1024 * 1024)} MB");

        RuleFor(x => x.File.FileName)
            .Must(HaveValidExtension)
            .When(x => x.File != null)
            .WithMessage($"Le fichier doit être au format Excel ({string.Join(", ", AllowedExtensions)})");

        RuleFor(x => x.File.ContentType)
            .Must(BeValidContentType)
            .When(x => x.File != null)
            .WithMessage("Le type de contenu du fichier n'est pas valide pour un fichier Excel");
    }

    private static bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private static bool BeValidContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
}
