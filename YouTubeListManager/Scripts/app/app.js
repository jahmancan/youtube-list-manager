'use strict';

var mainModule = angular.module('YouTubeListManager', ['dragularModule', 'ngRoute', 'ngResource', 'ngAnimate']);

//angular routing
mainModule.config(['$routeProvider', '$locationProvider', function ($routeProvider, $locationProvider) {
    $routeProvider
        .when('/Manager/Index', {
            controller: 'playListController',
            templateUrl: '/Scripts/app/templates/playlist.html',
        })
        .when('/Manager/Suggestions/:playListId', {
            controller: 'suggestionsController',
            templateUrl: '/Scripts/app/templates/suggestions.html',
        })
        .otherwise({
            redirectTo: '/Manager/Index'
        });;

    //var navigationType = !!(window.history && history.pushState);

    //$locationProvider.html5Mode(navigationType);
}]);

//ui routing
//mainModule.config(["$stateProvider", "$urlRouterProvider",
//    function ($stateProvider, $urlRouterProvider) {

//        $stateProvider
//            .state('index', {
//                url: '/playlists',
//                templateUrl: './scripts/app/templates/playlists.html',
//                controller: 'playlist'
//            }).state('suggestions', {
//                url: '/suggestions/:playlistId',
//                templateUrl: './scripts/app/templates/suggestions.html',
//                controller: 'suggestion'
//            });

//        $urlRouterProvider.otherwise('/playlists');
//    }]);