var playListItem = (
    function () {
        function playListItem() {
            this.VideoInfo = {
                Title: "",
                Privacy: "public",
                Hash: "",
                ThumbnailUrl: "",
                Duration: 0,
                Live: true
            };
            this.Position = 0;
        };

        return playListItem;
    }
)();