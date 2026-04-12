/**
 ** This file holds the mock factory and utility functions for D365 CRM unit testing
 ** It simulates the Xrm SDK objects (executionContext, formContext, getAttribute, getControl) so we can test form events without a live D365 environment
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */

//* Node.js built-ins for reading and executing source files
const fs = require('fs');
const path = require('path');

//! This region holds the web resource loading utility that simulates how D365 loads scripts in a browser
//#region Script_Loader

/**
 *  *This function loads one or more web resource files into Jest's global scope
 *  *It concatenates all files into one script, replaces const/let with var so declarations are function-scoped,
 *  *then extracts all top-level variable names and explicitly assigns them to Jest's global object
 *  @param {...string} fileNames - Names of files inside WebResources/JS/ to load (e.g., 'common_library.js', 'contacts_library.js')
 */
const loadWebResources = (...fileNames) => {
    //* Concatenate all files into one string because in D365, scripts loaded on the same form share the same global scope
    var combinedCode = fileNames.map(fn =>
        fs.readFileSync(path.resolve(process.cwd(), 'WebResources', 'JS', fn), 'utf8')
    ).join('\n');

    //* Replace const and let with var so the code runs inside a Function body where all declarations are function-scoped
    //* This is safe for our codebase because we don't rely on const reassignment prevention or let block-scoping
    combinedCode = combinedCode.replace(/\bconst\b/g, 'var');
    combinedCode = combinedCode.replace(/\blet\b/g, 'var');

    //* Extract top-level var names (column 0 only) to avoid inner function variables like "    var onFormLoad = ..."
    var varNamePattern = /^var\s+(\w+)\s*=/gm;
    var varNames = [];
    var match;
    while ((match = varNamePattern.exec(combinedCode)) !== null) {
        varNames.push(match[1]);
    }

    //* Append assignments that copy each top-level variable to Jest's global object
    //* Without this, variables declared inside the Function body would vanish when it returns
    var globalAssignments = varNames.map(name =>
        'g["' + name + '"] = ' + name + ';'
    ).join('\n');

    //* Wrap in a new Function that takes Jest's global as parameter "g", execute, and the assignments at the end make everything accessible
    var fn = new Function('g', combinedCode + '\n' + globalAssignments);
    fn(global);
}

//#endregion

//! This region holds the mock factory that creates fake D365 Xrm SDK objects for testing
//#region Mock_Factory

/**
 *  *This function creates a complete mock of the D365 execution context chain: executionContext -> formContext -> attributes/controls
 *  *It replicates the chain so our source code runs exactly as it would on a real D365 form
 *  @param {Object} config - Configuration object
 *  @param {Object} config.attributeValues - Map of field logical names to their values (e.g., { "preferredcontactmethodcode": 2 })
 *  @param {Object} config.controlStates - Map of control logical names to their state objects (e.g., { "estimatedvalue": { disabled: false } })
 *  @returns {Object} - { executionContext, formContext, attributes, controls }
 */
const createMockContext = ({ attributeValues = {}, controlStates = {} } = {}) => {
    //* Lookup objects so getAttribute/getControl can return the right mock by logical name
    var attributes = {};
    var controls = {};

    //* Build a mock attribute for each field -- jest.fn() creates a spy that records all calls for assertion
    for (const [name, value] of Object.entries(attributeValues)) {
        attributes[name] = {
            //* getValue returns the configured value, simulating reading a field on the D365 form
            getValue: jest.fn().mockReturnValue(value),
            //* setRequiredLevel is what contacts_library.js calls to make fields required/optional
            setRequiredLevel: jest.fn(),
            //* setValue included for future tests that verify field value changes
            setValue: jest.fn(),
        };
    }

    //* Build a mock control for each control name -- controls are the UI elements (text boxes, dropdowns)
    for (const [name, state] of Object.entries(controlStates)) {
        controls[name] = {
            //* setDisabled is what opportunity_library.js calls to lock/unlock fields
            setDisabled: jest.fn(),
            //* getDisabled returns the current disabled state
            getDisabled: jest.fn().mockReturnValue(state.disabled || false),
            //* setVisible included for future visibility-toggle tests
            setVisible: jest.fn(),
        };
    }

    //* ui holds the form-level notification methods that our save validation uses to show / clear error banners
    var ui = {
        setFormNotification: jest.fn(),
        clearFormNotification: jest.fn(),
    };

    //* formContext is the central object our source code interacts with via executionContext.getFormContext()
    var formContext = {
        //* Returns the mock attribute by logical name, or null if not found (matches real D365 behavior)
        getAttribute: jest.fn((name) => attributes[name] || null),
        //* Returns the mock control by logical name, or null if not found (matches real D365 behavior)
        getControl: jest.fn((name) => controls[name] || null),
        //* ui object holds setFormNotification / clearFormNotification used during save validation
        ui: ui,
    };

    //* eventArgs is what D365 passes through getEventArgs() -- preventDefault() blocks the save operation
    var eventArgs = {
        preventDefault: jest.fn(),
    };

    //* executionContext wraps formContext -- this is what D365 passes to every event handler
    var executionContext = {
        getFormContext: jest.fn().mockReturnValue(formContext),
        getEventArgs: jest.fn().mockReturnValue(eventArgs),
    };

    //* Return all objects so tests can pass executionContext to handlers and assert on attributes/controls/eventArgs/ui
    return { executionContext, formContext, attributes, controls, eventArgs, ui };
}

//#endregion

//* Export both utilities so test files can require them
module.exports = { loadWebResources, createMockContext };
