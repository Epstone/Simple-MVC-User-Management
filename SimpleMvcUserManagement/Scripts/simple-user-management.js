
$(function () {

  var userTableArea = new UserTableArea();
  userTableArea.init(".simple-user-table");

  var addUserArea = new AddUserArea();
  addUserArea.init(".simple-user-table");

  var roleMgmt = new RoleManagement();
  roleMgmt.init();
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
      userTable.tablesorter({ widthFixed: true, widgets: ['zebra'] }).tablesorterPager({ container: $("#pager"), positionFixed: false });

      // bind user link deletion event handler for all rows
      userTable.on('click', '.delete-user', deleteUser);
    });
  }

  /* adds a new user to the user table body */
  function appendUser(user) {

    // build row and append to table
    var row = _myHtml.buildUserTableRow(user);
    userTable.children('tbody').append(row);

  }

  /* deletes a user through the membership service and the according row in the user table. */
  function deleteUser(e) {

    e.preventDefault();
    var id = $(this).data("user-id");
    var row = $(this).closest("tr");

    //delete user by id and check result
    $.post("/{controllerName}/DeleteUser", { userId: id }, function (response) {


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

    // add user button eventhandler
    $("#btn-create-user").click(createUser);


  }

  /* Creates a new user if all field data is valid */
  function createUser(e) {

    e.preventDefault();

    var username = $("#tbx-add-username").val();
    var pwd = $("#tbx-add-password").val();
    var pwd2 = $("#tbx-add-repeat-password").val();
    var email = $("#tbx-add-email").val();
    var roles = [];

    $("#add-user-roles :selected").each(function (i, selected) {
      roles[i] = $(selected).text();
    });

    console.log(roles);
    //verify that both passwords are equal
    if (pwd !== pwd2 || pwd === "") {
      _myHelper.showError("The passwords are empty or do not match.");
      return;
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

  function bindAddRoleButtonClick() {

    $("#btn-add-role").click(function (e) {

      e.stopPropagation();
      var rolename = $("#role-name").val();

      //check that the role input is not empty
      if (rolename.length === 0) {
        _myHelper.showError("You have not entered a role name");

      } else {
        addRole(rolename);
      }

    });


  }

  function addRole(roleName) {

    $.post("/{controllerName}/CreateRole", { roleName: roleName }, function (response) {
      _myHelper.processServerResponse(response, function () {

        addRoleToRoleSelectBox(roleName);

      });

    });

  }

  function bindDeleteRoleButtonClick() {

    $("#btn-delete-role").click(function (e) {

      e.stopPropagation();
      var selectedOption = roleSelectBox.children(":selected");

      if (selectedOption.length === 1) {
        var rolename = selectedOption.text();
        var allowPopulatedRoleDeletion = $("#allow-populated-role").is(":checked");

        deleteRole(rolename, allowPopulatedRoleDeletion);

      } else {
        _myHelper.showError("You have not selected a role to delete.");
      }
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
  /* generates a new user table row as  jquery element */
  buildUserTableRow: function (user) {

    return $('<tr>'
            + '<td>' + user.id + '</td>'
						+ '<td>' + _myHelper.encodeHtml(user.name) + '</td>'
            + '<td>' + user.registrationDate + '</td>'
            + '<td>' + _myHelper.encodeHtml(user.email) + '</td>'
            + '<td>' + user.isLockedOut + '</td>'
            + '<td><a data-user-id=' + user.id + ' href="" class="delete-user">Delete</a></td>'
						+ '</tr>');
  }

}

var _myHelper = {

  /* helper function for processing the server response. Triggers either an error or success message window 
  /* and calls provided functions if neccessary. */
  processServerResponse: function (response, onSuccess, onError) {

    if (response.isSuccess) {
      _myHelper.showSuccess(response.message);

      // call success callback
      if ($.isFunction(onSuccess))
        onSuccess.apply();

    } else {

      //show error message
      _myHelper.showError(response.message, "error");
      if ($.isFunction(onError))
        onError.apply();
    }


  },


  showSuccess: function (msg) { _myHelper.showMessage(msg, "success"); },

  showError: function (msg) { _myHelper.showMessage(msg, "error"); },

  /* Shows an error or success notification */
  showMessage: function (message, cssClass) {

    var id = "simple-user-info";

    //remove all old info windows
    var oldInfo = $("#" + id);
    if (oldInfo)
      oldInfo.remove();

    var $info = $("<div/>").text(message).attr("id", id).addClass(cssClass)
                          .appendTo("body")
                          .fadeIn(1000).on("mouseover", fadeOutInfoWindow);

    // Hide the info window after some seconds
    setTimeout(fadeOutInfoWindow, 4000);

    //click anywhere to remove info window
    $(document).one("click", fadeOutInfoWindow);

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


