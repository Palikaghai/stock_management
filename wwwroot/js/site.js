(() => {
    const date = document.querySelector("[data-current-date]");
    if (date) date.textContent = new Intl.DateTimeFormat(undefined, { weekday: "short", month: "short", day: "numeric" }).format(new Date());

    const toggle = document.querySelector(".menu-toggle");
    const menuIcon = toggle?.querySelector("i");
    const closeSidebar = () => {
        document.body.classList.remove("nav-open");
        toggle?.setAttribute("aria-expanded", "false");
        if (menuIcon) menuIcon.className = "bi bi-list";
    };
    toggle?.addEventListener("click", () => {
        const open = document.body.classList.toggle("nav-open");
        toggle.setAttribute("aria-expanded", String(open));
        if (menuIcon) menuIcon.className = open ? "bi bi-x-lg" : "bi bi-list";
    });
    document.querySelector(".backdrop")?.addEventListener("click", closeSidebar);
    document.querySelectorAll(".side-nav a, .sidebar-bottom a").forEach(link => {
        link.addEventListener("click", closeSidebar);
    });
    document.addEventListener("keydown", event => {
        if (event.key === "Escape") closeSidebar();
    });

    const themeToggles = document.querySelectorAll("[data-theme-toggle]");
    const setTheme = (theme, persist = false) => {
        document.documentElement.setAttribute('data-theme', theme);
        if (persist) localStorage.setItem('smartstock-theme', theme);

        themeToggles.forEach(themeToggle => {
            const icon = themeToggle.querySelector('i');
            if (icon) {
                icon.className = theme === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
            }
            themeToggle.setAttribute('aria-pressed', String(theme === 'dark'));
            themeToggle.setAttribute('aria-label', `Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`);
            const label = themeToggle.querySelector('[data-theme-label]');
            if (label) label.textContent = theme === 'dark' ? 'Dark mode is on' : 'Light mode is on';
        });

        if (window.Chart) {
            Chart.defaults.color = theme === 'dark' ? '#c2d0e1' : '#8b97a8';
        }
    };

    const initialTheme = localStorage.getItem('smartstock-theme') === 'dark' ? 'dark' : 'light';
    setTheme(initialTheme);
    themeToggles.forEach(themeToggle => themeToggle.addEventListener('click', () => {
        const current = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        setTheme(current, true);
    }));

    document.querySelectorAll("[data-profile-menu]").forEach(menu => {
        const trigger = menu.querySelector("[data-profile-trigger]");
        const dropdown = menu.querySelector("[data-profile-dropdown]");
        if (!trigger || !dropdown) return;

        const closeMenu = () => {
            dropdown.hidden = true;
            trigger.setAttribute("aria-expanded", "false");
        };
        trigger.addEventListener("click", () => {
            const open = dropdown.hidden;
            dropdown.hidden = !open;
            trigger.setAttribute("aria-expanded", String(open));
        });
        document.addEventListener("click", event => {
            if (!menu.contains(event.target)) closeMenu();
        });
        document.addEventListener("keydown", event => {
            if (event.key === "Escape" && !dropdown.hidden) {
                closeMenu();
                trigger.focus();
            }
        });
    });

    document.querySelectorAll('.password-toggle-btn').forEach(button => {
        button.addEventListener('click', () => {
            const wrapper = button.closest('.input-with-action');
            const input = wrapper?.querySelector('.password-field');
            if (!input) return;

            const isPassword = input.type === 'password';
            input.type = isPassword ? 'text' : 'password';
            const icon = button.querySelector('i');
            if (icon) {
                icon.className = isPassword ? 'bi bi-eye' : 'bi bi-eye-slash';
            }
        });
    });
})();
