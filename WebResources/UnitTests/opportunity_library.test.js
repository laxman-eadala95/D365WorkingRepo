/**
 ** This file holds all the unit test cases for the Opportunities Library (opportunity_library.js)
 ** It validates form event handlers (OnLoad, OnChange, OnSave) and the Estimated Revenue enable/disable toggle logic based on Opportunity Type
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

//* Import the mock factory and script loader from our helpers directory
const { loadWebResources, createMockContext } = require('./helpers/xrm-mock-factory');

//! Load the source code files into global scope before any tests run
//! common_library.js must load first because opportunity_library.js depends on its constants (opportunityTypes, opportunityFieldLogicalNames)
beforeAll(() => {
    loadWebResources('common_library.js', 'opportunity_library.js');
});

//! This region holds tests that validate the constants defined in common_library.js for the Opportunities entity
//! These act as a safety net -- if someone accidentally changes a constant value, these will catch it immediately
//#region Constants_Validation

describe('Opportunity Constants Validation', () => {

    //* TC-K04: These must match the exact logical names in the D365 Opportunity entity schema
    //* Even a single character difference would cause getAttribute() or getControl() to return null
    test('TC-K04: opportunityFieldLogicalNames should have correct D365 schema names', () => {
        expect(global.opportunityFieldLogicalNames.estimatedvalue).toBe("estimatedvalue");
        expect(global.opportunityFieldLogicalNames.opportunityTypeCode).toBe("opportunitytypecode");
        expect(global.opportunityFieldLogicalNames.totalunits).toBe("totalunits");
        expect(global.opportunityFieldLogicalNames.priceperunit).toBe("priceperunit");
        expect(global.opportunityFieldLogicalNames.discountamount).toBe("discountamount");
    });

    //* TC-K05: These integer values come from the D365 Opportunity Type option set and must match exactly
    //* If they don't match, the switch statement in toggleEstimatedRevenueStatusByOpportunityType would hit the wrong case
    test('TC-K05: opportunityTypes should have correct option set values', () => {
        expect(global.opportunityTypes.FixedPrice).toBe(1);
        expect(global.opportunityTypes.VariablePrice).toBe(2);
    });
});

//#endregion

//! This region holds tests for the revealing module pattern structure of opportunitiesLib
//! If the module structure is broken, D365 form events won't find the handlers
//#region Module_Structure

describe('opportunitiesLib Module Structure', () => {

    //* TC-O10: opportunitiesLib.OnLoad() must be callable and must expose OnFormLoad
    //* If this is broken, the D365 form OnLoad event would fail silently
    test('TC-O10: OnLoad should be a function returning an object with OnFormLoad', () => {
        expect(typeof global.opportunitiesLib.OnLoad).toBe('function');
        var onLoadModule = global.opportunitiesLib.OnLoad();
        expect(onLoadModule).toHaveProperty('OnFormLoad');
        expect(typeof onLoadModule.OnFormLoad).toBe('function');
    });

    //* TC-O11: D365 calls OnOpportunityTypeChange when the user changes the Opportunity Type dropdown
    //* OnEstimatedRevenueFieldsChange is registered on totalunits, priceperunit, discountamount fields
    test('TC-O11: OnChange should expose OnOpportunityTypeChange and OnEstimatedRevenueFieldsChange', () => {
        expect(typeof global.opportunitiesLib.OnChange).toBe('function');
        var onChangeModule = global.opportunitiesLib.OnChange();
        expect(onChangeModule).toHaveProperty('OnOpportunityTypeChange');
        expect(typeof onChangeModule.OnOpportunityTypeChange).toBe('function');
        expect(onChangeModule).toHaveProperty('OnEstimatedRevenueFieldsChange');
        expect(typeof onChangeModule.OnEstimatedRevenueFieldsChange).toBe('function');
    });

    //* TC-O12: Even though OnFormSave is currently empty, the structure must exist so D365 can bind to it
    test('TC-O12: OnSave should be a function returning an object with OnFormSave', () => {
        expect(typeof global.opportunitiesLib.OnSave).toBe('function');
        var onSaveModule = global.opportunitiesLib.OnSave();
        expect(onSaveModule).toHaveProperty('OnFormSave');
        expect(typeof onSaveModule.OnFormSave).toBe('function');
    });
});

//#endregion

//! This region holds the core business logic tests for toggleEstimatedRevenueStatusByOpportunityType triggered via OnLoad
//! When an opportunity is "Fixed Price", the estimated revenue is predetermined and should not be editable
//#region OnLoad_EstimatedRevenue_Toggle_Tests

describe('Opportunity OnLoad - toggleEstimatedRevenueStatusByOpportunityType', () => {

    //* TC-O01: When opportunity type is FixedPrice (1), the Estimated Revenue control should be disabled
    //* Business rule: Fixed price opportunities have a predetermined revenue -- users should not be able to change it
    test('TC-O01: FixedPrice opportunity type should disable the Estimated Revenue field', () => {
        //* We need both an attribute (to read the type value) and a control (to disable the UI element)
        var { executionContext, controls } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.FixedPrice,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        //* (opportunityType === opportunityTypes.FixedPrice) evaluates to true, so setDisabled(true) is expected
        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(true);
    });

    //* TC-O02: When opportunity type is NOT FixedPrice, the Estimated Revenue control should be enabled
    //* Any value other than 1 (FixedPrice) should keep the revenue field editable
    test('TC-O02: Non-FixedPrice opportunity type should enable the Estimated Revenue field', () => {
        //* Using value 99 as an arbitrary non-FixedPrice type to prove the comparison is strict
        var { executionContext, controls } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: 99,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        //* (99 === 1) is false, so setDisabled(false) -- field stays editable
        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(false);
    });

    //* TC-O03: A new opportunity might not have a type selected yet (null)
    //* null === 1 evaluates to false, so the revenue field should remain editable
    test('TC-O03: Null opportunity type should enable the Estimated Revenue field', () => {
        var { executionContext, controls } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: null,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(false);
    });

    //* TC-O04: undefined === 1 is also false, so the revenue field should stay editable
    //* Covers edge cases where getAttribute returns an attribute whose getValue returns undefined
    test('TC-O04: Undefined opportunity type should enable the Estimated Revenue field', () => {
        var { executionContext, controls } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: undefined,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(false);
    });

    //* TC-O04a: When opportunity type is Variable Price the Estimated Revenue field should be disabled and auto-calculated
    //* Formula: Estimated Revenue = (Total Units * Unit Price) - Discount = (10 * 100) - 50 = 950
    test('TC-O04a: VariablePrice should disable Estimated Revenue and auto-calculate using the formula', () => {
        var { executionContext, controls, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.VariablePrice,
                [global.opportunityFieldLogicalNames.totalunits]: 10,
                [global.opportunityFieldLogicalNames.priceperunit]: 100,
                [global.opportunityFieldLogicalNames.discountamount]: 50,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(true);
        //* (10 * 100) - 50 = 950
        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .toHaveBeenCalledWith(950);
    });

    //* TC-O04b: When formula fields are null they should default to 0 so the math does not break
    //* (0 * 0) - 0 = 0
    test('TC-O04b: VariablePrice with null formula fields should calculate as 0', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.VariablePrice,
                [global.opportunityFieldLogicalNames.totalunits]: null,
                [global.opportunityFieldLogicalNames.priceperunit]: null,
                [global.opportunityFieldLogicalNames.discountamount]: null,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .toHaveBeenCalledWith(0);
    });

    //* TC-O04c: When only some formula fields are filled the null ones should default to 0
    //* (5 * 200) - 0 = 1000
    test('TC-O04c: VariablePrice with partial formula fields should handle nulls as 0', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.VariablePrice,
                [global.opportunityFieldLogicalNames.totalunits]: 5,
                [global.opportunityFieldLogicalNames.priceperunit]: 200,
                [global.opportunityFieldLogicalNames.discountamount]: null,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);

        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .toHaveBeenCalledWith(1000);
    });
});

//#endregion

//! This region holds defensive edge case tests to ensure the code handles unexpected inputs gracefully
//#region Defensive_Edge_Cases

describe('Opportunity Form - Defensive Edge Cases', () => {

    //* TC-O07: Passing null as executionContext should not throw any errors
    //* The code uses optional chaining (executionContext?.getFormContext()) which handles this
    test('TC-O07: Null executionContext should not throw errors', () => {
        expect(() => {
            global.opportunitiesLib.OnLoad().OnFormLoad(null);
        }).not.toThrow();
    });

    //* TC-O08: When getFormContext() returns null, the if(formContext) guard should prevent calling the utility function
    test('TC-O08: Null formContext should exit early without calling setDisabled', () => {
        var executionContext = {
            getFormContext: jest.fn().mockReturnValue(null),
        };

        expect(() => {
            global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);
        }).not.toThrow();

        //* Verify getFormContext was called -- the function did attempt to get it before bailing out
        expect(executionContext.getFormContext).toHaveBeenCalled();
    });

    //* TC-O09: When the estimatedvalue control does not exist on the form (getControl returns null)
    //* The code now uses optional chaining on getControl so it should handle missing controls gracefully
    test('TC-O09: Missing estimatedvalue control should not throw errors with optional chaining', () => {
        //* Mock with the attribute (so getValue works) but WITHOUT the control (so getControl returns null)
        var { executionContext } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.FixedPrice,
            },
            controlStates: {}
        });

        expect(() => {
            global.opportunitiesLib.OnLoad().OnFormLoad(executionContext);
        }).not.toThrow();
    });
});

//#endregion

//! This region validates that OnChange fires the exact same toggle logic as OnLoad
//! Both OnLoad and OnChange call toggleEstimatedRevenueStatusByOpportunityType
//#region OnChange_Tests

describe('Opportunity OnChange - OnOpportunityTypeChange', () => {

    //* TC-O05a: OnChange with FixedPrice should produce the same result as OnLoad with FixedPrice (TC-O01)
    //* When a user changes the dropdown to "Fixed Price" on a live form, the field must be immediately disabled
    test('TC-O05a: OnChange with FixedPrice should disable Estimated Revenue (mirrors TC-O01)', () => {
        var { executionContext, controls } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.FixedPrice,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        //* Call via OnChange handler instead of OnLoad -- different entry point, same underlying logic
        global.opportunitiesLib.OnChange().OnOpportunityTypeChange(executionContext);

        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(true);
    });

    //* TC-O05b: OnChange with non-FixedPrice should produce the same result as TC-O02
    test('TC-O05b: OnChange with non-FixedPrice should enable Estimated Revenue (mirrors TC-O02)', () => {
        //* Using value 99 as an arbitrary non-FixedPrice type, same as TC-O02
        var { executionContext, controls } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: 99,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnChange().OnOpportunityTypeChange(executionContext);

        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(false);
    });

    //* TC-O05c: OnChange with VariablePrice should disable the field and auto-calculate (mirrors TC-O04a)
    test('TC-O05c: OnChange with VariablePrice should disable and auto-calculate Estimated Revenue', () => {
        var { executionContext, controls, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.VariablePrice,
                [global.opportunityFieldLogicalNames.totalunits]: 8,
                [global.opportunityFieldLogicalNames.priceperunit]: 50,
                [global.opportunityFieldLogicalNames.discountamount]: 25,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
            controlStates: {
                [global.opportunityFieldLogicalNames.estimatedvalue]: { disabled: false },
            }
        });

        global.opportunitiesLib.OnChange().OnOpportunityTypeChange(executionContext);

        expect(controls[global.opportunityFieldLogicalNames.estimatedvalue].setDisabled)
            .toHaveBeenCalledWith(true);
        //* (8 * 50) - 25 = 375
        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .toHaveBeenCalledWith(375);
    });
});

//#endregion

//! This region validates the OnEstimatedRevenueFieldsChange handler
//! When a formula field changes and the type is Variable Price, Estimated Revenue should be recalculated automatically
//#region OnChange_FormulaFields_Tests

describe('Opportunity OnChange - OnEstimatedRevenueFieldsChange', () => {

    //* TC-O05d: When a formula field changes and type is Variable Price the Estimated Revenue should be recalculated
    //* (5 * 200) - 100 = 900
    test('TC-O05d: Formula field change when VariablePrice should recalculate Estimated Revenue', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.VariablePrice,
                [global.opportunityFieldLogicalNames.totalunits]: 5,
                [global.opportunityFieldLogicalNames.priceperunit]: 200,
                [global.opportunityFieldLogicalNames.discountamount]: 100,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
        });

        global.opportunitiesLib.OnChange().OnEstimatedRevenueFieldsChange(executionContext);

        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .toHaveBeenCalledWith(900);
    });

    //* TC-O05e: When a formula field changes but type is Fixed Price the Estimated Revenue should NOT be recalculated
    //* The formula only applies to Variable Price opportunities
    test('TC-O05e: Formula field change when FixedPrice should not recalculate Estimated Revenue', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: global.opportunityTypes.FixedPrice,
                [global.opportunityFieldLogicalNames.totalunits]: 5,
                [global.opportunityFieldLogicalNames.priceperunit]: 200,
                [global.opportunityFieldLogicalNames.discountamount]: 100,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
        });

        global.opportunitiesLib.OnChange().OnEstimatedRevenueFieldsChange(executionContext);

        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .not.toHaveBeenCalled();
    });

    //* TC-O05f: When a formula field changes but type is null (no type selected yet) the Estimated Revenue should NOT be recalculated
    test('TC-O05f: Formula field change when null type should not recalculate Estimated Revenue', () => {
        var { executionContext, attributes } = createMockContext({
            attributeValues: {
                [global.opportunityFieldLogicalNames.opportunityTypeCode]: null,
                [global.opportunityFieldLogicalNames.totalunits]: 5,
                [global.opportunityFieldLogicalNames.priceperunit]: 200,
                [global.opportunityFieldLogicalNames.discountamount]: 100,
                [global.opportunityFieldLogicalNames.estimatedvalue]: null,
            },
        });

        global.opportunitiesLib.OnChange().OnEstimatedRevenueFieldsChange(executionContext);

        expect(attributes[global.opportunityFieldLogicalNames.estimatedvalue].setValue)
            .not.toHaveBeenCalled();
    });

    //* TC-O05g: Null executionContext on formula field change should not throw
    test('TC-O05g: Null executionContext should not throw errors on formula field change', () => {
        expect(() => {
            global.opportunitiesLib.OnChange().OnEstimatedRevenueFieldsChange(null);
        }).not.toThrow();
    });
});

//#endregion

//! This region validates the OnSave handler
//#region OnSave_Tests

describe('Opportunity OnSave', () => {

    //* TC-O06: OnSave handler should be callable without errors even though it's currently empty
    //* D365 will invoke this on every form save -- if it throws, the save would fail
    test('TC-O06: OnFormSave should be callable without throwing errors', () => {
        var { executionContext } = createMockContext();

        expect(() => {
            global.opportunitiesLib.OnSave().OnFormSave(executionContext);
        }).not.toThrow();
    });
});

//#endregion
