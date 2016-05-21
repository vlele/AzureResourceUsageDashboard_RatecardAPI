var app = angular.module('app', ['ngRoute', 'ngCookies']);
app.config(['$routeProvider', function ($routeProvider) {

    //dashboard pulls usage data grouped by subscription
    $routeProvider.when('/dash/:monthId?', {
        templateUrl: '/angular/templates/dashboard.html',
        controller: 'dashboardCtrl'
    });

    //dashboard pulls usage data grouped by Azure Services
    $routeProvider.when('/byService/:monthId?', {
        templateUrl: '/angular/templates/byServices.html',
        controller: 'servicesCtrl'
    });

    $routeProvider.when('/subscriptionDetail', {
        templateUrl: '/angular/templates/subscription-details.html',
        controller: 'subscriptionCtrl'
    });
    $routeProvider.otherwise({
        redirectTo: '/dash'
    });
}]);

app.run();