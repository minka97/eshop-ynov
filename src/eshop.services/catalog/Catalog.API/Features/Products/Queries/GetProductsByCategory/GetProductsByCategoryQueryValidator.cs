using FluentValidation;

namespace Catalog.API.Features.Products.Queries.GetProductsByCategory;
/// <summary>
/// 
/// </summary>
public class GetProductsByCategoryQueryValidator
    : AbstractValidator<GetProductsByCategoryQuery>
{
    /// <summary>
    /// 
    /// </summary>
    public GetProductsByCategoryQueryValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MinimumLength(2).WithMessage("Category must contain at least 2 characters");
    }
}