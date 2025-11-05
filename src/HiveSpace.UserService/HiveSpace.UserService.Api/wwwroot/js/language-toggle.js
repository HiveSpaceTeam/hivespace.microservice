// Language Toggle JavaScript - CSP Compliant with Storage
(function () {
  'use strict';

  const CULTURE_COOKIE_NAME = 'culture';
  const SUPPORTED_CULTURES = ['vi', 'en'];
  const DEFAULT_CULTURE = 'vi';

  // Get culture from cookie with fallback
  function getCultureFromStorage() {
    try {
      const cookies = document.cookie.split(';');
      for (let cookie of cookies) {
        const [name, value] = cookie.trim().split('=');
        if (name === CULTURE_COOKIE_NAME && SUPPORTED_CULTURES.includes(value)) {
          return value;
        }
      }
    } catch (e) {
      console.warn('Failed to read culture from cookie:', e);
    }
    return null;
  }

  // Store culture in cookie only
  function storeCulture(culture) {
    try {
      if (SUPPORTED_CULTURES.includes(culture)) {
        // Store in cookie for server-side reading
        document.cookie = `${CULTURE_COOKIE_NAME}=${culture}; path=/; SameSite=Lax`;

        console.log('Culture stored in cookie:', culture);
      }
    } catch (e) {
      console.warn('Failed to store culture:', e);
    }
  }

  // Clear culture cookie (used on redirect back)
  function clearCultureStorage() {
    try {
      // Clear cookie
      document.cookie = `${CULTURE_COOKIE_NAME}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT`;

      console.log('Culture cookie cleared');
    } catch (e) {
      console.warn('Failed to clear culture cookie:', e);
    }
  }

  function toggleLanguageMenu() {
    const menu = document.getElementById('languageMenu');
    const arrow = document.getElementById('dropdownArrow');

    if (!menu || !arrow) {
      console.error('Language menu elements not found');
      return;
    }

    if (menu.classList.contains('hidden')) {
      // Show menu
      menu.classList.remove('hidden');
      arrow.style.transform = 'rotate(180deg)';
      console.log('Language menu opened');
    } else {
      // Hide menu
      menu.classList.add('hidden');
      arrow.style.transform = 'rotate(0deg)';
      console.log('Language menu closed');
    }
  }

  function closeLanguageMenu() {
    const menu = document.getElementById('languageMenu');
    const arrow = document.getElementById('dropdownArrow');

    if (!menu || !arrow) return;

    if (!menu.classList.contains('hidden')) {
      menu.classList.add('hidden');
      arrow.style.transform = 'rotate(0deg)';
      console.log('Language menu closed');
    }
  }

  // Handle culture selection
  function selectCulture(culture) {
    if (!SUPPORTED_CULTURES.includes(culture)) {
      console.warn('Unsupported culture:', culture);
      return;
    }

    console.log('Selecting culture:', culture);

    // Store in cookie
    storeCulture(culture);

    console.log('Culture stored, reloading page...');

    // Reload page to apply new culture (middleware will read from cookie)
    window.location.reload();
  }



  // Check if we're returning from OIDC callback and clear storage
  function checkAndClearStorageOnCallback() {
    const url = new URL(window.location.href);
    const path = url.pathname.toLowerCase();

    // Clear storage if returning from OIDC callback or external auth
    if (path.includes('/callback') ||
      path.includes('/signin-') ||
      url.searchParams.has('code') ||
      url.searchParams.has('state')) {
      clearCultureStorage();
      console.log('Cleared culture storage on OIDC callback');
    }
  }

  // Clear culture cookie when navigating away from Identity Server
  function setupClearOnNavigation() {
    // Handle form submissions and redirects (like successful login)
    document.addEventListener('submit', function (event) {
      // Check if this is a login form or other form that might redirect externally
      const form = event.target;
      if (form && form.action) {
        try {
          const actionUrl = new URL(form.action, window.location.origin);
          const currentHost = window.location.host;

          // If form submits to external domain, clear cookie
          if (actionUrl.host !== currentHost) {
            clearCultureStorage();
            console.log('Cleared culture cookie on form submission to external domain');
          }
        } catch (e) {
          // Invalid URL, ignore
        }
      }
    });

    // Handle clicks on external links
    document.addEventListener('click', function (event) {
      const link = event.target.closest('a');
      if (link && link.href) {
        try {
          const linkUrl = new URL(link.href);
          const currentHost = window.location.host;

          // If clicking on an external link (different domain), clear the culture cookie
          if (linkUrl.host !== currentHost && linkUrl.protocol.startsWith('http')) {
            clearCultureStorage();
            console.log('Cleared culture cookie on external link click to:', linkUrl.host);
          }
        } catch (e) {
          // Invalid URL, ignore
        }
      }
    });

    // Handle browser back/forward button navigation
    window.addEventListener('popstate', function (event) {
      // When user clicks back/forward button, we want to clear the culture cookie
      // since they might be navigating back to an external site
      clearCultureStorage();
      console.log('Cleared culture cookie on browser back/forward navigation');
    });

    // Handle beforeunload event for any navigation away from the page
    window.addEventListener('beforeunload', function () {
      // Since we can't reliably detect the destination in beforeunload,
      // we'll rely on server-side cookie clearing for external redirects
      // This is just a fallback for any client-side navigation
    });
  }



  // Initialize when DOM is ready
  document.addEventListener('DOMContentLoaded', function () {
    // Check if we need to clear storage on callback
    checkAndClearStorageOnCallback();

    // Setup clearing culture on navigation away from site
    setupClearOnNavigation();

    // Attach click handler to button
    const toggleButton = document.getElementById('languageToggle');
    if (toggleButton) {
      toggleButton.addEventListener('click', toggleLanguageMenu);
    }

    // Attach click handlers to language links
    const languageLinks = document.querySelectorAll('.language-option');
    languageLinks.forEach(link => {
      link.addEventListener('click', function (e) {
        e.preventDefault();
        const culture = this.dataset.culture;
        if (culture) {
          selectCulture(culture);
        }
      });
    });

    // Close menu when clicking outside
    document.addEventListener('click', function (event) {
      const toggle = document.getElementById('languageToggle');
      const menu = document.getElementById('languageMenu');

      if (toggle && menu && !toggle.contains(event.target) && !menu.contains(event.target)) {
        closeLanguageMenu();
      }
    });

    // Close menu when pressing Escape key
    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape') {
        closeLanguageMenu();
      }
    });
  });

  // Make functions available globally if needed
  window.toggleLanguageMenu = toggleLanguageMenu;
  window.selectCulture = selectCulture;
  window.getCultureFromStorage = getCultureFromStorage;
})();