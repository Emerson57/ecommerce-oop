(function () {
    const form = document.querySelector("[data-account-password-form]");

    if (!form) {
        return;
    }

    const newPasswordInput = form.querySelector("[data-account-password-new]");
    const confirmPasswordInput = form.querySelector("[data-account-password-confirm]");
    const submitButton = form.querySelector("[data-account-password-submit]");
    const feedback = form.querySelector("[data-account-password-feedback]");
    const rulesContainer = form.querySelector("[data-account-password-rules]");

    if (!newPasswordInput || !confirmPasswordInput || !submitButton || !feedback || !rulesContainer) {
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

    function triggerValidation(element) {
        if (!window.jQuery || !window.jQuery.validator) {
            return;
        }

        const $form = window.jQuery(form);
        if (!$form.data("validator")) {
            return;
        }

        window.jQuery(element).valid();
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

        feedback.classList.remove("text-muted", "text-danger", "text-success");

        if (!hasNewPassword && !hasConfirmPassword) {
            feedback.textContent = "Repite la nueva contraseña para validar coincidencia antes de continuar.";
            feedback.classList.add("text-muted");
        } else if (!hasConfirmPassword) {
            feedback.textContent = "Confirma la nueva contraseña para completar la validación.";
            feedback.classList.add("text-muted");
        } else if (passwordsMatch) {
            feedback.textContent = "La nueva contraseña coincide correctamente.";
            feedback.classList.add("text-success");
        } else {
            feedback.textContent = "La confirmación no coincide con la nueva contraseña.";
            feedback.classList.add("text-danger");
        }

        updateInputState(newPasswordInput, passwordRulesAreValid, hasNewPassword);
        updateInputState(confirmPasswordInput, passwordsMatch, hasConfirmPassword);
        submitButton.disabled = !(passwordRulesAreValid && passwordsMatch);

        triggerValidation(newPasswordInput);
        triggerValidation(confirmPasswordInput);
    }

    newPasswordInput.addEventListener("input", updateFeedback);
    confirmPasswordInput.addEventListener("input", updateFeedback);
    newPasswordInput.addEventListener("blur", updateFeedback);
    confirmPasswordInput.addEventListener("blur", updateFeedback);

    updateFeedback();
})();
