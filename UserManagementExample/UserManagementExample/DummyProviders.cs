using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Moq;

namespace UserManagementExample
{
  public static class DummyProviders
  {
    public static RoleProvider CreateDummyRoleProvider()
    {
      Mock<RoleProvider> roleProvider = new Mock<RoleProvider>();

      //setup GetAllRoles()
      roleProvider.Setup(x => x.GetAllRoles()).Returns(new string[] { "Admins", "Guests", "Users", "Others" });

      //setup DeleteRole()
      roleProvider.Setup(x => x.DeleteRole(It.IsAny<string>(), It.IsAny<bool>())).Returns(true);

      return roleProvider.Object;
    }

    public static MembershipProvider CreateDummyMembershipProvider()
    {
      Mock<MembershipProvider> membershipMock = new Mock<MembershipProvider>();

      membershipMock.SetupGet(x => x.Name).Returns("test");

      // setup CreateUser()
      MembershipCreateStatus membershipCreateStatus = MembershipCreateStatus.Success;
      membershipMock.Setup(x => x.CreateUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, true, null, out membershipCreateStatus))
          .Returns((string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, MembershipCreateStatus status) => CreateDummyUser(42, username, email, DateTime.Now));

      // setup GetAllUsers()
      var total = It.IsAny<int>();
      membershipMock.Setup(x => x.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), out total)).Returns(() => GetDummyUsers());

      //setup DeleteUser()
      membershipMock.Setup(x=>x.DeleteUser(It.IsAny<string>(),It.IsAny<bool>())).Returns(true);

      //setup 
      membershipMock.Setup(x => x.GetUser(It.IsAny<object>(), It.IsAny<bool>())).Returns(CreateDummyUser(42, null, null, DateTime.Now));
      return membershipMock.Object;
    }

    /// <summary>
    /// Builds a membership collection full of mocked users
    /// </summary>
    /// <returns></returns>
    private static MembershipUserCollection GetDummyUsers()
    {
      MembershipUserCollection users = new MembershipUserCollection();
      var now = DateTime.Now;

      var start = DateTime.Now.Second;
      for (int i = 0; i < 500; i++)
      {
        var dummyUser = CreateDummyUser(i, "Dummy User " + i, "just@a-dummy" + i + ".com", now.AddMinutes(i * (-1)));
        users.Add(dummyUser);
      }
      var end = DateTime.Now.Second;

      return users;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    private static MembershipUser CreateDummyUser(int id, string username, string email, DateTime registrationDate)
    {
      var now = DateTime.Now;
      Mock<MembershipUser> dummyUserMock = new Mock<MembershipUser>();

      dummyUserMock.SetupGet(u => u.ProviderUserKey).Returns(id);
      dummyUserMock.SetupGet(u => u.UserName).Returns(username);
      dummyUserMock.SetupGet(u => u.Email).Returns(email);
      dummyUserMock.SetupGet(u => u.LastLoginDate).Returns(now);
      dummyUserMock.SetupGet(u => u.CreationDate).Returns(now);
      dummyUserMock.SetupGet(u => u.IsLockedOut).Returns(false);

      return dummyUserMock.Object;
    }

  }
}
