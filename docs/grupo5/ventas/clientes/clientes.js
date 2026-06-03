/* ==========================================================
   HEVELAB — clientes.js
   Vanilla JS para búsqueda, validación y control del acordeón
   ========================================================== */
(function () {
  'use strict';

  document.addEventListener('DOMContentLoaded', function () {
    
    // 1. Búsqueda en tiempo real en la lista de clientes
    var busquedaInput = document.getElementById('busquedaInput');
    if (busquedaInput) {
      busquedaInput.addEventListener('input', function () {
        var term = busquedaInput.value.toLowerCase().trim();
        var rows = document.querySelectorAll('.cliente-row');
        var visibleCount = 0;
        
        rows.forEach(function (row) {
          var razon = (row.getAttribute('data-razon') || '').toLowerCase();
          var documento = (row.getAttribute('data-documento') || '').toLowerCase();
          
          if (razon.includes(term) || documento.includes(term)) {
            row.style.display = '';
            visibleCount++;
          } else {
            row.style.display = 'none';
          }
        });
        
        var contador = document.getElementById('contadorClientes');
        if (contador) {
          contador.textContent = 'Mostrando ' + visibleCount + ' clientes';
        }
      });
    }

    // 2. Confirmación para el cambio de estado (ToggleEstado)
    var toggleForms = document.querySelectorAll('.form-toggle-estado');
    toggleForms.forEach(function (form) {
      form.addEventListener('submit', function (e) {
        var confirmed = confirm('¿Confirmas cambiar el estado de este cliente?');
        if (!confirmed) {
          e.preventDefault();
        }
      });
    });

    // 3. Validación de formulario antes del submit
    var clienteForm = document.getElementById('clienteForm');
    if (clienteForm) {
      // Limpiar estados de error al escribir/modificar
      var inputsToWatch = clienteForm.querySelectorAll('.form-control, .form-select');
      inputsToWatch.forEach(function (input) {
        var clearErr = function () {
          input.classList.remove('is-invalid');
        };
        input.addEventListener('input', clearErr);
        input.addEventListener('change', clearErr);
      });

      clienteForm.addEventListener('submit', function (e) {
        var isValid = true;
        var formAlert = document.getElementById('formAlert');
        
        // Limpiar errores previos
        inputsToWatch.forEach(function (input) {
          input.classList.remove('is-invalid');
        });
        if (formAlert) {
          formAlert.classList.add('d-none');
        }

        // Validar Razón Social
        var razonSocialInput = document.getElementById('RazonSocialInput');
        if (razonSocialInput && !razonSocialInput.value.trim()) {
          razonSocialInput.classList.add('is-invalid');
          isValid = false;
        }

        // Validar N° Documento
        var numeroDocumentoInput = document.getElementById('NumeroDocumentoInput');
        if (numeroDocumentoInput && !numeroDocumentoInput.value.trim()) {
          numeroDocumentoInput.classList.add('is-invalid');
          isValid = false;
        }

        // Validar Correo Electrónico
        var emailInput = document.getElementById('EmailInput');
        if (emailInput) {
          var emailVal = emailInput.value.trim();
          var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
          if (!emailVal || !emailRegex.test(emailVal)) {
            emailInput.classList.add('is-invalid');
            isValid = false;
          }
        }

        // Validar Límite de Crédito
        var limiteCreditoInput = document.getElementById('LimiteCreditoInput');
        if (limiteCreditoInput) {
          var limiteVal = parseFloat(limiteCreditoInput.value);
          if (isNaN(limiteVal) || limiteVal < 0) {
            limiteCreditoInput.classList.add('is-invalid');
            isValid = false;
          }
        }

        if (!isValid) {
          e.preventDefault();
          if (formAlert) {
            formAlert.classList.remove('d-none');
          }
          window.scrollTo({ top: 0, behavior: 'smooth' });
        }
      });
    }

  });
})();
