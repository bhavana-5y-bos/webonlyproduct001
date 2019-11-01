/* --------------------------- LOGOUT USER -----------------------------------*/
function InitiateLogout() {
    DisplayConfirmationDialog({
        Message: "Are you sure you want to logout?",
        CallFrom: "Logout",
        OkData: { Label: "Yes", Data: null },
        CancelData: { Label: "No", Data: null }
    });
}

function TriggerLogout() {
    $.ajax({
        type: "POST",
        url: "/Auth/Logout",
        success: function (response) {
            location.reload();
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END LOGOUT USER -----------------------------------*/

/* --------------------------- UPDATE PROFILE -----------------------------------*/
function UpdateProfileInfo() {

    var data = new Object();
    data.FirstName = $("#profileFirstName")[0].value;
    data.LastName = $("#profileLastName")[0].value;
    data.Email = $("#profileEmail")[0].value;

    $.ajax({
        type: "POST",
        url: "/Profile/UpdateProfileInfo",
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            DisplayDialog({ Success: true, Message: response });
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}

function UpdateUsername() {
    $.ajax({
        type: "POST",
        url: "/Profile/UpdateUsername",
        data: JSON.stringify($("#profileUsername")[0].value),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            DisplayDialog({ Success: true, Message: response });
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END UPDATE PROFILE -----------------------------------*/

/* --------------------------- DELETE USER -----------------------------------*/
function OnUserDeleteClicked(selectedrow) {
    var selectedUserId = selectedrow.id;
    DisplayConfirmationDialog({
        Message: "Are you sure you want to delete the selected user?",
        CallFrom: "DeleteUser",
        OkData: { Label: "Yes", Data: selectedUserId },
        CancelData: { Label: "No", Data: null }
    });
}

function TriggerDeleteUser(selectedUserId) {
    $.ajax({
        type: "POST",
        url: "/Users/DeleteUser",
        data: JSON.stringify(selectedUserId),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (response.indexOf("success") > -1) {
                DisplayDialog({ Success: true, Message: response });
                $("#tableUsers tr[id='" + selectedUserId + "']").remove();
            }
            else {
                DisplayDialog({ Success: false, Message: response });
            }
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END DELETE USER -----------------------------------*/

/* --------------------------- CHANGE PASSWORD BY ADMIN -----------------------------------*/
function OnChangePasswordClicked(selectedrow) {
    var selectedUserId = selectedrow.id;
    $('#hiddenUserIdChangePwd').val(selectedUserId);
    $('#myModal').modal('show');

    $('#password').val('');
    $('#confirmpassword').val('');
    document.getElementById("pass_type").innerHTML = "";
    document.getElementById("passwordMessage").innerHTML = "";
    document.getElementById("message").innerHTML = "";
}

function SetPassword() {
    var enteredPassword = $('#password').val();
    if (enteredPassword.length === 0) {
        $('#passwordMessage').html("Passwords must be at least 8 characters and contain at least one upper case (A - Z), one lower case (a - z), one number(0 - 9) and an special character(e.g. !@#$ %^&*)").css('color', 'red');
        $('#confirmpassword').val('');
        return false;
    }
    if (enteredPassword.length >= 0) {
        var strongRegex = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");
        if (!strongRegex.test(enteredPassword)) {
            $('#passwordMessage').html("Passwords must be at least 8 characters and contain at least one upper case (A - Z), one lower case (a - z), one number(0 - 9) and an special character(e.g. !@#$ %^&*)").css('color', 'red');
            $('#confirmpassword').val('');
            return false;
        }
    }
    if (enteredPassword === $('#confirmpassword').val()) {
        var userId = $("#hiddenUserIdChangePwd").val();

        var data = {};
        data.userId = userId;
        data.password = enteredPassword;

        $.ajax({
            type: "POST",
            url: "/Auth/ForcePasswordChange",
            data: JSON.stringify(data),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $('#hiddenUserIdChangePwd').val();
                DisplayDialog({ Success: true, Message: response });
                $('#myModal').modal('hide');
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });
    } else {
        $('#message').html('Passwords do not match').css('color', 'red');
        return false;
    }
}
/* --------------------------- END CHANGE PASSWORD BY ADMIN -----------------------------------*/

/* --------------------------- EDIT USER -----------------------------------*/
function OnUserEditClicked(selectedrow) {
    var selectedUserId = selectedrow.id;
    selectedUserId = selectedUserId.replace(/\+/g, '%2B');
    $('#hiddenUserEdit').attr("href", "/Users/EditUser?userId=" + selectedUserId);
    $('#hiddenUserEdit')[0].click();
}

function UpdateUserInfo() {

    var isDataValid = ValidateEditUserInput();

    if (isDataValid) {
        var userId = $("#hiddenEditUserId")[0].value;

        var user = {
            "UpdatedId": userId,
            "FirstName": $("#editUserFirstName")[0].value,
            "LastName": $("#editUserLastName")[0].value,
            "Username": $("#editUserUsername")[0].value,
            "Email": $("#email")[0].value
        };

        var jsonObject = JSON.stringify(user);
        $.ajax({
            type: "POST",
            url: "/Users/UpdateUserInfo",
            data: jsonObject,
            contentType: 'application/json; charset=utf-8',
            dataType: "text",
            success: function (response) {
                if (response.indexOf('success') > -1) {
                    DisplayDialog({ Success: true, Message: response });
                }
                else {
                    DisplayDialog({ Success: false, Message: response });
                }
            },
            failure: function (response) {
                console.log(response.Message);
            },
            error: function (response) {
                console.log(response.Message);
            }
        });

        var isUserAllowed = $('#IsUserAllowed').val();
        if (isUserAllowed === "True") {
            var roleElements = $("#divRoleBase input:checked");
            var rolesList = [];
            if (roleElements !== null && roleElements.length > 0) {
                for (var i = 0; i < roleElements.length; i++) {
                    var role = new Object();
                    role.Id = roleElements[i].id;
                    role.Name = roleElements[i].name;
                    rolesList.push(role);
                }
            }

            var data = new Object();
            data.UpdatedRoles = rolesList;
            data.UserId = userId;

            jsonObject = null;
            jsonObject = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: "/Roles/UpdateUserRolesByAdmin",
                data: jsonObject,
                contentType: 'application/json; charset=utf-8',
                dataType: "text",
                success: function (response) {
                    DisplayDialog({ Success: true, Message: response });
                },
                failure: function (response) {
                    console.log(response.Message);
                },
                error: function (response) {
                    console.log(response.Message);
                }
            });
        }
    }
}

function ValidateEditUserInput() {
    var flag = true;

    if ($("#editUserFirstName")[0].value === null || $("#editUserFirstName")[0].value === "") {
        $("#spanFirstName")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#editUserLastName")[0].value === null || $("#editUserLastName")[0].value === "") {
        $("#spanLastName")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#email")[0].value === null || $("#email")[0].value === "") {
        $("#spanEmail")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#editUserUsername")[0].value === null || $("#editUserUsername")[0].value === "") {
        $("#spanUsername")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#email")[0].value !== null && $("#email")[0].value !== "") {
        var isEmailValid = IsEmailValid($("#email")[0].value);
        if (!isEmailValid) {
            $("#spanEmail")[0].innerHTML = "Incorrect email format";
            flag = false;
        }
    }
    var isUserAllowed = $('#IsUserAllowed').val();
    if (isUserAllowed === "True") {
        var roleElements = $("#divRoleBase input:checked");
        if (roleElements.length === 0) {
            $("#spanRoles")[0].innerHTML = "The user must be associated with at least one role";
            flag = false;
        }
    }
    return flag;
}
/* --------------------------- END EDIT USER -----------------------------------*/

/* --------------------------- EDIT ROLE -----------------------------------*/
function OnRoleEditClicked(selectedrow) {
    var selectedRoleId = selectedrow.id;
    $('#hiddenRoleEdit').attr("href", "/Roles/EditRole?roleId=" + selectedRoleId);
    $('#hiddenRoleEdit')[0].click();
}
/* --------------------------- END EDIT ROLE -----------------------------------*/

/* --------------------------- DELETE ROLE -----------------------------------*/
function OnRoleDeleteClicked(selectedrow) {
    var selectedRoleId = selectedrow.id;
    DisplayConfirmationDialog({
        Message: "Are you sure you want to delete the selected role? \n Note: If this role is already associated with users, they no longer will have access to the previliges of this role.",
        CallFrom: "DeleteRole",
        OkData: { Label: "Yes", Data: selectedRoleId },
        CancelData: { Label: "No", Data: null }
    });
}

function TriggerDeleteRole(selectedRoleId) {
    $.ajax({
        type: "POST",
        url: "/Roles/DeleteRole",
        data: JSON.stringify(selectedRoleId),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (response.indexOf("success") > -1) {
                DisplayDialog({ Success: true, Message: response });
                $("#tableRoles tr[id='" + selectedRoleId + "']").remove();
            }
            else
                DisplayDialog({ Success: false, Message: response });
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END DELETE ROLE -----------------------------------*/

/* --------------------------- ROLE - MANAGE PERMISSIONS -----------------------------------*/
function OnManagePermissionsClicked(selectedrow) {
    var roleId = selectedrow.id;
    var roleName = $('#' + roleId)[0].children[0].innerHTML;
    $('#hiddenRoleManagePerms').attr("href", "/Roles/RoleManagePermissions?roleId=" + roleId + '&roleName=' + roleName);
    $('#hiddenRoleManagePerms')[0].click();
}
/* --------------------------- END ROLE - MANAGE PERMISSIONS -----------------------------------*/

/* --------------------------- MANAGE PERMISSIONS -----------------------------------*/
function OnPermissionsSave() {
    var modulesList = $('.modulecheck:checkbox:checked');
    var operationsList = $('.operationcheck:checkbox:checked');
    var ownerId = $("#hiddenOwnerId").val();

    var updatedModulesList = [];
    var updatedOperationsList = [];

    for (i = 0; i < modulesList.length; i++) {
        var permissionSet = new Object();
        permissionSet.ModuleId = modulesList[i].id;
        permissionSet.Name = modulesList[i].name;
        permissionSet.Code = modulesList[i].getAttribute('code');
        permissionSet.IsDefault = modulesList[i].getAttribute('isdefault') === 'isdefault' ? 1 : 0;
        updatedModulesList.push(permissionSet);
    }

    for (j = 0; j < operationsList.length; j++) {
        var permissionsOperation = new Object();
        permissionsOperation.ModuleId = operationsList[j].getAttribute('moduleid');
        permissionsOperation.Name = operationsList[j].name;
        permissionsOperation.Code = operationsList[j].getAttribute('code');
        permissionsOperation.IsDefault = operationsList[j].getAttribute('isdefault') === 'isdefault' ? 1 : 0;
        permissionsOperation.OperationId = operationsList[j].id;
        updatedOperationsList.push(permissionsOperation);
    }

    var data = new Object();
    data.OwnerId = ownerId;
    data.Modules = updatedModulesList;
    data.Operations = updatedOperationsList;

    $.ajax({
        type: "POST",
        url: "/Permissions/UpdatePermissions",
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            DisplayDialog({ Success: true, Message: response });
        },
        failure: function (response) {
            console.log(response.Message);
        },
        error: function (response) {
            console.log(response.Message);
        }
    });
}
/* --------------------------- END MANAGE PERMISSIONS -----------------------------------*/

/* --------------------------- ADD USER -----------------------------------*/
function AddUserSendEmailClicked(obj) {
    if (obj.id === "addUserSendEmailSpan") {
        $("#addUserSendEmail").trigger('click');
    }

    if ($("#addUserSendEmail")[0].checked === true) {
        $("#addUserPasswordBase").addClass("none");
        $("#addUserConfirmPasswordBase").addClass("none");
    }
    else {
        $("#addUserPasswordBase").removeClass("none");
        $("#addUserConfirmPasswordBase").removeClass("none");
    }
}

function AddUser() {
    var isFormDataValid = ValidateInput();
    if (isFormDataValid) {

        var user = {
            "FirstName": $("#addUserFirstName")[0].value,
            "LastName": $("#addUserLastName")[0].value,
            "Username": $("#addUserUsername")[0].value,
            "Email": $("#addUserEmail")[0].value
        };

        var password = $("#addUserPassword")[0].value;

        var isUserAllowed = $('#IsUserAllowed').val();
        if (isUserAllowed === "True") {
            var roleElements = $("#divRoleBase input:checked");
            var rolesList = [];
            if (roleElements !== null && roleElements.length > 0) {
                for (var i = 0; i < roleElements.length; i++) {
                    var role = new Object();
                    role.Id = roleElements[i].id;
                    role.Name = roleElements[i].name;
                    rolesList.push(role);
                }
            }

            var data = new Object();
            data.Roles = rolesList;
            data.User = user;
            data.Password = password;
            data.IsEmailToSend = $("#addUserSendEmail")[0].checked;
            jsonObject = null;
            var jsonObject = JSON.stringify(data);

            $.ajax({
                type: "POST",
                url: "/Users/AddUser",
                data: jsonObject,
                contentType: 'application/json; charset=utf-8',
                dataType: "text",
                success: function (response) {
                    if (response.indexOf("success") > -1) {
                        DisplayDialog({ Success: true, Message: response });

                        setTimeout(function () {
                            window.history.back();
                        }, 2000);
                    }
                    else {
                        DisplayDialog({ Success: false, Message: response });
                    }

                },
                failure: function (response) {
                    console.log(response.Message);
                },
                error: function (response) {
                    console.log(response.Message);
                }
            });
        }
    }
}

function ValidateInput() {
    var flag = true;
    if ($("#addUserSendEmail")[0].checked !== true) {

        var strongRegex = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])(?=.{8,})");
        if (!strongRegex.test($("#addUserPassword")[0].value)) {
            $("#spanPassword")[0].innerHTML = "Passwords must be at least 8 characters and contain at least one upper case (A - Z), one lower case (a - z), one number(0 - 9) and an special character(e.g. !@#$ %^&*)";
            flag = false;
        }

        if ($("#addUserPassword")[0].value !== $("#addUserConfirmPassword")[0].value) {
            $("#confirmPasswordMismatch")[0].innerHTML = "Passwords do not match";
            flag = false;
        }
    }
    if ($("#addUserFirstName")[0].value === null || $("#addUserFirstName")[0].value === "") {
        $("#spanFirstName")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#addUserLastName")[0].value === null || $("#addUserLastName")[0].value === "") {
        $("#spanLastName")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#addUserEmail")[0].value === null || $("#addUserEmail")[0].value === "") {
        $("#spanEmail")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#addUserUsername")[0].value === null || $("#addUserUsername")[0].value === "") {
        $("#spanUsername")[0].innerHTML = "Required";
        flag = false;
    }
    if ($("#addUserEmail")[0].value !== null && $("#addUserEmail")[0].value !== "") {
        var isEmailValid = IsEmailValid($("#addUserEmail")[0].value);
        if (!isEmailValid) {
            $("#spanEmail")[0].innerHTML = "Incorrect email format";
            flag = false;
        }
    }
    var isUserAllowed = $('#IsUserAllowed').val();
    if (isUserAllowed === "True") {
        var roleElements = $("#divRoleBase input:checked");
        if (roleElements.length === 0) {
            $("#spanRoles")[0].innerHTML = "The user must be associated with at least one role";
            flag = false;
        }
    }
    return flag;
}

function IsEmailValid(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}
/* --------------------------- END ADD USER -----------------------------------*/

/* --------------------------- NAVIGATION NENU - STATE MANAGEMENT -----------------------------------*/
function NavigationState(currentModuleId) {
    var moduleDiv = $("#" + currentModuleId);

    if (moduleDiv[0].nodeName === "DIV") {
        var parentId = moduleDiv[0].getAttribute("parentmoduleid");
        while (parentId !== null || parentId !== "null") {
            moduleDiv.toggle(200);
            moduleDiv.parent('li').addClass('active');
            NavigationState(parentId);
        }
    }
    else {
        parentId = moduleDiv[0].getAttribute("parentmoduleid");
        NavigationState(parentId);
    }
}
/* --------------------------- END NAVIGATION NENU - STATE MANAGEMENT -----------------------------------*/

/* --------------------------- DISPLAY MESSAGE FOR CUSTOM MODULES -----------------------------------*/
function DisplayDashboardCustomMessage(currentModuleId) {
    var moduleDiv = $("#" + currentModuleId);
    var moduleName = '';
    if (moduleDiv[0].nodeName === "DIV") {
        moduleName = moduleDiv[0].previousElementSibling.innerText;
    }
    else {
        moduleName = moduleDiv.children('a')[0].innerText;
    }

    $("#divMessage").empty().append('You are in the <strong>' + moduleName + '</strong> module');
}
/* --------------------------- END DISPLAY MESSAGE FOR CUSTOM MODULES -----------------------------------*/

function CheckPasswordStrength() {
    var val = null;

    if (document.getElementById("newPassword") !== null) {
        val = document.getElementById("newPassword").value;
    }
    else if (document.getElementById("addUserPassword") !== null) {
        val = document.getElementById("addUserPassword").value;
    }
    else if (document.getElementById("password") !== null) {
        val = document.getElementById("password").value;
    }

    var meter = document.getElementById("meter");
    var no = 0;
    if (val !== "") {
        // If the password length is less than or equal to 6
        if (val.length <= 6) no = 1;

        // If the password length is greater than 6 and contain any lowercase alphabet or any number or any special character
        if (val.length > 6 && (val.match(/[a-z]/) || val.match(/\d+/) || val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/))) no = 2;

        // If the password length is greater than 6 and contain alphabet,number,special character respectively
        if (val.length > 6 && ((val.match(/[a-z]/) && val.match(/\d+/)) || (val.match(/\d+/) && val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/)) || (val.match(/[a-z]/) && val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/)))) no = 3;

        // If the password length is greater than 6 and must contain alphabets,numbers and special characters
        if (val.length > 6 && val.match(/[a-z]/) && val.match(/\d+/) && val.match(/.[!,@,#,$,%,^,&,*,?,_,~,-,(,)]/)) no = 4;

        if (no === 1) {
            $("#meter").animate({
                width: '50px'
            }, 300);
            meter.style.backgroundColor = "red";
            document.getElementById("pass_type").innerHTML = "Very Weak";
        }

        if (no === 2) {
            $("#meter").animate({
                width: '100px'
            }, 300);
            meter.style.backgroundColor = "#F5BCA9";
            document.getElementById("pass_type").innerHTML = "Weak";
        }

        if (no === 3) {
            $("#meter").animate({
                width: '150px'
            }, 300);
            meter.style.backgroundColor = "#FF8000";
            document.getElementById("pass_type").innerHTML = "Fair";
        }

        if (no === 4) {
            $("#meter").animate({
                width: '200px'
            }, 300);
            meter.style.backgroundColor = "#328c48";
            document.getElementById("pass_type").innerHTML = "Strong";
        }
    } else {
        meter.style.backgroundColor = "white";
        document.getElementById("pass_type").innerHTML = "";
    }
}