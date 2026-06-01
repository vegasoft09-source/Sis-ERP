/* ================================================================
   HEVELAB — formulario-cliente.js
   Validación del formulario de nuevo/editar cliente
   ================================================================ */
(function () {
    'use strict';

    var form = document.getElementById('clienteForm');
    if (!form || form.tagName !== 'FORM') return;

    function setError(inputId, errId, msg) {
        var inp = document.getElementById(inputId);
        var err = document.getElementById(errId);
        if (inp) inp.classList.add('is-error');
        if (err) { err.textContent = msg || err.textContent; err.classList.add('is-visible'); }
    }

    function clearError(inputId, errId) {
        var inp = document.getElementById(inputId);
        var err = document.getElementById(errId);
        if (inp) inp.classList.remove('is-error');
        if (err) err.classList.remove('is-visible');
    }

    var docRules = {
        'RUC':       { digits: true,  length: 11, exact: true,  hint: '11 dígitos numéricos (RUC)' },
        'DNI':       { digits: true,  length: 8,  exact: true,  hint: '8 dígitos numéricos (DNI)' },
        'CE':        { digits: false, length: 12, exact: false, hint: 'Máx. 12 caracteres alfanuméricos' },
        'Pasaporte': { digits: false, length: 15, exact: false, hint: 'Máx. 15 caracteres alfanuméricos' }
    };

    var tipoDocHidden = document.getElementById('TipoDocumentoHidden');
    var tipoDocSel    = document.getElementById('TipoDocumentoSelect');
    var docInput      = document.getElementById('NumeroDocumentoInput');
    var docHint       = document.getElementById('docHint');
    var radioPersona  = document.getElementById('radioPersona');
    var radioEmpresa  = document.getElementById('radioEmpresa');
    var tipoClienteH  = document.getElementById('TipoClienteHidden');

    function getTipoDoc() {
        if (tipoDocHidden) return tipoDocHidden.value;
        if (tipoDocSel) return tipoDocSel.value;
        return 'DNI';
    }

    function getRules() {
        return docRules[getTipoDoc()] || docRules['DNI'];
    }

    function syncTipoEntidad(resetDoc) {
        var esEmpresa = radioEmpresa && radioEmpresa.checked;
        if (tipoClienteH) tipoClienteH.value = esEmpresa ? 'Persona Jurídica' : 'Persona Natural';
        if (resetDoc) {
            var doc = esEmpresa ? 'RUC' : 'DNI';
            if (tipoDocHidden) tipoDocHidden.value = doc;
            if (tipoDocSel) tipoDocSel.value = doc;
            if (docInput) docInput.value = '';
        }
        updateDocRules();
    }

    function updateDocRules() {
        var rules = getRules();
        if (docInput) docInput.maxLength = rules.length;
        if (docHint) docHint.textContent = rules.hint;
        clearError('NumeroDocumentoInput', 'docErr');
    }

    if (radioPersona) radioPersona.addEventListener('change', function () { syncTipoEntidad(true); });
    if (radioEmpresa) radioEmpresa.addEventListener('change', function () { syncTipoEntidad(true); });
    if (tipoDocSel) tipoDocSel.addEventListener('change', function () {
        if (tipoDocHidden) tipoDocHidden.value = tipoDocSel.value;
        updateDocRules();
    });

    if (tipoDocHidden && tipoDocSel) tipoDocSel.value = tipoDocHidden.value;
    syncTipoEntidad(false);

    if (docInput) {
        docInput.addEventListener('input', function () {
            var rules = getRules();
            if (rules.digits) this.value = this.value.replace(/\D/g, '');
            clearError('NumeroDocumentoInput', 'docErr');
        });
        docInput.addEventListener('keypress', function (e) {
            if (getRules().digits && !/\d/.test(e.key) && !e.ctrlKey && !e.metaKey) e.preventDefault();
        });
    }

    var telInput = document.getElementById('TelefonoInput');
    if (telInput) {
        telInput.addEventListener('keypress', function (e) {
            if (!/\d/.test(e.key) && !e.ctrlKey && !e.metaKey) e.preventDefault();
        });
        telInput.addEventListener('input', function () {
            this.value = this.value.replace(/\D/g, '').slice(0, 9);
            clearError('TelefonoInput', 'telErr');
        });
    }

    ['RazonSocialInput', 'EmailInput', 'LimiteCreditoInput'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('input', function () { el.classList.remove('is-error'); });
    });

    var btnPhoto = document.getElementById('btnAddPhoto');
    if (btnPhoto) {
        btnPhoto.addEventListener('click', function () {
            alert('La carga de fotografía estará disponible próximamente.');
        });
    }

    var alertBox = document.getElementById('formAlert');
    var alertMsg = document.getElementById('formAlertMsg');

    form.addEventListener('submit', function (e) {
        var valid = true;

        if (alertBox) alertBox.classList.remove('is-visible');
        form.querySelectorAll('.cl-form__input').forEach(function (i) { i.classList.remove('is-error'); });
        form.querySelectorAll('.cl-form__err').forEach(function (i) { i.classList.remove('is-visible'); });

        var razon = document.getElementById('RazonSocialInput');
        if (razon && !razon.value.trim()) {
            razon.classList.add('is-error');
            var razonErr = document.getElementById('razonErr');
            if (razonErr) razonErr.classList.add('is-visible');
            valid = false;
        }

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

        if (telInput && telInput.value.trim() !== '') {
            if (!/^\d{9}$/.test(telInput.value.trim())) {
                telInput.classList.add('is-error');
                var telErr = document.getElementById('telErr');
                if (telErr) telErr.classList.add('is-visible');
                valid = false;
            }
        }

        var emailInput = document.getElementById('EmailInput');
        if (emailInput) {
            var emailVal = emailInput.value.trim();
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailVal || !emailRegex.test(emailVal)) {
                emailInput.classList.add('is-error');
                var emailErr = document.getElementById('emailErr');
                if (emailErr) emailErr.classList.add('is-visible');
                valid = false;
            }
        }

        var lcInput = document.getElementById('LimiteCreditoInput');
        if (lcInput) {
            var lcVal = parseFloat(lcInput.value);
            if (isNaN(lcVal) || lcVal < 0) {
                lcInput.classList.add('is-error');
                var creditErr = document.getElementById('creditErr');
                if (creditErr) creditErr.classList.add('is-visible');
                valid = false;
            }
        }

        if (!valid) {
            e.preventDefault();
            if (alertBox) {
                alertBox.classList.add('is-visible');
                if (alertMsg) alertMsg.textContent = 'Por favor corrija los campos marcados antes de enviar.';
            }
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    });

})();
