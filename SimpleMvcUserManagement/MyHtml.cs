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

    public static string UserTableArea()
    {
      string tableHtml = ScriptPack.user_table_area;

      return tableHtml;
    }

    public static string TableJavaScript()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(_javascriptBlock, ScriptPack.jquery_tablesorter_min);
      builder.AppendFormat(_javascriptBlock, ScriptPack.tablesorter_pager);

      //builder.Append("<script type='text/javascript' src='http://tablesorter.com/__jquery.tablesorter.js'></script>");
      //builder.Append("<script type='text/javascript' src='http://tablesorter.com/addons/pager/jquery.tablesorter.pager.js'></script>");

      builder.AppendFormat(_javascriptBlock, ScriptPack.simple_user_management.Replace("{controllerName}", "UserManagement"));

      return builder.ToString();
    }

    public static string TableCss()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(_styleSheetBlock, ScriptPack.tablesorter_style);

      return builder.ToString();
    }
    public static string StyleCss()
    {
      return string.Format(_styleSheetBlock, ScriptPack.style);
    }

    public static string AddUserForm()
    {
      return ScriptPack.add_user_form;
    }

    public static string ManageRolesForm()
    {
      return ScriptPack.manage_roles_form;
    }


  }
}
