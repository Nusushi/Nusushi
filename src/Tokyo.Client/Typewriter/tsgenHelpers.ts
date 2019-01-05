/// <reference types="jquery" />
export function Json(url: string, method: string, data: any): JQueryXHR {
    // TODO does not work reliably: cache set to false for all controller actions by default (reason: ipad bug where function was httpGET originally and cached response was used for same function as httpPOST)
    let request: JQueryXHR;
    if (data !== null) {
        request = jQuery.ajax(url, { method: method, data: JSON.stringify(data), contentType: "application/json; charset=utf-8" });
    }
    else {
        request = jQuery.ajax(url, { method: method });
    }
    request.fail((data: JQueryXHR): void => {
        if ((data.responseJSON !== undefined) && (data.responseJSON.StatusText !== undefined))
        {
            console.log(data.responseJSON.StatusText);
        }
    });
    return request; // TODO multiple queues, running, done, fail, reschedule; compare revision and 
    // TODO pause when detecting holes
};
export function Action(url: string, method: string, data: any): JQueryXHR {
    // TODO does not work reliably: cache set to false for all controller actions by default (reason: ipad bug where function was httpGET originally and cached response was used for same function as httpPOST)
    let request: JQueryXHR;
    if (data !== null) {
        request = jQuery.ajax(url, { method: method, data: JSON.stringify(data), contentType: "application/json; charset=utf-8" });
    }
    else {
        request = jQuery.ajax(url, { method: method });
    }
    request.fail((data: JQueryXHR): void => {
        if (data.statusText !== undefined) {
            console.log(data.statusText);
        }
    });
    return request;
};