var LeadFormHandler = (function () {
    "use strict";

    // Constants
    const CONTACT_FIELD = "parentcontactid";
    const LEAD_SOURCE_FIELD = "leadsourcecode";
    const CUSTOM_FIELD = "new_customfield";
    const INDUSTRY_FIELD = "industrycode";
    const EMAIL_FIELD = "emailaddress1";

    // Public methods
    return {
        onLoad: function (executionContext) {
            const formContext = executionContext.getFormContext();

            // Register events
            formContext.getAttribute(CONTACT_FIELD).addOnChange(LeadFormHandler.onContactChange);
            formContext.getAttribute(LEAD_SOURCE_FIELD).addOnChange(LeadFormHandler.onLeadSourceChange);

            // Initial logic
            LeadFormHandler.toggleCustomField(formContext);
            LeadFormHandler.validateEmail(formContext);
        },

        onContactChange: function (executionContext) {
            const formContext = executionContext.getFormContext();
            const contact = formContext.getAttribute(CONTACT_FIELD).getValue();

            if (contact && contact.length > 0) {
                const contactId = contact[0].id.replace("{", "").replace("}", "");
                Xrm.WebApi.retrieveRecord("contact", contactId, "?$select=jobtitle,emailaddress1").then(
                    function success(result) {
                        if (result.jobtitle) {
                            formContext.getAttribute("jobtitle").setValue(result.jobtitle);
                        }
                        if (result.emailaddress1) {
                            formContext.getAttribute(EMAIL_FIELD).setValue(result.emailaddress1);
                        }
                    },
                    function (error) {
                        console.error("Error retrieving contact: ", error.message);
                    }
                );
            }
        },

        onLeadSourceChange: function (executionContext) {
            const formContext = executionContext.getFormContext();
            LeadFormHandler.toggleCustomField(formContext);
        },

        toggleCustomField: function (formContext) {
            const leadSource = formContext.getAttribute(LEAD_SOURCE_FIELD).getValue();
            const showCustom = leadSource === 100000001; // Custom source code

            formContext.getControl(CUSTOM_FIELD).setVisible(showCustom);
            if (!showCustom) {
                formContext.getAttribute(CUSTOM_FIELD).setValue(null);
            }
        },

        validateEmail: function (formContext) {
            const emailAttr = formContext.getAttribute(EMAIL_FIELD);
            const email = emailAttr.getValue();

            if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
                formContext.ui.setFormNotification("Invalid email format.", "ERROR", "email_validation");
            } else {
                formContext.ui.clearFormNotification("email_validation");
            }
        },

        onSave: function (executionContext) {
            const formContext = executionContext.getFormContext();
            const industry = formContext.getAttribute(INDUSTRY_FIELD).getValue();

            if (!industry) {
                executionContext.getEventArgs().preventDefault();
                formContext.ui.setFormNotification("Industry is required before saving.", "ERROR", "industry_required");
            }
        }
    };
})();
