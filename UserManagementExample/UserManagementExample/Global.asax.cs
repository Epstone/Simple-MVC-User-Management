using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SimpleMvcUserManagement;
using Moq;
using System.Web.Security;

namespace UserManagementExample
{
  // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
  // visit http://go.microsoft.com/?LinkId=9394801

  public class MvcApplication : System.Web.HttpApplication
  {
    public static void RegisterGlobalFilters(GlobalFilterCollection filters)
    {
      filters.Add(new HandleErrorAttribute());
    }

    public static void RegisterRoutes(RouteCollection routes)
    {
      routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

      UserManagementController.RegisterMe();


      routes.MapRoute(
          "Default", // Route name
          "{controller}/{action}/{id}", // URL with parameters
          new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
      );

    }

    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();

      RegisterGlobalFilters(GlobalFilters.Filters);
      RegisterRoutes(RouteTable.Routes);

      //add dummy membership and role provider for the demo page

      UserManagementController.MembershipProvider = CreateDummyMembershipProvider();
      UserManagementController.RoleProvider = CreateDummyRoleProvider();

    }

    private RoleProvider CreateDummyRoleProvider()
    {
      Mock<RoleProvider> roleProvider = new Mock<RoleProvider>();

      roleProvider.Setup(x => x.GetAllRoles()).Returns(new string[] { "Admins", "Guests", "Users", "Others" });
      //roleProvider.Setup(x=> x.CreateRole()

      return roleProvider.Object;
    }

    private static MembershipProvider CreateDummyMembershipProvider()
    {
      var now = DateTime.Now;
      Mock<MembershipProvider> membershipMock = new Mock<MembershipProvider>();

      MembershipCreateStatus membershipCreateStatus = MembershipCreateStatus.Success;

      // setup CreateUser()
      membershipMock.Setup(x => x.CreateUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null, true, 42, out membershipCreateStatus))
          .Returns((string username, string password, string email) =>
            new MembershipUser("dummy", username, 42, email, null, null, true, false, now, now, now, now, now));

      // setup GetAllUsers()
      var total = It.IsAny<int>();
      membershipMock.Setup(x => x.GetAllUsers(It.IsAny<int>(), It.IsAny<int>(), out total)).Returns(delegate()
      {

        MembershipUserCollection users = new MembershipUserCollection();

        for (int i = 0; i < 1000; i++)
        {
          Mock<MembershipUser> dummyUser = new Mock<MembershipUser>();

          dummyUser.SetupGet(u => u.ProviderUserKey).Returns(i);
          dummyUser.SetupGet(u => u.UserName).Returns("Dummy User " + i);
          dummyUser.SetupGet(u => u.Email).Returns("just@a-dummy" + i + ".com");
          dummyUser.SetupGet(u => u.LastLoginDate).Returns(now.AddMinutes(i * (-1)));
          dummyUser.SetupGet(u => u.CreationDate).Returns(now.AddMinutes(i * (-1)));
          dummyUser.SetupGet(u => u.IsLockedOut).Returns(false);

          users.Add(dummyUser.Object);
        }

        return users;
      });

      return membershipMock.Object;
    }
  }
}