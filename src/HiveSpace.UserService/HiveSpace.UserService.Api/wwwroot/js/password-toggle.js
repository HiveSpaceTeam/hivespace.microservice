document.addEventListener('DOMContentLoaded', function () {
  const passwordInput = document.querySelector('input[type="password"]');
  const passwordToggle = document.querySelector('.password-toggle');
  const showIcon = document.querySelector('.password-toggle-show');
  const hideIcon = document.querySelector('.password-toggle-hide');

  if (passwordToggle && passwordInput) {
    passwordToggle.addEventListener('click', function () {
      const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
      passwordInput.setAttribute('type', type);

      // Toggle visibility of icons
      showIcon.classList.toggle('hidden');
      hideIcon.classList.toggle('hidden');
    });
  }
});