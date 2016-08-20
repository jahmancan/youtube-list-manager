mainModule.controller('suggestionsController',
    ['$rootScope', '$scope', '$location', '$routeParams', 'dragularService', 'youTubeListManagerDataService', 
    function ($rootScope, $scope, $location, $routeParams, dragularService, youTubeListManagerDataService) {

        $scope.showSuggestions = function () {
            youTubeListManagerDataService.getSuggestions($scope.model.searchKey, $scope.model.nextPageSuggestionsToken).then(function (response) {
                var getSearchResultsAsync = function(searchResultResponse) {
                    var searchResults = searchResultResponse.Response;
                    $scope.model.nextPageSuggestionsToken = searchResultResponse.NextPageToken;

                    if (searchResults.length > 0)
                        angular.forEach(searchResults, function(value) {
                            $scope.model.suggestions.push(value);
                        });

                    if ($scope.model.nextPageSuggestionsToken !== null && $scope.model.autoLoad) {
                        youTubeListManagerDataService.getSuggestions($scope.model.searchKey, nextPageToken).then(function(nextResponse) {
                            getSearchResultsAsync(nextResponse);
                        });
                    }
                };

                getSearchResultsAsync(response);
            });
        };

        $scope.$on('PlayListfetched', function () {
            $scope.model.playListItemsFetched = true;

            $scope.model.markedItems = $scope.model.playlist.PlayListItems.filter(function (playListItem) { return playListItem.VideoInfo.Live === false });
            if ($scope.model.markedItems.length === 0)
                return;

            console.log($scope.model.markedItems);

            $scope.model.current = $scope.model.markedItems[0];
            $scope.model.currentIndex = 0;
            $scope.model.searchKey = $scope.model.current.VideoInfo.Title;

            $scope.showSuggestions();
        });

        $scope.getNextExpired = function () {

            var nextIndex = $scope.model.currentIndex + 1;
            if (nextIndex >= $scope.model.markedItems.length)
                return;

            var nextItem = $scope.model.markedItems[nextIndex];
            $scope.model.current = nextItem;
            $scope.model.currentIndex = nextIndex;
            $scope.model.searchKey = nextItem.VideoInfo.Title;

            $scope.showSuggestions();
        };

        $scope.save = function() {
            youTubeListManagerDataService.savePlaylist($scope.model.playlist);
        };

        var init = function () {
            $scope.model = new suggestionsModel();

            youTubeListManagerDataService.getPlayList($routeParams.playListId).then(function (playListResponse) {

                var getPlayListItemsAsync = function (response) {
                    var responseItem = angular.isUndefined(response.Response) ? response : response.Response;
                    var isInnerPageTokenPresent = !angular.isUndefined(responseItem.PlayListItemsNextPageToken);
                    var nextPageToken = (isInnerPageTokenPresent)
                        ? responseItem.PlayListItemsNextPageToken
                        : response.NextPageToken;

                    if (isInnerPageTokenPresent)
                        $scope.model.playlist = responseItem;
                    else {
                        //response item is playlistItem list
                        angular.forEach(responseItem, function (value) {
                            $scope.model.playlist.PlayListItems.push(value);
                        });
                    }

                    if (nextPageToken !== null) {
                        youTubeListManagerDataService.getPlayListItems(nextPageToken, $scope.model.playlist.Hash).then(function (nextResponse) {
                            getPlayListItemsAsync(nextResponse);
                        });
                    } else
                        $scope.$broadcast('PlayListfetched');
                }

                getPlayListItemsAsync(playListResponse);
            });
        };

        init();
    }]
);