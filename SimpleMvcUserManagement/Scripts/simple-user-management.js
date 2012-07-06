
$(function () {

  var addUserArea = new AddUserArea();
  addUserArea.init(".simple-user-table");

  var roleMgmt = new RoleManagement();
  roleMgmt.init();

  var userTableArea = new UserTableArea();
  userTableArea.init(".simple-user-table");

});



function UserTableArea() {

  var userTable;

  /* Initializes the user table and fills it with server data. */
  this.init = function (tableSelector) {

    // get user table
    userTable = $(tableSelector);

    // fill the user table with all 
    $.post('/{controllerName}/GetAllUsers', {}, function (users) {

      // add users to table
      for (var i in users) {
        appendUser(users[i]);
      }

      //initialize table sorter and pager
      userTable.tablesorter({ widthFixed: true, widgets: ['zebra'] }).tablesorterPager({ container: $("#pager"), positionFixed: false, removeRows: false });

      // bind user action link event handlers for all rows
      userTable.on('click', '.delete-user', deleteUser);
      userTable.on("click", ".unlock-user", unlockUser);
      userTable.on("click", ".manage-roles", manageRoles);
    });
  }

  /* Shows a manage roles dialog with checkboxes for each role if the user clicks on the manage roles action link. */
  function manageRoles(e) {

    // hide any other manage roles dialog
    $(".manage-roles-dialog").remove();

    var $clickedLink = $(this);
    var user = $(this).closest("tr").data("user");

    // ask server which roles the user is currently in and which else exist
    $.post("/{controllerName}/GetUserRoleStatus", { username: user.name }, function (response) {

      // on success do...
      _myHelper.processServerResponse(response, function () {

        //build role dialog
        var $dlg = $("<div/>").addClass("manage-roles-dialog").hide();
        var $roleList = $("<ul />").addClass("role-list");

        var roleInfo = response.data;
        for (var i in roleInfo) {

          var rolename = roleInfo[i].rolename;
          var cbxId = "cbx-" + rolename;

          //create a list item, a checkbox, a label foreach role

          var $li = $("<li />");
          var $cbx = $("<input type='checkbox' class='cbx-role' />").val(rolename).attr("checked", roleInfo[i].isInRole).attr("id", cbxId);
          var $label = $("<label />").text(rolename).attr("for", cbxId);

          // append checkbox and label to list item
          $li.append($cbx).append($label).appendTo($roleList);
        }

        //append role list to dialog
        $dlg.append($roleList);

        $clickedLink.after($dlg);
        $dlg.fadeIn();

        //bind add or remove role for the created checkboxes
        $dlg.find(".cbx-role").change(function (e) {

          e.stopPropagation();

          var data = {
            rolename: $(this).val(),
            isInRole: $(this).is(":checked"),
            username: user.name
          };

          // start adding or removing the role for the given user
          addRemoveRoleForUser(data);

        });

        //hide manage roles dialog on document click but not on a click inside the dialog
        $dlg.on("click", function (e) { e.stopPropagation() });
        $(document).one("click", function (e) { $dlg.fadeOut() });


      });

    });

    return false;
  }

  /* Adds or removes a role for a user */
  function addRemoveRoleForUser(data) {

    $.post("/{controllerName}/AddRemoveRoleForUser", data, function (response) {

      _myHelper.processServerResponse(response);

    });


  }


  /* Unlocks the user account when the unlock user link is clicked*/
  function unlockUser(e) {

    e.preventDefault();

    var user = $(this).closest("tr").data("user");
    var lockoutLink = $(this);


    // ask server for switching lockout state
    $.post("/{controllerName}/UnlockUser", { userName: user.name }, function (response) {

      _myHelper.processServerResponse(response, function () {

        //on success replace link with success info
        lockoutLink.replaceWith("<span>Just Unlocked</span>");

      });
    });

  }

  /* Adds a new user to the user table body */
  function appendUser(user) {

    // build row and append to table
    var row = _myHtml.buildUserTableRow(user);
    userTable.children('tbody').append(row);

  }

  /* Deletes a user through the membership service and the according row in the user table. */
  function deleteUser(e) {

    e.preventDefault();

    var row = $(this).closest("tr");
    var user = row.data("user");

    // cancel user deletion if confirmation is negative
    var result = confirm("Do you really want to delete the user account for " + user.name + " ?");
    if (!result)
      return;

    //delete user by id and check result
    $.post("/{controllerName}/DeleteUser", { username: user.name }, function (response) {

      _myHelper.processServerResponse(response, function () {

        //disable table pager
        userTable.trigger('disable.pager');

        //remove the specified row from the table
        row.remove();

        //enable pager again
        userTable.trigger('enable.pager');
      });

    });

  }

}

