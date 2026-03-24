(function () {
    const form = document.querySelector("[data-admin-create-form]");

    if (!form) {
        return;
    }

    const passwordInput = form.querySelector("[data-admin-create-password]");
    const confirmPasswordInput = form.querySelector("[data-admin-create-confirm-password]");
    const submitButton = form.querySelector("[data-admin-create-submit]");
    const confirmFeedback = form.querySelector("[data-admin-create-confirm-feedback]");
    const passwordRulesContainer = form.querySelector("[data-admin-create-password-rules]");

    if (!passwordInput || !confirmPasswordInput || !submitButton || !confirmFeedback || !passwordRulesContainer) {
        return;
    }

    const options = {
        minPasswordLength: Number(form.dataset.passwordMinLength || 8),
        maxPasswordLength: Number(form.dataset.passwordMaxLength || 100)
    };

    const passwordRuleElements = {
        length: passwordRulesContainer.querySelector("[data-password-rule='length']"),
        uppercase: passwordRulesContainer.querySelector("[data-password-rule='uppercase']"),
        lowercase: passwordRulesContainer.querySelector("[data-password-rule='lowercase']"),
        digit: passwordRulesContainer.querySelector("[data-password-rule='digit']"),
        special: passwordRulesContainer.querySelector("[data-password-rule='special']")
    };

    function updateInputState(element, isValid, shouldShowState) {
        element.classList.remove("is-valid", "is-invalid");

        if (!shouldShowState) {
            return;
        }

        element.classList.add(isValid ? "is-valid" : "is-invalid");
    }

    function setMessage(element, cssClass, message) {
        element.classList.remove("text-muted", "text-danger", "text-success", "text-info");
        element.classList.add(cssClass);
        element.textContent = message;
    }

    function triggerClientValidation(element) {
        if (!window.jQuery || !window.jQuery.validator) {
            return;
        }

        const $form = window.jQuery(form);

        if (!$form.data("validator")) {
            return;
        }

        window.jQuery(element).valid();
    }

    function evaluatePasswordRules() {
        const password = passwordInput.value || "";
        const hasPassword = password.length > 0;
        const results = {
            length: password.length >= options.minPasswordLength && password.length <= options.maxPasswordLength,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            digit: /[0-9]/.test(password),
            special: /[^A-Za-z0-9]/.test(password)
        };

        Object.entries(passwordRuleElements).forEach(function ([ruleName, element]) {
            if (!element) {
                return;
            }

            element.classList.remove("text-muted", "text-danger", "text-success");
            element.classList.add(results[ruleName] ? "text-success" : hasPassword ? "text-danger" : "text-muted");
        });

        const isValid = Object.values(results).every(Boolean);
        updateInputState(passwordInput, isValid, hasPassword);
        return isValid;
    }

    function evaluatePasswordConfirmation() {
        const password = passwordInput.value || "";
        const confirmation = confirmPasswordInput.value || "";
        const hasConfirmation = confirmation.length > 0;
        const matches = password.length > 0 && confirmation.length > 0 && password === confirmation;

        if (!password && !confirmation) {
            setMessage(confirmFeedback, "text-muted", "Repite la contraseña para validar coincidencia.");
            updateInputState(confirmPasswordInput, false, false);
            return false;
        }

        if (!hasConfirmation) {
            setMessage(confirmFeedback, "text-muted", "Confirma la contraseña para completar la validación.");
            updateInputState(confirmPasswordInput, false, false);
            return false;
        }

        if (!matches) {
            setMessage(confirmFeedback, "text-danger", "Las contraseñas no coinciden.");
            updateInputState(confirmPasswordInput, false, true);
            return false;
        }

        setMessage(confirmFeedback, "text-success", "Las contraseñas coinciden correctamente.");
        updateInputState(confirmPasswordInput, true, true);
        return true;
    }

    function updateSubmitState() {
        const isPasswordValid = evaluatePasswordRules();
        const isConfirmationValid = evaluatePasswordConfirmation();

        submitButton.disabled = !(isPasswordValid && isConfirmationValid);
    }

    [passwordInput, confirmPasswordInput].forEach(function (element) {
        element.addEventListener("input", function () {
            triggerClientValidation(passwordInput);
            triggerClientValidation(confirmPasswordInput);
            updateSubmitState();
        });

        element.addEventListener("blur", function () {
            triggerClientValidation(passwordInput);
            triggerClientValidation(confirmPasswordInput);
            updateSubmitState();
        });
    });

    form.addEventListener("submit", function () {
        submitButton.setAttribute("disabled", "disabled");
        submitButton.textContent = "Creando...";
    });

    updateSubmitState();
})();
