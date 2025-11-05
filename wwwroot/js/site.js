// Aura scripts
(function () {
    // Tooltip Bootstrap (si usas Bootstrap 5)
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Confirmación antes de eliminar
    const deletes = document.querySelectorAll('.btn-delete');
    deletes.forEach(btn => {
        btn.addEventListener('click', e => {
            if (!confirm('¿Estás seguro de eliminar este registro?')) {
                e.preventDefault();
            }
        });
    });

    // Auto-hide alert después de 4 s
    const alerts = document.querySelectorAll('.alert-auto');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.classList.add('fade');
            alert.remove();
        }, 4000);
    });
})();