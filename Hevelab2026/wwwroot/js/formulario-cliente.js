/* ================================================================
   HEVELAB — formulario-cliente.js
   Validación del formulario de nuevo/editar cliente
   ================================================================ */
(function () {
    'use strict';

    /* ─── Helpers ─── */
    function setError(inputId, errId, msg) {
        var inp = document.getElementById(inputId);
        var err = document.getElementById(errId);
        if (inp) inp.classList.add('is-error');
        if (err) { err.textContent = msg; err.classList.add('visible'); }
    }
    function clearError(inputId, errId) {
        var inp = document.getElementById(inputId);
        var err = document.getElementById(errId);
        if (inp) inp.classList.remove('is-error');
        if (err) err.classList.remove('visible');
    }

    /* ─── Reglas de documento por tipo ─── */
    var docRules = {
        'RUC':       { digits: true,  length: 11, exact: true,  hint: '11 dígitos numéricos (RUC)' },
        'DNI':       { digits: true,  length: 8,  exact: true,  hint: '8 dígitos numéricos (DNI)' },
        'CE':        { digits: false, length: 12, exact: false, hint: 'Máx. 12 caracteres alfanuméricos' },
        'Pasaporte': { digits: false, length: 15, exact: false, hint: 'Máx. 15 caracteres alfanuméricos' }
    };

    var tipoDocSel = document.getElementById('TipoDocumentoSelect');
    var docInput   = document.getElementById('NumeroDocumentoInput');
    var docHint    = document.getElementById('docHint');

    function getRules() {
        var tipo = tipoDocSel ? tipoDocSel.value : 'DNI';
        return docRules[tipo] || docRules['DNI'];
    }

    function updateDocRules() {
        var rules = getRules();
        if (docInput) docInput.maxLength = rules.length;
        if (docHint)  docHint.textContent = rules.hint;
        clearError('NumeroDocumentoInput', 'docErr');
        // Limpiar valor si cambia el tipo
        if (docInput) docInput.value = '';
    }

    if (tipoDocSel) {
        tipoDocSel.addEventListener('change', updateDocRules);
        updateDocRules(); // init
    }

    /* Solo números en documento si la regla lo requiere */
    if (docInput) {
        docInput.addEventListener('input', function () {
            var rules = getRules();
            if (rules.digits) {
                this.value = this.value.replace(/\D/g, '');
            }
            clearError('NumeroDocumentoInput', 'docErr');
        });
        docInput.addEventListener('keypress', function (e) {
            if (getRules().digits && !/\d/.test(e.key) && !e.ctrlKey && !e.metaKey) {
                e.preventDefault();
            }
        });
    }

    /* ─── Teléfono: solo 9 dígitos numéricos ─── */
    var telInput = document.getElementById('TelefonoInput');
    if (telInput) {
        /* Permitir solo dígitos mientras se escribe */
        telInput.addEventListener('keypress', function (e) {
            if (!/\d/.test(e.key) && !e.ctrlKey && !e.metaKey) {
                e.preventDefault();
            }
        });
        /* Limpiar no-dígitos al pegar o escribir */
        telInput.addEventListener('input', function () {
            this.value = this.value.replace(/\D/g, '').slice(0, 9);
            clearError('TelefonoInput', 'telErr');
        });
        /* Bloquear pegado con caracteres no numéricos */
        telInput.addEventListener('paste', function (e) {
            e.preventDefault();
            var pasted = (e.clipboardData || window.clipboardData).getData('text');
            var digits = pasted.replace(/\D/g, '').slice(0, 9);
            var start  = this.selectionStart;
            var end    = this.selectionEnd;
            var cur    = this.value;
            this.value = (cur.slice(0, start) + digits + cur.slice(end)).slice(0, 9);
        });
    }

    /* ─── Limpiar errores en otros campos al modificar ─── */
    ['RazonSocialInput', 'EmailInput', 'LimiteCreditoInput'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) {
            el.addEventListener('input', function () {
                el.classList.remove('is-error');
            });
        }
    });

    /* ─── Validación al enviar ─── */
    var form     = document.getElementById('clienteForm');
    var alertBox = document.getElementById('formAlert');
    var alertMsg = document.getElementById('formAlertMsg');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        var valid = true;

        /* Reset */
        if (alertBox) alertBox.classList.add('d-none');
        form.querySelectorAll('.frm-input').forEach(function (i) { i.classList.remove('is-error'); });
        form.querySelectorAll('.frm-err').forEach(function (i) { i.classList.remove('visible'); });

        /* Razón Social */
        var razon = document.getElementById('RazonSocialInput');
        if (razon && !razon.value.trim()) {
            razon.classList.add('is-error');
            var razonErr = document.getElementById('razonErr');
            if (razonErr) razonErr.classList.add('visible');
            valid = false;
        }

        /* Número de Documento */
        var docVal = docInput ? docInput.value.trim() : '';
        var rules  = getRules();
        if (!docVal) {
            setError('NumeroDocumentoInput', 'docErr', 'El número de documento es obligatorio.');
            valid = false;
        } else if (rules.digits && !/^\d+$/.test(docVal)) {
            setError('NumeroDocumentoInput', 'docErr', 'Solo se permiten dígitos numéricos.');
            valid = false;
        } else if (rules.exact && docVal.length !== rules.length) {
            setError('NumeroDocumentoInput', 'docErr', 'Debe tener exactamente ' + rules.length + ' dígitos.');
            valid = false;
        }

        /* Teléfono (opcional, pero si se llena debe ser exactamente 9 dígitos) */
        if (telInput && telInput.value.trim() !== '') {
            if (!/^\d{9}$/.test(telInput.value.trim())) {
                telInput.classList.add('is-error');
                var telErr = document.getElementById('telErr');
                if (telErr) telErr.classList.add('visible');
                valid = false;
            }
        }

        /* Email */
        var emailInput = document.getElementById('EmailInput');
        if (emailInput) {
            var emailVal = emailInput.value.trim();
            /* Regex básica sin @ que confunda a Razor (archivo .js, OK) */
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailVal || !emailRegex.test(emailVal)) {
                emailInput.classList.add('is-error');
                var emailErr = document.getElementById('emailErr');
                if (emailErr) emailErr.classList.add('visible');
                valid = false;
            }
        }

        /* Límite de crédito */
        var lcInput = document.getElementById('LimiteCreditoInput');
        if (lcInput) {
            var lcVal = parseFloat(lcInput.value);
            if (isNaN(lcVal) || lcVal < 0) {
                lcInput.classList.add('is-error');
                var creditErr = document.getElementById('creditErr');
                if (creditErr) creditErr.classList.add('visible');
                valid = false;
            }
        }

        if (!valid) {
            e.preventDefault();
            if (alertBox) {
                alertBox.classList.remove('d-none');
                if (alertMsg) {
                    alertMsg.textContent = 'Por favor corrija los campos marcados antes de enviar.';
                }
            }
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    });

})();
