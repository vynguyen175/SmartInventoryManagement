// Global AJAX loader toggle
$(document).ajaxStart(function () {
    $('#loader').show();
});

$(document).ajaxStop(function () {
    $('#loader').hide();
});

$(document).ready(function() {
    // When the search/filter form is submitted
    $('#searchFilterForm').submit(function (event) {
        event.preventDefault();  // Prevent form submission
        var formData = $(this).serialize();  // Get the form data

        // Show loader
        $('#loader').show();

        // Make AJAX request to fetch filtered products
        $.ajax({
            url: '/Product/Index',  // The correct URL for fetching filtered data
            type: 'GET',
            data: formData,  // Send form data (search, filter)
            success: function(data) {
                console.log('AJAX Success:', data); // Debugging response

                // Directly update the product list and form
                $('#productListContainer').html(data);  // The data should already contain the updated product list
                $('#searchFilterContainer').html($(data).find('#searchFilterContainer').html());

                // Hide loader
                $('#loader').hide();
            },

            error: function() {
                alert("An error occurred while fetching the data.");
                $('#loader').hide();
            }
        });
    });
});

// AJAX for Create Product Form submission
$('form#createProductForm').submit(function(event) {
    event.preventDefault();
    var formData = $(this).serialize();

    // Show loader
    $('#loader').show();

    // Directly using the Razor URL generation syntax in JavaScript
    var createProductUrl = '@Url.Action("Create", "Product")';

    $.ajax({
        url: createProductUrl, // POST to Create action using the dynamically generated URL
        type: 'POST',
        data: formData,
        success: function(response) {
            $('#loader').hide();
            if (response.success) {
                alert(response.message); // Show success message
                // Redirect to the Product Index page
                window.location.href = response.redirectUrl; // Use the redirect URL returned from the controller
            } else {
                alert("Error creating product!");
            }
        },
        error: function() {
            $('#loader').hide();
            alert('Error creating product!');
        }
    });
});
