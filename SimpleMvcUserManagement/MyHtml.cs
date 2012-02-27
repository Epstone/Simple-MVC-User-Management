using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SimpleMvcUserManagement.Scripts;
using System.Text.RegularExpressions;

namespace SimpleMvcUserManagement
{
  /// <summary>
  /// Static Html Generator for building the user interface html, javascript and css code.
  /// </summary>
  public static class MyHtml
  {
    const string _styleSheetBlock = "<style type='text/css'>{0}</style>";
    const string _javascriptBlock = "<script type='text/javascript'>{0}</script>";

    /// <summary>
    /// Generates the user html table code.
    /// </summary>
    /// <returns>(recommended) User table html code</returns>
    public static IHtmlString UserTableArea()
    {
      string tableHtml = ScriptPack.user_table_area;

      return new HtmlString( tableHtml);
    }

    /// <summary>
    /// (recommended) Builds the add user form for creating new user accounts.
    /// </summary>
    /// <returns>(recommended) Add user form html code</returns>
    public static IHtmlString AddUserForm()
    {
      var addUserForm = ScriptPack.add_user_form;

      return new HtmlString(addUserForm);
    }

    /// <summary>
    /// Builds the role management form.
    /// </summary>
    /// <returns>(recommended) Role managment html code</returns>
    public static IHtmlString ManageRolesForm()
    {
      var manageRolesForm = ScriptPack.manage_roles_form;

      return new HtmlString(manageRolesForm);
    }

    /// <summary>
    /// Required: Builds three javascript blocks containing the tablesorter, pager plugin and the main ui/ajax javascript code.
    /// </summary>
    /// <returns>(required) Javascript code for generating the user interface.</returns>
    public static IHtmlString UiJavascript()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(_javascriptBlock, ScriptPack.jquery_tablesorter_min);
      builder.AppendFormat(_javascriptBlock, ScriptPack.tablesorter_pager_min);

      builder.AppendFormat(_javascriptBlock, ScriptPack.simple_user_management.Replace("{controllerName}", "UserManagement"));

      return new HtmlString(builder.ToString());
    }

    /// <summary>
    /// (optional) Builds a table styling css block. Could be used as starting point for custom styles. Optional, put in head section.
    /// </summary>
    /// <returns>(optional) A basic css style block for the user table.</returns>
    public static IHtmlString TableCss()
    {
      var builder = new StringBuilder();
      builder.AppendFormat(_styleSheetBlock, ScriptPack.tablesorter_style);

      return new HtmlString(builder.ToString());
    }

    /// <summary>
    /// (recommended) Builds a basic layout style css block. It mainly contains css for the info-message overlay and some form styling.
    /// </summary>
    /// <returns>(recommended) Basic style css block</returns>
    public static IHtmlString StyleCss()
    {
      var styleBlock = string.Format(_styleSheetBlock, ScriptPack.style);
     
      return new HtmlString(styleBlock);
    }

    


  }
}
