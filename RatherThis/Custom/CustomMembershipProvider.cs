using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Security;
using Ninject;
using RatherThis.Domain.Abstract;
using RatherThis.Domain.Entities;

namespace RatherThis.Custom
{
    public class CustomMembershipProvider : MembershipProvider
    {
        [Inject]
        public IUserRepository UserRepository { get; set; }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public MembershipUser CreateUser(string username, string password, string email, string gender, out MembershipCreateStatus status)
        {
            //check if username exists
            if (UserRepository.Users.Where(u => u.Username.Equals(username)).Count() > 0)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
            //check if email exists
            if (UserRepository.Users.Where(u => u.Email.Equals(email)).Count() > 0)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            User newUser = new User();
            newUser.Email = email;
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
            newUser.Username = username;
            newUser.Gender = gender;
            UserRepository.SaveUser(newUser);
            status = MembershipCreateStatus.Success;

            return newUser.ConvertToMembershipUser();
        }

        public MembershipUser CreateUser(string username, string password, string email, string gender, string accountType, string externalID, out MembershipCreateStatus status)
        {
            //check if username exists
            /* don't do this check if we're using this create user method, because we're auto generating the username
            if (UserRepository.Users.Where(u => u.Username.Equals(username)).Count() > 0)
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }*/
            //check if email exists
            if (UserRepository.Users.Where(u => u.Email.Equals(email)).Count() > 0)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            User newUser = new User();
            newUser.Email = email;
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
            newUser.Username = username;
            newUser.Gender = gender;
            newUser.AccountType = accountType;
            newUser.ExternalUserID = externalID;
            UserRepository.SaveUser(newUser);
            status = MembershipCreateStatus.Success;

            return newUser.ConvertToMembershipUser();
        }

        public MembershipUser UpdateUser(Guid userId, string username, string password, string email, out MembershipCreateStatus status)
        {
            User currentUser = UserRepository.Users.Where(u => u.UserID == userId).FirstOrDefault();
            //if we are updating username,  make sure username is unique
            if (username != currentUser.Username)
            {
                //check if username exists
                if (UserRepository.Users.Where(u => u.Username.Equals(username) && u.UserID != userId).Count() > 0)
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                    return null;
                }
            }

            if (email != currentUser.Email)
            {
                //check if email exists
                if (UserRepository.Users.Where(u => u.Email.Equals(email) && u.UserID != userId).Count() > 0)
                {
                    status = MembershipCreateStatus.DuplicateEmail;
                    return null;
                }
            }

            
            currentUser.Email = email;
            if (!string.IsNullOrEmpty(password))
            {
                currentUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
            }
            currentUser.Username = username;
            UserRepository.SaveUser(currentUser);
            status = MembershipCreateStatus.Success;

            return currentUser.ConvertToMembershipUser();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string email, bool userIsOnline)
        {
            User user = UserRepository.Users.Where(u => u.Email.Equals(email)).FirstOrDefault();
            if (user == null)
                return null;

            return user.ConvertToMembershipUser();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 4; }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string email, string password)
        {
            User user = UserRepository.Users.Where(u => u.Email.Equals(email)).FirstOrDefault();
            if (user == null)
                return false;
            bool isPwMatch = BCrypt.Net.BCrypt.Verify(password, user.Password);

            return isPwMatch;
        }
    }
}