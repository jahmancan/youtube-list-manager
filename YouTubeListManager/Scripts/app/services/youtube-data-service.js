mainModule.factory('youTubeListManagerDataService', 
    ['$http', '$q', 
    function ($http, $q) {
        return {
            getSuggestions: function (searchKey, requestToken) {
                var deferred = $q.defer();
                $http.get('/api/video/get/' + searchKey + (requestToken !== "" && requestToken !== null && !angular.isUndefined(requestToken) ? '/' + requestToken : "")).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getPlayListItems: function(requestToken, playlistId) {
                var deferred = $q.defer();
                $http.get('/api/playlistitem/get/' + playlistId + '/' + requestToken).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getPlayList: function (playlistId) {
                var deferred = $q.defer();
                $http.get('/api/playlist/get/' + playlistId).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getAllPlaylists: function (requestToken) {
                var deferred = $q.defer();
                $http.get('/playlist/getall' + (requestToken !== "" && requestToken !== null && !angular.isUndefined(requestToken) ? "/" + requestToken : "")).success(deferred.resolve).error(deferred.reject);
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