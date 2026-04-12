/**
 ** Jest configuration for D365 CRM Web Resources unit tests
 ** Created On: 12/04/2026
 ** Author: Laxman Eadala
 */
module.exports = {
    //* Use Node test environment -- our D365 code only uses formContext and executionContext which we mock
    testEnvironment: "node",

    //* Scope test discovery to the UnitTests folder to avoid scanning source files
    roots: ["<rootDir>/WebResources/UnitTests"],
};
