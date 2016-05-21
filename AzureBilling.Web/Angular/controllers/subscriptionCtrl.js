var app = angular.module('app');
app.controller('subscriptionCtrl', ['$scope', '$http', function ($scope, $http) {

    $scope.submitEdit = function()
    {
        $http.post('/subscription/SaveSubscriptionInfo',$scope.subscriptions).then(function successCallback(response) {
          
        });
    }

    var getSubscriptions = function(){
        $http({
            method: 'GET',
            url: '/subscription/GetSubscriptionDetails'
        }).then(function successCallback(response) {
            $scope.subscriptions = response.data;
        });
    }

    getSubscriptions();
}]);