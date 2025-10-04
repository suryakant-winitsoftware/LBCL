//window.initDatepicker = (elementIds) => {
//    elementIds.forEach((elementId) => {
//        $('#' + elementId).datepicker({
//            format: 'dd/mm/yyyy',
//            autoclose: true
//        }).on('changeDate', function (e) {
//            var selectedValue = e.target.value;
//            var columnName = $(this).data('columnName');
//            DotNet.invokeMethod("Winit.UIComponents.Web", "DateChanged", columnName, selectedValue);
//        });
//    });
//};
