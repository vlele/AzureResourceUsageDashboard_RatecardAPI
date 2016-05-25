var app = angular.module('app');
app.controller('subscriptionCtrl', ['$scope', '$http', function ($scope, $http) {

    var getSubscriptions = function(){
        $http({
            method: 'GET',
            url: '/subscription/GetSubscriptionDetails'
        }).then(function successCallback(response) {
            $scope.subscription = response.data.Subscriptions;
        });
    }

    getSubscriptions();
}]);