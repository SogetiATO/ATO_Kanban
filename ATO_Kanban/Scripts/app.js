var KanbanApp = angular.module("KanbanApp", ["ngResource", "ngCookies", "ngRoute"]).
    config(function ($routeProvider) {
        $routeProvider.
            when('/', { controller: ListCtrl, templateUrl: 'list.html' }).
            when('/users', { controller: ListCtrl, templateUrl: 'users.html' }).
            otherwise({ redirectTo: '/' });
    });

KanbanApp.factory('Api', function ($resource) {
    return {
        Todo: $resource('/api/todo/:id', { id: '@id' }, { update: { method: 'PUT' } }),
        Status: $resource('/api/status/:id', { id: '@id' }, { update: { method: 'PUT' } }),
        Priority: $resource('/api/priority/:id', { id: '@id' }, { update: { method: 'PUT' } }),
        User: $resource('/api/user/:id', { id: '@id' }, { update: { method: 'PUT' } }),
        Login: $resource('/api/user', { })
    }
});

var ListCtrl = function ($cookieStore, $scope, $location, Api, $cookies) {
    // Gets list of todos based on status
    $scope.search = function (statusText) {
        Api.Todo.query({
            status: statusText,
        },
        function (data) {
            switch (statusText) {
                case "Unclaimed":
                    $scope.unclaimed = data;
                    break;
                case "Claimed":
                    $scope.claimed = data;
                    break;
                case "Completed":
                    $scope.completed = data;
                    break;
                case "Approved":
                    $scope.approved = data;
                    break;
            }
        });
    };

    // Gets list of priorities for drop down on create todo modal
    $scope.prioritySearch = function () {
        Api.Priority.query({},
        function (data) {
            $scope.priorities = data;
        });
    };

    $scope.userSearch = function () {
        Api.User.query({},
        function (data) {
            $scope.users = data;
        });
    };
    // Initial functions to set up main screen.
    $scope.reset = function () {
        $scope.loggedInUser = "";
        $scope.getCookie();

        // No need to query database if the user isn't logged in.
        if ($cookies.userID > 0) {
            $scope.users = [];
            $scope.userSearch();
            $scope.priorities = [];
            $scope.prioritySearch();
            $scope.statuses = [];
            $scope.unclaimed = [];
            $scope.claimed = [];
            $scope.completed = [];
            $scope.approved = [];
            $scope.search("Unclaimed");
            $scope.search("Claimed");
            $scope.search("Completed");
            $scope.search("Approved");
            $scope.grades = [
                { Grade: "A" },
                { Grade: "B" },
                { Grade: "C" },
            ];
        }
    };

    // Functionality of changing the status and ClaimedByID of a todo
    $scope.claim = function () {
        var todoID = this.todo.ID;
        var oldStatus = this.todo.Status;
        var newStatus = this.todo.Status;

        if (this.todo.Status.ID != 4) {
            Api.Status.get({ id: this.todo.Status.ID + 1 }, function (data) {
                newStatus = data;
            });
        };

        $scope.item = Api.Todo.get({ id: todoID }, function (data) {
            $scope.item.Status = newStatus;
            if (newStatus.ID == 2) {
                $scope.item.ClaimedByID = $cookies.userID;
            }
            $scope.updateTodo(oldStatus, $scope.item);
        });
    }

    // Function that Claim calls
    $scope.updateTodo = function (oldStatus, todo) {
        var todoID = todo.ID;

        Api.Todo.update({ id: todoID }, todo, function () {
            $("#todo_" + oldStatus.Type + "_" + todo.ID).fadeOut();
            $("#todo_" + oldStatus.Type + "_" + todo.ID).attr("id", "#todo_" + todo.Status.Type + "_" + todo.ID);

            switch (todo.Status.ID) {
                case 2:
                    $scope.claimed = $scope.claimed.concat(todo);
                    break;
                case 3:
                    $scope.completed = $scope.completed.concat(todo);
                    break;
                case 4:
                    $scope.approved = $scope.approved.concat(todo);
                    break;
            };
        });
    };

    // When the 'Revise' button is clicked it sets a scope variable so that it can pull the title/description for the modal
    $scope.setActiveTodo = function () {
        $scope.activeTodo = this.todo;
    }

    // Visibility for assignee of the todo to click approve
    $scope.approval = function () {
        if (this.todo.AssigneeID == $scope.loggedInUser.ID) {
            return true;
        }
        return false;
    };

    $scope.owner = function () {
        if (this.todo.ClaimedByID == $scope.loggedInUser.ID) {
            return true;
        }
        return false;
    }

    $scope.revise = function () {
        var todoID = $scope.activeTodo.ID;
        var oldStatus = $scope.activeTodo.Status;
        var newStatus = $scope.activeTodo.Status;

        if ($scope.activeTodo.Status.ID != 4) {
            Api.Status.get({ id: $scope.activeTodo.Status.ID - 1 }, function (data) {
                newStatus = data;
            });
        };

        $scope.item = Api.Todo.get({ id: todoID }, function (data) {
            $scope.item.Status = newStatus;
            $scope.item.ReasonForRevision = $scope.activeTodo.Revision;
            $scope.updateTodo(oldStatus, $scope.item);
        });
    };

    // Deltes Todo from DB and removes from the page
    // Not graphically available
    $scope.deleteTodo = function () {
        // Available from modal only if optional or creator is viewing
        // remove from correct scope
        var todoID = this.todo.ID;
        Todo.delete({ id: todoID }, function () {
            $("#todo_" + todoID).fadeOut();
        });
    }

    // Saves a new todo from the modal
    $scope.saveTodo = function () {
        var item = $scope.formItem;

        // Sets the new item with all the data from the modal
        item.PriorityID = $scope.formItem.Priority;
        item.AssigneeID = $cookies.userID;
        item.StatusID = 1; // Might be smarter to set this in the backend where we can set it to unclaimed;
        item.EmailATO = $scope.formItem.EmailATO;
        item.RequiresApproval = $scope.formItem.RequiresApproval;
        item.Optional = $scope.formItem.Optional;
        item.IsPublic = $scope.formItem.IsPublic;
        
        Api.Todo.save(item, function (data) {
            data.Status = Api.Status.get({ id: data.StatusID });        // I believe these two lines need to be here to pass the correct id to the backend
            data.Priority = Api.Priority.get({ id: data.PriorityID });  //
            $scope.unclaimed = $scope.unclaimed.concat(data);           // Adds the newly created todo to the unclaimed group
            $scope.formItem = null;                                         // Resets the modal fields
        });
    };


    // Saves a new user from the modal
    $scope.saveUser = function () {
        var user = $scope.formUser;
        user.Grade = $scope.formUser.Grade.Grade;

        Api.User.save(user, function (data) {
            $scope.formUser = null;     // Resets the modal fields
        });
    };

    // Update user from modal
    $scope.updateUser = function () {
        var user = $scope.activeUser;
        user.Name = $scope.activeUser.Name;
        user.Username = $scope.activeUser.Username;
        user.Grade = $scope.activeUser.Grade.Grade;

        Api.User.update({ id: user.ID }, user, function () {
            var test = "test";
        });
    }

    // Uses special version of User to check for username and password. Passes data to $scope.setLoginInfo(data);
    $scope.login = function () {
        var user = $scope.user;
        $scope.correctLogin = false;
        Api.Login.get({ userName: $scope.user.Username, password: $scope.user.Password },
            function (data) {
                $cookies.userID = data.ID;
                $scope.setLoginInfo(data);
                $scope.reset();
            }
        );
    };

    // Checks cookies for a userID. Passes data to $scope.setLoginInfo(data);
    $scope.getCookie = function () {
        var userID = $cookies.userID;
        $scope.correctLogin = false;
        if (userID > 0) {
            Api.User.get({ id: userID }, function (data) {
                $scope.setLoginInfo(data);
            });
        }
    };

    // Sets $scope info for a user from login or cookies
    $scope.setLoginInfo = function (data) {
        $scope.loggedInUser = data;
        $scope.correctLogin = true;
        $scope.allowCreate();
    };

    // Only allows users with grade of C to see the 'Create Task' button
    $scope.allowCreate = function () {
        if ($scope.loggedInUser.Grade == "C") {
            return true;
        }
        return false;
    };

    // Sets UserID in cookies to null and refreshes the page.
    $scope.logout = function () {
        $cookies.userID = null;
    }

    $scope.updateUserModal = function () {
        $scope.activeUser = this.user;
    }
    // Initial setup
    $scope.reset();
};

var NewCtrl = function ($scope, $location, Api) {

    $scope.saveTodo = function () {
        Todo.save($scope.item, function () {
        });
    };
}