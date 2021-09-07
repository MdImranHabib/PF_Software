using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.ViewModel
{

    public class VM_UserInfo
    {
        public Guid UserId { get; set; }


        [DataType(DataType.EmailAddress, ErrorMessage = "Please input valid email address")]
        public string Email { get; set; }


        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public int BranchID { get; set; }
        public bool IsActive { get; set; }
        public bool EmailNotificationActive { get; set; }

        [StringLength(15, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        //[Required(ErrorMessage = "Department name is required")]
        public string DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        [Required]
        public int? CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string IdentificationNumber { get; set; }
    }

    public class VM_UserRole
    {
        public string RoleName { get; set; }
    }

    public class VM_RolesInModules
    {
        public int PageID { get; set; }
        public int RoleID { get; set; }
        public string ModuleName { get; set; }
        public string AccessPage { get; set; }
        public string PageName { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanVisit { get; set; }
        public bool CanExecute { get; set; }
    }
}
