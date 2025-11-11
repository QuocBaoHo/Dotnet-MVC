using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRStaffManagement.Models
{
    public class EditStaffViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Staff ID is required")]
        [Display(Name = "Staff ID")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Staff ID must be between 3 and 20 characters")]
        public string StaffId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Staff name is required")]
        [Display(Name = "Staff Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Staff name must be between 2 and 100 characters")]
        public string StaffName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Starting date is required")]
        [Display(Name = "Starting Date")]
        [DataType(DataType.Date)]
        public DateTime StartingDate { get; set; }

        public string? PhotoPath { get; set; } // existing photo path

        [Display(Name = "Upload Photo")]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif" }, ErrorMessage = "Only image files (.jpg, .jpeg, .png, .gif) are allowed")]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = "File size cannot exceed 2MB")]
        public IFormFile? Photo { get; set; } // optional on edit
    }
}
