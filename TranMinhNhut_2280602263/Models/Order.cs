using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TranMinhNhut_2280602263.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [ValidateNever]  // Add this attribute
        public required string UserId { get; set; }

        [Required]
        [ValidateNever]  // Add this attribute
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [ValidateNever]  // Add this attribute
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public required string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public required string PhoneNumber { get; set; }

        public string? Notes { get; set; }

        [ForeignKey(nameof(UserId))]
        [ValidateNever]
        public required ApplicationUser ApplicationUser { get; set; }

        [ValidateNever]  // Add this attribute
        public required List<OrderDetail> OrderDetails { get; set; } = new();
    }
}
