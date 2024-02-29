"use strict";
exports.__esModule = true;
exports.getErrorCode = void 0;
function isErrorWithCode(error) {
    return typeof error === 'object' && error !== null && 'code' in error && typeof error.code === 'string';
}
function getErrorCode(error) {
    if (isErrorWithCode(error))
        return error.code;
}
exports.getErrorCode = getErrorCode;
