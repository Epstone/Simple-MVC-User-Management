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
    /// <summary>
    /// You can define the used membership provider by setting this Property.
    /// </summary>
    public static MembershipProvider MembershipProvider { get; set; }

    /// <summary>
    /// You can define the used role provider by setting this Property.
    /// </summary>
    public static RoleProvider RoleProvider { get; set; }

    /// <summary>
    /// The account service which should be used for any provider interaction
    /// </summary>
    public IUserAccountService _accountService { get; set; }

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
    public JsonResult DeleteUser(string username)
    {
      MyJsonResult result;

      try
      {
        _accountService.DeleteUser(username);
        result = MyJsonResult.CreateSuccess("The user " + username + " has been deleted.");
      }
      catch (Exception ex)
      {
        result = MyJsonResult.CreateError(ex);
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
        result = MyJsonResult.CreateError(ex);
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

    /// <summary>
    /// Unlocks the user by the specified username.
    /// </summary>
    /// <param name="userName">The user to unlock</param>
    /// <returns>Success or error information</returns>
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

      return Json(result);
    }

    /// <summary>
    /// Gets a list of roles including the information wether the user is in that role or not.
    /// </summary>
    /// <param name="username">The user which role information should be gathered.</param>
    /// <returns>A list of roles including the information wether the user is in that role or not.</returns>
    public JsonResult GetUserRoleStatus(string username)
    {
      if (string.IsNullOrEmpty(username))
      {
        throw new ArgumentException("No user name specified in request");
      }

      var allRoles = this._accountService.GetAllRoles();
      var userRoles = this._accountService.GetRolesForUser(username);

      var result = new MyJsonResult()
      {
        data = from role in allRoles
               select new
               {
                 rolename = role,
                 isInRole = userRoles.Contains(role)
               },
        isSuccess = true
      };


      return Json(result);
    }

    /// <summary>
    /// Adds or removes a role for a user account.
    /// </summary>
    /// <param name="username">The user which roles should be modified.</param>
    /// <param name="rolename">The role which should be added or removed.</param>
    /// <param name="isInRole">The new role status for the user account. If false, the role will be deleted for the user account.</param>
    /// <returns></returns>
    public JsonResult AddRemoveRoleForUser(string username, string rolename, bool isInRole)
    {
      MyJsonResult result;

      try
      {
        _accountService.AddRemoveRoleForUser(username, rolename, isInRole);

        var action = isInRole ? "added" : "removed";
        var msg = string.Format("The role {0} has been {1} for user {2}.", rolename, action, username);

        result = MyJsonResult.CreateSuccess(msg);
      }
      catch (ArgumentException ex)
      {
        result = MyJsonResult.CreateError("Could not remove role for user: " + ex.Message);
      }

      return Json(result);
    }

    const string _isAuthorized = "IsAuthorizedForUserManagement";

    /// <summary>
    /// Handles user authorization to access the user account management controller methods for each single HTTP Request. 
    /// </summary>
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

    const string userMgmtControllerName = "UserManagement";

    /// <summary>
    /// Checks wether the user management controller is the target for the current request
    /// </summary>
    /// <returns>Returns true if the user management controller will be called in the current request</returns>
    public static bool IsTargeted()
    {
      HttpContext context = System.Web.HttpContext.Current;
      RouteData routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));

      if (routeData != null
          && routeData.Values != null)
      {
        object controllerName = string.Empty;
        bool hasController = routeData.Values.TryGetValue("controller", out controllerName);

        return (hasController && userMgmtControllerName == (controllerName as string));
      }

      return false;
    }
  }
}
