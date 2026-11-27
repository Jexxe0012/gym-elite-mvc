(() => {
    const form = document.getElementById("form-nueva-venta");
    const quantities = Array.from(form.querySelectorAll(".cantidad"));
    const total = document.getElementById("total");
    const status = document.getElementById("estado-nueva-venta");
    const submitButton = form.querySelector('button[type="submit"]');
    const receipt = document.getElementById("comprobante-nueva-venta");
    const currency = new Intl.NumberFormat("es-SV", { style: "currency", currency: "USD" });

    function updateTotal() {
        const amount = quantities.reduce((sum, input) => {
            return sum + (Number(input.value) || 0) * Number(input.dataset.precio);
        }, 0);
        total.textContent = currency.format(amount);
    }

    form.addEventListener("input", () => {
        status.textContent = "";
        updateTotal();
    });

    form.addEventListener("submit", async (event) => {
        if (event.defaultPrevented) return;
        if (submitButton.disabled) {
            event.preventDefault();
            return;
        }

        const invalid = quantities.find((input) => {
            const quantity = Number(input.value);
            return input.value === "" || !Number.isInteger(quantity) || quantity < 0 || quantity > Number(input.max);
        });

        if (invalid || !quantities.some((input) => Number(input.value) > 0)) {
            event.preventDefault();
            status.classList.add("text-danger");
            status.textContent = invalid
                ? "Ingresa una cantidad entera entre cero y las existencias disponibles."
                : "Ingresa una cantidad mayor que cero para al menos un producto.";
            (invalid ?? quantities[0])?.focus();
            return;
        }

        event.preventDefault();
        const body = new FormData(form);
        status.classList.remove("text-danger");
        status.textContent = "Registrando venta…";
        submitButton.disabled = true;
        quantities.forEach((input) => { input.readOnly = true; });

        try {
            const response = await fetch(form.action, {
                method: "POST",
                credentials: "same-origin",
                headers: { Accept: "application/json" },
                body
            });

            if (response.redirected || response.status === 401 || response.status === 403) {
                status.textContent = "Verifica tu sesión y tus permisos antes de continuar.";
                status.classList.add("text-danger");
                return;
            }

            if (response.status === 400) {
                const error = response.headers.get("content-type")?.includes("application/json")
                    ? await response.json() : {};
                status.textContent = error.mensaje ?? "Actualiza la página antes de volver a enviar el formulario.";
                status.classList.add("text-danger");
                if (error.mensaje) submitButton.disabled = false;
                return;
            }
            if (!response.ok) throw new Error("No se recibió confirmación.");

            const sale = await response.json();
            receipt.href = sale.comprobante;
            receipt.hidden = false;
            status.textContent = `Venta ${sale.numero} registrada. Total ${currency.format(sale.total)}.`;
            quantities.forEach((input) => { input.value = "0"; });
            updateTotal();
        } catch (error) {
            console.error("Error al confirmar la venta:", error);
            status.classList.add("text-danger");
            status.textContent = "No se pudo confirmar el resultado. Revisa el listado de ventas antes de volver a intentarlo.";
        } finally {
            if (!submitButton.disabled) quantities.forEach((input) => { input.readOnly = false; });
        }
    });

    updateTotal();
})();
