(function ($) {
    if (!$ || !$.validator) {
        return;
    }

    function normalizeNumericValue(value) {
        if (typeof value !== "string") {
            return value;
        }

        const trimmedValue = value.trim().replace(/\s+/g, "");
        if (trimmedValue === "") {
            return trimmedValue;
        }

        const lastCommaIndex = trimmedValue.lastIndexOf(",");
        const lastDotIndex = trimmedValue.lastIndexOf(".");
        const decimalSeparatorIndex = Math.max(lastCommaIndex, lastDotIndex);

        if (decimalSeparatorIndex < 0) {
            return trimmedValue;
        }

        const integerPart = trimmedValue.slice(0, decimalSeparatorIndex).replace(/[.,]/g, "");
        const fractionalPart = trimmedValue.slice(decimalSeparatorIndex + 1).replace(/[.,]/g, "");

        return fractionalPart.length === 0
            ? integerPart
            : `${integerPart}.${fractionalPart}`;
    }

    function parseNumericValue(value) {
        const normalizedValue = normalizeNumericValue(value);
        const parsedValue = Number(normalizedValue);

        return Number.isFinite(parsedValue)
            ? parsedValue
            : Number.NaN;
    }

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || !Number.isNaN(parseNumericValue(value));
    };

    $.validator.methods.range = function (value, element, param) {
        if (this.optional(element)) {
            return true;
        }

        const parsedValue = parseNumericValue(value);
        const minimum = parseNumericValue(String(param[0]));
        const maximum = parseNumericValue(String(param[1]));

        return !Number.isNaN(parsedValue)
            && !Number.isNaN(minimum)
            && !Number.isNaN(maximum)
            && parsedValue >= minimum
            && parsedValue <= maximum;
    };

    $.validator.methods.min = function (value, element, param) {
        if (this.optional(element)) {
            return true;
        }

        const parsedValue = parseNumericValue(value);
        const minimum = parseNumericValue(String(param));

        return !Number.isNaN(parsedValue)
            && !Number.isNaN(minimum)
            && parsedValue >= minimum;
    };

    $.validator.methods.max = function (value, element, param) {
        if (this.optional(element)) {
            return true;
        }

        const parsedValue = parseNumericValue(value);
        const maximum = parseNumericValue(String(param));

        return !Number.isNaN(parsedValue)
            && !Number.isNaN(maximum)
            && parsedValue <= maximum;
    };
})(window.jQuery);
