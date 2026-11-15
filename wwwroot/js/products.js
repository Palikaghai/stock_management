/**
 * SmartStock - Products & Categories Module Script
 * Member 2 Implementation
 */
(() => {
    // 1. Alert Dismiss Buttons
    document.querySelectorAll(".ss-alert-close").forEach(btn => {
        btn.addEventListener("click", () => {
            const alert = btn.closest(".ss-alert");
            if (alert) {
                alert.style.opacity = "0";
                alert.style.transition = "opacity 0.2s ease-out";
                setTimeout(() => alert.remove(), 200);
            }
        });
    });

    // 2. SKU Input Auto-Uppercase & Formatting
    const skuInput = document.querySelector("input[name='Sku']");
    if (skuInput) {
        skuInput.addEventListener("input", (e) => {
            const start = skuInput.selectionStart;
            const end = skuInput.selectionEnd;
            skuInput.value = skuInput.value.toUpperCase();
            skuInput.setSelectionRange(start, end);
        });
    }

    // 3. Quick Delete Modal / Confirmation Handler for Products & Categories
    const modalIds = ["deleteConfirmModal", "deleteCategoryModal"];
    modalIds.forEach(modalId => {
        const deleteModal = document.getElementById(modalId);
        if (deleteModal) {
            deleteModal.addEventListener("show.bs.modal", (event) => {
                const trigger = event.relatedTarget;
                if (!trigger) return;

                const name = trigger.getAttribute("data-name");
                const type = trigger.getAttribute("data-type") || "item";
                const deleteUrl = trigger.getAttribute("data-delete-url");

                const namePlaceholder = deleteModal.querySelector("[data-item-name]");
                const typePlaceholder = deleteModal.querySelector("[data-item-type]");
                const deleteForm = deleteModal.querySelector("form");

                if (namePlaceholder) namePlaceholder.textContent = name;
                if (typePlaceholder) typePlaceholder.textContent = type;
                if (deleteForm && deleteUrl) deleteForm.setAttribute("action", deleteUrl);
            });
        }
    });

    // 4. Auto-submit category filter on selection change if requested
    const autoFilter = document.querySelector("[data-auto-filter]");
    if (autoFilter) {
        autoFilter.addEventListener("change", () => {
            const form = autoFilter.closest("form");
            if (form) form.submit();
        });
    }
})();
