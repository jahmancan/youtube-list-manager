mainModule.filter('durationFormatter', [
    function() {
        return function(duration) {
            var minutes = Math.floor(duration / 60);
            var seconds = duration % 60;

            if (seconds < 10)
                seconds = "0" + seconds;

            if (minutes < 10)
                minutes = "0" + minutes;

            return minutes + ":" + seconds;
        };
    }
]);