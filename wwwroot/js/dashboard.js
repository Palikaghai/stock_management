(() => {
    const node = document.getElementById("chart-data");
    if (!node || typeof Chart === "undefined") return;

    const getIsDark = () => document.documentElement.getAttribute("data-theme") === "dark";
    const d = JSON.parse(node.textContent);
    const colors = ["#3978f6", "#7d6ce3", "#20a37a", "#f2ad45", "#38a7b3", "#ef7880", "#8796aa"];
    Chart.defaults.font.family = "'DM Sans', sans-serif";
    Chart.defaults.color = getIsDark() ? '#c2d0e1' : '#8b97a8';

    const chart = (id, type, labels, datasets, options = {}) => {
        const canvas = document.getElementById(id);
        if (!canvas) return;

        const isDark = getIsDark();
        const gridColor = isDark ? 'rgba(255,255,255,0.08)' : '#f0f2f6';
        const fillColors = isDark ? ['#8ab4ff', '#b7a8ff', '#60d8a7', '#f2bf63', '#89d3e8', '#ff9a9a', '#b4bfd4'] : colors;

        new Chart(canvas, {
            type,
            data: { labels, datasets },
            options: {
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: "bottom", labels: { usePointStyle: true, boxWidth: 7, color: isDark ? '#dfeafc' : '#607087' } }
                },
                scales: { x: { grid: { display: false }, ticks: { color: Chart.defaults.color } }, y: { beginAtZero: true, grid: { color: gridColor }, ticks: { precision: 0, color: Chart.defaults.color } }, ...options.scales },
                ...options
            }
        });
    };

    const categoryChart = [{ data: d.categoryValues, backgroundColor: colors, borderWidth: 3, borderColor: getIsDark() ? '#101b2d' : '#fff' }];
    chart("categoryChart", "doughnut", d.categories, categoryChart, { cutout: "70%" });
    chart("movementChart", "bar", d.week, [
        { label: "Stock IN", data: d.stockIn, backgroundColor: "#3978f6", borderRadius: 4, maxBarThickness: 18 },
        { label: "Stock OUT", data: d.stockOut, backgroundColor: getIsDark() ? '#9cc0ff' : '#b8c9f5', borderRadius: 4, maxBarThickness: 18 }
    ], { scales: { x: { grid: { display: false } }, y: { beginAtZero: true, grid: { color: getIsDark() ? 'rgba(255,255,255,0.08)' : '#f0f2f6' }, ticks: { precision: 0 } } } });
    chart("valuationChart", "bar", d.categories, [{ label: "Inventory value", data: d.valuations, backgroundColor: colors, borderRadius: 5 }],
        { indexAxis: "y", plugins: { legend: { display: false } }, scales: { x: { beginAtZero: true, ticks: { callback: v => "₹" + Number(v).toLocaleString("en-IN"), color: getIsDark() ? '#dfeafc' : '#607087' }, grid: { color: getIsDark() ? 'rgba(255,255,255,0.08)' : '#f0f2f6' } } } });
    chart("monthlyChart", "line", d.months, [
        { label: "Stock IN", data: d.monthlyIn, borderColor: "#3978f6", backgroundColor: "#3978f61a", fill: true, tension: .35, pointRadius: 3 },
        { label: "Stock OUT", data: d.monthlyOut, borderColor: "#20a37a", backgroundColor: "#20a37a12", fill: true, tension: .35, pointRadius: 3 }
    ], { scales: { x: { grid: { display: false } }, y: { beginAtZero: true, grid: { color: getIsDark() ? 'rgba(255,255,255,0.08)' : '#f0f2f6' }, ticks: { precision: 0 } } } });
})();
