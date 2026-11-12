(() => {
    const node = document.getElementById("chart-data");
    if (!node || typeof Chart === "undefined") return;
    const d = JSON.parse(node.textContent);
    const colors = ["#3978f6", "#7d6ce3", "#20a37a", "#f2ad45", "#38a7b3", "#ef7880", "#8796aa"];
    Chart.defaults.font.family = "'DM Sans', sans-serif";
    Chart.defaults.color = "#8b97a8";
    const chart = (id, type, labels, datasets, options = {}) => {
        const canvas = document.getElementById(id);
        if (canvas) new Chart(canvas, { type, data: { labels, datasets }, options: { maintainAspectRatio: false, plugins: { legend: { position: "bottom", labels: { usePointStyle: true, boxWidth: 7 } } }, ...options } });
    };
    chart("categoryChart", "doughnut", d.categories, [{ data: d.categoryValues, backgroundColor: colors, borderWidth: 3, borderColor: "#fff" }], { cutout: "70%" });
    chart("movementChart", "bar", d.week, [
        { label: "Stock IN", data: d.stockIn, backgroundColor: "#3978f6", borderRadius: 4, maxBarThickness: 18 },
        { label: "Stock OUT", data: d.stockOut, backgroundColor: "#b8c9f5", borderRadius: 4, maxBarThickness: 18 }
    ], { scales: { x: { grid: { display: false } }, y: { beginAtZero: true, grid: { color: "#f0f2f6" }, ticks: { precision: 0 } } } });
    chart("valuationChart", "bar", d.categories, [{ label: "Inventory value", data: d.valuations, backgroundColor: colors, borderRadius: 5 }],
        { indexAxis: "y", plugins: { legend: { display: false } }, scales: { x: { beginAtZero: true, ticks: { callback: v => "₹" + Number(v).toLocaleString("en-IN") } } } });
    chart("monthlyChart", "line", d.months, [
        { label: "Stock IN", data: d.monthlyIn, borderColor: "#3978f6", backgroundColor: "#3978f61a", fill: true, tension: .35, pointRadius: 3 },
        { label: "Stock OUT", data: d.monthlyOut, borderColor: "#20a37a", backgroundColor: "#20a37a12", fill: true, tension: .35, pointRadius: 3 }
    ], { scales: { x: { grid: { display: false } }, y: { beginAtZero: true, grid: { color: "#f0f2f6" }, ticks: { precision: 0 } } } });
})();
