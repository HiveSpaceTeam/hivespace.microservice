// Client-side form validation for form inputs with data attributes
document.addEventListener('DOMContentLoaded', function () {
  // Clear all error messages on page load/refresh
  clearAllErrors();

  // Flag to prevent validation when navigating between auth pages
  let isNavigating = false;

  // Add click listeners to auth navigation links and social login buttons to set the flag
  const authLinks = document.querySelectorAll('.signup-link, .signin-link, .social-button, .language-toggle-button');
  authLinks.forEach(function (link) {
    // Use mousedown instead of click to fire before navigation
    link.addEventListener('mousedown', function (e) {
      isNavigating = true;
      // Reset flag after a short delay (navigation should complete by then)
      setTimeout(function () {
        isNavigating = false;
      }, 100);
    });

    // Also add pointerdown for touch devices
    link.addEventListener('pointerdown', function (e) {
      isNavigating = true;
      // Reset flag after a short delay (navigation should complete by then)
      setTimeout(function () {
        isNavigating = false;
      }, 100);
    });
  });

  // Initialize validation on all form inputs with validation rules
  document.querySelectorAll('.form-input[data-validation-rules]').forEach(function (input) {
    input.addEventListener('blur', function () {
      // Skip validation if user is navigating to another auth page
      if (isNavigating) {
        return;
      }
      validateField(this);
    });

    // Clear errors on focus
    input.addEventListener('focus', function () {
      clearFieldError(this);
    });
  });

  function validateField(input) {
    try {
      const rules = JSON.parse(input.getAttribute('data-validation-rules') || '{}');
      const messages = JSON.parse(input.getAttribute('data-error-messages') || '{}');
      // Ensure the input has an id we can reference for an error container
      ensureInputId(input);
      const errorContainer = getErrorContainer(input);
      // Don't trim password fields (keep exact value). Trim other inputs for validation convenience.
      const value = input.type === 'password' ? input.value : input.value.trim();
      let errorMessage = '';

      // Clear previous error first
      clearFieldError(input);

      // Required validation
      if (rules.required && !value) {
        errorMessage = messages.required || 'This field is required.';
      }
      // Email validation
      else if (rules.email && value && !isValidEmail(value)) {
        errorMessage = messages.email || 'Please enter a valid email address.';
      }
      // Min length validation
      else if (rules.minLength && value && value.length < rules.minLength) {
        errorMessage = messages.minLength || 'Minimum ' + rules.minLength + ' characters required.';
      }
      // Max length validation
      else if (rules.maxLength && value && value.length > rules.maxLength) {
        errorMessage = messages.maxLength || 'Maximum ' + rules.maxLength + ' characters allowed.';
      }
      // Pattern validation
      else if (rules.pattern && value && !new RegExp(rules.pattern).test(value)) {
        errorMessage = messages.pattern || 'Invalid format.';
      }
      // Password strength validation
      else if (rules.passwordStrength && value && !isStrongPassword(value)) {
        errorMessage = messages.passwordStrength || getPasswordStrengthMessage(value);
      }
      // Compare validation (for ConfirmPassword)
      else if (rules.compare && value) {
        const compareInput = document.querySelector(`[name="${rules.compare}"]`);
        if (compareInput && value !== compareInput.value) {
          errorMessage = messages.compare || 'Password and confirmation password do not match.';
        }
      }

      // Display error if any
      if (errorMessage) {
        showFieldError(input, errorMessage);
      }
    } catch (e) {
      // swallow validation parse errors silently
    }
  }

  function showFieldError(input, message) {
    const errorContainer = getErrorContainer(input);
    if (errorContainer) {
      errorContainer.textContent = message;
      errorContainer.style.display = 'block';
      input.classList.add('has-error');
      // link for accessibility
      input.setAttribute('aria-invalid', 'true');
      input.setAttribute('aria-describedby', errorContainer.id);
    }
  }

  function clearFieldError(input) {
    const errorContainer = getErrorContainer(input);
    if (errorContainer) {
      errorContainer.style.display = 'none';
      errorContainer.textContent = '';

      // Remove error container ID from aria-describedby
      const ariaDescribedBy = input.getAttribute('aria-describedby');
      if (ariaDescribedBy) {
        const ids = ariaDescribedBy.split(' ').filter(id => id !== errorContainer.id);
        if (ids.length === 0) {
          input.removeAttribute('aria-describedby');
        } else {
          input.setAttribute('aria-describedby', ids.join(' '));
        }
      }
    }
    input.classList.remove('has-error');
    input.removeAttribute('aria-invalid');
  }

  // Ensure input has an id (used to create/find error container). If missing, generate one.
  function ensureInputId(input) {
    if (!input.id) {
      input.id = 'input-' + Math.random().toString(36).slice(2, 9);
    }
  }

  // Find or create an error container for an input.
  // Strategy: prefer element with id `${input.id}-error`, then `[data-error-for="${input.name}"]`, then a sibling with class 'client-error-text'.
  // If none found, create a span directly after the input and return it.
  function getErrorContainer(input) {
    try {
      const byId = document.getElementById(input.id + '-error');
      if (byId) return byId;

      if (input.name) {
        const byData = document.querySelector('[data-error-for="' + CSS.escape(input.name) + '"]');
        if (byData) return byData;
      }

      // look for a next sibling with expected class
      const next = input.nextElementSibling;
      if (next && (next.classList.contains('client-error-text') || next.classList.contains('server-error-text') || next.classList.contains('form-error-text') || next.id === input.id + '-error')) {
        return next;
      }

      // Look within the parent form group for error containers (for password inputs and other wrapped inputs)
      const formGroup = input.closest('.form-group');
      if (formGroup) {
        const errorInGroup = formGroup.querySelector('.client-error-text, .server-error-text, .form-error-text, #' + CSS.escape(input.id + '-error'));
        if (errorInGroup) return errorInGroup;
      }

      // As a last resort, create an inline span for errors and insert after input
      const span = document.createElement('span');
      span.id = input.id + '-error';
      span.className = 'client-error-text';
      span.style.display = 'none';
      span.setAttribute('aria-live', 'polite');
      if (input.parentNode) {
        if (input.nextSibling) {
          input.parentNode.insertBefore(span, input.nextSibling);
        } else {
          input.parentNode.appendChild(span);
        }
      }
      return span;
    } catch (e) {
      return null;
    }
  }

  function clearAllErrors() {
    // Clear all client validation errors
    document.querySelectorAll('.client-error-text').forEach(function (errorElement) {
      errorElement.style.display = 'none';
      errorElement.textContent = '';
    });

    // Clear all server validation errors  
    document.querySelectorAll('.server-error-text, .form-error-text').forEach(function (errorElement) {
      errorElement.style.display = 'none';
      errorElement.textContent = '';
    });

    // // Clear all API error messages
    // document.querySelectorAll('.api-error-message').forEach(function (errorElement) {
    //   errorElement.style.display = 'none';
    //   errorElement.textContent = '';
    // });

    // Clear validation summary errors
    document.querySelectorAll('.alert-danger, .danger').forEach(function (errorElement) {
      errorElement.style.display = 'none';
      errorElement.innerHTML = '';
    });

    // Clear validation summary lists
    document.querySelectorAll('[data-valmsg-summary="true"]').forEach(function (summary) {
      summary.style.display = 'none';
      summary.innerHTML = '';
    });

    // Remove error classes from all inputs
    document.querySelectorAll('.form-input').forEach(function (input) {
      input.classList.remove('has-error', 'input-validation-error', 'has-success');
    });

    // DO NOT clear .error-container elements - these are for server-side API errors
    // and should only be cleared by server-side logic, not client-side JavaScript

    // Clear any field validation spans
    document.querySelectorAll('[data-valmsg-for]').forEach(function (span) {
      span.style.display = 'none';
      span.textContent = '';
    });
  }

  function isValidEmail(email) {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailPattern.test(email);
  }

  function isStrongPassword(password) {
    // Match server-side regex: ^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$
    // And minimum 12 characters as per Register InputModel validation
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[@$!%*?&]/.test(password);
    const hasMinLength = password.length >= 12;
    const onlyAllowedChars = /^[A-Za-z\d@$!%*?&]+$/.test(password);

    return hasUpper && hasLower && hasNumber && hasSpecial && hasMinLength && onlyAllowedChars;
  }

  function getPasswordStrengthMessage(password) {
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[@$!%*?&]/.test(password);
    const onlyAllowedChars = /^[A-Za-z\d@$!%*?&]+$/.test(password);

    const errors = [];

    if (password.length < 12) {
      errors.push('at least 12 characters');
    }
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
      errors.push('special character (@$!%*?&)');
    }
    if (!onlyAllowedChars) {
      errors.push('only letters, numbers, and special characters (@$!%*?&)');
    }

    if (errors.length > 0) {
      return 'Password must contain: ' + errors.join(', ');
    }

    return 'Password is not strong enough';
  }

  // Form submission validation (validate all fields with data attributes)
  function validateAllFields() {
    let isValid = true;
    document.querySelectorAll('.form-input[data-validation-rules]').forEach(function (input) {
      validateField(input);
      if (input.classList.contains('has-error')) {
        isValid = false;
      }
    });
    return isValid;
  }

  // Attach form validation to form submit
  const form = document.querySelector('form');
  if (form) {
    form.addEventListener('submit', function (e) {
      if (!validateAllFields()) {
        e.preventDefault();
        return false;
      }
    });
  }
});