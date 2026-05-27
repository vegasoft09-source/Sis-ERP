/* ==========================================================
   HEVELAB — site.js
   Vanilla JS: sidebar toggle, tema claro/oscuro, reloj, dropdown
   ========================================================== */
(function () {
  'use strict';

  /* ---- DOM refs ---- */
  var sidebar    = document.getElementById('hl-sidebar');
  var overlay    = document.getElementById('hl-overlay');
  var btnMenu    = document.getElementById('hl-btn-menu');
  var btnClose   = document.getElementById('hl-btn-close');
  var btnToggle  = document.getElementById('hl-btn-toggle');
  var toggleIcon = document.getElementById('hl-toggle-icon');
  var clock      = document.getElementById('hl-clock-time');
  var btnTheme   = document.getElementById('hl-btn-theme');
  var themeIcon  = document.getElementById('hl-theme-icon');
  var profileBtn = document.getElementById('hl-profile-btn');
  var profileDd  = document.getElementById('hl-profile-dropdown');
  var btnLogout  = document.getElementById('hl-btn-logout');

  /* ---- Theme ---- */
  var THEME_KEY = 'hevelab.colorScheme';

  function getTheme() {
    return localStorage.getItem(THEME_KEY) ||
      (window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light');
  }

  function applyTheme(theme) {
    document.documentElement.setAttribute('data-theme', theme);
    if (themeIcon) {
      themeIcon.innerHTML = theme === 'dark' ? getSunSvg() : getMoonSvg();
    }
    if (btnTheme) {
      btnTheme.title = theme === 'dark' ? 'Modo claro' : 'Modo oscuro';
    }
  }

  function toggleTheme() {
    var current = getTheme();
    var next = current === 'dark' ? 'light' : 'dark';
    localStorage.setItem(THEME_KEY, next);
    applyTheme(next);
  }

  /* ---- Palette ---- */
  var PALETTE_KEY = 'hevelab.systemPalette';
  var PALETTES = {
    indigo: { primary:'#4338ca', onPrimary:'#ffffff', soft:'#e0e7ff', border:'#a5b4fc', ring:'#818cf8' },
    teal:   { primary:'#0d9488', onPrimary:'#ffffff', soft:'#99f6e4', border:'#5eead4', ring:'#2dd4bf' },
    purple: { primary:'#7c3aed', onPrimary:'#ffffff', soft:'#ddd6fe', border:'#c4b5fd', ring:'#a78bfa' },
    blue:   { primary:'#2563eb', onPrimary:'#ffffff', soft:'#bfdbfe', border:'#93c5fd', ring:'#60a5fa' },
    rose:   { primary:'#e11d48', onPrimary:'#ffffff', soft:'#fecdd3', border:'#fda4af', ring:'#fb7185' },
    amber:  { primary:'#d97706', onPrimary:'#111827', soft:'#fde68a', border:'#fcd34d', ring:'#fbbf24' },
  };

  function applyPalette(id) {
    var p = PALETTES[id] || PALETTES.indigo;
    var root = document.documentElement;
    root.style.setProperty('--sys-primary',       p.primary);
    root.style.setProperty('--sys-on-primary',    p.onPrimary);
    root.style.setProperty('--sys-soft',          p.soft);
    root.style.setProperty('--sys-border',        p.border);
    root.style.setProperty('--sys-ring',          p.ring);
    root.style.setProperty('--sys-primary-hover', 'color-mix(in srgb, ' + p.primary + ' 82%, black)');
    root.style.setProperty('--sys-soft-bg',       'color-mix(in srgb, ' + p.soft + ' 18%, white)');
  }

  /* ---- Sidebar collapsed ---- */
  var SIDEBAR_KEY = 'hevelab.sidebarCollapsed';

  function getSidebarCollapsed() {
    return localStorage.getItem(SIDEBAR_KEY) === 'true';
  }

  function applySidebarCollapsed(collapsed) {
    if (!sidebar) return;
    if (collapsed) {
      sidebar.classList.add('collapsed');
    } else {
      sidebar.classList.remove('collapsed');
    }
    if (toggleIcon) {
      toggleIcon.innerHTML = collapsed ? getChevronRightSvg() : getChevronLeftSvg();
    }
    localStorage.setItem(SIDEBAR_KEY, String(collapsed));
  }

  /* ---- Mobile sidebar ---- */
  function openMobileSidebar() {
    if (sidebar)  sidebar.classList.add('mobile-open');
    if (overlay)  overlay.classList.add('active');
    document.body.style.overflow = 'hidden';
  }

  function closeMobileSidebar() {
    if (sidebar)  sidebar.classList.remove('mobile-open');
    if (overlay)  overlay.classList.remove('active');
    document.body.style.overflow = '';
  }

  /* ---- Active nav link ---- */
  function setActiveNavLink() {
    var path = window.location.pathname.toLowerCase();
    var links = document.querySelectorAll('.hl-nav-link');
    links.forEach(function (link) {
      link.classList.remove('active');
      var href = (link.getAttribute('href') || '').toLowerCase();
      if (href && (path === href || (href !== '/' && path.startsWith(href)))) {
        link.classList.add('active');
      }
    });
  }

  /* ---- Clock ---- */
  function tickClock() {
    if (!clock) return;
    var now = new Date();
    clock.textContent = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  /* ---- Profile dropdown ---- */
  function toggleProfileDropdown() {
    if (!profileDd) return;
    profileDd.classList.toggle('open');
  }

  function closeProfileDropdown() {
    if (profileDd) profileDd.classList.remove('open');
  }

  /* ---- SVG helpers ---- */
  function getSunSvg() {
    return '<svg class="hl-icon hl-icon-lg" viewBox="0 0 24 24"><circle cx="12" cy="12" r="4"/><line x1="12" y1="2" x2="12" y2="4"/><line x1="12" y1="20" x2="12" y2="22"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="2" y1="12" x2="4" y2="12"/><line x1="20" y1="12" x2="22" y2="12"/><line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>';
  }
  function getMoonSvg() {
    return '<svg class="hl-icon hl-icon-lg" viewBox="0 0 24 24"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>';
  }
  function getChevronLeftSvg() {
    return '<svg class="hl-icon" viewBox="0 0 24 24"><polyline points="15 18 9 12 15 6"/></svg>';
  }
  function getChevronRightSvg() {
    return '<svg class="hl-icon" viewBox="0 0 24 24"><polyline points="9 18 15 12 9 6"/></svg>';
  }

  /* ---- Init ---- */
  function init() {
    // Theme
    applyTheme(getTheme());

    // Palette
    applyPalette(localStorage.getItem(PALETTE_KEY) || 'indigo');

    // Sidebar collapsed state (desktop only)
    applySidebarCollapsed(getSidebarCollapsed());

    // Active link
    setActiveNavLink();

    // Clock
    tickClock();
    setInterval(tickClock, 1000);

    // Branding
    var brandName = localStorage.getItem('hevelab.brandName');
    if (brandName) {
      var el = document.getElementById('hl-brand-name');
      if (el) el.textContent = brandName;
    }
    var brandLogo = localStorage.getItem('hevelab.brandLogo');
    if (brandLogo) {
      var logoEl = document.getElementById('hl-brand-logo');
      if (logoEl) logoEl.src = brandLogo;
    }

    /* ---- Event listeners ---- */
    if (btnMenu)   btnMenu.addEventListener('click', openMobileSidebar);
    if (btnClose)  btnClose.addEventListener('click', closeMobileSidebar);
    if (overlay)   overlay.addEventListener('click', closeMobileSidebar);

    if (btnToggle) {
      btnToggle.addEventListener('click', function () {
        var isCollapsed = sidebar && sidebar.classList.contains('collapsed');
        applySidebarCollapsed(!isCollapsed);
      });
    }

    if (btnTheme) btnTheme.addEventListener('click', toggleTheme);

    if (profileBtn) profileBtn.addEventListener('click', function (e) {
      e.stopPropagation();
      toggleProfileDropdown();
    });

    document.addEventListener('mousedown', function (e) {
      if (profileDd && !profileDd.contains(e.target) && profileBtn && !profileBtn.contains(e.target)) {
        closeProfileDropdown();
      }
    });

    if (btnLogout) {
      btnLogout.addEventListener('click', function () {
        closeProfileDropdown();
        // TODO: implement real logout
        window.location.href = '/';
      });
    }

    // Nav links close mobile sidebar on click
    document.querySelectorAll('.hl-nav-link').forEach(function (link) {
      link.addEventListener('click', closeMobileSidebar);
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
