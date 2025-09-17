// Custom form validation with blur events
document.addEventListener('DOMContentLoaded', function () {
  const emailInput = document.querySelector('input[name="Input.Email"]');
  const passwordInput = document.querySelector('input[name="Input.Password"]');

  if (emailInput) {
    setupEmailValidation(emailInput);
  }

  if (passwordInput) {
    setupPasswordValidation(passwordInput);
  }
});

function setupEmailValidation(emailInput) {
  const errorSpan = emailInput.parentElement.querySelector('.form-error-text');

  emailInput.addEventListener('blur', function () {
    validateEmail(emailInput, errorSpan);
  });

  // Clear error on focus
  emailInput.addEventListener('focus', function () {
    clearValidationState(emailInput, errorSpan);
  });
}

function setupPasswordValidation(passwordInput) {
  const errorSpan = passwordInput.parentElement.parentElement.querySelector('.form-error-text');

  passwordInput.addEventListener('blur', function () {
    validatePassword(passwordInput, errorSpan);
  });

  // Clear error on focus
  passwordInput.addEventListener('focus', function () {
    clearValidationState(passwordInput, errorSpan);
  });
}

function validateEmail(emailInput, errorSpan) {
  const email = emailInput.value.trim();
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  // Check if email is required and not empty
  if (!email) {
    showError(emailInput, errorSpan, 'Email is required');
    return false;
  }

  // Check email format
  if (!emailRegex.test(email)) {
    showError(emailInput, errorSpan, 'Please enter a valid email address');
    return false;
  }

  showSuccess(emailInput, errorSpan);
  return true;
}

function validatePassword(passwordInput, errorSpan) {
  const password = passwordInput.value;

  // Check if password is required and not empty
  if (!password) {
    showError(passwordInput, errorSpan, 'Password is required');
    return false;
  }

  // Password strength validation
  const hasUpper = /[A-Z]/.test(password);
  const hasLower = /[a-z]/.test(password);
  const hasNumber = /[0-9]/.test(password);
  const hasSpecial = /[^A-Za-z0-9]/.test(password);

  const errors = [];

  if (!hasUpper) {
    errors.push('uppercase letter');
  }
  if (!hasLower) {
    errors.push('lowercase letter');
  }
  if (!hasNumber) {
    errors.push('number');
  }
  if (!hasSpecial) {
    errors.push('special character');
  }

  if (password.length < 8) {
    errors.push('at least 8 characters');
  }

  if (errors.length > 0) {
    const errorMessage = `Password must contain: ${errors.join(', ')}`;
    showError(passwordInput, errorSpan, errorMessage);
    return false;
  }

  showSuccess(passwordInput, errorSpan);
  return true;
}

function showError(input, errorSpan, message) {
  input.classList.remove('has-success');
  input.classList.add('has-error');

  if (errorSpan) {
    errorSpan.textContent = message;
    errorSpan.style.display = 'block';
  }
}

function showSuccess(input, errorSpan) {
  input.classList.remove('has-error');
  input.classList.add('has-success');

  if (errorSpan) {
    errorSpan.textContent = '';
    errorSpan.style.display = 'none';
  }
}

function clearValidationState(input, errorSpan) {
  input.classList.remove('has-error', 'has-success');

  if (errorSpan) {
    errorSpan.textContent = '';
    errorSpan.style.display = 'none';
  }
}

// Form submission validation
function validateForm() {
  const emailInput = document.querySelector('input[name="Input.Email"]');
  const passwordInput = document.querySelector('input[name="Input.Password"]');

  let isValid = true;

  if (emailInput) {
    const emailErrorSpan = emailInput.parentElement.querySelector('.form-error-text');
    if (!validateEmail(emailInput, emailErrorSpan)) {
      isValid = false;
    }
  }

  if (passwordInput) {
    const passwordErrorSpan = passwordInput.parentElement.parentElement.querySelector('.form-error-text');
    if (!validatePassword(passwordInput, passwordErrorSpan)) {
      isValid = false;
    }
  }

  return isValid;
}

// Attach form validation to form submit
document.addEventListener('DOMContentLoaded', function () {
  const form = document.querySelector('form');
  if (form) {
    form.addEventListener('submit', function (e) {
      if (!validateForm()) {
        e.preventDefault();
        return false;
      }
    });
  }
});