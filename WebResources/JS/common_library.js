/**
 ** This file holds all the common functions and constants of the system, to help resuability of code
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

var udhg_common = udhg_common || {};

//! This region of code will hold all the constant at app level
//#region App_Constancts

//* Field requirement levels
const requiredLevel = {
    "None": "none",
    "Required": "required",
    "Recommended": "recommended"
}
//#endregion

//! This region of code will hold all the constant or variables that are required for Contacts library
//#region Contacts_Constants

//* Maintained this constant of logical names so that instead of hardcoding throughout the codebase this constants can help in reusability
const contactFieldLogicalNames = {
    preferredcontactmethodcode: "preferredcontactmethodcode",
    emailaddress1: "emailaddress1",
    mobilephone: "mobilephone",
}

//* This constant holds the options of Prefered Mode Of Contact
const contactModes = {
    "Any": 1,
    "Email": 2,
    "Phone": 3,
    "Fax": 4,
    "Mail": 5
}

//* This constant holds the unique IDs for form notifications on the Contact form so they can be shown and cleared reliably
const contactFormNotifications = {
    emailOrPhoneRequired: "CONTACT_EMAIL_OR_PHONE_REQUIRED"
}

//#endregion

//! This region of code will hold all the constant or variables that are required for Opportunities library
//#region Opportunities_Constants

//* Maintained this constant of logical names so that instead of hardcoding throughout the codebase this constants can help in reusability
const opportunityFieldLogicalNames = {
    estimatedvalue: "estimatedvalue",
    opportunityTypeCode: "opportunitytypecode",
    totalunits: "totalunits",
    priceperunit: "priceperunit",
    discountamount: "discountamount",
}

//* This constant holds the options of Opportunity Type
const opportunityTypes = {
    "FixedPrice": 1,
    "VariablePrice": 2,
}

//#endregion