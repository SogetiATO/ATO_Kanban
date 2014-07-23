var KanbanApp = angular.module("KanbanApp", ["ngResource", "ngRoute"]).
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
        Status: $resource('/api/status/:id', { id: '@id' }, { update: { method: 'PUT' } })
    }
});

var ListCtrl = function ($scope, $location, Api) {
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

    $scope.reset = function () {
        $scope.statuses = [];
        $scope.unclaimed = [];
        $scope.claimed = [];
        $scope.completed = [];
        $scope.approved = [];
        $scope.search("Unclaimed");
        $scope.search("Claimed");
        $scope.search("Completed");
        $scope.search("Approved");
    };

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

    $scope.updateTodo = function (oldStatus, todo) {
        $scope.action = "Update";
        var todoID = todo.ID;
        $scope.item = Api.Todo.get({ id: todoID });

        Api.Todo.update({ id: todoID }, todo, function () {
            $("#todo_" + oldStatus.Type + "_" + todo.ID).fadeOut();//400, function(){
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
    }

    $scope.sort_by = function (ord) {
        if ($scope.sort_order === ord) {
            $scope.sort_desc = !$scope.sort_desc;
        }
        else {
            $scope.sort_order = ord;
            $scope.sort_desc = false;
        }
        $scope.reset();
    };
    $scope.do_show = function (asc, col) {
        return (asc != $scope.sort_desc) && ($scope.sort_order == col);
    };
    $scope.show_more = function () {
        $scope.search();
        return !$scope.no_more;
    };
    $scope.showArrow = function (asc, col) {
        return (asc != $scope.sort_desc) && ($scope.sort_order == col);
    }
    $scope.deleteTodo = function () {
        var todoID = this.todo.ID;
        Todo.delete({ id: todoID }, function () {
            $("#todo_" + todoID).fadeOut();
        });
    }

    $scope.sort_order = 'Priority';
    $scope.desc = false;

    $scope.reset();

    $scope.saveTodo = function () {
        $scope.action = "Create";
        //Todo.save($scope.item, function () {
        //    $location.path('/');
        //});
    };
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