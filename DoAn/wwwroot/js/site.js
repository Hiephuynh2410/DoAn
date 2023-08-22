document.addEventListener("DOMContentLoaded", function () {
    var deleteLinks = document.querySelectorAll(".delete-link");
    var customPopup = document.getElementById("customPopup");
    var confirmDeleteButton = document.getElementById("confirmDelete");
    var cancelDeleteButton = document.getElementById("cancelDelete");
    var currentProviderId = null;

    deleteLinks.forEach(function (link) {
        link.addEventListener("click", function (event) {
            event.preventDefault();
            currentProviderId = link.getAttribute("data-providerid");
            customPopup.style.display = "block";
        });
    });

    confirmDeleteButton.addEventListener("click", function () {
        customPopup.style.display = "none";
        if (currentProviderId !== null) {
            // Use Ajax to call the backend Delete action
            var xhr = new XMLHttpRequest();
            xhr.open("POST", "@Url.Action("Delete", "Provider")", true);
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            xhr.onreadystatechange = function () {
                if (xhr.readyState === XMLHttpRequest.DONE) {
                    if (xhr.status === 200) {
                        // Successful deletion, you might want to refresh the page or update the UI
                        location.reload(); // Refresh the page
                    } else {
                        // Handle error
                        console.error("Error deleting provider.");
                    }
                }
            };
            xhr.send("providerId=" + encodeURIComponent(currentProviderId));
        }
    });

    cancelDeleteButton.addEventListener("click", function () {
        customPopup.style.display = "none";
        currentProviderId = null;
    });
});
