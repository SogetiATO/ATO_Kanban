var KanbanApp = angular.module("KanbanApp", ["ngResource", "ngCookies", "ngRoute"]).
    config(function ($routeProvider) {
        $routeProvider.
            when('/', { controller: ListCtrl, templateUrl: 'list.html' }).
            when('/new', { controller: NewCtrl, templateUrl: 'new.html' }).
            when('/edit/:editID', { controller: EditCtrl, templateUrl: 'new.html' }).
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


    // Initial functions to set up main screen.
    $scope.reset = function () {
        $scope.loggedInUser = "";
        $scope.getCookie();

        // No need to query database if the user isn't logged in.
        if ($cookies.userID > 0) {
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
            $scope.updateTodo(oldStatus, $scope.item);
        });
    }

    // TODO
    // Need to create functionality for approval or revision
    $scope.updateTodo = function (oldStatus, todo) {
        var todoID = todo.ID;
        $scope.item = Api.Todo.get({ id: todoID });

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

    // Visibility for assignee of the todo to click approve
    // Still needs some functionality
    $scope.approval = function () {
        if (this.todo.AssigneeID == $scope.loggedInUser.ID) {
            return false;
        }
        return true;
    };


    // TODO
    $scope.revise = function () {
        // Pop up a modal with the info
        // Grab reasoning from text box
        // Pass to backend
        // Make sure backend changes to the correct stats
        // Have AngularJS remove item from completed and move back to claimed
    };

    // TODO
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
        var item = $scope.item;

        // Sets the new item with all the data from the modal
        item.PriorityID = $scope.item.Priority;
        item.AssigneeID = $cookies.userID;
        item.StatusID = 1; // Might be smarter to set this in the backend where we can set it to unclaimed;
        item.EmailATO = $scope.item.EmailATO;
        item.RequiresApproval = $scope.item.RequiresApproval;
        item.Optional = $scope.item.Optional;
        item.IsPublic = $scope.item.IsPublic;
        
        Api.Todo.save(item, function (data) {
            data.Status = Api.Status.get({ id: data.StatusID });        // I believe these two lines need to be here to pass the correct id to the backend
            data.Priority = Api.Priority.get({ id: data.PriorityID });  //
            $scope.unclaimed = $scope.unclaimed.concat(data);           // Adds the newly created todo to the unclaimed group
            $scope.item = null;                                         // Resets the modal fields
        });
    };

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

    // Initial setup
    $scope.reset();
};

var NewCtrl = function ($scope, $location, Todo) {
    $scope.saveTodo = function () {
        Todo.save($scope.item, function () {
            $location.path('/');
        });
    };
    $scope.action = "Create";
}

var EditCtrl = function ($scope, $location, $routeParams, Todo) {
    $scope.saveTodo = function () {
        Todo.update({ id: todoID }, $scope.item, function () {
            $location.path('/');
        });
    };

    $scope.action = "Update";
    var todoID = $routeParams.editID;
    $scope.item = Todo.get({ id: todoID });

}