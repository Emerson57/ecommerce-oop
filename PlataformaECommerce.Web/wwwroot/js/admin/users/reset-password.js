(function () {
    const form = document.querySelector("[data-reset-password-form]");

    if (!form) {
        return;
    }

    const newPasswordInput = form.querySelector("[data-reset-password-new]");
    const confirmPasswordInput = form.querySelector("[data-reset-password-confirm]");
    const submitButton = form.querySelector("[data-reset-password-submit]");
    const matchFeedback = form.querySelector("[data-reset-password-match-feedback]");
    const rulesContainer = form.querySelector("[data-reset-password-rules]");

    if (!newPasswordInput || !confirmPasswordInput || !submitButton || !matchFeedback || !rulesContainer) {
        return;
    }

    const ruleElements = {
        length: rulesContainer.querySelector("[data-password-rule='length']"),
        uppercase: rulesContainer.querySelector("[data-password-rule='uppercase']"),
        lowercase: rulesContainer.querySelector("[data-password-rule='lowercase']"),
        digit: rulesContainer.querySelector("[data-password-rule='digit']"),
        special: rulesContainer.querySelector("[data-password-rule='special']")
    };

    function updateRuleState(element, isValid, hasValue) {
        if (!element) {
            return;
        }

        element.classList.remove("text-muted", "text-danger", "text-success");
        element.classList.add(isValid ? "text-success" : hasValue ? "text-danger" : "text-muted");
    }

    function updateInputState(element, isValid, shouldShowState) {
        element.classList.remove("is-valid", "is-invalid");

        if (!shouldShowState) {
            return;
        }

        element.classList.add(isValid ? "is-valid" : "is-invalid");
    }

    function triggerClientValidation() {
        if (!window.jQuery || !window.jQuery.validator) {
            return;
        }

        const $newPassword = window.jQuery(newPasswordInput);
        const $confirmPassword = window.jQuery(confirmPasswordInput);

        if (!$newPassword.closest("form").data("validator")) {
            return;
        }

        $newPassword.valid();
        $confirmPassword.valid();
    }

    function evaluatePasswordRules(password) {
        return {
            length: password.length >= 8 && password.length <= 100,
            uppercase: /[A-Z]/.test(password),
            lowercase: /[a-z]/.test(password),
            digit: /[0-9]/.test(password),
            special: /[^A-Za-z0-9]/.test(password)
        };
    }

    function updateFeedback() {
        const newPassword = newPasswordInput.value || "";
        const confirmPassword = confirmPasswordInput.value || "";
        const hasNewPassword = newPassword.length > 0;
        const hasConfirmPassword = confirmPassword.length > 0;
        const ruleResults = evaluatePasswordRules(newPassword);
        const passwordRulesAreValid = Object.values(ruleResults).every(Boolean);
        const passwordsMatch = hasNewPassword && hasConfirmPassword && newPassword === confirmPassword;

        Object.entries(ruleElements).forEach(function ([ruleName, element]) {
            updateRuleState(element, ruleResults[ruleName], hasNewPassword);
        });

        matchFeedback.classList.remove("text-muted", "text-danger", "text-success");

        if (!hasNewPassword && !hasConfirmPassword) {
            matchFeedback.textContent = "Ingrese y confirme la nueva contraseña para validar coincidencia en tiempo real.";
            matchFeedback.classList.add("text-muted");
        } else if (!hasConfirmPassword) {
            matchFeedback.textContent = "Confirme la nueva contraseña para completar la validación.";
            matchFeedback.classList.add("text-muted");
        } else if (passwordsMatch) {
            matchFeedback.textContent = "Las contraseñas coinciden correctamente.";
            matchFeedback.classList.add("text-success");
        } else {
            matchFeedback.textContent = "Las contraseñas no coinciden.";
            matchFeedback.classList.add("text-danger");
        }

        updateInputState(newPasswordInput, passwordRulesAreValid, hasNewPassword);
        updateInputState(confirmPasswordInput, passwordsMatch, hasConfirmPassword);
        submitButton.disabled = !(passwordRulesAreValid && passwordsMatch);

        triggerClientValidation();
    }

    newPasswordInput.addEventListener("input", updateFeedback);
    confirmPasswordInput.addEventListener("input", updateFeedback);
    newPasswordInput.addEventListener("blur", updateFeedback);
    confirmPasswordInput.addEventListener("blur", updateFeedback);

    updateFeedback();
})();
