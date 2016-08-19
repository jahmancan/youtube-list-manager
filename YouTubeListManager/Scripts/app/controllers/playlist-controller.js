mainModule.controller('playListController',
    ['$rootScope', '$scope', '$location', 'youTubeListManagerDataService', 'config',
    function ($rootScope, $scope, $location, youTubeListManagerDataService, config) {

        $scope.selectPlayList = function(playlistId) {
            $location.path('/Manager/Suggestions/' + playlistId);
        };

        var init = function () {
            $scope.model = [];

            youTubeListManagerDataService.getAllPlaylists().then(function (response) {

                var getPlayListItemsAsync = function (playlistResponse) {
                    var playlists = playlistResponse.Response;
                    var nextPageToken = playlistResponse.NextPageToken;

                    if (playlists.length > 0)
                        $scope.model = $scope.model.concat(playlists);

                    if (nextPageToken !== null) {
                        youTubeListManagerDataService.getAllPlaylists(nextPageToken).then(function (response) {
                            getPlayListItemsAsync(response);
                        });
                    }
                }

                getPlayListItemsAsync(response);
            });
        };

        init();
    }]
);