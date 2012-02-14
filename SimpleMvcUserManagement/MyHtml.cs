using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SimpleMvcUserManagement.Scripts;
using System.Text.RegularExpressions;

namespace SimpleMvcUserManagement
{
  public static class MyHtml
  {
    const string _styleSheetBlock = "<style type='text/css'>{0}</style>";
    const string _javascriptBlock = "<script type='text/javascript'>{0}</script>";

    public static IHtmlString UserTableArea()
    {
      string tableHtml = ScriptPack.user_table_area;

      return new HtmlString( tableHtml);
    }

    public static IHtmlString TableJavaScript()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(_javascriptBlock, ScriptPack.jquery_tablesorter_min);
      builder.AppendFormat(_javascriptBlock, ScriptPack.tablesorter_pager);

      //builder.Append("<script type='text/javascript' src='http://tablesorter.com/__jquery.tablesorter.js'></script>");
      //builder.Append("<script type='text/javascript' src='http://tablesorter.com/addons/pager/jquery.tablesorter.pager.js'></script>");

      builder.AppendFormat(_javascriptBlock, ScriptPack.simple_user_management.Replace("{controllerName}", "UserManagement"));

      return new HtmlString(builder.ToString());
    }

    public static IHtmlString TableCss()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(_styleSheetBlock, ScriptPack.tablesorter_style);

      return new HtmlString(builder.ToString());
    }
    public static IHtmlString StyleCss()
    {
      var styleBlock = string.Format(_styleSheetBlock, ScriptPack.style);
     
      return new HtmlString(styleBlock);
    }

    public static IHtmlString AddUserForm()
    {
      var addUserForm = ScriptPack.add_user_form;
      
      return new HtmlString(addUserForm);
    }

    public static IHtmlString ManageRolesForm()
    {
      var manageRolesForm = ScriptPack.manage_roles_form;

      return new HtmlString(manageRolesForm);
    }


  }
}
