using FluentValidation;

namespace Catalog.API.Features.Products.Queries.ExportProducts;

/// <summary>
/// Validates the ExportProductsQuery to ensure that pagination parameters are valid.
/// </summary>
public class ExportProductsQueryValidator : AbstractValidator<ExportProductsQuery>
{
    private const int MaxPageSize = 1000;

    public ExportProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Le numéro de page doit être supérieur ou égal à 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La taille de page doit être supérieure ou égale à 1")
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"La taille de page ne doit pas dépasser {MaxPageSize}");
    }
}
