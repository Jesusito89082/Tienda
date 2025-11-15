// Script para búsqueda con autocompletado
(function () {
    'use strict';

    const searchInput = document.getElementById('searchInput');
    const searchSuggestions = document.getElementById('searchSuggestions');
    const searchForm = document.getElementById('searchForm');
    let searchTimeout = null;
    let currentFocus = -1;

if (!searchInput || !searchSuggestions) {
    return;
    }

    // Función para buscar sugerencias
    function buscarSugerencias(term) {
      if (term.length < 2) {
     ocultarSugerencias();
        return;
        }

     fetch(`/Busqueda/Sugerencias?term=${encodeURIComponent(term)}`)
      .then(response => response.json())
  .then(data => {
     mostrarSugerencias(data);
 })
   .catch(error => {
     console.error('Error al buscar sugerencias:', error);
            });
    }

 // Función para mostrar sugerencias
    function mostrarSugerencias(data) {
        searchSuggestions.innerHTML = '';
  
   if (data.length === 0) {
const noResults = document.createElement('div');
    noResults.className = 'suggestion-item no-results';
 noResults.innerHTML = '<i class="bi bi-inbox"></i> No se encontraron resultados';
            searchSuggestions.appendChild(noResults);
            searchSuggestions.style.display = 'block';
            return;
        }

  data.forEach((item, index) => {
        const suggestionItem = document.createElement('div');
            suggestionItem.className = 'suggestion-item';
  suggestionItem.setAttribute('data-index', index);

 if (item.tipo === 'categoria') {
  suggestionItem.innerHTML = `
   <div class="suggestion-content">
   <div class="suggestion-icon bg-secondary">
   <i class="bi bi-tag-fill"></i>
      </div>
  <div class="suggestion-info">
     <div class="suggestion-title">${item.nombre}</div>
  <div class="suggestion-subtitle">Categoría - ${item.totalProductos} productos</div>
     </div>
       </div>
            `;
     suggestionItem.onclick = () => window.location.href = item.url;
      } else {
      suggestionItem.innerHTML = `
              <div class="suggestion-content">
        <div class="suggestion-image">
      ${item.imagen 
     ? `<img src="${item.imagen}" alt="${item.nombre}" />` 
     : '<i class="bi bi-image"></i>'}
</div>
              <div class="suggestion-info">
     <div class="suggestion-title">${item.nombre}</div>
                   <div class="suggestion-subtitle">${item.categoria} • ${formatearPrecio(item.precio)}</div>
    </div>
      </div>
        `;
  suggestionItem.onclick = () => window.location.href = item.url;
            }

   searchSuggestions.appendChild(suggestionItem);
        });

  searchSuggestions.style.display = 'block';
    }

    // Función para ocultar sugerencias
    function ocultarSugerencias() {
        searchSuggestions.style.display = 'none';
        searchSuggestions.innerHTML = '';
 currentFocus = -1;
    }

    // Función para formatear precio
    function formatearPrecio(precio) {
        return '?' + precio.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
    }

    // Función para navegar con teclado
 function navegarSugerencias(direction) {
        const items = searchSuggestions.querySelectorAll('.suggestion-item:not(.no-results)');
        
        if (items.length === 0) return;

        // Remover clase active de todos los items
        items.forEach(item => item.classList.remove('active'));

  // Calcular nuevo índice
  currentFocus += direction;

    if (currentFocus >= items.length) {
   currentFocus = 0;
        } else if (currentFocus < 0) {
            currentFocus = items.length - 1;
        }

        // Agregar clase active al item actual
        items[currentFocus].classList.add('active');
        items[currentFocus].scrollIntoView({ block: 'nearest' });
    }

    // Event listeners
    searchInput.addEventListener('input', function (e) {
        const term = e.target.value.trim();
        
 // Cancelar búsqueda anterior
        if (searchTimeout) {
         clearTimeout(searchTimeout);
        }

     // Buscar después de 300ms de inactividad
        searchTimeout = setTimeout(() => {
        buscarSugerencias(term);
     }, 300);
    });

    searchInput.addEventListener('keydown', function (e) {
        const suggestions = searchSuggestions.style.display === 'block';
        
     if (!suggestions) return;

      if (e.key === 'ArrowDown') {
            e.preventDefault();
            navegarSugerencias(1);
        } else if (e.key === 'ArrowUp') {
     e.preventDefault();
   navegarSugerencias(-1);
        } else if (e.key === 'Enter') {
            const activeItem = searchSuggestions.querySelector('.suggestion-item.active');
       if (activeItem && !activeItem.classList.contains('no-results')) {
         e.preventDefault();
            activeItem.click();
         }
        } else if (e.key === 'Escape') {
            ocultarSugerencias();
        }
    });

    // Cerrar sugerencias al hacer clic fuera
    document.addEventListener('click', function (e) {
 if (!searchInput.contains(e.target) && !searchSuggestions.contains(e.target)) {
  ocultarSugerencias();
    }
    });

    // Mostrar sugerencias al hacer focus si hay texto
    searchInput.addEventListener('focus', function () {
        const term = searchInput.value.trim();
      if (term.length >= 2) {
          buscarSugerencias(term);
        }
    });

    // Limpiar al hacer submit
    searchForm.addEventListener('submit', function () {
        ocultarSugerencias();
    });

})();
