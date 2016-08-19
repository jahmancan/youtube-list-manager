mainModule.factory('youTubeListManagerDataService', 
    ['$http', '$q', 
    function ($http, $q) {
        return {
            getSuggestions: function (searchKey, requestToken) {
                var deferred = $q.defer();
                $http.get('/video/get/' + searchKey + (requestToken !== "" && requestToken !== null ? '/' + requestToken : "")).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getPlayList: function (playlistId) {
                var deferred = $q.defer();
                $http.get('/playlist/get/' + playlistId).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            getAllPlaylists: function (requestToken) {
                var deferred = $q.defer();
                $http.get('/playlist/getall' + (requestToken !== "" && requestToken !== null && !angular.isUndefined(requestToken) ? "/" + requestToken : "")).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            },

            updatePlaylist: function (playlist) {
                var deferred = $q.defer();
                $http.post('/playlist/save', playlist).success(deferred.resolve).error(deferred.reject);
                return deferred.promise;
            }
        }
    }]
);