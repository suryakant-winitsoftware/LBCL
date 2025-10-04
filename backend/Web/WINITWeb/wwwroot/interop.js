function saveAsFile(fileName, byteString) {
    const link = document.createElement('a');
    link.href = 'data:application/octet-stream;base64,' + byteString;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}


function scrollToElementFunction(elementId) {
    console.log("mahir");
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
    }
};

document.addEventListener("click", function (event) {
    const dropdown = document.getElementById("showDropDown");

    if (dropdown != null && !dropdown.contains(event.target)) { // Check if clicked element is outside the dropdown

        dropdown.style.display = "none";


    }
});


function validateDateRange(input) {
    var currentDate = new Date();
    var inputDate = new Date(input.value);

    // Calculate the date 3 months ago from the current date
    var threeMonthsAgo = new Date();
    threeMonthsAgo.setMonth(currentDate.getMonth() - 3);

    if (inputDate < threeMonthsAgo || inputDate > currentDate) {
        alert("Cheque date should be within the past 3 months.");
        input.value = ""; // Clear the input value
    }
}

window.initializeDatePicker = (element, dotNetReference, startDate, endDate) => {
    var options = {
        format: 'dd/mm/yyyy',
        autoclose: true,
        todayHighlight: true // Highlight today's date
    };

    if (startDate) {
        options.startDate = new Date(startDate);
    }

    if (endDate) {
        options.endDate = new Date(endDate);
    }

    $(element).datepicker(options).on('changeDate', function (e) {
        var selectedDate = e.format(0, 'mm/dd/yyyy');
        dotNetReference.invokeMethodAsync('DateSelected', selectedDate);
    });
};

window.initNumberInput = function (dotNetHelper) {
    document.querySelectorAll('input[type="number"]').forEach(input => {
        input.addEventListener('input', function(e) {
            const value = e.target.value;
            dotNetHelper.invokeMethodAsync('EnforceMaxValue', value);
        });

        // Prevent typing numbers larger than max
        input.addEventListener('keydown', function(e) {
            if (e.key >= 0 && e.key <= 9) {
                const newValue = this.value + e.key;
                if (parseFloat(newValue) > parseFloat(this.max)) {
                    e.preventDefault();
                }
            }
        });

        // Handle paste events
        input.addEventListener('paste', function(e) {
            const pastedText = (e.clipboardData || window.clipboardData).getData('text');
            if (parseFloat(pastedText) > parseFloat(this.max)) {
                e.preventDefault();
                this.value = this.max;
            }
        });
    });
};