//Set this to true for previewing
var preview = false;

//JQuerys document.onReady function
//Gets called after the webpage is loaded
$(function() {
	//Set a timer to refresh our data every 1000 milliseconds
	setInterval(refresh, 1000);
});

//Remember popup visibility
var visible = false;

//Remember previous streak
var prevStreak = 0;

function refresh() {
	if(preview) {
		showPopup(Math.round(Math.random()*200));
		return;
	}

	//JSON query the addon service
	$.getJSON("http://127.0.0.1:9938", function(data) {
		//If data was successfully gotten
		if(data.success) {
			//Get memory readout
			var ro = data.memoryReadout;

			//Get current note streak from readout
			var streak = ro.currentHitStreak;

			//If the previous streak is larger than current streak, the streak must have broken
			if(prevStreak > streak) {
				prevStreak = 0;
			}

			//Check all values between previous streak and current streak
			for (var i = prevStreak; i < streak; i++) {
				//If we hit 50, show popup
				if(i == 50) {
					showPopup(50);
					break;
				}

				//If we hit a multiple of 100, show popup
				if(i % 100 == 0) {
					showPopup(i);
					break;
				}
			}

			//Remember the streak
			prevStreak = streak;
		}
	}, "json");
}

//Shows the popup if it is hidden
function showPopup(streak) {
	if(visible) return;

	//Ignore 0 note streaks
	if(streak == 0) return;

	//Set the text and make it visible
	$("h1.streak").text(streak+" NOTE STREAK!").css("display","block").css("animation-name","popup-animation"+(streak > 100 ? "-rainbow" : ""));

	//Allow re-showing after 3.5 seconds (animation duration + 500ms)
	setTimeout(function() {
		visible = false;
		$("h1.streak").css("display","none");
	}, 3500);

	//Remember visibility state
	visible = true;
}