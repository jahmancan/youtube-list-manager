var suggestions = (
    function () {
        function suggestions() {
            this.playlist = {
                title: "",
                privacy: "public",
                hash: "",
                thumbnailUrl: "",
                itemCount: 0,

                playListItems: []
            };
            this.playlistStatus = "";

            this.suggestions = [];
            this.currentIndex = 0;
            this.current = null;

            this.nextPageSuggestionsToken = "";
            this.playListItemsFetched = false;
            this.autoLoad = false;
            this.searchKey = "";
        };

        return suggestions;
})();