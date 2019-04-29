using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Delives.pk.Models
{
    public class ChangePasswordModel
    {
        public string UserId { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmNewPassword { get; set; }
    }


    public class ChangePhonenumberModel
    {
        public string UserId { get; set; }

        public string NewPhoneNumber { get; set; }

    }


    public class ForgotPasswordRequestModel
    {

        public string PhoneNumber { get; set; }

    }

    public class ChnageUserPasswordResponseModel
    {
        public string UserId { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ChnagePasswordWithCodeModel
    {
        public string UserId { get; set; }

        public string Phone { get; set; }

        public string Code { get; set; }

        public string NewPassword { get; set; }
    }
}