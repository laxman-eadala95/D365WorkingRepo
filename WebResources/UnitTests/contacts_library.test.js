/**
 ** This file holds all the unit test cases for the Contacts Library (contacts_library.js)
 ** It validates form event handlers (OnLoad, OnChange, OnSave) and the field requirement toggle logic based on Preferred Mode of Contact
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

//* Import the mock factory and script loader from our helpers directory
const { loadWebResources, createMockContext } = require('./helpers/xrm-mock-factory');

//! Load the source code files into global scope before any tests run
//! common_library.js must load first because contacts_library.js depends on its constants (contactModes, requiredLevel, contactFieldLogicalNames)
beforeAll(() => {
    loadWebResources('common_library.js', 'contacts_library.js');
});

//! This region holds tests that validate the constants defined in common_library.js are correct
//! These act as a safety net -- if someone accidentally changes a constant value, these will catch it immediately
//#region Constants_Validation

describe('Contacts Constants Validation', () => {

    //* TC-K01: These exact strings ("none", "required", "recommended") are what the D365 Xrm SDK expects in setRequiredLevel()
    //* If any value is wrong (e.g., "Required" instead of "required"), D365 would silently ignore the call
    test('TC-K01: requiredLevel should have correct D365 requirement level values', () => {
        expect(global.requiredLevel.None).toBe("none");
        expect(global.requiredLevel.Required).toBe("required");
        expect(global.requiredLevel.Recommended).toBe("recommended");
    });

    //* TC-K02: These must match the exact logical names in the D365 Contact entity schema
    //* Even a single character difference (e.g., "emailAddress1" vs "emailaddress1") would cause getAttribute() to return null
    test('TC-K02: contactFieldLogicalNames should have correct D365 schema names', () => {
        expect(global.contactFieldLogicalNames.preferredcontactmethodcode).toBe("preferredcontactmethodcode");
        expect(global.contactFieldLogicalNames.emailaddress1).toBe("emailaddress1");
        expect(global.contactFieldLogicalNames.mobilephone).toBe("mobilephone");
    });

    //* TC-K03: These integer values come from the D365 Preferred Method of Contact option set and must match exactly
    //* If they don't match, the switch statement in setFieldRequriementByPreferedModeOfContact would always hit the default case
    test('TC-K03: contactModes should have correct option set integer values', () => {
        expect(global.contactModes.Any).toBe(1);
        expect(global.contactModes.Email).toBe(2);
        expect(global.contactModes.Phone).toBe(3);
        expect(global.contactModes.Fax).toBe(4);
        expect(global.contactModes.Mail).toBe(5);
    });
});

//#endregion

//! This region holds tests for the revealing module pattern structure of contactsLib
//! If the module structure is broken, D365 form events won't find the handlers and the form will load without custom logic
//#region Module_Structure

describe('contactsLib Module Structure', () => {

    //* TC-C11: contactsLib.OnLoad is an IIFE that returns an object exposing OnFormLoad
    //* If this is broken, the D365 form OnLoad event would fail silently
    test('TC-C11: OnLoad should be an object with OnFormLoad', () => {
        expect(typeof global.contactsLib.OnLoad).toBe('object');
        expect(global.contactsLib.OnLoad).toHaveProperty('OnFormLoad');
        expect(typeof global.contactsLib.OnLoad.OnFormLoad).toBe('function');
    });

    //* TC-C12: D365 calls this when the user changes the Preferred Contact Method dropdown
    //* The method name must match exactly what is registered in D365 form event configuration
    test('TC-C12: OnChange should be an object with OnPreferredContactMethodChange', () => {
        expect(typeof global.contactsLib.OnChange).toBe('object');
        expect(global.contactsLib.OnChange).toHaveProperty('OnPreferredContactMethodChange');
        expect(typeof global.contactsLib.OnChange.OnPreferredContactMethodChange).toBe('function');
    });

    //* TC-C13: The structure must exist so D365 can bind to the OnFormSave handler
    test('TC-C13: OnSave should be an object with OnFormSave', () => {
        expect(typeof global.contactsLib.OnSave).toBe('object');
        expect(global.contactsLib.OnSave).toHaveProperty('OnFormSave');
        expect(typeof global.contactsLib.OnSave.OnFormSave).toBe('function');
    });
});

//#endregion

//! This region holds the core business logic tests for setFieldRequriementByPreferedModeOfContact triggered via OnLoad
//! It validates the switch-case logic that controls field requirements based on the user's preferred contact method
//#region OnLoad_Field_Requirement_Tests

describe('Contact OnLoad - setFieldRequriementByPreferedModeOfContact', () => {

    //* TC-C01: When preferred contact method is Email (2), Email Address must be required and Mobile Phone optional
    //* Business rule: If a contact prefers email, we must capture their email address to reach them
    test('TC-C01: Email preference should make emailaddress1 required and mobilephone optional', () => {
        //* Mock preferredcontactmethodcode returning 2 (Email), and both target attributes for setRequiredLevel calls
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Email,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        //* Invoke the OnFormLoad handler -- same call D365 makes when the Contact form opens
        global.contactsLib.OnLoad.OnFormLoad(executionContext);

        //* emailaddress1 should be "required" and mobilephone should be "none" for Email preference
        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.Required);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
    });

    //* TC-C02: When preferred contact method is Phone (3), Mobile Phone must be required and Email Address optional
    //* Business rule: If a contact prefers phone, we must capture their phone number to reach them
    test('TC-C02: Phone preference should make mobilephone required and emailaddress1 optional', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Phone,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnLoad.OnFormLoad(executionContext);

        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.Required);
    });

    //* TC-C03: When preferred contact method is Any (1), both fields should be optional
    //* "Any" falls into the default case of the switch since there is no explicit case for it
    test('TC-C03: Any preference should make both fields optional (default case)', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Any,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnLoad.OnFormLoad(executionContext);

        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
    });

    //* TC-C04: Fax (4) also falls into the default case -- no email or phone needed for fax communication
    test('TC-C04: Fax preference should make both fields optional (default case)', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Fax,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnLoad.OnFormLoad(executionContext);

        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
    });

    //* TC-C05: Mail (5) also falls into the default case -- postal mail doesn't need email or phone
    test('TC-C05: Mail preference should make both fields optional (default case)', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Mail,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnLoad.OnFormLoad(executionContext);

        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
    });

    //* TC-C06: A new contact record might not have a preferred method selected yet (null)
    //* null doesn't match any case label so it hits the default -- both fields should be optional
    test('TC-C06: Null preference should make both fields optional (default case)', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: null,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnLoad.OnFormLoad(executionContext);

        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
    });
});

//#endregion

//! This region holds defensive edge case tests to ensure the code handles unexpected inputs gracefully
//! In D365, edge cases like null executionContext can happen during ribbon button clicks or custom API calls
//#region Defensive_Edge_Cases

describe('Contact Form - Defensive Edge Cases', () => {

    //* TC-C07: Passing null as executionContext should not throw any errors
    //* The code uses optional chaining (executionContext?.getFormContext()) which should handle this gracefully
    test('TC-C07: Null executionContext should not throw errors', () => {
        expect(() => {
            global.contactsLib.OnLoad.OnFormLoad(null);
        }).not.toThrow();
    });

    //* TC-C08: When getFormContext() returns null, the if(formContext) guard should prevent calling the utility function
    test('TC-C08: Null formContext should exit early without calling setRequiredLevel', () => {
        var executionContext = {
            getFormContext: jest.fn().mockReturnValue(null),
        };

        expect(() => {
            global.contactsLib.OnLoad.OnFormLoad(executionContext);
        }).not.toThrow();

        //* Verify getFormContext was called -- the function did attempt to get it before bailing out
        expect(executionContext.getFormContext).toHaveBeenCalled();
    });
});

//#endregion

//! This region validates that OnChange fires the exact same field requirement logic as OnLoad
//! Both OnLoad and OnChange call setFieldRequriementByPreferedModeOfContact -- if OnChange didn't, field requirements would only update on page refresh
//#region OnChange_Tests

describe('Contact OnChange - OnPreferredContactMethodChange', () => {

    //* TC-C09a: OnChange with Email should produce the same result as OnLoad with Email (TC-C01)
    //* When a user changes the dropdown to "Email" on a live form, requirements must update immediately
    test('TC-C09a: OnChange with Email should trigger same logic as OnLoad (mirrors TC-C01)', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Email,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        //* Call via OnChange handler instead of OnLoad -- different entry point, same underlying logic
        global.contactsLib.OnChange.OnPreferredContactMethodChange(executionContext);

        //* Assertions identical to TC-C01
        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.Required);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
    });

    //* TC-C09b: OnChange with Phone should produce the same result as OnLoad with Phone (TC-C02)
    test('TC-C09b: OnChange with Phone should trigger same logic as OnLoad (mirrors TC-C02)', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.preferredcontactmethodcode]: global.contactModes.Phone,
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnChange.OnPreferredContactMethodChange(executionContext);

        expect(attributes[global.contactFieldLogicalNames.emailaddress1].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.None);
        expect(attributes[global.contactFieldLogicalNames.mobilephone].setRequiredLevel)
            .toHaveBeenCalledWith(global.requiredLevel.Required);
    });
});

//#endregion

//! This region validates the OnSave handler -- it must block saves when both Email and Mobile Phone are empty
//! and allow saves when at least one of them is filled
//#region OnSave_Tests

describe('Contact OnSave - validateEmailOrPhoneBeforeSave', () => {

    //* TC-C10: When both Email and Mobile Phone are empty the save should be blocked
    //* preventDefault() tells D365 to cancel the save operation, and setFormNotification shows the user why
    test('TC-C10: Both fields empty should prevent save and show form notification', () => {
        var { executionContext, eventArgs, ui } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnSave.OnFormSave(executionContext);

        expect(eventArgs.preventDefault).toHaveBeenCalled();
        expect(ui.setFormNotification).toHaveBeenCalledWith(
            "At least one of Email or Mobile Phone must be filled before saving.",
            "ERROR",
            global.contactFormNotifications.emailOrPhoneRequired
        );
    });

    //* TC-C10a: Empty strings are also considered empty -- the form should not save with blank values
    test('TC-C10a: Both fields as empty strings should prevent save and show form notification', () => {
        var { executionContext, eventArgs, ui } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.emailaddress1]: "",
                [global.contactFieldLogicalNames.mobilephone]: "",
            }
        });

        global.contactsLib.OnSave.OnFormSave(executionContext);

        expect(eventArgs.preventDefault).toHaveBeenCalled();
        expect(ui.setFormNotification).toHaveBeenCalled();
    });

    //* TC-C10b: When only Email is filled the save should go through and the notification should be cleared
    test('TC-C10b: Email filled should allow save and clear notification', () => {
        var { executionContext, eventArgs, ui } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.emailaddress1]: "test@example.com",
                [global.contactFieldLogicalNames.mobilephone]: null,
            }
        });

        global.contactsLib.OnSave.OnFormSave(executionContext);

        expect(eventArgs.preventDefault).not.toHaveBeenCalled();
        expect(ui.clearFormNotification).toHaveBeenCalledWith(
            global.contactFormNotifications.emailOrPhoneRequired
        );
    });

    //* TC-C10c: When only Mobile Phone is filled the save should go through and the notification should be cleared
    test('TC-C10c: Mobile Phone filled should allow save and clear notification', () => {
        var { executionContext, eventArgs, ui } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.emailaddress1]: null,
                [global.contactFieldLogicalNames.mobilephone]: "1234567890",
            }
        });

        global.contactsLib.OnSave.OnFormSave(executionContext);

        expect(eventArgs.preventDefault).not.toHaveBeenCalled();
        expect(ui.clearFormNotification).toHaveBeenCalledWith(
            global.contactFormNotifications.emailOrPhoneRequired
        );
    });

    //* TC-C10d: When both fields are filled the save should go through -- this is the ideal scenario
    test('TC-C10d: Both fields filled should allow save and clear notification', () => {
        var { executionContext, eventArgs, ui } = createMockContext({
            attributeValues: {
                [global.contactFieldLogicalNames.emailaddress1]: "test@example.com",
                [global.contactFieldLogicalNames.mobilephone]: "1234567890",
            }
        });

        global.contactsLib.OnSave.OnFormSave(executionContext);

        expect(eventArgs.preventDefault).not.toHaveBeenCalled();
        expect(ui.clearFormNotification).toHaveBeenCalledWith(
            global.contactFormNotifications.emailOrPhoneRequired
        );
    });

    //* TC-C10e: Null executionContext should not throw -- same defensive pattern as OnLoad (TC-C07)
    test('TC-C10e: Null executionContext should not throw errors on save', () => {
        expect(() => {
            global.contactsLib.OnSave.OnFormSave(null);
        }).not.toThrow();
    });

    //* TC-C10f: When getFormContext() returns null the if(formContext) guard should prevent calling the validation
    test('TC-C10f: Null formContext should exit early without calling validation', () => {
        var executionContext = {
            getFormContext: jest.fn().mockReturnValue(null),
        };

        expect(() => {
            global.contactsLib.OnSave.OnFormSave(executionContext);
        }).not.toThrow();

        expect(executionContext.getFormContext).toHaveBeenCalled();
    });
});

//#endregion

//! This region validates the contactFormNotifications constant used by the save validation
//#region ContactFormNotifications_Validation

describe('Contact Form Notifications Constants Validation', () => {

    //* TC-K04: The notification unique ID must match exactly -- if it changes, clearFormNotification would fail to clear the right banner
    test('TC-K04: contactFormNotifications should have correct unique IDs', () => {
        expect(global.contactFormNotifications.emailOrPhoneRequired).toBe("CONTACT_EMAIL_OR_PHONE_REQUIRED");
    });
});

//#endregion
