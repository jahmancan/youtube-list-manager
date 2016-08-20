mainModule.controller('suggestionsController',
    ['$rootScope', '$scope', '$location', '$routeParams', 'dragularService', 'youTubeListManagerDataService', 'modelFactory', 'config',
    function ($rootScope, $scope, $location, $routeParams, dragularService, youTubeListManagerDataService, modelFactory, config) {

        $scope.showSuggestions = function () {
            youTubeListManagerDataService.showSearchResults(modelFactory.createSearchRequest($scope.model)).then(function (response) {
                var getSearchResultsAsync = function(searchResultResponse) {
                    var searchResults = searchResultResponse.Response;
                    $scope.model.nextPageSuggestionsToken = searchResultResponse.NextPageToken;

                    if (searchResults.length > 0)
                        angular.forEach(searchResults, function(value) {
                            $scope.model.suggestions.push(value);
                        });

                    if ($scope.model.nextPageSuggestionsToken !== null && $scope.model.autoLoad) {
                        youTubeListManagerDataService.showSearchResults(modelFactory.createSearchRequest($scope.model)).then(function (nextResponse) {
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

            $scope.model.current = $scope.model.markedItems[0];
            $scope.model.searchKey = $scope.model.current.VideoInfo.Title;

            $scope.showSuggestions();
        });

        $scope.getNextConflict = function () {

            var nextIndex = $scope.model.currentIndex + 1;
            if (nextIndex >= $scope.model.markedItems.length)
                return;

            var nextItem = $scope.model.markedItems[nextIndex];
            $scope.model.current = nextItem;
            $scope.model.currentIndex = nextIndex;
            $scope.model.searchKey = nextItem.VideoInfo.Title;

            $scope.showSuggestions();
        };

        $scope.resolveConflict = function(video) {
            var playListItemIndex = $scope.model.playlist.PlayListItems.findIndex(function(i) { return i.VideoInfo.Hash === $scope.model.current.VideoInfo.Hash; });
            $scope.model.playlist.PlayListItems[playListItemIndex].VideoInfo = video;
        };

        $scope.save = function() {
            youTubeListManagerDataService.savePlaylist($scope.model.playlist);
        };

        var init = function () {
            $scope.model = new suggestions();

            youTubeListManagerDataService.getPlayList($routeParams.playListId).then(function (playListResponse) {

                var getPlayListItemsAsync = function (response) {
                    var responseItem = angular.isUndefined(response.Response) ? response : response.Response;
                    var isInnerPageTokenPresent = !angular.isUndefined(responseItem.PlayListItemsNextPageToken);
                    var nextPageToken = (isInnerPageTokenPresent)
                        ? responseItem.PlayListItemsNextPageToken
                        : response.NextPageToken;

                    if (isInnerPageTokenPresent) {
                        $scope.model.playlist = responseItem;
                        $scope.model.playlistStatus = config.privacyStatus[responseItem.PrivacyStatus];
                    } else {
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