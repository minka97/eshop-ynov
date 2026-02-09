namespace Discount.Grpc.Models;

public class Coupon
{
    public int Id { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public CouponType Type { get; set; }
    
    public string? Code { get; set; }
    
    public double? Percentage { get; set; }
    
    public double? Amount { get; set; }
    
    public bool IsCumulative { get; set; }
    
    public double? MaxCumulativePercentage { get; set; }
    
    public int? MaxRedemptions { get; set; }
    
    public enum CouponType
    {
        Percentage,
        Code
    }
}