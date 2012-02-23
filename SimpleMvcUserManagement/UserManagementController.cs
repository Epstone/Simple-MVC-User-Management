using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;
using System.Collections;
using System.Web;
using System.Web.UI;


namespace SimpleMvcUserManagement
{
  [UserManagementAuthorization]
  public class UserManagementController : Controller
  {

    public static MembershipProvider MembershipProvider { get; set; }
    public static RoleProvider RoleProvider { get; set; }
    IUserAccountService _accountService { get; set; }

    public static void RegisterMe()
    {

      var routes = RouteTable.Routes;

      using (routes.GetWriteLock())
      {
        routes.MapRoute("SimpleUserManagementRoute",
          "SimpleUserManagement/{action}",
          new { controller = "UserManagement", action = "GetAllUsers" },
          new string[] { "SimpleMvcUserManagement" });
      }

    }

    /// <summary>
    /// Initialize controller and the account service
    /// </summary>
    /// <param name="requestContext">Current http request context</param>
    protected override void Initialize(RequestContext requestContext)
    {
      base.Initialize(requestContext);

      var roleProvider = RoleProvider ?? Roles.Provider;
      var membershipProvider = MembershipProvider ?? Membership.Provider;

      if (this._accountService == null) this._accountService = new UserAccountService(membershipProvider, roleProvider);
    }

    /// <summary>
    /// Returns a list of all current users
    /// </summary>
    /// <returns>All known membership users</returns>
    public JsonResult GetAllUsers()
    {
      var users = _accountService.GetAllUsers();
      var result = from MembershipUser user in users
                   select CreateJsonUserObject(user);

      return Json(result);
    }

    /// <summary>
    /// Creates an anonymous user object for later json serialization
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private object CreateJsonUserObject(MembershipUser user)
    {
      return new
      {
        id = user.ProviderUserKey,
        name = user.UserName,
        registrationDate = user.CreationDate.ToString("yyyy-MM-dd"),
        email = user.Email,
        isLockedOut = user.IsLockedOut
      };
    }

    /// <summary>
    /// Deletes a user from through the membership service.
    /// </summary>
    /// <param name="userId">The id of the user account which should be deleted.</param>
    /// <returns>Result info for the user account deletion action.</returns>
    public JsonResult DeleteUser(string userId)
    {
      MyJsonResult result;

      try
      {
        _accountService.DeleteUser(userId);
        result = MyJsonResult.CreateSuccess("The user with the id " + userId + " has been deleted.");
      }
      catch (Exception ex)
      {
        result = MyJsonResult.CreateError(ex.Message);
      }

      return Json(result);
    }

    /// <summary>
    /// Creates a new user account
    /// </summary>
    /// <param name="username">A unique username</param>
    /// <param name="password">A hopefully secure password</param>
    /// <param name="email">A unique email address</param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public JsonResult CreateUser(string username, string password, string email)
    {
      MembershipCreateStatus status;
      MyJsonResult result;

      //create user
      var user = _accountService.CreateUser(username, password, email, out status);

      if (status == MembershipCreateStatus.Success)
      {
        result = MyJsonResult.CreateSuccess("The user account for " + username + " has been created.");
        result.data = CreateJsonUserObject(user);
      }
      else
      {
        result = MyJsonResult.CreateError(AccountValidation.ErrorCodeToString(status));
      }

      return Json(result);
    }

    /// <summary>
    /// Returns an array containing all known role names.
    /// </summary>
    /// <returns></returns>
    public JsonResult GetAllRoles()
    {
      string[] roles = _accountService.GetAllRoles();

      return Json(roles);
    }

    /// <summary>
    /// Creates a role by a given name.
    /// </summary>
    /// <param name="roleName">The role to create.</param>
    /// <returns>Information about the user creation success</returns>
    public JsonResult CreateRole(string roleName)
    {
      MyJsonResult result;
      try
      {
        _accountService.CreateRole(roleName);
        result = MyJsonResult.CreateSuccess("The role " + roleName + " has been created.");
      }
      catch (Exception ex)
      {
        result = MyJsonResult.CreateError(ex.Message);
      }

      return Json(result);
    }

    /// <summary>
    /// Deletes a role by name.
    /// </summary>
    /// <param name="roleName">The role to delete.</param>
    /// <param name="allowPopulatedRoleDeletion">Allow role deletion even if users are still mapped to it.</param>
    /// <returns>Returns result information about the role deletion.</returns>
    public JsonResult DeleteRole(string roleName, bool allowPopulatedRoleDeletion)
    {
      MyJsonResult result;

      try
      {
        if (_accountService.DeleteRole(roleName, !allowPopulatedRoleDeletion))
          result = MyJsonResult.CreateSuccess("The role " + roleName + " has been deleted.");
        else
          result = MyJsonResult.CreateError("The role " + roleName + " could not be deleted.");
      }
      catch (Exception ex)
      {
        result = MyJsonResult.CreateError(ex.Message);
      }


      return Json(result);
    }

    public JsonResult UnlockUser(string userName)
    {
      MyJsonResult result;

      try
      {
        if (_accountService.UnlockUser(userName))
          result = MyJsonResult.CreateSuccess("The account for " + userName + " has been unlocked");
        else
          result = MyJsonResult.CreateError("Could not unlock the account for " + userName);
      }
      catch (Exception ex)
      {
        result = MyJsonResult.CreateError(ex.Message);
      }

      return Json( result);
    }


    const string _isAuthorized = "IsAuthorizedForUserManagement";

    public static bool IsRequestAuthorized
    {

      get
      {
        IDictionary items = System.Web.HttpContext.Current.Items;
        if (!items.Contains(_isAuthorized))
        {
          items[_isAuthorized] = false;
        }
        return (bool)items[_isAuthorized];
      }

      set
      {
        IDictionary items = System.Web.HttpContext.Current.Items;
        if (!items.Contains(_isAuthorized))
        {
          items[_isAuthorized] = value;
        }
        else
        {
          items[_isAuthorized] = value;
        }
      }
    }
  }
}
