/// <reference path="../jsx.ts" />
"use strict";
import { VNode } from "maquette";
import * as maquette from "maquette";
const h = maquette.h;
import { CaliforniaController, CaliforniaClientViewModel, CaliforniaEvent, CaliforniaProject } from "./CaliforniaGenerated";
import { PagePreview } from "./PagePreview";
import { PagePreviewVM } from "./../ViewModels/PagePreviewVM";
import { PropertyBar } from "./PropertyBar";
import { PropertyBarVM, PopupMode } from "../ViewModels/PropertyBarVM";
import { CaliforniaRouter } from "./CaliforniaRouter";
import { ClientState, EditViewMode } from "./ClientState";
import { CaliforniaClientPartialData } from "../Typewriter/CaliforniaClientPartialData";

export const DEFAULT_EXCEPTION: string = "unexpected error"; // TODO could redirect/overlay an error page with instructions: reload page, wait, clear history, clear local storage, clear cookies, etc.
export const UI_Z_INDEX: number = 11;
const RESIZE_HANDLER_DELAY_MS: number = 200;

export function getArrayForEnum(targetEnum: any): string[] {
    return Object.keys(targetEnum).map(key => targetEnum[key as any]).filter(value => typeof value === "string") as string[];
};

export function parseIntFromAttribute(element: EventTarget | null, attributeName: string): number { // can throw
    if (element === undefined) {
        console.log("cannot read attribute: element is null");
    }
    // TODO disable debug variant
    let attr: Attr | null = (element as HTMLElement).attributes.getNamedItem(attributeName);
    if (attr === null) {
        console.log("could not find attribute " + attributeName + " on target");
        console.log(element);
        return 0;
    }
    else {
        return parseInt(attr.value);
    }
};

export function parseStringFromAttribute(element: EventTarget | null, attributeName: string): string { // can throw
    if (element === undefined) {
        console.log("cannot read attribute: element is null");
    }
    // TODO disable debug variant
    let attr: Attr | null = (element as HTMLElement).attributes.getNamedItem(attributeName);
    if (attr === null) {
        console.log("could not find attribute " + attributeName + " on target");
        console.log(element);
        return "";
    }
    else {
        return attr.value;
    }
};

export class CaliforniaApp {
    // static html references
    public californiaMainDiv: HTMLElement = document.getElementById("california-main") as HTMLElement;
    public styleSheet: CSSStyleSheet = (document.getElementById("california-styles") as HTMLStyleElement).sheet as CSSStyleSheet;
    // rendered vdom references
    public pagePreviewHolder: HTMLElement | undefined = undefined;
    // module instances
    public static CaliforniaAppInstance: CaliforniaApp;
    public projector: maquette.Projector;
    public controller: CaliforniaController;
    public router: CaliforniaRouter;
    public state: ClientState;
    public pagePreview: PagePreview;
    public pagePreviewVM: PagePreviewVM;
    private _propertyBars: PropertyBar[] = [];
    private _propertyBarVMs: PropertyBarVM[] = [];
    private _propertyBarBoxTreeDomReferences: (HTMLDivElement | undefined)[] = [];
    private _propertyBarBoxTreeScrollHandled: boolean[] = [];
    public isAjaxRequestRunning: boolean = false; // TODO need a value that contains update data (so user cant perform operations on invalid data, which is time between request finish and update references + render)
    // smooth resize handler
    public resizeRtime: Date = new Date();
    public resizeTimeout: boolean = false;
    // UI layout
    public navigationHeigthPx: number = 32;// TODO 64;
    public controlAreaWidthPx: number = 500;
    private _propertyBarCount: number = 4; // expected to be at least > 0; also compatible with visiblePropertyBarCount

    public clientData: CaliforniaClientViewModel = {
        CurrentRevision: 0,
        StatusText: "",
        CaliforniaEvent: CaliforniaEvent.ReadInitialClientData,
        CaliforniaProject: new CaliforniaProject(),
        AllCssProperties: [],
        StyleAtomCssPropertyMapping: {},
        UrlToReadAndEdit: "",
        UrlToReadOnly: "",
        PartialUpdate: new CaliforniaClientPartialData(),
        ThirdPartyFonts: []
    };
    
