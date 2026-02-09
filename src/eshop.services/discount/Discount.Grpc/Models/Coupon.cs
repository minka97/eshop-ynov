using System.ComponentModel.DataAnnotations;

namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string ProductName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}