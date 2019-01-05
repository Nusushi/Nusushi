/// <reference types="jquery" />
export function Json(url: string, method: string, data: any, beforeSendCallback: any): JQueryXHR {
    // TODO does not work reliably: cache set to false for all controller actions by default (reason: ipad bug where function was httpGET originally and cached response was used for same function as httpPOST)
    let request: JQueryXHR;
    if (data !== null) {
        request = jQuery.ajax(url, { method: method, data: JSON.stringify(data), contentType: "application/json; charset=utf-8", beforeSend: beforeSendCallback });
    }
    else {
        request = jQuery.ajax(url, { method: method, beforeSend: beforeSendCallback });
    }
    request.fail((data: JQueryXHR): void => {
        if ((data.responseJSON !== undefined) && (data.responseJSON.StatusText !== undefined)) { // TODO is this still correct 
            console.log(data.responseJSON.StatusText);
        }
        // TODO print error / status text does not work when development is enabled and html error page is returned => extract error message
    });
    return request; // TODO multiple queues, running, done, fail, reschedule; compare revision and 
    // TODO pause when detecting holes
};
export function Action(url: string, method: string, data: any, beforeSendCallback: any): JQueryXHR {
    // TODO does not work reliably: cache set to false for all controller actions by default (reason: ipad bug where function was httpGET originally and cached response was used for same function as httpPOST)
    let request: JQueryXHR;
    if (data !== null) {
        request = jQuery.ajax(url, { method: method, data: JSON.stringify(data), contentType: "application/json; charset=utf-8", beforeSend: beforeSendCallback });
    }
    else {
        request = jQuery.ajax(url, { method: method, beforeSend: beforeSendCallback });
    }
    request.fail((data: JQueryXHR): void => {
        if (data.statusText !== undefined) { // TODO is this still correct
            console.log(data.statusText);
        }
    });
    return request;
};