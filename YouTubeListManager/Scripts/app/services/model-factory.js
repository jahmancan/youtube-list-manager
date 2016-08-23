mainModule.factory('modelFactory',
    [
        'config', 
        function (config) {

            var createPlaylistItem = function(video) {

            };

            var createSearchRequest = function(suggestionsModel) {
                var request = new searchRequest();
                request.SearchKey = suggestionsModel.current.VideoInfo.Title;
                request.NextPageToken = suggestionsModel.nextPageSuggestionsToken;
                request.UsedVideoIdList = suggestionsModel.playlist.PlayListItems.map(function (playListItem) { return playListItem.VideoInfo.Hash; });
                //todo: implement videoDuration parameter in js

                return request;
            };
           

            var factory = {
                createPlaylistItem: createPlaylistItem,
                createSearchRequest: createSearchRequest
            };

            return factory;
        }
    ]
);


