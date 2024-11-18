// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import { IEntity } from 'dataverse-ify';
import { sdkify } from 'dataverse-ify/lib/dataverse-ify/sdkify/sdkify';
import { EntityCollection } from 'dataverse-ify/lib/types/EntityCollection';

export async function invokeFlow<TResponse>(
    url: string,
    parameters: Record<string, string | number | boolean>,
): Promise<TResponse> {
    const payload = {
        eventData: JSON.stringify(parameters),
    };

    const jQueryPromise = shell.ajaxSafePost<string>({
        type: 'POST',
        url,
        data: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json',
        },
    });

    const response = await toNativePromise<string>(jQueryPromise);
    return JSON.parse(response) as TResponse;
}

type WebApiResponse = { value: object[] };

export async function retrieveMultiple<T extends IEntity>(
    entityPluralName: string,
    entityLogicalName: string,
    fetch: string,
) {
    const listingQuery = shell.ajaxSafePost<WebApiResponse>({
        type: 'GET',
        url: `/_api/${entityPluralName}?fetchXml=${encodeURIComponent(fetch)}`,
        data: '',
        headers: {
            'Content-Type': 'application/json',
        },
    });

    const responseObject = await toNativePromise(listingQuery);

    return (await sdkify<T>(responseObject.value, entityLogicalName)) as EntityCollection<T>;
}

export async function toNativePromise<T>(jqueryPromise: JQueryPromise<T>): Promise<T> {
    return new Promise<T>((resolve, reject) => {
        jqueryPromise.done((r: T) => resolve(r)).fail((e) => reject(e));
    });
}
