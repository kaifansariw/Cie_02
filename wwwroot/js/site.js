// ==============================================================================
// site.js
// Online Shopping Product Management System
// Client-side interactions, table search filtering, view mode switcher, and tooltips.
// ==============================================================================

document.addEventListener("DOMContentLoaded", function () {
    // Initialize all Bootstrap tooltips on the page
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // View Switcher logic (Table vs Grid view in Catalog)
    const viewTableBtn = document.getElementById("viewTableBtn");
    const viewGridBtn = document.getElementById("viewGridBtn");
    const catalogTableView = document.getElementById("catalogTableView");
    const catalogGridView = document.getElementById("catalogGridView");

    if (viewTableBtn && viewGridBtn && catalogTableView && catalogGridView) {
        // Check saved preference in localStorage
        const savedView = localStorage.getItem("shopcore_view_mode") || "table";
        applyViewMode(savedView);

        viewTableBtn.addEventListener("click", function () {
            applyViewMode("table");
        });

        viewGridBtn.addEventListener("click", function () {
            applyViewMode("grid");
        });

        function applyViewMode(mode) {
            if (mode === "grid") {
                catalogTableView.classList.add("d-none");
                catalogGridView.classList.remove("d-none");
                viewGridBtn.classList.add("btn-primary");
                viewGridBtn.classList.remove("btn-outline-secondary");
                viewTableBtn.classList.add("btn-outline-secondary");
                viewTableBtn.classList.remove("btn-primary");
                localStorage.setItem("shopcore_view_mode", "grid");
            } else {
                catalogTableView.classList.remove("d-none");
                catalogGridView.classList.add("d-none");
                viewTableBtn.classList.add("btn-primary");
                viewTableBtn.classList.remove("btn-outline-secondary");
                viewGridBtn.classList.add("btn-outline-secondary");
                viewGridBtn.classList.remove("btn-primary");
                localStorage.setItem("shopcore_view_mode", "table");
            }
        }
    }
});
