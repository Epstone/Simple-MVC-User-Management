using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SimpleMvcUserManagement;
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
      UserManagementController.MembershipProvider = DummyProviders.CreateDummyMembershipProvider();
      UserManagementController.RoleProvider = DummyProviders.CreateDummyRoleProvider();

    }
    protected void Application_AuthenticateRequest()
    {
      // check if the user management controller is the target controller for this request
      if (UserManagementController.IsTargeted())
      {
        // Do any custom authorization checks here (e.g. Roles.IsUserInRole("Admin")) 
        UserManagementController.IsRequestAuthorized = true;
      }
    }


  }
}