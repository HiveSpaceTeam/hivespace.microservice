document.addEventListener('DOMContentLoaded', function () {
  // Handle multiple password inputs on the same page
  // Use setTimeout to ensure DOM is fully ready and all other scripts have loaded
  setTimeout(function () {
    initializePasswordToggles();
  }, 100);

  // Also try to initialize again after a longer delay in case of dynamic content
  setTimeout(function () {
    initializePasswordToggles();
  }, 500);
});

function initializePasswordToggles() {
  const passwordWrappers = document.querySelectorAll('.password-input-wrapper');

  passwordWrappers.forEach(function (wrapper, index) {
    // Check if already initialized
    if (wrapper.dataset.toggleInitialized) {
      return;
    }

    const passwordInput = wrapper.querySelector('input[type="password"]');
    const passwordToggle = wrapper.querySelector('.password-toggle');
    const eyeIcon = wrapper.querySelector('.show-when-hidden');
    const eyeOffIcon = wrapper.querySelector('.show-when-visible');

    if (passwordToggle && passwordInput && eyeIcon && eyeOffIcon) {
      // Mark as initialized
      wrapper.dataset.toggleInitialized = 'true';

      passwordToggle.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();

        const currentType = passwordInput.getAttribute('type');
        const newType = currentType === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', newType);

        // Toggle the password-visible class on the wrapper to control icon visibility
        wrapper.classList.toggle('password-visible', newType === 'text');
      });
    }
  });
}