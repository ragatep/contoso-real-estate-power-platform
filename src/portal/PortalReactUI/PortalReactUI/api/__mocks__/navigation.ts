// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// mock version of navigateToPage
export const navigateToPage = jest.fn((url: string) => {
    console.log('Navigating to:', url);
});
