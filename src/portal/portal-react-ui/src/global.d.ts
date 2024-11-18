// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// global.d.ts
export {}; // This makes the file a module

declare global {
    interface Window {
        CRE_SETTING_CHECKOUT_FLOW_URL: string;
        CRE_CURRENT_USER_ID: string;
        CRE_UI_ROUTE: string;
    }
    interface Shell {
        ajaxSafePost<TResponse>(options: AjaxSafePostOptions): JQueryPromise<TResponse>;
        getTokenDeferred(): JQueryPromise<string>;
    }

    interface AjaxSafePostOptions {
        type: string;
        url: string;
        data: string;
        headers: Record<string, string>;
    }

    const shell: Shell;
}
