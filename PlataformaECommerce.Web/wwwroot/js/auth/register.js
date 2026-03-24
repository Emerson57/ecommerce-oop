(function () {
    const form = document.querySelector("[data-register-form]");

    if (!form) {
        return;
    }

    const nameInput = form.querySelector("[data-register-name]");
    const emailInput = form.querySelector("[data-register-email]");
    const passwordInput = form.querySelector("[data-register-password]");
    const confirmPasswordInput = form.querySelector("[data-register-confirm-password]");
    const preferencesInput = form.querySelector("[data-register-preferences]");
    const termsInput = form.querySelector("[data-register-terms]");
    const privacyInput = form.querySelector("[data-register-privacy]");
    const submitButton = form.querySelector("[data-register-submit]");

    const nameFeedback = form.querySelector("[data-register-name-feedback]");
    const emailFeedback = form.querySelector("[data-register-email-feedback]");
    const confirmFeedback = form.querySelector("[data-register-confirm-feedback]");
    const preferencesFeedback = form.querySelector("[data-register-preferences-feedback]");
    const consentFeedback = form.querySelector("[data-register-consent-feedback]");
    const passwordRulesContainer = form.querySelector("[data-register-password-rules]");

    if (!nameInput || !emailInput || !passwordInput || !confirmPasswordInput || !preferencesInput || !termsInput || !privacyInput || !submitButton || !nameFeedback || !emailFeedback || !confirmFeedback || !preferencesFeedback || !consentFeedback || !passwordRulesContainer) {
        return;
    }

    const options = {
        emailCheckUrl: form.dataset.emailCheckUrl || "",
        minNameLength: Number(form.dataset.nameMinLength || 3),
        maxNameLength: Number(form.dataset.nameMaxLength || 100),
        minPasswordLength: Number(form.dataset.passwordMinLength || 8),
        maxPasswordLength: Number(form.dataset.passwordMaxLength || 100),
        minPreferenceLength: Number(form.dataset.preferenceMinLength || 2),
        maxPreferenceLength: Number(form.dataset.preferenceMaxLength || 50),
        maxPreferenceCount: Number(form.dataset.preferenceMaxCount || 20)
    };

    const passwordRuleElements = {
        length: passwordRulesContainer.querySelector("[data-password-rule='length']"),
        uppercase: passwordRulesContainer.querySelector("[data-password-rule='uppercase']"),
        lowercase: passwordRulesContainer.querySelector("[data-password-rule='lowercase']"),
        digit: passwordRulesContainer.querySelector("[data-password-rule='digit']"),
        special: passwordRulesContainer.querySelector("[data-password-rule='special']")
    };

    let emailCheckAbortController = null;
    let emailValidationVersion = 0;
    let emailState = { isValid: false, isAvailable: false, isChecking: false, code: null, isTransientFailure: false };

    function setMessage(element, cssClass, message) {
        element.classList.remove("text-muted", "text-danger", "text-success", "text-info");
        element.classList.add(cssClass);
        element.textContent = message;
    }

    function updateInputState(element, isValid, shouldShowState) {
        element.classList.remove("is-valid", "is-invalid");

        if (!shouldShowState) {
            return;
        }

        element.classList.add(isValid ? "is-valid" : "is-invalid");
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

    function evaluateName() {
        const value = (nameInput.value || "").trim();

        if (value.length === 0) {
            setMessage(nameFeedback, "text-muted", `Ingresa entre ${options.minNameLength} y ${options.maxNameLength} caracteres.`);
            updateInputState(nameInput, false, false);
            return false;
        }

        if (value.length < options.minNameLength || value.length > options.maxNameLength) {
            setMessage(nameFeedback, "text-danger", `El nombre debe tener entre ${options.minNameLength} y ${options.maxNameLength} caracteres.`);
            updateInputState(nameInput, false, true);
            return false;
        }

        setMessage(nameFeedback, "text-success", "Nombre válido.");
        updateInputState(nameInput, true, true);
        return true;
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

    function evaluatePreferences() {
        const rawValue = preferencesInput.value || "";

        if (!rawValue.trim()) {
            setMessage(preferencesFeedback, "text-muted", "Campo opcional. Puedes registrar intereses separados por comas.");
            updateInputState(preferencesInput, true, false);
            return true;
        }

        const values = rawValue
            .split(",")
            .map(function (value) { return value.trim(); })
            .filter(function (value) { return value.length > 0; });

        if (values.length > options.maxPreferenceCount) {
            setMessage(preferencesFeedback, "text-danger", `No puedes registrar más de ${options.maxPreferenceCount} intereses iniciales.`);
            updateInputState(preferencesInput, false, true);
            return false;
        }

        const hasInvalidValue = values.some(function (value) {
            return value.length < options.minPreferenceLength || value.length > options.maxPreferenceLength;
        });

        if (hasInvalidValue) {
            setMessage(preferencesFeedback, "text-danger", `Cada interés debe tener entre ${options.minPreferenceLength} y ${options.maxPreferenceLength} caracteres.`);
            updateInputState(preferencesInput, false, true);
            return false;
        }

        setMessage(preferencesFeedback, "text-success", "Intereses válidos.");
        updateInputState(preferencesInput, true, true);
        return true;
    }

    function evaluateConsents() {
        const areRequiredConsentsAccepted = termsInput.checked && privacyInput.checked;

        if (areRequiredConsentsAccepted) {
            setMessage(consentFeedback, "text-success", "Consentimientos obligatorios confirmados.");
        } else {
            setMessage(consentFeedback, "text-danger", "Debes aceptar los términos y la política de tratamiento de datos para continuar.");
        }

        return areRequiredConsentsAccepted;
    }

    function isEmailFormatValid(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    async function evaluateEmailAvailability() {
        const email = (emailInput.value || "").trim();
        const currentVersion = ++emailValidationVersion;

        if (emailCheckAbortController) {
            emailCheckAbortController.abort();
        }

        if (!email) {
            emailState = { isValid: false, isAvailable: false, isChecking: false, code: null, isTransientFailure: false };
            setMessage(emailFeedback, "text-muted", "Usaremos este correo como identificador principal de acceso.");
            updateInputState(emailInput, false, false);
            return false;
        }

        if (!isEmailFormatValid(email)) {
            emailState = { isValid: false, isAvailable: false, isChecking: false, code: "Register.EmailFormatInvalid", isTransientFailure: false };
            setMessage(emailFeedback, "text-danger", "El correo electrónico no tiene un formato válido.");
            updateInputState(emailInput, false, true);
            return false;
        }

        if (!options.emailCheckUrl) {
            emailState = { isValid: true, isAvailable: true, isChecking: false, code: "Register.EmailValidationBypassed", isTransientFailure: false };
            setMessage(emailFeedback, "text-success", "Correo electrónico válido.");
            updateInputState(emailInput, true, true);
            return true;
        }

        emailCheckAbortController = new AbortController();
        emailState = { isValid: true, isAvailable: false, isChecking: true, code: null, isTransientFailure: false };
        setMessage(emailFeedback, "text-info", "Validando disponibilidad del correo...");

        try {
            const url = new URL(options.emailCheckUrl, window.location.origin);
            url.searchParams.set("email", email);

            const response = await fetch(url.toString(), {
                method: "GET",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                signal: emailCheckAbortController.signal
            });

            if (currentVersion !== emailValidationVersion) {
                return false;
            }

            if (response.status === 204) {
                emailState = { isValid: false, isAvailable: false, isChecking: false, code: "General.RequestCanceled", isTransientFailure: false };
                setMessage(emailFeedback, "text-muted", "La validación del correo fue reiniciada.");
                updateInputState(emailInput, false, false);
                return false;
            }

            if (!response.ok) {
                let errorResult = {
                    code: "Register.EmailAvailabilityUnavailable",
                    message: "No fue posible validar el correo electrónico en este momento.",
                    isTransientFailure: true
                };

                try {
                    const problemResult = await response.json();

                    if (problemResult && typeof problemResult === "object") {
                        errorResult = {
                            code: typeof problemResult.code === "string" && problemResult.code.trim().length > 0
                                ? problemResult.code
                                : errorResult.code,
                            message: typeof problemResult.message === "string" && problemResult.message.trim().length > 0
                                ? problemResult.message
                                : errorResult.message,
                            isTransientFailure: Boolean(problemResult.isTransientFailure)
                        };
                    }
                } catch {
                    // La respuesta de error puede no contener cuerpo JSON y se mantiene el mensaje por defecto.
                }

                emailState = {
                    isValid: false,
                    isAvailable: false,
                    isChecking: false,
                    code: errorResult.code,
                    isTransientFailure: errorResult.isTransientFailure
                };
                setMessage(emailFeedback, "text-danger", errorResult.message);
                updateInputState(emailInput, false, true);
                return false;
            }

            const result = await response.json();

            if (currentVersion !== emailValidationVersion) {
                return false;
            }

            emailState = {
                isValid: Boolean(result.isValid),
                isAvailable: Boolean(result.isAvailable),
                isChecking: false,
                code: typeof result.code === "string" ? result.code : null,
                isTransientFailure: Boolean(result.isTransientFailure)
            };

            setMessage(emailFeedback, emailState.isValid && emailState.isAvailable ? "text-success" : "text-danger", result.message || "No fue posible validar el correo electrónico.");
            updateInputState(emailInput, emailState.isValid && emailState.isAvailable, true);
            return emailState.isValid && emailState.isAvailable;
        } catch (error) {
            if (error && error.name === "AbortError") {
                return false;
            }

            emailState = { isValid: false, isAvailable: false, isChecking: false };
            setMessage(emailFeedback, "text-danger", "No fue posible validar el correo electrónico en este momento.");
            updateInputState(emailInput, false, true);
            return false;
        }
    }

    function updateSubmitState() {
        const isFormValid =
            evaluateName() &&
            evaluatePasswordRules() &&
            evaluatePasswordConfirmation() &&
            evaluatePreferences() &&
            evaluateConsents();

        submitButton.setAttribute("aria-disabled", (!isFormValid).toString());
    }

    async function revalidateEmailAndSubmitState() {
        await evaluateEmailAvailability();
        triggerClientValidation(emailInput);
        updateSubmitState();
    }

    [nameInput, passwordInput, confirmPasswordInput, preferencesInput].forEach(function (element) {
        element.addEventListener("input", function () {
            if (element === passwordInput || element === confirmPasswordInput) {
                triggerClientValidation(passwordInput);
                triggerClientValidation(confirmPasswordInput);
            } else {
                triggerClientValidation(element);
            }

            updateSubmitState();
        });

        element.addEventListener("blur", function () {
            if (element === passwordInput || element === confirmPasswordInput) {
                triggerClientValidation(passwordInput);
                triggerClientValidation(confirmPasswordInput);
            } else {
                triggerClientValidation(element);
            }

            updateSubmitState();
        });
    });

    [termsInput, privacyInput].forEach(function (element) {
        element.addEventListener("change", function () {
            triggerClientValidation(element);
            updateSubmitState();
        });
    });

    let emailDebounceTimer = null;

    emailInput.addEventListener("input", function () {
        triggerClientValidation(emailInput);
        window.clearTimeout(emailDebounceTimer);
        emailDebounceTimer = window.setTimeout(revalidateEmailAndSubmitState, 350);
    });

    emailInput.addEventListener("blur", function () {
        triggerClientValidation(emailInput);
        window.clearTimeout(emailDebounceTimer);
        void revalidateEmailAndSubmitState();
    });

    form.addEventListener("submit", function (event) {
        triggerClientValidation(nameInput);
        triggerClientValidation(emailInput);
        triggerClientValidation(passwordInput);
        triggerClientValidation(confirmPasswordInput);
        triggerClientValidation(termsInput);
        triggerClientValidation(privacyInput);

        updateSubmitState();
    });

    updateSubmitState();
    void revalidateEmailAndSubmitState();
})();
