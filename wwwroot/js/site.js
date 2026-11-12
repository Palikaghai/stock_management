(() => {
    const date = document.querySelector("[data-current-date]");
    if (date) date.textContent = new Intl.DateTimeFormat(undefined, { weekday: "short", month: "short", day: "numeric" }).format(new Date());
    const toggle = document.querySelector(".menu-toggle");
    const close = () => { document.body.classList.remove("nav-open"); toggle?.setAttribute("aria-expanded", "false"); };
    toggle?.addEventListener("click", () => {
        const open = document.body.classList.toggle("nav-open");
        toggle.setAttribute("aria-expanded", String(open));
    });
    document.querySelector(".backdrop")?.addEventListener("click", close);
    document.addEventListener("keydown", e => { if (e.key === "Escape") close(); });
    document.querySelectorAll(".muted-link").forEach(link => link.addEventListener("click", e => e.preventDefault()));
})();
