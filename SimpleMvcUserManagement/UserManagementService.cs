using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Web.Mvc;
using System.Web;

namespace SimpleMvcUserManagement
{
  public class UserAccountService : IUserAccountService
  {
    MembershipProvider _membership;
    RoleProvider _roleProvider;

    public UserAccountService()
      : this(null, null)
    {

    }
    public UserAccountService(MembershipProvider membershipProvider, RoleProvider roleProvider)
    {
      this._membership = membershipProvider ?? UserManagementController.MembershipProvider ?? Membership.Provider;
      this._roleProvider = roleProvider ?? UserManagementController.RoleProvider ?? Roles.Provider;
    }

    public MembershipUserCollection GetAllUsers(int page, int size, out int totalRecords)
    {
      return this._membership.GetAllUsers(page, size, out totalRecords);
    }

    /// <summary>
    /// Returns all registered users.
    /// </summary>
    /// <returns>All registered membership users.</returns>
    public MembershipUserCollection GetAllUsers()
    {
      int totalRecords = 0;
      return GetAllUsers(0, 0x7fffffff, out totalRecords);
    }

    /// <summary>
    /// Deletes a user all his membership information by User ID.
    /// </summary>
    /// <param name="userId">The users ProviderUserKey</param>
    public void DeleteUser(object userId)
    {
      var userName = this._membership.GetUser(userId, false).UserName;
      _membership.DeleteUser(userName, true);
    }

    public MembershipUser CreateUser(string username, string password, string email, out MembershipCreateStatus createStatus)
    {
      return _membership.CreateUser(username, password, email, null, null, true, null, out createStatus);
    }


    public string[] GetAllRoles()
    {
      return _roleProvider.GetAllRoles();
     
    }


    public void AddUserToRoles(MembershipUser user, string[] roles)
    {
      _roleProvider.AddUsersToRoles(new string[] { user.UserName }, roles);
    }


    public void CreateRole(string roleName)
    {
      _roleProvider.CreateRole(roleName);
    }


    public bool DeleteRole(string roleName, bool throwOnPopulatedRole)
    {
      return _roleProvider.DeleteRole(roleName, throwOnPopulatedRole);
    }
  }
}
