/**
 ** This file holds all the functions related to Opportunities Table
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

//* This line indicated the declaration of the Opportunities library object. I have used this pattern to make code segrigated to its responsibility and more readable
var opportunitiesLib = opportunitiesLib || {};

//! This region of code will hold all the form events code for Opportunities library
//#region Events_Functions
/**
 * !This section hold all the functions that have to be triggered on load events of the form
 * @param {*} executionContext 
 * @returns 
 */
opportunitiesLib.OnLoad = (function (executionContext) {

    /**
     *  *This function is used to trigger the main onload custom function
     *  @param {Form execution context} executionContext 
     */
    const onFormLoad = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext)
            toggleEstimatedRevenueStatusByOpportunityType(formContext);
    }

    return {
        OnFormLoad: onFormLoad
    };
})();

/**
 * ! This section holds all the functions that have to be triggered on change events of fields
 */
opportunitiesLib.OnChange = (function (executionContext) {

    /**
     *  *This function is to toggle the status of Estimated Revenue field based on Opportunity Type
     *  @param {*} executionContext 
     */
    const onOpportunityTypeChange = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext)
            toggleEstimatedRevenueStatusByOpportunityType(formContext);
    }

    /**
     *  *This function recalculates Estimated Revenue when Total Units, Price Per Unit, or Discount change
     *  *Only recalculates when the Opportunity Type is Variable Price since the formula does not apply to other types
     *  @param {*} executionContext 
     */
    const onFormulaFieldChange = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext) {
            let opportunityType = formContext?.getAttribute(opportunityFieldLogicalNames.opportunityTypeCode)?.getValue();
            //! Only recalculate if the opportunity type is Variable Price since the formula only applies to that type
            if(opportunityType === opportunityTypes.VariablePrice)
                calculateEstimatedRevenueForVariablePrice(formContext);
        }
    }

    return {
        OnOpportunityTypeChange: onOpportunityTypeChange,
        OnFormulaFieldChange: onFormulaFieldChange,
    };
})();

/**
 * ! This section hold all the functions that have to be triggered on form save event
 */
opportunitiesLib.OnSave = (function (executionContext) {

    /**
     * *This function is used to trigger the main onsave custom function
     * @param {Form execution context} executionContext 
     */
    const onFormSave = (executionContext) => {
        let formContext = executionContext?.getFormContext();
        //Trigger only if the From Context from execution context have captured to prevent any errors
        if(formContext)
            toggleEstimatedRevenueStatusByOpportunityType(formContext);
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
 *  *This is a utility function used to toggle the status of Estimated Revenue field based on Opportunity Type
 *  *For Fixed Price and Variable Price the field is disabled, for all other types it is enabled
 *  @param {formContext} formContext 
 */
const toggleEstimatedRevenueStatusByOpportunityType = (formContext) => {
    let opportunityType = formContext?.getAttribute(opportunityFieldLogicalNames.opportunityTypeCode)?.getValue();

    //! Use switch case based on the opportunity type to keep it more scalable and cleaner
    switch (opportunityType) {
        //! If opportunity type is Fixed Price disable the Estimated Revenue field since it is predetermined
        case opportunityTypes.FixedPrice:
            formContext?.getControl(opportunityFieldLogicalNames.estimatedvalue)?.setDisabled(true);
            break;
        //! If opportunity type is Variable Price disable the field and auto-calculate using the formula
        case opportunityTypes.VariablePrice:
            formContext?.getControl(opportunityFieldLogicalNames.estimatedvalue)?.setDisabled(true);
            calculateEstimatedRevenueForVariablePrice(formContext);
            break;
        //! For all other types enable the Estimated Revenue field for manual entry
        default:
            formContext?.getControl(opportunityFieldLogicalNames.estimatedvalue)?.setDisabled(false);
            break;
    }
}

/**
 *  *This function calculates Estimated Revenue using the formula: (Total Units * Unit Price) - Discount
 *  *Null or empty field values default to 0 so the math does not break
 *  @param {formContext} formContext 
 */
const calculateEstimatedRevenueForVariablePrice = (formContext) => {
    let totalUnits = formContext?.getAttribute(opportunityFieldLogicalNames.totalunits)?.getValue() || 0;
    let unitPrice = formContext?.getAttribute(opportunityFieldLogicalNames.priceperunit)?.getValue() || 0;
    let discount = formContext?.getAttribute(opportunityFieldLogicalNames.discountamount)?.getValue() || 0;

    let estimatedRevenue = (totalUnits * unitPrice) - discount;
    formContext?.getAttribute(opportunityFieldLogicalNames.estimatedvalue)?.setValue(estimatedRevenue);
}

//#endregion