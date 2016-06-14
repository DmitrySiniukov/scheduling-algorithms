function init() {
	$('.confirmation').on('click', function () {
		return confirm('Ви впевнені?');
	});

	gantt.config.xml_date="%d-%m-%Y %H:%i";
	gantt.config.scale_unit = "minute";
	gantt.config.step = 10;
	gantt.config.date_scale = "%H:%i";
	gantt.config.date_grid = "%H:%i";
	gantt.config.min_column_width = 30;
	gantt.config.duration_unit = "minute";
	gantt.config.duration_step = 1;
	gantt.config.scale_height = 40;
    gantt.config.scale_width = 10;
    gantt.config.readonly = true;
	
	gantt.config.subscales = [
		{unit:"day", step:1, date : "%j %F, %l"},
		{ unit: "minute", step: 1, date: "%i" }
	];

	var jsonStr = $('#gantt-chart-data').val();
    if (jsonStr != null) {
        var ganttData = JSON.parse(jsonStr);

        $("#gantt-chart").dhx_gantt({
            data: ganttData
        });
    }

    $(".jquery-date-time").datetimepicker();

    var lowerDateTimeSiblings = $(".date-time-lower");
    for (var i = 0; i < lowerDateTimeSiblings.length; i++) {
        var value = $(lowerDateTimeSiblings[i]).val();
        $(lowerDateTimeSiblings[i].previousElementSibling).val(value);
    }
}