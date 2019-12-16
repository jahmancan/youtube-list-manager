mainModule.factory('youTubeListManagerDataService', 
    ['$http', '$q', 
    function ($http, $q) {
        return {
            showSearchResults: function (request) {
                var deferred = $q.defer();
                $http.post('/video/post', request ).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getPlayListItems: function(requestToken, playlistId) {
                var deferred = $q.defer();
                $http.get('/api/playlistitem/getasync/' + playlistId + '/' + requestToken).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getPlayList: function (playlistId) {
                var deferred = $q.defer();
                $http.get('/api/playlist/getasync/' + playlistId).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getAllPlaylists: function (requestToken) {
                var deferred = $q.defer();
                $http.get('/playlist/getallasync' + (requestToken !== "" && requestToken !== null && !angular.isUndefined(requestToken) ? "/" + requestToken : "")).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            savePlaylist: function (playlist) {
                var deferred = $q.defer();
                $http.post('/playlist/save', playlist).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            }
        }
    }]
);