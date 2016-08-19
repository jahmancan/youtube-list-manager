mainModule.controller('suggestionsController',
    ['$rootScope', '$scope', '$location', '$routeParams', 'dragularService', 'youTubeListManagerDataService', 'suggestionsModel',
    function ($rootScope, $scope, $location, $routeParams, dragularService, youTubeListManagerDataService, suggestionsModel) {

        console.log($routeParams);
        console.log($routeParams.playListId);

        var init = function () {
            $scope.model = suggestionsModel;

            youTubeListManagerDataService.getPlayList($routeParams.playListId).then(function (response) {

                var getPlayListItemsAsync = function (playlistResponse) {
                    $scope.model.playlist = playlistResponse;
                }

                getPlayListItemsAsync(response);
            });
        };

        init();
    }]
);