function AddUserArea() {

  var userTable;

  /* Initializes all controlls of the add-user-form */
  this.init = function (userTableSelector) {

    userTable = $(userTableSelector);

    //stop initialization if add user form is unused
    if (!$("#add-user-form"))
      return;

    // binds the add user buttons click event to the create user function
    $("#btn-create-user").click(createUser);


  }

  /* Creates a new user if all field data is valid */
  function createUser(e) {

    var username = $("#tbx-add-username").val();
    var pwd = $("#tbx-add-password").val();
    var pwd2 = $("#tbx-add-repeat-password").val();
    var email = $("#tbx-add-email").val();
    var roles = [];

    $("#add-user-roles :selected").each(function (i, selected) {
      roles[i] = $(selected).text();
    });

    //verify that both passwords are equal
    if (pwd !== pwd2 || pwd === "" || username === "" || email === "") {
      _myHelper.showError("You have missed something or the passwords do not match.");
      return false;
    }

    var postData = { username: username,
      password: pwd,
      email: email,
      roles: roles
    };

    //send user creation request to server
    $.ajax({
      type: "POST",
      url: "/{controllerName}/CreateUser",
      data: postData,
      dataType: "json",
      traditional: true,
      success: function (response) {
        _myHelper.processServerResponse(response, function () {

          // build a user row and add it to the table by maintaining paging and sorting state
          var user = response.data;
          var $row = _myHtml.buildUserTableRow(user);
          userTable.find('tbody').append($row)
                             .trigger('addRows', [$row]);

        });
      }
    });

    return false;
  }
}


function RoleManagement() {

  var roleSelectBox = $(".role-select-box");

  this.init = function () {

    // fill role select boxes
    fillRoleSelectBox();

    bindAddRoleButtonClick();
    bindDeleteRoleButtonClick();
  }

  function fillRoleSelectBox() {

    //fill role select box
    $.post("/{controllerName}/GetAllRoles", {}, function (roles) {

      for (var i in roles) {
        addRoleToRoleSelectBox(roles[i]);
      }
    });

  }

  function addRoleToRoleSelectBox(role) {
    roleSelectBox.append($("<option value=" + role + ">" + role + "</option>"));
  }

  /* Starts the role creation process when the add role button is clicked */
  function bindAddRoleButtonClick() {

    $("#btn-add-role").click(function (e) {

      var rolename = $("#role-name").val();

      //check that the role input is not empty
      if (rolename.length === 0) {
        _myHelper.showError("You have not entered a role name");

      } else {
        addRole(rolename);
      }

      return false;
    });


  }

  /* Sends a role creation request to the server */
  function addRole(roleName) {

    $.post("/{controllerName}/CreateRole", { roleName: roleName }, function (response) {
      _myHelper.processServerResponse(response, function () {

        //on success add the role to the selectbox
        addRoleToRoleSelectBox(roleName);

      });

    });

  }

  /* Handles the delete role button click event */
  function bindDeleteRoleButtonClick() {

    $("#btn-delete-role").click(function (e) {

      var selectedOption = roleSelectBox.children(":selected");

      // verify that a role has been selected
      if (selectedOption.length === 1) {
        var rolename = selectedOption.text();
        var allowPopulatedRoleDeletion = $("#allow-populated-role").is(":checked");

        //confirm role deletion
        var confirmationResult = confirm("Do you really want to delete the role named " + rolename + " ?");
        if (!confirmationResult) return false;

        // delete the role
        deleteRole(rolename, allowPopulatedRoleDeletion);

      } else {
        _myHelper.showError("You have not selected a role to delete.");
      }

      return false;
    });

  }

  function deleteRole(roleName, allowPopulatedRoleDeletion) {

    $.post("/{controllerName}/DeleteRole", { roleName: roleName, allowPopulatedRoleDeletion: allowPopulatedRoleDeletion }, function (response) {
      _myHelper.processServerResponse(response, function () {

        //on sucess remove role from select box
        roleSelectBox.children("[value='" + roleName + "']").remove();

      });

    });

  }

}

