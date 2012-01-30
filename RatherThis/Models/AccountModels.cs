using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RatherThis.Custom;

namespace RatherThis.Models
{

    #region Models

    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required(ErrorMessage="Please enter email.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid email address.")]
        [StringLength(200, ErrorMessage="Email must be less than 200 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage="Please enter password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }


    public class RegisterModel
    {
        [Required(ErrorMessage="Please pick a username.")]
        [Display(Name = "Username")]
        [StringLength(100, ErrorMessage="Username must be less than 100 characters.")]
        public string UserName { get; set; }

        [Required(ErrorMessage="Please enter email.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid email address.")]
        [StringLength(200, ErrorMessage="Email must be less than 200 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage="Please select gender.")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage="Please enter password.")]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please agree with our Terms and Privacy Policy.")]
        public bool IsAgreeToTerms { get; set; }
    }

    public class EditProfileModel
    {
        [Required(ErrorMessage = "Please pick a username.")]
        [Display(Name = "Username")]
        [StringLength(100, ErrorMessage = "Username must be less than 100 characters.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter email.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid email address.")]
        [StringLength(200, ErrorMessage = "Email must be less than 200 characters.")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserSummaryViewModel
    {
        public string Name { get; set; }
        public int NumQuestions { get; set; }
        public int NumAnswers { get; set; }
        public string Username { get; set; }
        public string GreetingMessage { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage="Please enter email.")]
        [StringLength(200, ErrorMessage="Email must be less than 200 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        public bool IsTokenExpired { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter email.")]
        [ValidatePasswordLength]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class UserDetailViewModel
    {
        public Dictionary<object, string> ResultViewModels { get; set; }
        public int CurrentPage { get; set; }
        public string Filter { get; set; }
        public int TotalPages { get; set; }
        public string Username { get; set; }
        public int NumAsked { get; set; }
        public int NumAnswered { get; set; }
        public string UserGender { get; set; }
    }
    #endregion

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string email, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email, string gender);
        MembershipCreateStatus UpdateUser(Guid userId, string username, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
        Guid GetCurrentUserId();
        bool IsAuthenticated();
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly CustomMembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = (CustomMembershipProvider)provider ?? (CustomMembershipProvider)Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string email, string password)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");

            return _provider.ValidateUser(email, password);
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }


        public MembershipCreateStatus CreateUser(string userName, string password, string email, string gender)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");
            if (gender == null) throw new ArgumentException("Value cannot be null or empty.", "gender");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, gender, out status);
            return status;
        }

        //additional methods
        public bool IsAuthenticated()
        {
            return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
        }


        public Guid GetCurrentUserId()
        {
            if (IsAuthenticated())
            {
                FormsIdentity identity = HttpContext.Current.User.Identity as FormsIdentity;
                //currently storing the userid in the forms authentication ticket's userdata property
                string userData = identity.Ticket.UserData;
                Guid userId = Guid.Empty;
                if (!Guid.TryParse(userData, out userId))
                    throw new FormatException("Could not parse the user data to get the current user's id");
                return userId;
            }
            else
            {
                return Guid.Empty;
            }
        }


        public MembershipCreateStatus UpdateUser(Guid userId, string username, string password, string email)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.UpdateUser(userId, username, password, email, out status);
            return status;
        }
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string email, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string email, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            HttpCookie authCookie = FormsAuthentication.GetAuthCookie(email, createPersistentCookie);
            //get the authentication ticket from the encrypted auth cookie
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
            //create a new FormsAuthenticationTicket that includes our custom data
            MembershipUser user = Membership.GetUser(email);
            string userDataString = user.ProviderUserKey.ToString();
            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, userDataString);
            //update the authcookie value
            authCookie.Value = FormsAuthentication.Encrypt(newTicket);
            //add auth cookie to the cookies collection
            HttpContext.Current.Response.Cookies.Add(authCookie);
            //FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
    #endregion

    #region Validation
    public static class AccountValidation
    {
        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
    {
        private const string _defaultErrorMessage = "'{0}' must be at least {1} characters.";
        private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

        public ValidatePasswordLengthAttribute()
            : base(_defaultErrorMessage)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                name, _minCharacters);
        }

        public override bool IsValid(object value)
        {
            string valueAsString = value as string;
            return (valueAsString != null && valueAsString.Length >= _minCharacters);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            return new[]{
                new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
            };
        }
    }
    #endregion

}
