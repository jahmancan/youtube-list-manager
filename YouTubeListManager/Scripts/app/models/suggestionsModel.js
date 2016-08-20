var suggestionsModel = (
    function () {
        function suggestionsModel() {
            this.playlist = {
                title: "",
                privacy: "public",
                hash: "",
                thumbnailUrl: "",
                itemCount: 0,

                playListItems: []
            };

            this.suggestions = [];

            this.playListItemsFetched = false;
            this.autoLoad = false;
            this.searchKey = "";
        };

        return suggestionsModel;
})();