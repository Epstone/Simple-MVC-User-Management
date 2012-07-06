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
      this._membership = membershipProvider ?? Membership.Provider;
      this._roleProvider = roleProvider ?? Roles.Provider;
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
    public void DeleteUser(string  username)
    {
      _membership.DeleteUser(username, true);
    }

    public MembershipUser CreateUser(string username, string password, string email, out MembershipCreateStatus createStatus)
    {
      return _membership.CreateUser(username, password, email, null, null, true, null, out createStatus);
    }

    /// <summary>
    /// Gets a list of all the roles for the configured applicationName.
    /// </summary>
    /// <returns>  A string array containing the names of all the roles stored in the data source
    ///    for the configured applicationName.</returns>
    public string[] GetAllRoles()
    {
      return _roleProvider.GetAllRoles();

    }

    /// <summary>
    ///  Gets a list of the roles that a specified user is in for the configured applicationName.
    /// </summary>
    /// <param name="username">The user to return a list of roles for.</param>
    /// <returns> A string array containing the names of all the roles that the specified user
    ///    is in for the configured applicationName.</returns>
    public string[] GetRolesForUser(string username)
    {
      return _roleProvider.GetRolesForUser(username);
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


    public bool UnlockUser(string userName)
    {
      return _membership.UnlockUser(userName);
    }





    public void AddRemoveRoleForUser(string username, string rolename, bool isInRole)
    {

      if (isInRole)
        _roleProvider.AddUsersToRoles(new string[] { username }, new string[] { rolename });
      else
        _roleProvider.RemoveUsersFromRoles(new string[] { username }, new string[] { rolename });
    }
  }
}