    public constructor() {
        this.projector = maquette.createProjector();
        //if (MathJax.HTML.Cookie !== undefined) { TODO
        this.pagePreview = new PagePreview(this);
        this.pagePreviewVM = this.pagePreview.viewModel;
        for (let i: number = 0; i < this.propertyBarCount; i++) {
            let propertyBar = new PropertyBar(this, i);
            this._propertyBars.push(propertyBar);
            this._propertyBarVMs.push(propertyBar.viewModel);
            this._propertyBarBoxTreeDomReferences.push(undefined);
            this._propertyBarBoxTreeScrollHandled.push(false);
        }
        this.controller = new CaliforniaController(this);
        this.router = new CaliforniaRouter(this);
        this.state = new ClientState(this);
        // get client data TODO send with initial page
        this.controller.InitialClientDataJson(new Date().toString())
            .done(function (data: any): void {
                CaliforniaApp.CaliforniaAppInstance.router.updateData(data, true);
            }).fail(function (): void {
                console.log("could not get data");
            });
        // initialize projector 
        document.addEventListener("DOMContentLoaded", function (): void {
            CaliforniaApp.CaliforniaAppInstance.projector.append(CaliforniaApp.CaliforniaAppInstance.californiaMainDiv, CaliforniaApp.CaliforniaAppInstance.renderCaliforniaApp); // californiaMainDiv has height=100% for iPad
        });
        window.addEventListener("resize", function (): void {
            // smooth resize
            CaliforniaApp.CaliforniaAppInstance.resizeRtime = new Date();
            if (CaliforniaApp.CaliforniaAppInstance.resizeTimeout === false) {
                CaliforniaApp.CaliforniaAppInstance.resizeTimeout = true;
                setTimeout(CaliforniaApp.CaliforniaAppInstance.resizeCheckHandler, RESIZE_HANDLER_DELAY_MS);
            }
        });
    };

    public get propertyBarCount(): number {
        // constant
        return this._propertyBarCount;
    };

    public get propertyBars(): PropertyBar[] {
        return this._propertyBars;
    };

    public get propertyBarVMs(): PropertyBarVM[] {
        return this._propertyBarVMs;
    };

    public get propertyBarBoxTreeDomReferences(): (HTMLDivElement|undefined)[] {
        return this._propertyBarBoxTreeDomReferences;
    };

    public get propertyBarBoxTreeScrollHandled(): boolean[] {
        return this._propertyBarBoxTreeScrollHandled;
    };

    private resizeCheckHandler = (): void => {
        let curTime: number = Date.now();
        if ((curTime - CaliforniaApp.CaliforniaAppInstance.resizeRtime.getMilliseconds()) < RESIZE_HANDLER_DELAY_MS) {// ignore resize event
            setTimeout(CaliforniaApp.CaliforniaAppInstance.resizeCheckHandler, RESIZE_HANDLER_DELAY_MS);
        } else { // Done resizing;resizedBrowser
            CaliforniaApp.CaliforniaAppInstance.resizeChangedHandler();
            CaliforniaApp.CaliforniaAppInstance.resizeTimeout = false;
        }
    };

    public resizeChangedHandler = (): void => {
        CaliforniaApp.CaliforniaAppInstance.state.overrideResponsiveDeviceId = 0;
        CaliforniaApp.CaliforniaAppInstance.router.setupUiForDevice(); // TODO costs too much energy, make it also manually available
        CaliforniaApp.CaliforniaAppInstance.projector.scheduleRender(); // TODO might be called too often // TODO document render calls by function
    };

    private renderCaliforniaApp = (): VNode => {
        let appStyles = {
            "width": "100%",
            "max-width": "100%",
            "height": "100%",
            "max-height": "100%",
            "display": "flex",
            "flex-flow": "row nowrap"
        };
        let renderedPropertyBars: VNode[] = []; // equally spaced, separate overflow:scroll
        let maxRenderedPropertyBarCount: number = this.state.visiblePropertyBarMaxCount > this._propertyBarCount ? this._propertyBarCount : this.state.visiblePropertyBarMaxCount
        for (let i = 0; i < maxRenderedPropertyBarCount; i++) {
            renderedPropertyBars.push(this._propertyBars[i].renderPropertyBar());
        }
        return <div styles={appStyles}>
            {this.pagePreview.renderPreviewArea()}
            {(this.state.isHideUserInterface || this.state.editViewMode === EditViewMode.PagePreviewOnly) ? undefined : renderedPropertyBars}
            {this._propertyBars[0].renderPropertyBarPoppersRenderOnce()}
        </div> as VNode;
    };
};
