/**
 ** This file holds all the common functions and constants of the system, to help reusability of code
 ** across Contact, Opportunity, and other form scripts. Load this web resource before entity-specific libraries.
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

//* This line indicated the declaration of the common library object (commonLib). I have used this pattern to make code segrigated to its responsibility and more readable
var commonLib = commonLib || {};

//! This region of code will hold all the constant at app level
//#region App_Constants

/**
 *  *This constant maps to Dynamics 365 field requirement levels used by getAttribute().setRequiredLevel()
 *  *None = optional, Required = must have value, Recommended = soft prompt without blocking save
 */
const requiredLevel = {
    "None": "none",
    "Required": "required",
    "Recommended": "recommended"
}
//#endregion

//! This region of code will hold all the constant or variables that are required for Contacts library
//#region Contacts_Constants

/**
 *  *Maintained this constant of logical names so that instead of hardcoding throughout the codebase this constants can help in reusability
 *  *Used by contacts_library for Preferred Contact Method, Email, Mobile Phone, and related validation
 */
const contactFieldLogicalNames = {
    preferredcontactmethodcode: "preferredcontactmethodcode",
    emailaddress1: "emailaddress1",
    mobilephone: "mobilephone",
}

/**
 *  *This constant holds the option set numeric values for Preferred Mode Of Contact on the Contact entity
 *  *Must stay in sync with the field metadata in the environment (Any, Email, Phone, Fax, Mail)
 */
const contactModes = {
    "Any": 1,
    "Email": 2,
    "Phone": 3,
    "Fax": 4,
    "Mail": 5
}

/**
 *  *This constant holds the unique IDs for form notifications on the Contact form so they can be shown and cleared reliably
 *  *Same id must be passed to clearFormNotification when dismissing a message shown with setFormNotification
 */
const contactFormNotifications = {
    emailOrPhoneRequired: "CONTACT_EMAIL_OR_PHONE_REQUIRED"
}

//#endregion

//! This region of code will hold all the constant or variables that are required for Opportunities library
//#region Opportunities_Constants

/**
 *  *Maintained this constant of logical names so that instead of hardcoding throughout the codebase this constants can help in reusability
 *  *Used by opportunity_library for Estimated Revenue, Opportunity Type, and variable-price formula fields
 */
const opportunityFieldLogicalNames = {
    estimatedvalue: "estimatedvalue",
    opportunityTypeCode: "le_opportunitytypecode",
    totalunits: "le_totalunits",//Minimum value is set to 1 in the field metadata to prevent setting the field to 0 which will break the formula
    priceperunit: "le_priceperunit",//Minimum value is set to 1 in the field metadata to prevent setting the field to 0 which will break the formula
    discountamount: "discountamount",
}

/**
 *  *This constant holds the option set numeric values for Opportunity Type (e.g. Fixed vs Variable Price)
 *  *Must stay in sync with the field metadata; drives Estimated Revenue lock and auto-calculation behavior
 */
const opportunityTypes = {
    "FixedPrice": 1,
    "VariablePrice": 2,
}

//#endregion
