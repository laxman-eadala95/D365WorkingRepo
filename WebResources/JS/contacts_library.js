/**
 ** This file holds all the functions related to Contact Table
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

//* This line indicated the declaration of the contacts library object. I have used this pattern to make code segrigated to its responsibility and more readable
var contactsLib = contactsLib || {};

//! This region of code will hold all the form events code for Contacts library
//#region Events_Functions
/**
 * !This section hold all the functions that have to be triggered on load events of the form
 * @param {*} executionContext 
 * @returns 
 */
contactsLib.OnLoad = (function (executionContext) {

    /**
     *  *This function is used to trigger the main onload custom function
     *  @param {Form execution context} executionContext 
     */
    const onFormLoad = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext)
            setFieldRequriementByPreferedModeOfContact(formContext);
    }

    return {
        OnFormLoad: onFormLoad
    };
})();

/**
 * ! This section holds all the functions that have to be triggered on change events of fields
 */
contactsLib.OnChange = (function (executionContext) {

    /**
     *  *This function is to set the state code selected to the Address 1 State field
     *  @param {*} executionContext 
     */
    const onPreferredContactMethodChange = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext)
            setFieldRequriementByPreferedModeOfContact(formContext);
    }

    return {
        OnPreferredContactMethodChange: onPreferredContactMethodChange
    };
})();

/**
 * ! This section hold all the functions that have to be triggered on form save event
 */
contactsLib.OnSave = (function (executionContext) {

    /**
     * *This function is used to trigger the main onsave custom function
     * @param {Form execution context} executionContext 
     */
    const onFormSave = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext)
            validateEmailOrPhoneBeforeSave(executionContext, formContext);
    }

    return {
        OnFormSave: onFormSave
    };
})();
//#endregion

/**
 * ! This region holds all the utility functions that are used across the file
 */
//#region Utility_Functions

/**
 *  *This function validates that at least one of Email or Mobile Phone is filled before allowing save
 *  *If both are empty it prevents the save and shows a form notification to the user
 *  @param {executionContext} executionContext
 *  @param {formContext} formContext 
 */
const validateEmailOrPhoneBeforeSave = (executionContext, formContext) => {
    let email = formContext?.getAttribute(contactFieldLogicalNames.emailaddress1)?.getValue();
    let mobilePhone = formContext?.getAttribute(contactFieldLogicalNames.mobilephone)?.getValue();

    //! If both email and mobile phone are empty prevent the save and show a form notification
    if(!email && !mobilePhone) {
        executionContext?.getEventArgs()?.preventDefault();
        formContext?.ui?.setFormNotification(
            "At least one of Email or Mobile Phone must be filled before saving.",
            "ERROR",
            contactFormNotifications.emailOrPhoneRequired
        );
    } else {
        //! Clear the notification if at least one of the fields is filled
        formContext?.ui?.clearFormNotification(contactFormNotifications.emailOrPhoneRequired);
    }
}

/**
 *  *This is a generic function  used to toggle the field requriement based on Prefered Contact Method  ˘of contact 
 *  @param {formContext} formContext 
 */
const setFieldRequriementByPreferedModeOfContact = (formContext) => {
    let preferredContactMethod = formContext?.getAttribute(contactFieldLogicalNames.preferredcontactmethodcode)?.getValue();

    //! Use switch case based on the selected mode of contact to keep is more scalable and cleaner
    switch (preferredContactMethod) {
        //! If selected mode of contact is Email set Email Address field required and Mobile Phone optional
        case contactModes.Email:
            formContext?.getAttribute(contactFieldLogicalNames.emailaddress1)?.setRequiredLevel(requiredLevel.Required);
            formContext?.getAttribute(contactFieldLogicalNames.mobilephone)?.setRequiredLevel(requiredLevel.None);    
            break;
        //! If selected mode of contact is Phone set Mobile Phone field required and Email Address optional
        case contactModes.Phone:
            formContext?.getAttribute(contactFieldLogicalNames.emailaddress1)?.setRequiredLevel(requiredLevel.None);
            formContext?.getAttribute(contactFieldLogicalNames.mobilephone)?.setRequiredLevel(requiredLevel.Required);
            break;
        //! If selected mode of contact is not either Phone / Email set Mobile Phone field optional and Email Address optional
        default:
            formContext?.getAttribute(contactFieldLogicalNames.emailaddress1)?.setRequiredLevel(requiredLevel.None);
            formContext?.getAttribute(contactFieldLogicalNames.mobilephone)?.setRequiredLevel(requiredLevel.None);
            break;
    }
}
//#endregion