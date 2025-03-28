using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TranMinhNhut_2280602263.Models;
using System.Linq;

namespace TranMinhNhut_2280602263.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserApiController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            try
            {
                // Validate input
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ", errors = ModelState.Values.SelectMany(v => v.Errors) });
                }

                // Check if email exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Email đã được sử dụng" });
                }

                // Validate role
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    return BadRequest(new { message = "Vai trò không hợp lệ" });
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true // Tự động xác nhận email
                };

                // Thêm validation cho mật khẩu
                var passwordValidator = new PasswordValidator<ApplicationUser>();
                var passwordValidationResult = await passwordValidator.ValidateAsync(_userManager, null, model.Password);
                if (!passwordValidationResult.Succeeded)
                {
                    return BadRequest(new { message = "Mật khẩu không đáp ứng yêu cầu", errors = passwordValidationResult.Errors });
                }

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Thêm role cho user
                    var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        // Nếu thêm role thất bại, xóa user đã tạo
                        await _userManager.DeleteAsync(user);
                        return BadRequest(new { message = "Không thể phân quyền cho người dùng", errors = roleResult.Errors });
                    }

                    // Trả về thông tin user đã tạo
                    return Ok(new
                    {
                        message = "Tạo người dùng thành công",
                        user = new
                        {
                            id = user.Id,
                            email = user.Email,
                            fullName = user.FullName,
                            phoneNumber = user.PhoneNumber,
                            role = model.Role
                        }
                    });
                }

                return BadRequest(new { message = "Tạo người dùng thất bại", errors = result.Errors });
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { message = "Cập nhật thông tin thành công" });

            return BadRequest(new { message = "Cập nhật thất bại", errors = result.Errors });
        }

        // API đổi mật khẩu
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
                return Ok(new { message = "Đổi mật khẩu thành công" });

            return BadRequest(new { message = "Đổi mật khẩu thất bại", errors = result.Errors });
        }

        // API phân quyền
        [HttpPost("{id}/roles")]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] UpdateRolesModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, model.Roles);

            return Ok(new { message = "Cập nhật vai trò thành công" });
        }

        // API xóa người dùng
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(new { message = "Xóa người dùng thành công" });

            return BadRequest(new { message = "Xóa người dùng thất bại", errors = result.Errors });
        }
    }

    public class CreateUserModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public string Role { get; set; }
    }

    public class UpdateUserModel
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class UpdateRolesModel
    {
        public List<string> Roles { get; set; }
    }

}

