(() => {
    const form = document.getElementById("filtro-productos");
    const results = document.getElementById("resultados-productos");
    const status = document.getElementById("estado-filtro-productos");
    const tableBody = document.getElementById("filas-productos");
    const emptyState = document.getElementById("productos-vacios");
    const rowTemplate = document.getElementById("fila-producto");
    const submitButton = form.querySelector("button");
    const currency = new Intl.NumberFormat("es-SV", { style: "currency", currency: "USD" });
    const counters = {
        productosActivos: document.getElementById("productos-activos"),
        productosStockBajo: document.getElementById("productos-stock-bajo"),
        unidadesDisponibles: document.getElementById("unidades-disponibles")
    };

    function createRow(product) {
        if (!Number.isSafeInteger(product.id) || product.id <= 0) {
            throw new Error("El servidor devolvió un identificador de producto inválido.");
        }

        const row = rowTemplate.content.firstElementChild.cloneNode(true);
        for (const cell of row.querySelectorAll("[data-field]")) {
            const field = cell.dataset.field;
            cell.textContent = field === "precioVenta" ? currency.format(product[field]) : product[field];
        }

        row.querySelector("[data-inactivo]").hidden = product.activo;
        const badge = row.querySelector('[data-field="estadoExistencias"]');
        if (product.estadoExistencias === "Disponible") {
            badge.classList.add("bg-success");
        } else if (product.estadoExistencias === "Stock bajo") {
            badge.classList.add("bg-warning", "text-dark");
        } else {
            badge.classList.add("bg-danger");
        }

        for (const link of row.querySelectorAll("a")) {
            const url = new URL(link.getAttribute("href"), document.baseURI);
            if (url.searchParams.get("id") === "0") {
                url.searchParams.set("id", product.id);
            } else {
                url.pathname = url.pathname.replace(/\/0$/, "/" + product.id);
            }
            link.href = url.href;
        }

        return row;
    }

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (submitButton.disabled) return;

        const url = new URL(form.dataset.filterUrl, document.baseURI);
        url.search = new URLSearchParams(new FormData(form)).toString();

        submitButton.disabled = true;
        results.setAttribute("aria-busy", "true");
        status.classList.remove("text-danger");
        status.textContent = "Cargando productos.";

        try {
            const response = await fetch(url, { credentials: "same-origin", headers: { Accept: "application/json" } });
            if (response.redirected || response.status === 401 || response.status === 403) {
                status.classList.add("text-danger");
                status.textContent = response.status !== 403
                    ? "Vuelve a iniciar sesión para consultar productos."
                    : "Tu usuario no tiene permiso para consultar productos.";
                return;
            }
            if (!response.ok) throw new Error("Falló la consulta.");

            const data = await response.json();
            if (!Array.isArray(data.productos) ||
                !Object.keys(counters).every((key) => Number.isSafeInteger(data[key]))) {
                throw new Error("La respuesta del servidor tiene un formato inesperado.");
            }

            const rows = document.createDocumentFragment();
            for (const product of data.productos) {
                rows.append(createRow(product));
            }
            tableBody.replaceChildren(rows);
            emptyState.hidden = data.productos.length > 0;
            for (const [key, element] of Object.entries(counters)) {
                element.textContent = data[key];
            }
            status.textContent = "Resultados actualizados.";
        } catch (error) {
            console.error("Error al filtrar productos:", error);
            status.classList.add("text-danger");
            status.textContent = "No se pudo actualizar la lista. Vuelve a intentar.";
        } finally {
            submitButton.disabled = false;
            results.removeAttribute("aria-busy");
        }
    });
})();
