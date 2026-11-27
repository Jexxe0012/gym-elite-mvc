(() => {
    const form = document.getElementById("filtro-reportes");
    const from = form.elements.FechaDesde;
    const to = form.elements.FechaHasta;

    function validatePeriod() {
        to.setCustomValidity("");
        if (!from.value || !to.value) return;

        const start = from.valueAsDate;
        const end = to.valueAsDate;
        if (start > end) {
            to.setCustomValidity("La fecha inicial no puede ser posterior a la final.");
            return;
        }

        // UTC evita cambios de día y el ajuste a fin de mes coincide con DateOnly.AddMonths.
        const day = start.getUTCDate();
        start.setUTCDate(1);
        start.setUTCMonth(start.getUTCMonth() + 4);
        start.setUTCDate(0);
        start.setUTCDate(Math.min(day, start.getUTCDate()));
        if (end > start) {
            to.setCustomValidity("El período del reporte no puede superar tres meses.");
        }
    }

    from.addEventListener("input", validatePeriod);
    to.addEventListener("input", validatePeriod);
    const results = document.getElementById("resultados-reportes");
    const status = document.getElementById("estado-filtro-reportes");
    const submit = form.querySelector('button[type="submit"]');

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        validatePeriod();
        if (submit.disabled || !form.reportValidity()) return;

        const url = new URL(form.action);
        url.search = new URLSearchParams(new FormData(form)).toString();
        submit.disabled = true;
        results.setAttribute("aria-busy", "true");
        status.classList.remove("text-danger");
        status.textContent = "Generando reporte…";

        try {
            const response = await fetch(url, { credentials: "same-origin" });
            if (response.redirected || response.status === 401 || response.status === 403) {
                status.classList.add("text-danger");
                status.textContent = "Verifica tu sesión y tus permisos para consultar reportes.";
                return;
            }
            if (!response.ok) throw new Error("Falló la consulta de reportes.");

            const page = new DOMParser().parseFromString(await response.text(), "text/html");
            const updated = page.getElementById("resultados-reportes");
            if (updated?.dataset.generado !== "true") {
                throw new Error("El servidor no generó el reporte.");
            }
            results.replaceChildren(...updated.childNodes);
            status.textContent = "Reporte actualizado.";
        } catch (error) {
            console.error("Error al actualizar el reporte:", error);
            status.classList.add("text-danger");
            status.textContent = "No se pudo generar el reporte. Vuelve a intentar.";
        } finally {
            submit.disabled = false;
            results.removeAttribute("aria-busy");
        }
    });
})();