var _myHtml = {

  /* generates a new user table row as jquery element */
  buildUserTableRow: function (user) {

    var userRow = new UserRowBuilder(user);
    userRow.addCell(user.id,"user-id");
    userRow.addCell(user.name,"user-name");
    userRow.addCell(user.registrationDate,"reg-date");
    userRow.addCell(user.email,"email");

    //build lockout text or link
    var $lockoutElem;
    if (!user.isLockedOut) {
      $lockoutElem = $("<span/>").text("Is Unlocked");
    }
    else {
      $lockoutElem = $("<a />").addClass("unlock-user").text("Unlock Account");
    }
    userRow.addCellForElem($lockoutElem, "lock-state");

    //build manage roles link
    $manageRolesLink = $("<a />").addClass("manage-roles").text("Manage Roles");
    userRow.addCellForElem($manageRolesLink,"edit-roles");

    //build deletion link and append
    var $deletionLink = $("<a />").data("user-name", user.name).addClass("delete-user").text("Delete");
    userRow.addCellForElem($deletionLink, "action");

    return userRow.getElem();

  }

}

/* User Row Object for generating new rows containing user information */
function UserRowBuilder(user) {
  var $row = $("<tr />").data("user", user);

  /*Adds a new cell with the specified text */
  this.addCell = function (text, cssClass) {

    var $cell = $("<td />");
    $cell.text(text);

    //add cell css class
    $cell.addClass(cssClass);

    $row.append($cell);
  }

  /* Adds a new cell for the specified element */
  this.addCellForElem = function (elem, cssClass) {

    var $cell = $("<td />");
    $cell.append(elem);

    //add cell css class
    $cell.addClass(cssClass);

    $row.append($cell);
  }

  /* returns the row element itself */
  this.getElem = function () {
    return $row;
  }
}



var _myHelper = {

  /* helper function for processing the server response. Triggers either an error or success message window 
  /* and calls provided functions if neccessary. */
  processServerResponse: function (response, onSuccess, onError) {

    if (response.isSuccess) {
      if (response.message) // show message only if available
        _myHelper.showSuccess(response.message);

      // call success callback
      if ($.isFunction(onSuccess))
        onSuccess.apply();

    } else {

      //show error message
      if (response.message) // show message only if available
        _myHelper.showError(response.message, "error");

      if ($.isFunction(onError))
        onError.apply();
    }


  },


  showSuccess: function (msg) { _myHelper.showMessage(msg, "success"); },

  showError: function (msg) { _myHelper.showMessage(msg, "error"); },

  /* returns the info window element and creates one if it is not yet existing */
  infoWindow: function () {

    //remove old info window
    $("#simple-user-info").remove();

    //add new one
    return $("<div/>").attr("id", "simple-user-info")
               .appendTo("body");
  },

  /* Shows an error or success notification */
  showMessage: function (message, cssClass) {


    var $info = _myHelper.infoWindow().addClass(cssClass).text(message);

    //show info message
    $info.fadeIn(1000).on("mouseover", fadeOutInfoWindow);

    //hide after 4 seconds and on one document click
    setTimeout(function () { fadeOutInfoWindow(); }, 4000);
    $(document).one("click", fadeOutInfoWindow)

    //fadeOut info window
    function fadeOutInfoWindow() {
      $info.fadeOut(1000);
    }
  },

  /* Helper function for html encoding unsecure user input */
  encodeHtml: function (input) {

    return $("<div/>").text(input).html();

  }
}


