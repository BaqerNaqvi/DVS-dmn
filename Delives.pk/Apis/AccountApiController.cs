using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Delives.pk.Models;
using System.IO;
using System.Threading.Tasks;
using Services.Models;
using Services.Services;
using System.Data.Entity.Spatial;

namespace Delives.pk.Apis
{
    public class AccountApiController : ApiController
    {
        #region Private
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        public AccountApiController()
        {
        }

        public AccountApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }


        private string GetImageUrl(string userId)
        {
            var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);  // https : localhost

            string filename = HttpContext.Current.Server.MapPath("~/Content/ProfileImages/" + userId + ".png");  // C:Projects//

            if (!File.Exists(filename))
            {
                filename = baseUrl + "/Content/ProfileImages/defaultImage.png";
            }
            else
            {
                filename = baseUrl + "/Content/ProfileImages/" + userId + ".png";
            }
            return filename;
        }

        #endregion

        [Route("api/Account/Registration")]
        [System.Web.Mvc.HttpPost]
        public ResponseModel Register(RegisterViewModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.PhoneNumber,
                        // Email = model.PhoneNumber+"@delivers.pk",
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        CreationTime = CommonService.GetSystemTime(),
                        EditTime = CommonService.GetSystemTime(),
                        PhoneNumber = model.PhoneNumber,
                        Type = 1,    // 1-> user , 0 ->  delivery boy
                        IsApproved = true,
                        Status = true
                    };
                    var result = UserManager.Create(user, model.Password);
                    if (result.Succeeded)
                    {
                        response.Success = true;
                        response.Data = new {UserId= user.Id , PhoneNumber= model.PhoneNumber };
                        response.Messages.Add("Verification code has been sent");
                        GeneratePhoneCodeApiMethod(user.Id, model.PhoneNumber);
                    }
                    else
                    {
                        response.Messages.AddRange(result.Errors);
                    }
                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }          
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }   
            }
            return response;
        }

        [Authorize]
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/VerifyPhoneNumber")]
        public ResponseModel VerifyPhone(VerifyPhoneNumberCustomeModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };

            if (model == null || string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Code))
            {
                return new ResponseModel
                {
                    Messages = new List<string> { "Data not mapped" },
                };
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.FindById(model.UserId);
                    if (user != null)
                    {
                        var status = UserManager.ChangePhoneNumber(user.Id, user.PhoneNumber, model.Code);
                        if (status.Succeeded)
                        {
                            user.PhoneNumberConfirmed = true;
                            UserManager.Update(user);
                            response.Success = true;
                            response.Messages.Add("Your phone number has been verified.");
                        }
                        else
                        {
                            response.Messages.Add("Invalid Code");
                        }
                    }
                    else
                    {
                        response.Messages.Add("User not found");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }


      
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/GetPhoneNumberCode")]
        public ResponseModel ObtainPhoneNumberCode(GetPhoneNumberCodeModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.FindByName(model.PhoneNumber);
                    if (user != null)
                    {
                        GeneratePhoneCodeApiMethod(user.Id, user.PhoneNumber);
                        response.Success = true;
                        response.Messages.Add("Code has been sent");
                    }
                    else
                    {
                        response.Messages.Add("User not found");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }
       

        [Authorize]
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/Login")]
        public ResponseModel_Login Login(SignInModelForApis model)
        {
            var response = new ResponseModel_Login
            {
                Success = false,
                Messages = new List<string>(),
                Code=0
            };

            if (ModelState.IsValid)
            {                
                try
                {
                    var user = UserManager.Find(model.PhoneNumber, model.Password);
                    if (user != null)
                    {
                        if (!user.PhoneNumberConfirmed)
                        {
                            response.Success = false;
                            response.Messages.Add("Please verify phone number!");
                            response.Data = user;
                            response.Code = 2;
                        }
                        else if(user.Type != 1)
                        {
                            response.Success = false;
                            response.Messages.Add("Invalid Login Type");
                            response.Code = 3;
                            return response;
                        }
                        else
                        {
                            var formatedUser = user.MappUser();                           
                            formatedUser.ProfileImageUrl = GetImageUrl(formatedUser.Id);
                            response.Data = formatedUser;
                            response.Success = true;
                            response.Messages.Add("Successfully logged in.");
                            response.Code = 1;
                        }
                    }
                    else
                    {
                        response.Code = 4;
                        response.Messages.Add("Invalid phone number/password");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }
       


        [System.Web.Mvc.HttpPost]
        [Route("api/Account/IsUserApproved")]
        public ResponseModel_Approved CheckUserStatus(SignInModelForApis model)
        {
            var response = new ResponseModel_Approved
            {
                Success = false,
                Messages = new List<string>()
            };

            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.Find(model.PhoneNumber, model.Password);
                    if (user != null)
                    {
                        if (!user.PhoneNumberConfirmed)
                        {
                            response.Success = false;
                            response.Messages.Add("Please verify phone number!");
                            response.Data = user;
                            response.Code = 2;
                        }
                        else if (!user.IsApproved)
                        {
                            response.Success = false;
                            response.Messages.Add("Please contact admin to approve your user");
                            response.Data = user;
                            response.Code = 5;

                        }
                        else
                        {
                            response.Success = true;
                            response.Messages.Add("User is verified and approved");
                            response.Data = user;
                            response.Code = 1;

                        }
                    }
                    else
                    {
                        response.Messages.Add("Invalid phone number/password");
                        response.Code = 4;

                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }

        [Authorize]
        [Route("api/Account/ChangePassword")]
        public ResponseModel ChangePassword(ChangePasswordModel usermodel)
        {
            #region validation
            if (usermodel == null || string.IsNullOrEmpty(usermodel.NewPassword) || string.IsNullOrEmpty(usermodel.UserId))
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Data not mapped" },
                    Data = usermodel
                };
            }

            if (usermodel.NewPassword != usermodel.ConfirmNewPassword)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Password does not match" },
                    Data = usermodel
                };
            } 
            #endregion

            ApplicationUser user = UserManager.FindById(usermodel.UserId);
            if (user == null)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "User not found" },
                    Data = usermodel
                };
            }
            user.PasswordHash = UserManager.PasswordHasher.HashPassword(usermodel.NewPassword);
            var result = UserManager.Update(user);
            if (!result.Succeeded)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Failed to update password" },
                    Data = usermodel
                };
            }
            return new ResponseModel
            {
                Success = true,
                Messages = new List<string> { "Password updated" },
                Data = usermodel
            };
        }


        [Authorize]
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/ChangePhoneNumber")]
        public ResponseModel ChangePhoneNumber(ChangePhonenumberModel usermodel)
        {
            #region validation
            if (usermodel == null || string.IsNullOrEmpty(usermodel.NewPhoneNumber) ||
                string.IsNullOrEmpty(usermodel.UserId))
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Data not mapped" },
                    Data = usermodel
                };
            }

            #endregion

            var userObj = UserManager.FindById(usermodel.UserId);
            if (userObj != null)
            {
                userObj.PhoneNumber = usermodel.NewPhoneNumber;
                userObj.UserName = usermodel.NewPhoneNumber;
                userObj.PhoneNumberConfirmed = false;
                var result =  UserManager.Update(userObj);
                if (result.Succeeded)
                {
                    GeneratePhoneCodeApiMethod(userObj.Id, userObj.PhoneNumber);
                    return new ResponseModel
                    {
                        Success = true,
                        Messages = new List<string> { "Verification code has been sent to your new mobile number" },
                        Data = usermodel
                    };
                }
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Could not update mobile number" },
                    Data = usermodel
                };
            }
            return new ResponseModel
            {
                Success = false,
                Messages = new List<string> { "User not found with given User Id" },
                Data = usermodel
            };
        }

        [Authorize]
        [Route("api/Account/IsPhoneNumberVarified")]
        [System.Web.Mvc.HttpPost]
        public ResponseModel IsPhoneNumberVarified(ChangePhonenumberModel usermodel)
        {
            #region validation
            if (usermodel == null ||
                string.IsNullOrEmpty(usermodel.UserId))
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Data not mapped" },
                };
            }

            #endregion

            var userObj = UserManager.FindById(usermodel.UserId);
            if (userObj != null)
            {
                return new ResponseModel
                {
                    Success = userObj.PhoneNumberConfirmed,
                    Messages = new List<string> { "Phone Number Verified: "+ userObj.PhoneNumberConfirmed.ToString() },
                };
            }
            return new ResponseModel
            {
                Success = false,
                Messages = new List<string> { "User not found with given User Id" },
            };
        }


        #region Forgot Password
        [Route("api/User/RequestResetPassword")]
        [System.Web.Mvc.HttpPost]
        public ResponseModel RequestResetPassword(ForgotPasswordRequestModel usermodel)
        {
            if (usermodel == null || string.IsNullOrEmpty(usermodel.PhoneNumber))
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Data not mapped" },
                };
            }
          
            var user = UserManager.FindByName(usermodel.PhoneNumber);
            if (user == null)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "User does not exists with given phone number" },
                };
            }
            GeneratePhoneCodeApiMethod(user.Id, user.PhoneNumber);
            var model = new ChnageUserPasswordResponseModel
            {
                UserId = user.Id,
                PhoneNumber = usermodel.PhoneNumber
            };
            return new ResponseModel
            {
                Success = true,
                Data = model,
                Messages = new List<string> { "Verification code has been sent at mobile number" },
            };

        }



        [Route("api/User/RequestResetPasswordWithCode")]
        [System.Web.Mvc.HttpPost]
        public ResponseModel RequestResetPasswordWithCode(ChnagePasswordWithCodeModel usermodel)
        {
            if (string.IsNullOrEmpty(usermodel?.Code) || string.IsNullOrEmpty(usermodel.UserId)
                 || string.IsNullOrEmpty(usermodel.NewPassword))
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Data not mapped" },
                };
            }

            var user = UserManager.FindById(usermodel.UserId);
            if (user == null)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "User does not exist with user id" },
                };
            }

            var status = UserManager.ChangePhoneNumber(user.Id, user.PhoneNumber, usermodel.Code);
            if (!status.Succeeded)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Invalid/Expired verification code. Try new code" },
                };               
            }

            ApplicationUser appuser = UserManager.FindById(user.Id);
            appuser.PasswordHash = UserManager.PasswordHasher.HashPassword(usermodel.NewPassword);
            var result = UserManager.Update(appuser);
            if (!result.Succeeded)
            {
                return new ResponseModel
                {
                    Success = false,
                    Messages = new List<string> { "Failed to update password" },
                };               
            }

            return new ResponseModel
            {
                Success = true,
                Messages = new List<string> { "Password reset successfully" },
            };           
        }


        public string GeneratePhoneCodeApiMethod(string userId, string mobile)
        {
            var phoneCode = UserManager.GenerateChangePhoneNumberToken(userId, mobile);
            // mobile number 
            mobile = mobile.Substring(1).Replace("-", "");
            mobile = "92" + mobile;
            Services.Services.EmailService.SendSms(mobile, "Your verification code is : " + phoneCode+ ". Visit https://delivers.pk or call 0553828677 for details.");
            return phoneCode;
        }
        #endregion


        [System.Web.Mvc.HttpPost]
        [Route("api/Account/UpdateProfile")]
        public ResponseModel UpdateProfile(UpdateProfileModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.FindById(model.UserId);
                    if (user != null)
                    {
                        if (!string.IsNullOrEmpty(model.FirstName))
                        {
                            user.FirstName = model.FirstName;
                        }
                        if (!string.IsNullOrEmpty(model.LastName))
                        {
                            user.LastName = model.LastName;
                        }
                        if (!string.IsNullOrEmpty(model.Address))
                        {
                            user.Address = model.Address;
                        }
                        if (!string.IsNullOrEmpty(model.Cords) && model.Cords != "")
                        {
                            var latlng = model.Cords.Split('_').ToList();
                            if (latlng.Count == 2)
                            {
                                user.Location = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                            }
                        }

                        var status= UserManager.Update(user);
                        if (status.Succeeded)
                        {
                            response.Success = true;
                            response.Messages.Add("Profile updated");
                        }
                        else
                        {
                            response.Success = false;
                            response.Messages.AddRange(status.Errors);
                        }
                    }
                    else
                    {
                        response.Messages.Add("User not found");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }


        [System.Web.Mvc.HttpPost]
        [Route("api/Account/UpdateProfileImage")]
        public async Task<ResponseModel> UpdateProfilePicture()
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };

            try
            {
                if (HttpContext.Current.Request.Form == null || string.IsNullOrEmpty(HttpContext.Current.Request.Form["UserId"]))
                {
                    response.Messages.Add("UserId is missing!");
                    return response;
                }

                List<string> savedFilePath = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    response.Messages.Add("No multi-part/form-data found!");
                    return response;
                }

                var userid = HttpContext.Current.Request.Form["UserId"];
                string newFileName = userid + ".png";

                string rootPath = HttpContext.Current.Server.MapPath("~/Content/ProfileImages");
                var provider = new MultipartFileStreamProvider(rootPath);
                var task = await Request.Content.ReadAsMultipartAsync(provider);
                var fileFound = false;

                foreach (MultipartFileData dataitem in provider.FileData)
                {
                    try
                    {
                        string FileName = dataitem.Headers.ContentDisposition.FileName;
                        if (!string.IsNullOrEmpty(FileName))
                        {
                            FileName = FileName.Replace("\"", "");
                            var FileLength = new FileInfo(dataitem.LocalFileName);
                            string ext = Path.GetExtension(FileName);
                            var newFilePath = Path.Combine(rootPath, newFileName);
                            if (File.Exists(newFilePath))
                            {
                                File.Delete(newFilePath);
                            }
                            File.Move(dataitem.LocalFileName, newFilePath);
                            fileFound = true;
                        }
                        else
                        {
                            File.Delete(dataitem.LocalFileName);
                        }
                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }
                if (fileFound)
                {
                    response.Success = true;
                    var baseUrl = Request.RequestUri.GetLeftPart(UriPartial.Authority);

                    response.Messages.Add(baseUrl+"/Content/ProfileImages/" + newFileName);
                }
                else
                {
                    response.Messages.Add("Image file not found!");
                }

            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened!");
            }

            return response;

        }


        [System.Web.Mvc.HttpPost]
        [Route("api/Account/UpdateStatus")]
        public ResponseModel UpdateStatus(UpdateStatusModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.FindById(model.UserId);
                    if (user != null)
                    {
                        user.Status = model.Status;                       
                        var status = UserManager.Update(user);
                        if (status.Succeeded)
                        {
                            response.Success = true;
                            response.Messages.Add("Status updated");
                        }
                        else
                        {
                            response.Success = false;
                            response.Messages.AddRange(status.Errors);
                        }
                    }
                    else
                    {
                        response.Messages.Add("User not found");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }


        #region Delivery Boy

        [Route("api/Account/Delivery/Registration")]
        [System.Web.Mvc.HttpPost]
        public ResponseModel DeliveryRegister(RegisterViewModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.PhoneNumber,
                        // Email = model.PhoneNumber+"@delivers.pk",
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        CreationTime = CommonService.GetSystemTime(),
                        EditTime = CommonService.GetSystemTime(),
                        PhoneNumber = model.PhoneNumber,
                        IsApproved = false,
                        CNIC = model.CNIC,
                        Status = false,
                        Type = 0  // 1-> user , 0 ->  delivery boy
                    };
                    var result = UserManager.Create(user, model.Password);
                    if (result.Succeeded)
                    {
                        response.Success = true;
                        response.Data = new { UserId = user.Id, PhoneNumber = model.PhoneNumber };
                        response.Messages.Add("Verification code has been sent");
                        GeneratePhoneCodeApiMethod(user.Id, model.PhoneNumber);
                    }
                    else
                    {
                        response.Messages.AddRange(result.Errors);
                    }
                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }


        [Authorize]
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/Delivery/Login")]
        public ResponseModel DeliveryLogin(SignInModelForApis model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (ModelState.IsValid)
            {
                if (model.Type != 0)
                {
                    response.Messages.Add("Invalid Login Type");
                    return response;
                }

                try
                {
                    var user = UserManager.Find(model.PhoneNumber, model.Password);
                    if (user != null)
                    {
                        if (!user.IsApproved)
                        {
                            response.Success = false;
                            response.Messages.Add("Please contact admin for account approval");
                        }
                        else
                        {
                            if (!user.PhoneNumberConfirmed)
                            {
                                response.Success = false;
                                response.Messages.Add("Please verify phone number!");
                                response.Data = user;
                            }
                            else if (user.Type != 0)
                            {
                                response.Success = false;
                                response.Messages.Add("Invalid Login Type");
                                return response;
                            }
                            else
                            {
                                var formatedUser = user.MappUser();                                
                                formatedUser.ProfileImageUrl = GetImageUrl(formatedUser.Id);
                                response.Success = true;
                                response.Messages.Add("Successfully logged in.");
                                response.Data = formatedUser;
                            }
                        }
                    }
                    else
                    {
                        response.Messages.Add("Invalid phone number/password");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }


        /// <summary>
        /// Get User Info - Dboy and regular user as well
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Account/GetUserInfo")]
        public ResponseModel DboyInfo(DboyInfoRequesrModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.UserId))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }
            else
            {
                try
                {
                    var user = UserManager.FindById(model.UserId);
                    if (user != null)
                    {
                        var formatedUser = user.MappUser_Short();
                        formatedUser.ProfileImageUrl = GetImageUrl(formatedUser.Id);
                        response.Success = true;
                        response.Messages.Add("Success");
                        response.Data = formatedUser;
                    }
                    else
                    {
                        response.Messages.Add("User not found");
                    }                     
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }

        #endregion


        #region Restaurant

        [Authorize]
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/Login_Rest")]
        public ResponseModel RestaurantLogin(SignInModelForApis model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };

            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.Find(model.PhoneNumber, model.Password);
                    if (user != null)
                    {
                        if (!user.PhoneNumberConfirmed)
                        {
                            response.Success = false;
                            response.Messages.Add("Please verify phone number!");
                            response.Data = user;
                        }
                        else if (user.Type != 2)
                        {
                            response.Success = false;
                            response.Messages.Add("Invalid Login Type");
                            return response;
                        }
                        else
                        {
                            var formatedUser = user.MappUser();
                            formatedUser.ProfileImageUrl = GetImageUrl(formatedUser.Id);
                            var rest = RestuarantService.GetRestaurantForUserId(formatedUser.Id);
                            formatedUser.Restaurant = rest;

                            response.Data = formatedUser;
                            response.Success = true;
                            response.Messages.Add("Successfully logged in.");
                        }
                    }
                    else
                    {
                        response.Messages.Add("Invalid phone number/password");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }


        [Authorize]
        [System.Web.Mvc.HttpPost]
        [Route("api/Account/Logout_Rest")]
        public ResponseModel RestaurantLogout(SignInModelForApis model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };

            if (ModelState.IsValid)
            {
                try
                {
                    var user = UserManager.Find(model.PhoneNumber, model.Password);
                    if (user != null)
                    {
                        if (!user.PhoneNumberConfirmed)
                        {
                            response.Success = false;
                            response.Messages.Add("Please verify phone number!");
                            response.Data = user;
                        }
                        else if (user.Type != 2)
                        {
                            response.Success = false;
                            response.Messages.Add("Invalid Login Type");
                            return response;
                        }
                        else
                        {
                            var formatedUser = user.MappUser();
                            formatedUser.ProfileImageUrl = GetImageUrl(formatedUser.Id);
                            response.Data = formatedUser;
                            response.Success = true;
                            response.Messages.Add("Successfully logged out.");
                        }
                    }
                    else
                    {
                        response.Messages.Add("Invalid phone number/password");
                    }

                }
                catch (Exception error)
                {
                    response.Messages.Add(error.InnerException.Message);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(obj => obj.Errors))
                {
                    response.Messages.Add(error.ErrorMessage);
                }
            }
            return response;
        }

        #endregion

    }
}