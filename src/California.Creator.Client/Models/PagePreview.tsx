/// <reference path="../jsx.ts" />
"use strict";
import { VNode, VNodeProperties } from "maquette";
import * as maquette from "maquette";
const h = maquette.h;
import { CaliforniaApp, DEFAULT_EXCEPTION, parseIntFromAttribute, getArrayForEnum, UI_Z_INDEX } from "./CaliforniaApp";
import { PagePreviewVM } from "./../ViewModels/PagePreviewVM";
import { CaliforniaView, LayoutRow, StyleMolecule, LayoutBox, LayoutAtom, ResponsiveDevice, StyleAtom, StyleMoleculeAtomMapping, ContentAtom, StyleValue, SpecialLayoutBoxType, CaliforniaClientViewModel, LayoutType, LayoutBase, CaliforniaEvent } from "./CaliforniaGenerated";
import { SelectionMode, ReadyState, EditViewMode } from "./ClientState";
import { ContentAtomType } from "../Typewriter/ContentAtomType";
import { PropertyBarVM } from "../ViewModels/PropertyBarVM";
import { VERY_HIGH_VALUE } from "./PropertyBar";

let currentApp: CaliforniaApp;
let currentPagePreview: PagePreview;

export const HIGHLIGHT_BACKGROUND_COLOR_STRING: string = "rgb(233,233,233)";
export const MANUALLY_HIGHLIGHT_BACKGROUND_COLOR_STRING: string = "rgb(222,222,222)";

export class PagePreview {
    public viewModel: PagePreviewVM;
    // virtual style sheets
    public dynamicClientGridBreakpoints: number[] = [];
    public virtualStyleIndex: { [key: number]: number }[] = [];
    public virtualPseudoStyleIndex: { [key: string]: number }[] = [];
    private _visibleLayoutAtomDomNodeReferences: HTMLElement[] = [];
    private _activeViewLayoutAtomDomNodeReferences: { [key: string]: HTMLElement } = {};
    private _visibleLayoutAtomKeys: string[] = [];
    private _mostUpperVisibleLayoutAtomId: number = 0;

    constructor(californiaAppArg: CaliforniaApp) {
        currentPagePreview = this;
        currentApp = californiaAppArg;
        this.viewModel = new PagePreviewVM(this);
    };

    private get visibleLayoutAtomDomNodeReferences(): HTMLElement[] {
        return this._visibleLayoutAtomDomNodeReferences;
    };

    public get visibleLayoutAtomKeys(): string[] {
        return this._visibleLayoutAtomKeys;
    };

    private get activeViewLayoutAtomDomNodeReferences(): { [key: string]: HTMLElement } {
        return this._activeViewLayoutAtomDomNodeReferences;
    };

    public get mostUpperVisibleLayoutAtomId(): number {
        return this._mostUpperVisibleLayoutAtomId;
    };

    public renderPreviewArea = (): VNode => {
        let previewAreaStyles = {
            "flex": currentApp.state.editViewMode === EditViewMode.SidebarOnly ? "0 0 1px"  : "1 1 777px", // >1px for IE11
            "display": "flex",
            "flex-flow": "column nowrap",
            "width": "533px",
            "max-width": "100%",
            //"min-width": currentApp.state.editViewMode === EditViewMode.SidebarOnly ? undefined : "100px", // prevents hiding of preview area when using browser zoom or resizing TODO messes with the width computation from flex...
            "height": "100%", // 100% for iPad,
            "overflow": "visible" // for top nav TODO
        };
        let isRRButtonEnabled: boolean = currentApp.state.lastCaliforniaEventData.length > 0;
        return <div key="0" styles={previewAreaStyles}>
            {currentApp.state.isHideUserInterface ? undefined : currentPagePreview.renderNavigation()}
            {currentApp.state.editViewMode === EditViewMode.SidebarOnly ? undefined : currentPagePreview.renderPagePreviewHolder()}
            {currentApp.state.isHideUserInterface ? <div key="2" styles={{ "position": "absolute", "left": "0", "top": "0", "z-index": UI_Z_INDEX.toString(), "display": "flex", "flex-flow": "column nowrap" }}>
                <button key="a" onclick={currentPagePreview.previewClickHandler}>&#8230;</button>
                <button key="b" onclick={currentApp.propertyBars[0].insertLayoutRowIntoViewClickHandler}>+(R)</button>
                {isRRButtonEnabled ? <button key="c" onclick={currentPagePreview.repeatClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>RR</button> :
                    <button disabled key="c0" onclick={currentPagePreview.repeatClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>RR</button>}
            </div> : undefined}
        </div> as VNode;
    };

    public renderNavigation = (): VNode => {
        let navigationStyles = {
            "flex": `0 0 ${currentApp.navigationHeigthPx}px`,
            "position": "relative", // for loading indicator positioning,
            "display": "flex",
            "flex-flow": "row nowrap",
            "z-index": "3"
        };
        let loadingIndicatorStyles = {
            "position": "absolute",
            "right": "0px",
            "background-color": "red",
            "color": "white",
            "border": "solid black 1px"
        };
        let navigationButtonStyles = {
            "flex": "0 0 auto"
        };
        let isRRButtonEnabled: boolean = currentApp.state.lastCaliforniaEventData.length > 0;
        // can be used as TEST for database model (create/delete) and TEST display with no errors and TEST display all data and TEST data transmitted and so on
        return <div key="0" styles={navigationStyles}>
            {currentApp.isAjaxRequestRunning ? <p key="z" styles={loadingIndicatorStyles}>Loading...</p> : undefined}
            <button key="a" onclick={currentPagePreview.previewClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>Preview</button>
            <button key="b" eid={EditViewMode.PagePreviewOnly.toString()} onclick={currentPagePreview.changeEditModeClickHandler} styles={{ "flex": "0 0 auto", "background-color": currentApp.state.editViewMode === EditViewMode.PagePreviewOnly ? "red" : undefined }}>P</button>
            <button key="c" eid={EditViewMode.SidebarOnly.toString()} onclick={currentPagePreview.changeEditModeClickHandler} styles={{ "flex": "0 0 auto", "background-color": currentApp.state.editViewMode === EditViewMode.SidebarOnly ? "red" : undefined }}>S</button>
            <button key="d" onclick={currentPagePreview.toggleSideBarCount} cid="1" styles={{ "flex": "0 0 auto" }}>x1</button>
            <button key="e" onclick={currentPagePreview.toggleSideBarCount} cid="2" styles={{ "flex": "0 0 auto" }}>x2</button>
            <button key="f" onclick={currentPagePreview.toggleSideBarCount} cid="4" styles={{ "flex": "0 0 auto" }}>x4</button>
            {currentPagePreview.renderResponsiveDeviceSelectors()}
            <button key="h" onclick={currentPagePreview.changeSelectionModeClickHandler} styles={{ "flex": "0 0 auto", "background-color": currentApp.state.currentSelectionMode === SelectionMode.Content ? "red" : undefined, "color": currentApp.state.currentSelectionMode === SelectionMode.Styles ? "red" : undefined }}>{SelectionMode[currentApp.state.currentSelectionMode]}</button>
            <button key="i" onclick={currentPagePreview.publishClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>Save</button>
            <button key="j" onclick={currentPagePreview.publishAndOpenClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>Save&Open</button>
            <button key="k" onclick={currentPagePreview.refreshClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>JAX</button>
            {isRRButtonEnabled ? <button key="l" onclick={currentPagePreview.repeatClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>RR</button> :
                <button disabled key="l0" onclick={currentPagePreview.repeatClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>RR</button>}
        </div> as VNode;
    };

    public toggleSideBarCount = (evt: MouseEvent) => {
        let sidebarCount: number = parseIntFromAttribute(evt.target, "cid");
        if (sidebarCount > 0 && sidebarCount <= 4) {
            currentApp.state.visiblePropertyBarMaxCount = sidebarCount;
        }
        else {
            console.log(DEFAULT_EXCEPTION); // TODO
        }
    };

    public publishClickHandler = (evt: MouseEvent) => { // TODO optimize: 1 event handler, controlled by html
        currentPagePreview.publish(false);
    };

    public publishAndOpenClickHandler = (evt: MouseEvent) => {
        currentPagePreview.publish(true);
    };

    private publish = (isOpen: boolean) => {
        let currentCaliforniaView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == currentPagePreview.viewModel.activeCaliforniaViewId) as CaliforniaView;
        if (isOpen) {
            currentApp.controller.PublishAction(currentApp.clientData.CaliforniaProject.CaliforniaProjectId, currentCaliforniaView.Name).done((response: any) => {
                window.location.assign(window.location.origin + `/california/pub/${currentCaliforniaView.Name}`); // TODO hardcoded link // TODO security audit
            }).fail((req: any) => {
                console.log(DEFAULT_EXCEPTION);
            });
        }
        else {
            currentApp.controller.PublishAction(currentApp.clientData.CaliforniaProject.CaliforniaProjectId, currentCaliforniaView.Name);
        }
    };

    public refreshClickHandler = (evt: MouseEvent) => {
        if (!currentApp.state.isJaxOn) {
            // TODO is this load async? TODO security audit TODO localization TODO load of some sub dependent files fails TODO code duplication (almost except root path) with publish layout
            // TODO showProcessingMessages: false,
            var head = document.getElementsByTagName("head")[0], script;
            script = document.createElement("script");
            script.type = "text/x-mathjax-config";
            script.text =
                "MathJax.Hub.Config({\n" +
                "  root: \"../third_party/mathjax\",\n" +
                "  extensions: [\"tex2jax.js\"],\n" +
                "  jax: [\"input/TeX\", \"output/HTML-CSS\"],\n" +
                //"  skipStartupTypeset: true,\n" +
                //"  config: [\"TeX-AMS_HTML.js\"],\n" + TODO this seems not to be required or loaded by default
                "  tex2jax: { inlineMath: [['$','$'], ['\\\\(','\\\\)']], skipTags: [\"script\",\"noscript\",\"style\",\"textarea\",\"pre\",\"code\",\"input\"], processEscapes: true},\n" +
                "  TeX: { extensions: [\"AMSmath.js\", \"AMSsymbols.js\"], equationNumbers: { autoNumber: \"all\" } }, showProcessingMessages: true, messageStyle:\"normal\",\n" +
                "  \"HTML-CSS\": { availableFonts: [\"TeX\"], preferredFont: \"TeX\", imageFont: null }\n" + /*TODO concept for linebreaks: {automatic: true, width: \"container\"}*/
                "});";
            head.appendChild(script);
            script = document.createElement("script");
            script.type = "text/javascript";
            //script.src = "../third_party/mathjax/MathJax.js?delayStartupUntil=configured";
            script.src = "../third_party/mathjax/MathJax.js";
            script.onload = function () { currentApp.state.isJaxOn = true; };
            head.appendChild(script);
            //MathJax.Hub.Configured(); // TODO download + activate on user request / when latex elements are there
        }
        else {
            MathJax.Hub.Queue(["resetEquationNumbers", MathJax.InputJax.TeX]);
            currentPagePreview.resetEquationNumbersWhenModifying(true);
        }
    };

    public repeatClickHandler = (evt: MouseEvent) => {
        if (currentApp.state.lastCommand === CaliforniaEvent.CreateLayoutBoxForBoxOrRow) {
            currentApp.controller.CreateLayoutBoxForBoxOrRowJson(currentApp.state.lastCaliforniaEventData[0] as number, currentApp.state.lastCaliforniaEventData[1] as number).done(data => currentApp.router.updateData(data));
        }
        else if (currentApp.state.lastCommand === CaliforniaEvent.CreateLayoutRowForView) {
            currentApp.controller.CreateLayoutRowForViewJson(currentApp.state.lastCaliforniaEventData[0] as number, currentApp.state.lastCaliforniaEventData[1] as number).done(data => currentApp.router.updateData(data));
        }
        else {
            // TODO
            console.log(DEFAULT_EXCEPTION);
            return;
        }
    };

    public resetEquationNumbersWhenModifying = (isReprocess: boolean) => {
        if (currentApp.state.isJaxOn) {
            //MathJax.Hub.Queue(["resetEquationNumbers", MathJax.InputJax.TeX]); throws errors => on manual request
            if (isReprocess) {
                MathJax.Hub.Queue(["PreProcess", MathJax.Hub]); // TODO only when changed something with label after some delay
                MathJax.Hub.Queue(["Reprocess", MathJax.Hub]);
            }
        }
    };

    public previewClickHandler = (evt: MouseEvent) => {
        currentApp.state.isHideUserInterface = !currentApp.state.isHideUserInterface;
        currentApp.projector.renderNow(); // TODO workaround 2 renders for space calculation => use client width instead
        currentApp.resizeChangedHandler();
    };

    public changeEditModeClickHandler = (evt: MouseEvent) => {
        let selectedEditViewMode: EditViewMode = parseIntFromAttribute(evt.target, "eid");
        if (currentApp.state.editViewMode === selectedEditViewMode) {
            if (currentApp.state.editViewMode === EditViewMode.Default) {
                // do nothing
                return;
            }
            else {
                currentApp.state.editViewMode = EditViewMode.Default;
                currentApp.projector.renderNow(); // TODO workaround 2 renders for space calculation => use client width instead
            }
        }
        else {
            currentApp.state.editViewMode = selectedEditViewMode;
        }
        currentApp.resizeChangedHandler();
    };

    public changeSelectionModeClickHandler = (evt: MouseEvent) => {
        currentApp.state.currentSelectionMode = currentApp.state.currentSelectionMode === SelectionMode.Content ? SelectionMode.Styles : SelectionMode.Content;
        currentPagePreview.resetContentAtomEditMode();
    };

    public renderResponsiveDeviceSelectors = (): VNode => {
        let responsiveGroupStyles = {
            "flex": "0 0 auto",
            "display": "flex",
            "flex-flow": "row nowrap"
        };
        return <div key="1" styles={responsiveGroupStyles}>
            {(currentApp.clientData.CaliforniaProject.ResponsiveDevices !== undefined) ? currentApp.clientData.CaliforniaProject.ResponsiveDevices.map(r => {
                if (r.WidthThreshold < 0) { // responsive device "None" can not be used as override
                    return undefined;
                }
                let responsiveButtonStyles = {
                    "flex": "0 0 auto",
                    "background-color": r.ResponsiveDeviceId == currentApp.state.overrideResponsiveDeviceId ? "red" : undefined,
                    "color": (r.ResponsiveDeviceId == currentApp.state.currentResponsiveDeviceId && currentApp.state.overrideResponsiveDeviceId == 0) ? "red" : undefined
                };
                let responsiveDeviceIdString: string = r.ResponsiveDeviceId.toString();
                return <button key={responsiveDeviceIdString} rid={responsiveDeviceIdString} onclick={currentPagePreview.selectResponsiveDeviceClickHandler} styles={responsiveButtonStyles}>{r.NameShort}{/*TODO reenable, takes much space (min:{r.WidthThreshold}px)*/}</button> as VNode;
            }) : undefined}
        </div> as VNode;
    };

    public selectResponsiveDeviceClickHandler = (evt: MouseEvent) => {
        let selectedId = parseIntFromAttribute(evt.target, "rid");
        if (currentApp.state.overrideResponsiveDeviceId == selectedId) {
            currentApp.state.overrideResponsiveDeviceId = 0;
        }
        else {
            currentApp.state.overrideResponsiveDeviceId = selectedId;
        }
        currentPagePreview.updatePagePreviewDimensions();
    };

    public renderPagePreviewHolder = (): VNode => {
        // page preview size depends on current preview breakpoint
        let pagePreviewHolderStyles = { // inside flex-column-nowrap
            "flex": currentApp.state.isHideUserInterface ? "1 1 auto" : "1 1 100%", // 1px for IE11 => 100% for iPad
            "margin": (currentApp.state.isHideUserInterface || currentApp.state.editViewMode === EditViewMode.PagePreviewOnly) ? /*TODO*/"0" : `0 ${currentApp.state.targetPagePreviewHolderMarginPx}px`,
            "position": "relative", // TODO document: for fixed content
            "height": "100%", // 100% for iPad
            "max-height": "100%",
            "overflow": "auto"
        };
        let scrolledPagePreview = {
            "display": "flex",
            "flex-flow": "column nowrap"
            //"overflow": "auto", // parent element of page preview
            /*"height": "100%",*/ // TODO 100% for iPad but problem: div can be smaller or larger than 100%, 100% is wrong for the case of larger div
        };
        return <div key="1"
            styles={pagePreviewHolderStyles}
            onscroll={currentPagePreview.pagePreviewHolderScrollHandler}
            afterCreate={currentPagePreview.pagePreviewHolderAfterCreateHandler}
            afterUpdate={currentPagePreview.pagePreviewHolderAfterUpdateHandler}>
            <div key="p0" styles={scrolledPagePreview}>{currentPagePreview.renderPagePreview()}</div>
            {currentPagePreview.viewModel.fixedLayoutRowsProjector.results.map(r => r.renderMaquette())}
        </div> as VNode;
    };

    private pagePreviewHolderAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        currentApp.pagePreviewHolder = element as HTMLElement;
        currentApp.resizeChangedHandler();
    };

    private pagePreviewHolderAfterUpdateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        currentApp.pagePreviewHolder = element as HTMLElement;
        // TODO this method runs way too often!?
        currentPagePreview.updateVisibleLayoutAtoms();
    };

    private pagePreviewHolderScrollHandler = (evt: UIEvent) => {
        // TODO delay recalculation similar to resize
        if (currentApp.state.visiblePropertyBarMaxCount > 0) {
            currentPagePreview.updateVisibleLayoutAtoms();
        }
    };

    public syncScrollPositionFromBoxTree = (): void => {
        if (currentApp.propertyBarVMs[0].isSyncedWithPagePreview) {
            if (currentApp.pagePreviewHolder !== undefined) {
                let staticOffsetPx: number = currentApp.navigationHeigthPx;
                let targetLayoutAtomId: number = currentApp.propertyBars[0].mostUpperVisibleLayoutAtomId;
                //console.log("preview from tree for target layout #" + targetLayoutAtomId);
                let domNodeOfTargetLayout: HTMLElement | undefined = currentPagePreview._visibleLayoutAtomDomNodeReferences.find(r => parseIntFromAttribute(r, "aid" /*TODO use dict*/) == targetLayoutAtomId);
                if (domNodeOfTargetLayout === undefined) {
                    domNodeOfTargetLayout = currentPagePreview._activeViewLayoutAtomDomNodeReferences[targetLayoutAtomId] as HTMLElement;
                    currentApp.pagePreviewHolder.scrollTop = currentApp.pagePreviewHolder.scrollTop + (domNodeOfTargetLayout.getBoundingClientRect().top - staticOffsetPx);
                }
            }
        }
    };

    public updateVisibleLayoutAtoms = () => {
        // TODO called too often TODO initial render
        let pagePreviewHolder: HTMLDivElement = currentApp.pagePreviewHolder as HTMLDivElement;
        // update visible layout atom dom node references
        currentPagePreview._visibleLayoutAtomDomNodeReferences = [];
        currentPagePreview._visibleLayoutAtomKeys = [];
        let processedElementCount: number = 0;
        let mostUpperVisibleIndex: number = -1;
        let mostUpperVisibleLayoutAtomId: number = 0;
        let mostUpperVisibleDeltaTopLeft: number = pagePreviewHolder.clientHeight + 1; // otherwise element is below visible area
        let staticOffsetPx: number = currentApp.navigationHeigthPx; // TODO everywhere: pixel aliasing are maybe because comparing not numerically, but strictly (eps)
        let currentScrollTop: number = pagePreviewHolder.scrollTop;
        let minXPreview: number = 0; // 0 based for top left corner in viewport/pagepreview
        let maxXPreview: number = pagePreviewHolder.clientHeight;
        //console.log("scrolled preview to" + currentScrollTop + " at client height " + pagePreviewHolder.clientHeight + " from max height" + pagePreviewHolder.scrollHeight  + " minX " + minXPreview + " maxX" + maxXPreview);
        for(let elementKey in currentPagePreview._activeViewLayoutAtomDomNodeReferences) {
            // TODO order all layout elements and process only specific range or move processing range with scroll
            let domNode: HTMLElement = currentPagePreview._activeViewLayoutAtomDomNodeReferences[elementKey];
            let isDomNodeVisible: boolean = false;
            //console.log("processing element: clientTop " + domNode.clientHeight + ", offsetTop " + domNode.offsetTop);
            let boundingRectElement: ClientRect = domNode.getBoundingClientRect();
            //let firstClientRectElement: ClientRect = domNode.getClientRects()[0];
            let minXElementDeltaTopLeft: number = boundingRectElement.top - staticOffsetPx;
            let maxXElementDeltaBottomLeft: number = pagePreviewHolder.clientHeight - (boundingRectElement.top - staticOffsetPx + currentScrollTop + boundingRectElement.height) + currentScrollTop;
            if (boundingRectElement.height > 0) { // height can be 0 => invisible
                if (minXElementDeltaTopLeft >= 0.0 && minXElementDeltaTopLeft <= pagePreviewHolder.clientHeight) {
                    isDomNodeVisible = true;
                }
                else if (maxXElementDeltaBottomLeft >= 0.0 && maxXElementDeltaBottomLeft <= pagePreviewHolder.clientHeight) {
                    isDomNodeVisible = true;
                }
                else if (minXElementDeltaTopLeft <= 0.0 && maxXElementDeltaBottomLeft <= 0.0) {
                    isDomNodeVisible = true;
                }
            }
            //console.log("element: bounding rect top " + boundingRectElement.top + ", rect bottom " + boundingRectElement.bottom + ", minXElementDeltaTopLeft: " + minXElementDeltaTopLeft + ", maxXElementDeltaBottomLeft: " + maxXElementDeltaBottomLeft);
            if (isDomNodeVisible) {
                currentPagePreview._visibleLayoutAtomDomNodeReferences.push(domNode);
                currentPagePreview._visibleLayoutAtomKeys.push(elementKey);
                //console.log("visible element: first client rect top " + firstClientRectElement.top + ", rect bottom " + firstClientRectElement.bottom);
                if (minXElementDeltaTopLeft < mostUpperVisibleDeltaTopLeft) {
                    mostUpperVisibleDeltaTopLeft = minXElementDeltaTopLeft;
                    mostUpperVisibleIndex = currentPagePreview._visibleLayoutAtomKeys.length;
                    mostUpperVisibleLayoutAtomId = parseIntFromAttribute(domNode, "aid");
                }
            }
            if (mostUpperVisibleLayoutAtomId != currentPagePreview._mostUpperVisibleLayoutAtomId) {
                currentPagePreview._mostUpperVisibleLayoutAtomId = mostUpperVisibleLayoutAtomId;
                if (mostUpperVisibleLayoutAtomId != 0 && currentApp.propertyBarVMs[0].isSyncedWithPagePreview) {
                    currentApp.propertyBars[0].syncScrollPositionFromPagePreview();
                }
            }
            processedElementCount++;
        }
        //console.log("pagePreview scroll: processed " + processedElementCount + " object positions, visible: " + currentPagePreview._visibleLayoutAtomDomNodeReferences.length.toString() + " most upper visible index: " + mostUpperVisibleIndex + " most upper visible layout id: " + currentPagePreview._mostUpperVisibleLayoutAtomId);
    }

    public renderPagePreview = (): VNode => {
        // TODO do not set when body height is set in current view
        let pagePreviewStyles = {
            //TODO "flex": "1 1 auto", // 1px for IE11 => auto for iPhone
            //"background-color": "white",
            "width": (currentApp.state.isDataLoaded && !currentApp.state.isEnoughAvailableSpacePagePreview) ? (currentApp.state.targetPagePreviewWidthPx) + "px" : undefined,
            "display": "flex",
            //TODO "height": "1px" // TODO currentApp.state.isHideUserInterface ? /*TODO*/"100%" : "auto", // 1px => auto for iPhone
        };
        let isRenderView: boolean = currentApp.clientData.CaliforniaProject.CaliforniaViews !== undefined;
        let californiaViewBodyStyleString: string | undefined = undefined;
        if (currentApp.clientData.CaliforniaProject.CaliforniaViews !== undefined) {
            californiaViewBodyStyleString = (currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == currentPagePreview.viewModel.activeCaliforniaViewId) as CaliforniaView).SpecialStyleBodyStyleString; // TODO expensive
        }
        return isRenderView ? <div key={`vp${currentPagePreview.viewModel.activeCaliforniaViewId}`} class={californiaViewBodyStyleString} styles={pagePreviewStyles}>
            {currentPagePreview.viewModel.californiaViewProjector.results.map(r => r.renderMaquette())}
            </div> as VNode : <div key="vp0" styles={pagePreviewStyles}></div> as VNode;
    };

    public renderCaliforniaViewArray = (): maquette.Mapping<CaliforniaView, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<CaliforniaView, any>(
            function getSectionSourceKey(source: CaliforniaView) {
                return source.CaliforniaViewId;
            },
            function createSectionTarget(source: CaliforniaView) {
                let sourceCaliforniaViewIdString = source.CaliforniaViewId.toString();
                let layoutRows = currentPagePreview.renderLayoutRowArray(false);
                layoutRows.map(source.PlacedLayoutRows);
                // --- TODO code duplication
                
                // --- code duplication end
                // TODO show body+html style molecule in page preview
                return {
                    renderMaquette: function () {
                        let californiaViewStyles = {
                            "flex": "1 1 1px"
                        };
                        return <div class={source.SpecialStyleViewStyleString} key={sourceCaliforniaViewIdString} id={`california-v${source.CaliforniaViewId}_${source.Name}`}
                            vid={sourceCaliforniaViewIdString} styles={californiaViewStyles}>
                            {layoutRows.results.map(r => r.renderMaquette())}
                        </div>;
                    },
                    update: function (updatedSource: CaliforniaView) {
                        source = updatedSource;
                        layoutRows.map(source.PlacedLayoutRows);
                        sourceCaliforniaViewIdString = source.CaliforniaViewId.toString();
                    }
                };
            },
            function updateSectionTarget(updatedSource: CaliforniaView, target: { renderMaquette(): any, update(updatedSource: CaliforniaView): void }) {
                target.update(updatedSource);
            });
    };

    public renderLayoutRowArray = (isRenderFixedLayout: boolean): maquette.Mapping<LayoutRow, { renderMaquette: () => maquette.VNode }> => { // TODO code duplication with page preview
        return maquette.createMapping<LayoutRow, any>(
            function getSectionSourceKey(source: LayoutRow) {
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutRow) {
                let sourceLayoutRowIdString = source.LayoutBaseId.toString();
                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule; // TODO expensive
                let layoutBoxMapping = currentPagePreview.renderLayoutBoxArray(isRenderFixedLayout, styleMolecule.IsPositionFixed);
                let unsortedBoxes: LayoutBox[] = source.AllBoxesBelowRow.filter(b => b.PlacedBoxInBoxId === undefined);
                let sortedBoxes: LayoutBox[] = unsortedBoxes.sort((boxA: LayoutBox, boxB: LayoutBox) => {
                    if (boxA.LayoutSortOrderKey < boxB.LayoutSortOrderKey) {
                        return -1;
                    }
                    else if (boxA.LayoutSortOrderKey == boxB.LayoutSortOrderKey) {
                        return 0;
                    }
                    else { // boxA.LayoutSortOrderKey > boxB.LayoutSortOrderKey
                        return 1;
                    }
                });
                layoutBoxMapping.map(sortedBoxes);
                // --- TODO code duplication // TODO fix max-width for responsive views // TODO fix for bottom/right instead of top/left // TODO fix for isHideUserInterface // TODO try rendering fixed items as absolute positioned directly under page preview holder
                let pagePreviewOverrideStyles: { [key: string]: string | undefined };
                let marginTopOverrideValue: string | undefined;
                let topOverrideValue: string | undefined;
                let marginLeftOverrideValue: string | undefined;
                if (styleMolecule.IsPositionFixed) {
                    marginTopOverrideValue = `${styleMolecule.TopCssValuePx}px`;
                    topOverrideValue = styleMolecule.TopCssValuePx !== undefined ? `${currentApp.navigationHeigthPx}px` : undefined; // TODO expensive;
                    marginLeftOverrideValue = `${styleMolecule.LeftCssValuePx}px`;
                }
                else {
                    marginTopOverrideValue = undefined;
                    topOverrideValue = undefined;
                    marginLeftOverrideValue = undefined;
                }
                // --- TODO code duplication end
                let styleMoleculeId: number = styleMolecule.StyleMoleculeId;
                let styleMoleculeIdString: string = styleMoleculeId.toString();
                let layoutRowStyleClass: string = `s${styleMoleculeIdString}`;
                let holderKeyString: string = `${isRenderFixedLayout ? "f" : "g"}${sourceLayoutRowIdString}`;
                return {
                    renderMaquette: function () {
                        let renderedLayoutBoxes: VNode[] = layoutBoxMapping.results.length > 0 ? layoutBoxMapping.results.map(r => r.renderMaquette()) : []; /*TODO <div key="-1"><p>Add columns...</p></div>*/
                        let isHoveredInBoxTree: boolean = currentApp.state.hoveredBoxTreeLayoutBaseId == source.LayoutBaseId;
                        if (!isRenderFixedLayout && styleMolecule.IsPositionFixed) {
                            return undefined;
                        }
                        if (styleMolecule.IsPositionFixed) {
                            pagePreviewOverrideStyles = {
                                /*"margin-top": marginTopOverrideValue,
                                "margin-left": marginLeftOverrideValue,
                                "top": topOverrideValue,
                                "left": undefined,
                                "max-width": undefined,*/
                                "position": "absolute"
                            };
                        }
                        else {
                            if (isRenderFixedLayout) {
                                if (renderedLayoutBoxes.filter(v => v !== undefined).length == 0) {
                                    return undefined;
                                }
                            }
                            pagePreviewOverrideStyles = {
                                /*"margin-top": undefined,
                                "margin-left": undefined,
                                "top": undefined,
                                "left": undefined,
                                "max-width": undefined,*/
                                "position": undefined
                            };
                        }
                        pagePreviewOverrideStyles["background-color"] = isHoveredInBoxTree ? HIGHLIGHT_BACKGROUND_COLOR_STRING : undefined;  // TODO highlight first atom or draw outline instead (atom bg-color can override => no highlight visible)
                        if (source.LayoutBaseId == currentApp.state.highlightedLayoutBaseId) {
                            pagePreviewOverrideStyles["outline"] = "solid 1px black";
                            pagePreviewOverrideStyles["outline-offset"] = "-1px";
                            pagePreviewOverrideStyles["background-color"] = MANUALLY_HIGHLIGHT_BACKGROUND_COLOR_STRING;
                        }
                        else {
                            pagePreviewOverrideStyles["outline"] = undefined;
                            pagePreviewOverrideStyles["outline-offset"] = undefined;
                        }
                        if (styleMolecule.IsPositionFixed) {
                            //pagePreviewOverrideStyles["max-width"] = styleMolecule.IsPositionFixed && currentApp.state.isDataLoaded && currentApp.state.isEnoughAvailableSpacePagePreview && currentApp.state.editViewMode !== EditViewMode.PagePreviewOnly ? `${currentApp.state.targetPagePreviewWidthPx}px` : undefined; // TODO expensive TODO duplication
                            //pagePreviewOverrideStyles["left"] = styleMolecule.IsPositionFixed && styleMolecule.LeftCssValuePx !== undefined ? `${parseInt(styleMolecule.LeftCssValuePx) + currentApp.state.targetPagePreviewHolderMarginPx}px` : undefined; // TODO expensive // TODO dangerous // TODO magic string
                        }
                        return <div key={holderKeyString} class={layoutRowStyleClass} styles={pagePreviewOverrideStyles}>
                            {renderedLayoutBoxes}
                        </div> as VNode;
                    },
                    update: function (updatedSource: LayoutRow) {
                        source = updatedSource;
                        sourceLayoutRowIdString = source.LayoutBaseId.toString();
                        styleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule; // TODO expensive
                        layoutBoxMapping = currentPagePreview.renderLayoutBoxArray(isRenderFixedLayout, styleMolecule.IsPositionFixed);
                        unsortedBoxes = source.AllBoxesBelowRow.filter(b => b.PlacedBoxInBoxId === undefined); // in the case of a preview box (temporary change in layout structure) the placed in box reference is kept in inconsistent state => check id only
                        sortedBoxes = unsortedBoxes.sort((boxA: LayoutBox, boxB: LayoutBox) => {
                            if (boxA.LayoutSortOrderKey < boxB.LayoutSortOrderKey) {
                                return -1;
                            }
                            else if (boxA.LayoutSortOrderKey == boxB.LayoutSortOrderKey) {
                                return 0;
                            }
                            else { // boxA.LayoutSortOrderKey > boxB.LayoutSortOrderKey
                                return 1;
                            }
                        });
                        layoutBoxMapping.map(sortedBoxes);
                        // --- TODO code duplication
                        if (styleMolecule.IsPositionFixed) {
                            marginTopOverrideValue = `${styleMolecule.TopCssValuePx}px`;
                            topOverrideValue = styleMolecule.TopCssValuePx !== undefined ? `${currentApp.navigationHeigthPx}px` : undefined; // TODO expensive;
                            marginLeftOverrideValue = `${styleMolecule.LeftCssValuePx}px`;
                        }
                        else {
                            marginTopOverrideValue = undefined;
                            topOverrideValue = undefined;
                            marginLeftOverrideValue = undefined;
                        }
                        // --- TODO code duplication end
                        styleMoleculeId = styleMolecule.StyleMoleculeId;
                        styleMoleculeIdString = styleMoleculeId.toString();
                        layoutRowStyleClass = `s${styleMoleculeId}`;
                        holderKeyString = `${isRenderFixedLayout ? "f" : "g"}${sourceLayoutRowIdString}`;
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutRow, target: { renderMaquette(): any, update(updatedSource: LayoutRow): void }) {
                target.update(updatedSource);
            });
    };

    private renderLayoutBoxArray = (isRenderFixedLayout: boolean, isLayoutBoxInsideFixed: boolean): maquette.Mapping<LayoutBox, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<LayoutBox, any>(
            function getSectionSourceKey(source: LayoutBox) {
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutBox) {
                let sourceLayoutBoxIdString = source.LayoutBaseId.toString();
                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule; // TODO expensive
                let renderedLayoutAtoms = currentPagePreview.renderLayoutAtomArray(isRenderFixedLayout, styleMolecule.IsPositionFixed || isLayoutBoxInsideFixed);
                let renderedLayoutBoxes = currentPagePreview.renderLayoutBoxArray(isRenderFixedLayout, styleMolecule.IsPositionFixed || isLayoutBoxInsideFixed);
                // --- TODO code duplication // TODO fix max-width for responsive views // TODO fix for bottom/right instead of top/left // TODO fix for isHideUserInterface
                let pagePreviewOverrideStyles: { [key: string]: string | undefined };
                let marginTopOverrideValue: string | undefined;
                let topOverrideValue: string | undefined;
                let marginLeftOverrideValue: string | undefined;
                if (styleMolecule.IsPositionFixed) {
                    marginTopOverrideValue = `${styleMolecule.TopCssValuePx}px`;
                    topOverrideValue = styleMolecule.TopCssValuePx !== undefined ? `${currentApp.navigationHeigthPx}px` : undefined; // TODO expensive;
                    marginLeftOverrideValue = `${styleMolecule.LeftCssValuePx}px`;
                }
                else {
                    marginTopOverrideValue = undefined;
                    topOverrideValue = undefined;
                    marginLeftOverrideValue = undefined;
                }
                // --- TODO code duplication end
                let styleMoleculeId: number = styleMolecule.StyleMoleculeId;
                let styleMoleculeIdString: string = styleMoleculeId.toString();
                let layoutBoxStyleClass: string = `s${styleMoleculeId}`;
                let richTextTag: string = "p";
                if (source.SpecialLayoutBoxType === SpecialLayoutBoxType.RichText && source.PlacedInBoxAtoms.length > 0) {
                    let layoutIdOfFirstAtom: number = source.PlacedInBoxAtoms[0].LayoutBaseId;
                    let styleOfFirstAtom: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutIdOfFirstAtom) as StyleMolecule;// TODO expensive
                    if (styleOfFirstAtom.HtmlTag !== undefined) {
                        richTextTag = styleOfFirstAtom.HtmlTag;
                    }
                    else {
                        console.log(DEFAULT_EXCEPTION); // TODO
                    }
                }
                return {
                    renderMaquette: function () {
                        let renderedBoxContent: VNode[] = currentPagePreview.mapAndRenderLayoutBoxContent(source, source.PlacedInBoxAtoms, renderedLayoutAtoms, source.PlacedInBoxBoxes, renderedLayoutBoxes); // sorted twice
                        let isHoveredInBoxTree: boolean = currentApp.state.hoveredBoxTreeLayoutBaseId == source.LayoutBaseId;
                        if (!isRenderFixedLayout && styleMolecule.IsPositionFixed) {
                            return undefined;
                        }
                        if (styleMolecule.IsPositionFixed) {
                            pagePreviewOverrideStyles = {
                                /*"margin-top": marginTopOverrideValue,
                                "margin-left": marginLeftOverrideValue,
                                "top": topOverrideValue,
                                "left": undefined,
                                "max-width": undefined*/
                                "position": "absolute"
                            };
                        }
                        else {
                            if (isRenderFixedLayout && !isLayoutBoxInsideFixed) {
                                if (renderedBoxContent.filter(v => v !== undefined).length == 0) {
                                    return undefined;
                                }
                            }
                            pagePreviewOverrideStyles = {
                                /*"margin-top": undefined,
                                "margin-left": undefined,
                                "top": undefined,
                                "left": undefined,
                                "max-width": undefined*/
                                "position": undefined
                            };
                        }
                        pagePreviewOverrideStyles["background-color"] = isHoveredInBoxTree ? HIGHLIGHT_BACKGROUND_COLOR_STRING : undefined; // TODO highlight first atom or draw outline instead (atom bg-color can override => no highlight visible)
                        if (source.LayoutBaseId == currentApp.state.highlightedLayoutBaseId) {
                            pagePreviewOverrideStyles["outline"] = "solid 1px black";
                            pagePreviewOverrideStyles["outline-offset"] = "-1px";
                            pagePreviewOverrideStyles["background-color"] = MANUALLY_HIGHLIGHT_BACKGROUND_COLOR_STRING;
                        }
                        else {
                            pagePreviewOverrideStyles["outline"] = undefined;
                            pagePreviewOverrideStyles["outline-offset"] = undefined;
                        }
                        if (styleMolecule.IsPositionFixed) {
                            //pagePreviewOverrideStyles["max-width"] = styleMolecule.IsPositionFixed && currentApp.state.isDataLoaded && currentApp.state.isEnoughAvailableSpacePagePreview && currentApp.state.editViewMode !== EditViewMode.PagePreviewOnly ? `${currentApp.state.targetPagePreviewWidthPx}px` : undefined; // TODO expensive TODO duplication
                            //pagePreviewOverrideStyles["left"] = styleMolecule.IsPositionFixed && styleMolecule.LeftCssValuePx !== undefined ? `${parseInt(styleMolecule.LeftCssValuePx) + currentApp.state.targetPagePreviewHolderMarginPx}px` : undefined; // TODO expensive // TODO dangerous // TODO magic string
                        }
                        // TODO check everywhere: are comments from tsx stripped out?
                        return (source.SpecialLayoutBoxType === SpecialLayoutBoxType.Default) ? <div key={sourceLayoutBoxIdString} class={layoutBoxStyleClass} styles={pagePreviewOverrideStyles}>
                            {renderedBoxContent}
                        </div> : (source.SpecialLayoutBoxType === SpecialLayoutBoxType.UnsortedList) ?
                                <ul key={sourceLayoutBoxIdString} class={layoutBoxStyleClass} styles={pagePreviewOverrideStyles}>
                                {renderedBoxContent}
                            </ul> :
                                (source.SpecialLayoutBoxType === SpecialLayoutBoxType.SortedList) ?
                                    <ol key={sourceLayoutBoxIdString} class={layoutBoxStyleClass} styles={pagePreviewOverrideStyles}>
                                {renderedBoxContent}
                                    </ol> :
                                    (source.SpecialLayoutBoxType === SpecialLayoutBoxType.ListItem) ?
                                        <li key={sourceLayoutBoxIdString} class={layoutBoxStyleClass} styles={pagePreviewOverrideStyles}>
                                            {renderedBoxContent.length > 0 ? renderedBoxContent : <p>add atoms...</p>}
                                        </li> :
                                        (source.SpecialLayoutBoxType === SpecialLayoutBoxType.RichText) ?
                                            h(richTextTag, {
                                                key: sourceLayoutBoxIdString,
                                                styles: pagePreviewOverrideStyles,
                                                class: layoutBoxStyleClass
                                            }, [renderedBoxContent.length > 0 ? renderedBoxContent : h("p", ["add atoms..."])]) :
                                            <p>TODO Box Type not implemented!</p>;
                    },
                    update: function (updatedSource: LayoutBox) {
                        source = updatedSource;
                        sourceLayoutBoxIdString = source.LayoutBaseId.toString();
                        styleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule; // TODO expensive
                        renderedLayoutAtoms = currentPagePreview.renderLayoutAtomArray(isRenderFixedLayout, styleMolecule.IsPositionFixed || isLayoutBoxInsideFixed);
                        renderedLayoutBoxes = currentPagePreview.renderLayoutBoxArray(isRenderFixedLayout, styleMolecule.IsPositionFixed || isLayoutBoxInsideFixed);
                        // --- TODO code duplication
                        if (styleMolecule.IsPositionFixed) {
                            marginTopOverrideValue = `${styleMolecule.TopCssValuePx}px`;
                            topOverrideValue = styleMolecule.TopCssValuePx !== undefined ? `${currentApp.navigationHeigthPx}px` : undefined; // TODO expensive;
                            marginLeftOverrideValue = `${styleMolecule.LeftCssValuePx}px`;
                        }
                        else {
                            marginTopOverrideValue = undefined;
                            topOverrideValue = undefined;
                            marginLeftOverrideValue = undefined;
                        }
                        // --- TODO code duplication end
                        styleMoleculeId = styleMolecule.StyleMoleculeId; // TODO expensive
                        styleMoleculeIdString = styleMoleculeId.toString();
                        layoutBoxStyleClass = `s${styleMoleculeId}`;
                        richTextTag = "p";
                        if (source.SpecialLayoutBoxType === SpecialLayoutBoxType.RichText && source.PlacedInBoxAtoms.length > 0) {
                            let layoutIdOfFirstAtom: number = source.PlacedInBoxAtoms[0].LayoutBaseId;
                            let styleOfFirstAtom: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutIdOfFirstAtom) as StyleMolecule;// TODO expensive
                            if (styleOfFirstAtom.HtmlTag !== undefined) {
                                richTextTag = styleOfFirstAtom.HtmlTag;
                            }
                            else {
                                console.log(DEFAULT_EXCEPTION); // TODO
                            }
                        }
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutBox, target: { renderMaquette(): any, update(updatedSource: LayoutBox): void }) {
                target.update(updatedSource);
            });
    };

    private renderLayoutAtomArray = (isRenderFixedLayout: boolean, isLayoutAtomInsideFixed: boolean): maquette.Mapping<LayoutAtom, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<LayoutAtom, any>(
            function getSectionSourceKey(source: LayoutAtom) {
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutAtom) {
                let sourceLayoutAtomIdString = source.LayoutBaseId.toString();
                let sourceContentAtomIdString = source.HostedContentAtom.ContentAtomId.toString();
                // --- TODO code duplication // TODO fix max-width for responsive views // TODO fix for bottom/right instead of top/left // TODO fix for isHideUserInterface
                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule; // TODO expensive
                let pagePreviewOverrideStyles: { [key: string]: string | undefined };
                let marginTopOverrideValue: string | undefined;
                let topOverrideValue: string | undefined;
                let marginLeftOverrideValue: string | undefined;
                let backgroundOverrideValue: string | undefined;
                if (styleMolecule.IsPositionFixed) {
                    marginTopOverrideValue = `${styleMolecule.TopCssValuePx}px`;
                    topOverrideValue = styleMolecule.TopCssValuePx !== undefined ? `${currentApp.navigationHeigthPx}px` : undefined; // TODO expensive;
                    marginLeftOverrideValue = `${styleMolecule.LeftCssValuePx}px`;
                }
                else {
                    marginTopOverrideValue = undefined;
                    topOverrideValue = undefined;
                    marginLeftOverrideValue = undefined;
                }
                // --- TODO code duplication end
                let styleMoleculeId: number = styleMolecule.StyleMoleculeId;
                let styleMoleculeIdString: string = styleMoleculeId.toString();
                let layoutAtomStyleClass: string = `s${styleMoleculeIdString}`; // TODO create all of these constant strings when parsing data => one instance in memory !!!
                let hostedContentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.ContentAtomId == source.HostedContentAtom.ContentAtomId) as ContentAtom); // TODO expensive (2 copies of content)
                if (styleMolecule.HtmlTag === undefined) {
                    console.log(DEFAULT_EXCEPTION);
                    return undefined;
                }
                let layoutAtomHtmlTag: string = styleMolecule.HtmlTag !== undefined ? styleMolecule.HtmlTag : "p";
                if (source.PlacedAtomInBox.SpecialLayoutBoxType === SpecialLayoutBoxType.RichText) {
                    layoutAtomHtmlTag = "span";
                }
                return {
                    renderMaquette: function () {
                        // TODO since it is necessary to know which atoms are visible to the user, and this is tracked on scroll etc... it is possible to save visible state here and just perform style calculations when its visible
                        let isHoveredInBoxTree: boolean = currentApp.state.hoveredBoxTreeLayoutBaseId == source.LayoutBaseId;
                        if (isRenderFixedLayout && !isLayoutAtomInsideFixed) {
                            return undefined;
                        }
                        if (styleMolecule.IsPositionFixed) {
                            pagePreviewOverrideStyles = {
                                "margin-top": marginTopOverrideValue,
                                "margin-left": marginLeftOverrideValue,
                                "top": topOverrideValue,
                                "left": undefined,
                                "max-width": undefined,
                                "z-index": undefined,
                                "background-color": undefined
                            };
                        }
                        else {
                            pagePreviewOverrideStyles = {
                                "margin-top": undefined,
                                "margin-left": undefined,
                                "top": undefined,
                                "left": undefined,
                                "max-width": undefined,
                                "z-index": undefined,
                                "background-color": undefined
                            };
                        }
                        pagePreviewOverrideStyles["background-color"] = isHoveredInBoxTree ? HIGHLIGHT_BACKGROUND_COLOR_STRING : undefined;
                        if (source.LayoutBaseId == currentApp.state.highlightedLayoutBaseId) {
                            pagePreviewOverrideStyles["outline"] = "solid 1px black";
                            pagePreviewOverrideStyles["outline-offset"] = "-1px";
                            pagePreviewOverrideStyles["background-color"] = MANUALLY_HIGHLIGHT_BACKGROUND_COLOR_STRING;
                        }
                        else {
                            pagePreviewOverrideStyles["outline"] = undefined;
                            pagePreviewOverrideStyles["outline-offset"] = undefined;
                        }
                        if (styleMolecule.IsPositionFixed) {
                            pagePreviewOverrideStyles["max-width"] = styleMolecule.IsPositionFixed && currentApp.state.isDataLoaded && currentApp.state.isEnoughAvailableSpacePagePreview && currentApp.state.editViewMode !== EditViewMode.PagePreviewOnly ? `${currentApp.state.targetPagePreviewWidthPx}px` : undefined; // TODO expensive TODO duplication
                            pagePreviewOverrideStyles["left"] = styleMolecule.IsPositionFixed && styleMolecule.LeftCssValuePx !== undefined ? `${parseInt(styleMolecule.LeftCssValuePx) + currentApp.state.targetPagePreviewHolderMarginPx}px` : undefined; // TODO expensive // TODO dangerous // TODO magic string
                        }
                        let isEditedLayoutAtomId: boolean = source.LayoutBaseId == currentPagePreview.viewModel.editedLayoutAtomId;
                        pagePreviewOverrideStyles["z-index"] = isEditedLayoutAtomId ? "30" : undefined; // TODO document
                        
                        if (isEditedLayoutAtomId) {
                            // show input field instead of rendered atom
                            return <textarea key={`i${sourceLayoutAtomIdString}`}
                                class={layoutAtomStyleClass}
                                value={currentPagePreview.viewModel.tempContent}
                                oninput={currentPagePreview.contentAtomInputHandler}
                                onblur={currentPagePreview.contentAtomLostFocusHandler}
                                onkeydown={currentPagePreview.contentAtomKeyDownHandler}
                                afterCreate={currentPagePreview.contentAtomAfterCreateHandler}
                                afterUpdate={currentPagePreview.contentAtomAfterUpdateHandler}
                                exitAnimation={currentPagePreview.contentAtomExitAnimationHandler}
                                cid={sourceContentAtomIdString}
                                styles={pagePreviewOverrideStyles}></textarea> as VNode;
                        }
                        if (hostedContentAtom.ContentAtomType === ContentAtomType.Text) {
                            return h(layoutAtomHtmlTag, {
                                key: sourceLayoutAtomIdString,
                                class: layoutAtomStyleClass,
                                onclick: currentPagePreview.layoutAtomClickHandler,
                                ondblclick: currentPagePreview.layoutAtomDblClickHandler,
                                // custom html fields
                                aid: sourceLayoutAtomIdString,
                                cid: sourceContentAtomIdString,
                                styles: pagePreviewOverrideStyles,
                                afterCreate: currentPagePreview.layoutAtomAfterCreateHandler,
                                afterUpdate: currentPagePreview.layoutAtomAfterUpdateHandler,
                                exitAnimation: currentPagePreview.layoutAtomExitAnimationHandler,
                                onmouseenter: currentPagePreview.layoutAtomMouseEnterHandler,
                                onmouseleave: currentPagePreview.layoutAtomMouseLeaveHandler
                            }, [hostedContentAtom.TextContent]) // TODO no unset value for delete "animation" = set content to null then delete element
                        } // TODO this could be prepared on server in one "render/display content field"
                        else if (hostedContentAtom.ContentAtomType === ContentAtomType.Link) {
                            return h(layoutAtomHtmlTag, {
                                key: sourceLayoutAtomIdString,
                                class: layoutAtomStyleClass,
                                onclick: currentPagePreview.layoutAtomClickHandler,
                                ondblclick: currentPagePreview.layoutAtomDblClickHandler, // TODO open link in new tab
                                // custom html fields
                                aid: sourceLayoutAtomIdString,
                                cid: sourceContentAtomIdString,
                                href: "", // need to set to trigger browser url rendering
                                styles: pagePreviewOverrideStyles,
                                onmouseenter: currentPagePreview.layoutAtomMouseEnterHandler,
                                onmouseleave: currentPagePreview.layoutAtomMouseLeaveHandler
                            }, [hostedContentAtom.Url]) // TODO no unset value for delete "animation" = set content to null then delete element
                        }
                        else {
                            console.log(DEFAULT_EXCEPTION);
                            return undefined;
                        }
                    },
                    update: function (updatedSource: LayoutAtom) {
                        source = updatedSource;
                        sourceLayoutAtomIdString = source.LayoutBaseId.toString();
                        sourceContentAtomIdString = source.HostedContentAtom.ContentAtomId.toString();
                        styleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule; // TODO expensive
                        // --- TODO code duplication
                        if (styleMolecule.IsPositionFixed) {
                            marginTopOverrideValue = `${styleMolecule.TopCssValuePx}px`;
                            topOverrideValue = styleMolecule.TopCssValuePx !== undefined ? `${currentApp.navigationHeigthPx}px` : undefined; // TODO expensive;
                            marginLeftOverrideValue = `${styleMolecule.LeftCssValuePx}px`;
                        }
                        else {
                            marginTopOverrideValue = undefined;
                            topOverrideValue = undefined;
                            marginLeftOverrideValue = undefined;
                        }
                        // --- TODO code duplication end
                        styleMoleculeId = styleMolecule.StyleMoleculeId;
                        styleMoleculeIdString = styleMoleculeId.toString();
                        layoutAtomStyleClass = `s${styleMoleculeId}`;
                        hostedContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.ContentAtomId == source.HostedContentAtom.ContentAtomId) as ContentAtom); // TODO expensive (2 copies of content)
                        if (styleMolecule.HtmlTag === undefined) {
                            console.log(DEFAULT_EXCEPTION);
                        }
                        layoutAtomHtmlTag = styleMolecule.HtmlTag !== undefined ? styleMolecule.HtmlTag : "p";
                        if (source.PlacedAtomInBox.SpecialLayoutBoxType === SpecialLayoutBoxType.RichText) {
                            layoutAtomHtmlTag = "span";
                        }
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutAtom, target: { renderMaquette(): any, update(updatedSource: LayoutAtom): void }) {
                target.update(updatedSource);
            });
    };

    private resetContentAtomEditMode = () => {
        currentPagePreview.resetEquationNumbersWhenModifying(false);
        currentPagePreview.viewModel.editedLayoutAtomId = 0;
        currentPagePreview.viewModel.tempContent = "";
        currentPagePreview.viewModel.tempOriginalContent = "";
        currentPagePreview.viewModel.stylesOfEditedContent = {};
    };

    private contentAtomLostFocusHandler = (evt: FocusEvent) => {
        currentPagePreview.updateContentAtom(parseIntFromAttribute(evt.target, "cid"));
    };

    private updateContentAtom = (contentAtomId: number) => {
        if (currentPagePreview.viewModel.editedLayoutAtomId != 0) {
            if (currentPagePreview.viewModel.tempContent !== currentPagePreview.viewModel.tempOriginalContent) {
                let contentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(a => a.InstancedOnLayoutId == currentPagePreview.viewModel.editedLayoutAtomId) as ContentAtom); // TODO expensive but feels better
                if (contentAtom.ContentAtomType === ContentAtomType.Text) {
                    contentAtom.TextContent = currentPagePreview.viewModel.tempContent;
                }
                else if (contentAtom.ContentAtomType === ContentAtomType.Link) {
                    contentAtom.Url = currentPagePreview.viewModel.tempContent;
                }
                else {
                    console.log(DEFAULT_EXCEPTION);
                    return;
                }
                if (currentPagePreview.viewModel.tempContent === "") {
                    // TODO this code path should be possible wherever content is updated (?) (i.e. chained calls with insert atom)
                    currentApp.controller.DeleteLayoutJson(currentPagePreview.viewModel.editedLayoutAtomId, false).done((data: any) => currentApp.router.updateData(data));
                    // TODO focus previous layout atom (in same box? or all boxes and previous sortorder?)
                }
                else {
                    currentApp.state.currentReadyState = ReadyState.Pending;
                    currentApp.controller.UpdateTextContentAtomJson(contentAtomId, currentPagePreview.viewModel.tempContent).done((data: any) => {
                        currentApp.router.updateData(data);
                        currentPagePreview.resetEquationNumbersWhenModifying(false);
                    }).always((data: any) => currentApp.state.currentReadyState = ReadyState.Ok);
                }
            }
        }
        currentPagePreview.resetContentAtomEditMode();
    };

    private contentAtomKeyDownHandler = (evt: KeyboardEvent) => { // TODO code duplication
        if (evt.keyCode == 13 /*ENTER*/) {
            evt.preventDefault();
            if (evt.shiftKey === true) {
                currentPagePreview.contentAtomCreateNewLine(parseIntFromAttribute(evt.target, "cid")); // TODO multiple requests when data has changed => combined request chain; currently relies on function order
            }
            (evt.target as HTMLTextAreaElement).blur();
        }
        else if (evt.keyCode == 27 /*ESC*/) {
            evt.preventDefault();
            currentPagePreview.resetContentAtomEditMode();
            (evt.target as HTMLTextAreaElement).blur();
        }
        else if (evt.keyCode == undefined /*text area focus lost*/) {
            evt.preventDefault();
        }
        // TODO clean whitespaces
        // TODO autosize
    };

    private contentAtomCreateNewLine = (contentAtomId: number): void => {
        // TODO target atom type could be default type for box type (none => text, list => list item, ...) but does not work for headings
        // TODO target atom type could be that of current edited layout atom
        if (currentPagePreview.viewModel.editedLayoutAtomId != 0) {
            let editedLayoutAtom: LayoutAtom = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == currentPagePreview.viewModel.editedLayoutAtomId) as LayoutAtom; // TODO expensive
            let editedLayoutAtomIdChainedCall: number = currentPagePreview.viewModel.editedLayoutAtomId;
            let placedAtomInBoxIdChainedCall: number = editedLayoutAtom.PlacedAtomInBoxId;
            if (currentPagePreview.viewModel.tempContent !== currentPagePreview.viewModel.tempOriginalContent) {
                currentApp.state.currentReadyState = ReadyState.Pending;
                currentApp.controller.UpdateTextContentAtomJson(contentAtomId, currentPagePreview.viewModel.tempContent).done((data: any) => {
                    currentPagePreview.chainedAddMoveLayoutAtomCallWithFullUpdate(placedAtomInBoxIdChainedCall, editedLayoutAtomIdChainedCall);    
                }).always((data: any) => currentApp.state.currentReadyState = ReadyState.Ok);
                currentPagePreview.resetContentAtomEditMode();
            }
            else {
                currentPagePreview.chainedAddMoveLayoutAtomCallWithFullUpdate(placedAtomInBoxIdChainedCall, editedLayoutAtomIdChainedCall);
            }
        }
    }

    private chainedAddMoveLayoutAtomCallWithFullUpdate = (placedAtomInBoxIdChainedCall: number, editedLayoutAtomIdChainedCall: number): void => {
        // TODO should be one big backend operation or add trigger to backend to save intermediate data transfer
        currentApp.controller.CreateLayoutAtomForBoxJson(placedAtomInBoxIdChainedCall, editedLayoutAtomIdChainedCall).done((dataSub: CaliforniaClientViewModel) => {
            let subBoxAtoms: LayoutBase[] = dataSub.CaliforniaProject.LayoutMolecules.filter(m => m.LayoutType === LayoutType.Atom && (m as LayoutAtom).PlacedAtomInBoxId == placedAtomInBoxIdChainedCall);
            let createdLayoutMoleculeId: number = subBoxAtoms[subBoxAtoms.length - 1].LayoutBaseId; // TODO wild guess: last entry in list is the created atom
            currentApp.controller.MoveLayoutMoleculeNextToLayoutMoleculeJson(createdLayoutMoleculeId, editedLayoutAtomIdChainedCall, true).done((dataSubSub: any) => {
                currentApp.controller.MoveLayoutMoleculeNextToLayoutMoleculeJson(editedLayoutAtomIdChainedCall, createdLayoutMoleculeId, true).done((dataSubSubSub: any) => {
                    currentApp.router.updateData(dataSubSubSub);
                    let hostedContentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.InstancedOnLayoutId == createdLayoutMoleculeId) as ContentAtom); // TODO expensive (2 copies of content)
                    currentPagePreview.viewModel.tempContent = "";
                    currentPagePreview.viewModel.tempOriginalContent = "";
                    if (hostedContentAtom.ContentAtomType === ContentAtomType.Text) { // TODO code duplication for content selection at multiple places
                        currentPagePreview.viewModel.tempOriginalContent = hostedContentAtom.TextContent as string; // TODO default text should be empty when creating a new element with shortcut (pressing escape leaves element with default text instead of removing it again)
                    }
                    else if (hostedContentAtom.ContentAtomType === ContentAtomType.Link) {
                        currentPagePreview.viewModel.tempOriginalContent = hostedContentAtom.Url as string;
                    }
                    else {
                        console.log(DEFAULT_EXCEPTION);
                        return;
                    }
                    currentPagePreview.viewModel.editedLayoutAtomId = createdLayoutMoleculeId;
                });
            });
        });
    };

    private contentAtomAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        let targetElement: HTMLTextAreaElement = element as HTMLTextAreaElement;
        $(targetElement).css(currentPagePreview.viewModel.stylesOfEditedContent);
        if (currentApp.state.isSelectAllTextArea === true) { // TODO does not work with click/dblclick differentiation because click always fires before dblclick => use timeout or differentiate preselection wish with ui state
            targetElement.setSelectionRange(0, currentPagePreview.viewModel.tempContent.length);
        }
        targetElement.focus(); //TODO does not work with text area (on iphone) / TODO try renderNow
    };

    private contentAtomAfterUpdateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        // do nothing
    };

    private contentAtomExitAnimationHandler = (element: Element, removeElement: () => void, properties?: VNodeProperties): void => {
        removeElement();
    };
    
    private layoutAtomAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        if (currentApp.state.isJaxOn) {
            let targetElement: HTMLElement = element as HTMLElement;
            currentPagePreview._activeViewLayoutAtomDomNodeReferences[properties.key as string] = targetElement;
            if (targetElement.innerText.indexOf("$") != -1) {
                MathJax.Hub.Queue(["Typeset", MathJax.Hub, targetElement]); // TODO use Hub.Queue.Suspend
            }
        }
    };

    private layoutAtomExitAnimationHandler = (element: Element, removeElement: () => void, properties: VNodeProperties): void => {
        delete currentPagePreview._activeViewLayoutAtomDomNodeReferences[properties.key as string];
        removeElement();
    };

    private layoutAtomMouseEnterHandler = (evt: MouseEvent) => {
        let targetElement: HTMLElement = evt.target as HTMLInputElement;
        currentApp.state.hoveredPagePreviewLayoutBaseId = parseIntFromAttribute(targetElement, "aid");
    };

    private layoutAtomMouseLeaveHandler = (evt: MouseEvent) => {
        currentApp.state.hoveredPagePreviewLayoutBaseId = 0;
    };
    
    private layoutAtomAfterUpdateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        if (currentApp.state.isJaxOn) {
            let targetElement: HTMLElement = element as HTMLElement;
            var math: any = MathJax.Hub.getAllJax(targetElement)[0];
            if (math !== undefined) {
                /*let layoutAtomIdString: string | undefined = properties["aid"];
                if (layoutAtomIdString !== undefined) {
                    // TODO enable updater + renumber: disabled, because need to extract math from text content, else double content (once normal render, once as math)
                    let layoutAtomId: number = parseInt(layoutAtomIdString);
                    for (let i = 0; i < currentApp.propertyBarCount; i++) {
                        let targetVM: PropertyBarVM = currentApp.propertyBars[i].viewModel;
                        if (targetVM.editedLayoutAtomId == layoutAtomId) {
                            MathJax.Hub.Queue(["Text", math, targetVM.tempContent]);
                        }
                    }
                    currentPagePreview.resetEquationNumbersWhenModifying(false);
                }
                else {
                    console.log(DEFAULT_EXCEPTION);
                }*/
            }
            else if (targetElement.innerText.indexOf("$") != -1) {
                // TODO why is the formula destroyed when the rendered content changes => trigger formula update instead
                MathJax.Hub.Queue(["Typeset", MathJax.Hub, targetElement]);
                currentPagePreview.resetEquationNumbersWhenModifying(false);
            }
        // TODO also supports rerender (only when css changes)
        }
    };

    private contentAtomInputHandler = (evt: KeyboardEvent) => {
        currentPagePreview.viewModel.tempContent = (evt.target as HTMLTextAreaElement).value;
    };

    private layoutAtomDblClickHandler = (evt: MouseEvent) => {
        evt.preventDefault(); // TODO this is set for every content atom in preview... performance?
        if (currentApp.state.currentReadyState !== ReadyState.Ok) {
            console.log("pending...");
            return;
        }
        if (currentApp.state.currentSelectionMode === SelectionMode.Content) {
            let targetElement: HTMLElement = evt.currentTarget as HTMLElement;  // had to be currentTarget after innerHtml was manipulated using mathjax // TODO code duplication
            currentPagePreview.viewModel.stylesOfEditedContent = currentPagePreview.getStyleObject(targetElement); // TODO does this have to be everywhere i.e. animations?
            let contentId: number = parseIntFromAttribute(targetElement, "cid");
            let layoutId: number = parseIntFromAttribute(targetElement, "aid");
            let targetWidth: number = targetElement.clientWidth;
            let targetHeight: number = targetElement.clientHeight;
            let maxIterations: number = 10;
            let currentIteration: number = 0;
            while (currentIteration++ < maxIterations) {
                if (targetWidth == 0 || targetHeight == 0) { // happens for <span> inside rich text
                    if (targetElement.parentElement !== null) { // TODO recursive while loop max 10 iterations
                        targetWidth = targetElement.parentElement.clientWidth;
                        targetHeight = targetElement.parentElement.clientHeight;
                        targetElement = targetElement.parentElement;
                    }
                }
            }
            currentPagePreview.layoutAtomToTextAreaSetup(contentId, layoutId, true, targetWidth, targetHeight);
        }
        else {
            // TODO
        }
    };

    private layoutAtomClickHandler = (evt: MouseEvent) => {
        evt.preventDefault(); // TODO this is set for every content atom in preview... performance?
        if (currentApp.state.currentReadyState !== ReadyState.Ok) {
            console.log("pending...");
            return;
        }
        if (currentApp.state.currentSelectionMode === SelectionMode.Content) {
            let targetElement: HTMLElement = evt.currentTarget as HTMLElement;  // had to be currentTarget after innerHtml was manipulated using mathjax // TODO code duplication
            currentPagePreview.viewModel.stylesOfEditedContent = currentPagePreview.getStyleObject(targetElement); // TODO does this have to be everywhere i.e. animations?
            let contentId: number = parseIntFromAttribute(targetElement, "cid");
            let layoutId: number = parseIntFromAttribute(targetElement, "aid");
            let targetWidth: number = targetElement.clientWidth;
            let targetHeight: number = targetElement.clientHeight;
            let maxIterations: number = 10;
            let currentIteration: number = 0;
            while (currentIteration++ < maxIterations) {
                if (targetWidth == 0 || targetHeight == 0) { // happens for <span> inside rich text // TODO read padding or offset of first word in a rich text and insert fake whitespace in the input field
                    // because the input field is rectangular and then rendered completely below the preceding text element
                    if (targetElement.parentElement !== null) { // TODO recursive while loop max 10 iterations
                        targetWidth = targetElement.parentElement.clientWidth;
                        targetHeight = targetElement.parentElement.clientHeight;
                        targetElement = targetElement.parentElement;
                    }
                }
            }
            currentPagePreview.layoutAtomToTextAreaSetup(contentId, layoutId, true, targetWidth, targetHeight);
        }
        else {
            // TODO
        }
    };

    private layoutAtomToTextAreaSetup = (contentAtomId: number, layoutAtomId: number, isPreSelectAll: boolean, targetWidthPx: number, targetHeightPx: number): void => {
        // TODO get position, height + width and place as absolute above edited layout (no replacement element)
        currentApp.state.isSelectAllTextArea = isPreSelectAll;
        currentPagePreview.viewModel.stylesOfEditedContent["outline"] = "solid 1px rgb(200,200,200)"; // text area can't focus directly on iPhone => show intermediate state TODO try renderNow()
        currentPagePreview.viewModel.stylesOfEditedContent["outline-offset"] = "-1px";
        // keep space for rendered fonts the same (add space for scrollbar of textarea)
        if (currentPagePreview.viewModel.stylesOfEditedContent["width"] === undefined || currentPagePreview.viewModel.stylesOfEditedContent["height"] === undefined) {
            // always expected to be already set by browser or css
            console.log(DEFAULT_EXCEPTION);
        }
        else {
            let targetWidthString: string = (targetWidthPx == 0 ? 300 : targetWidthPx).toString() + "px";
            let targetHeightString: string = (targetHeightPx == 0 ? 300 : targetHeightPx).toString() + "px";
            currentPagePreview.viewModel.stylesOfEditedContent["width"] = targetWidthString;
            currentPagePreview.viewModel.stylesOfEditedContent["min-width"] = targetWidthString;
            currentPagePreview.viewModel.stylesOfEditedContent["height"] = targetHeightString;
            currentPagePreview.viewModel.stylesOfEditedContent["min-height"] = targetHeightString;
        }
        let hostedContentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.ContentAtomId == contentAtomId) as ContentAtom); // TODO expensive (2 copies of content)
        currentPagePreview.viewModel.tempContent = "";
        if (hostedContentAtom.ContentAtomType === ContentAtomType.Text) { // TODO code duplication for content selection at multiple places
            currentPagePreview.viewModel.tempContent = hostedContentAtom.TextContent as string;
        }
        else if (hostedContentAtom.ContentAtomType === ContentAtomType.Link) {
            currentPagePreview.viewModel.tempContent = hostedContentAtom.Url as string;
        }
        else {
            console.log(DEFAULT_EXCEPTION);
            return;
        }
        currentPagePreview.viewModel.tempOriginalContent = currentPagePreview.viewModel.tempContent;
        currentPagePreview.viewModel.editedLayoutAtomId = layoutAtomId;
    };
    
    private getStyleObject = (targetElement: HTMLElement): { [key: string]: string } => {
        var dom = $(targetElement).get(0);
        var style;
        var returns: { [key: string]: string } = {};
        //if (window.getComputedStyle) {
        var camelize = function (a: string, b: string) {
            return b.toUpperCase();
        };
        style = window.getComputedStyle(dom, undefined);
        for (var i = 0, l = style.length; i < l; i++) {
            var prop = style[i];
            var camel = prop.replace(/\-([a-z])/g, camelize);
            var val = style.getPropertyValue(prop);
            returns[camel] = val;
        };
        return returns;
        //};
        /*if (style = dom.currentStyle) {
            for (var prop in style) {
                returns[prop] = style[prop];
            };
            return returns;
        };*/
        //return $(targetElement).css();
    };

    public mapAndRenderLayoutBoxContent = (refLayoutBox: LayoutBox, // TODO mark as static / no side effects
        unsortedAtoms: LayoutAtom[], atomMapping: maquette.Mapping<LayoutAtom, { renderMaquette: () => maquette.VNode }>,
        unsortedBoxes: LayoutBox[], boxMapping: maquette.Mapping<LayoutBox, { renderMaquette: () => maquette.VNode }>): VNode[] => {
        if (unsortedAtoms.length == 0 && unsortedBoxes.length == 0) {
            return []; // [<p key="0">Add atoms...</p> as VNode]; TODO
        }

        let sortedAtoms: LayoutAtom[] = unsortedAtoms.sort((atomA: LayoutAtom, atomB: LayoutAtom) => {
            if (atomA.LayoutSortOrderKey < atomB.LayoutSortOrderKey) {
                return -1;
            }
            else if (atomA.LayoutSortOrderKey == atomB.LayoutSortOrderKey) {
                return 0;
            }
            else { // atomA.LayoutSortOrderKey > atomB.LayoutSortOrderKey
                return 1;
            }
        });
        let sortedBoxes: LayoutBox[] = unsortedBoxes.sort((boxA: LayoutBox, boxB: LayoutBox) => {
            if (boxA.LayoutSortOrderKey < boxB.LayoutSortOrderKey) {
                return -1;
            }
            else if (boxA.LayoutSortOrderKey == boxB.LayoutSortOrderKey) {
                return 0;
            }
            else { // boxA.LayoutSortOrderKey > boxB.LayoutSortOrderKey
                return 1;
            }
        });
        atomMapping.map(sortedAtoms);
        boxMapping.map(sortedBoxes);
        // expects atoms and boxes sorted by LayoutSortOrderKey and mappings in the same order
        let renderedAtomsAndBoxes: VNode[] = [];
        let atomIndex: number = 0;
        let boxIndex: number = 0;
        let atomsLength: number = sortedAtoms.length;
        let boxesLength: number = sortedBoxes.length;
        let totalItems: number = atomsLength + boxesLength;
        for (let i = 0; i < totalItems; i++) {
            let currentAtom: LayoutAtom | undefined = undefined;
            let currentBox: LayoutBox | undefined = undefined;
            if (atomIndex < atomsLength) {
                currentAtom = sortedAtoms[atomIndex];
            }
            if (boxIndex < boxesLength) {
                currentBox = sortedBoxes[boxIndex];
            }
            if (currentAtom !== undefined && currentBox !== undefined) {
                if (currentAtom.LayoutSortOrderKey < currentBox.LayoutSortOrderKey) {
                    renderedAtomsAndBoxes.push(atomMapping.results[atomIndex++].renderMaquette());
                }
                else {
                    renderedAtomsAndBoxes.push(boxMapping.results[boxIndex++].renderMaquette());
                }
            }
            else if (currentAtom !== undefined) {
                let remainingAtoms: number = atomsLength - atomIndex;
                for (let j = 0; j < remainingAtoms; j++) {
                    renderedAtomsAndBoxes.push(atomMapping.results[atomIndex++].renderMaquette());
                }
                break;
            }
            else {// if (currentBox !== undefined) {
                let remainingBoxes: number = boxesLength - boxIndex;
                for (let j = 0; j < remainingBoxes; j++) {
                    renderedAtomsAndBoxes.push(boxMapping.results[boxIndex++].renderMaquette());
                }
                break;
            }
        }
        return renderedAtomsAndBoxes;
    };

    public getCssRuleOf = (styleMolecule: StyleMolecule, responsiveDevice: ResponsiveDevice, stateModifier: string | undefined): string => {
        //let responsiveSetting: ReactorResponsiveSetting = currentApp.clientData.UserResponsiveSettings[responsiveId];
        let selector: string = `.s${styleMolecule.StyleMoleculeId}`;
        if ((stateModifier === undefined) || (stateModifier === "")) {
            let styleRule: string = `${selector}{`;
            // element style is target
            for (let styleAtomMapping of styleMolecule.MappedStyleAtoms.filter(styleAtomMap => styleAtomMap.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && (styleAtomMap.StateModifier === undefined || styleAtomMap.StateModifier === ""))) {
                let styleAtomId: number = (currentApp.clientData.CaliforniaProject.StyleAtoms.find(a => a.MappedToMoleculeId == styleAtomMapping.StyleMoleculeAtomMappingId) as StyleAtom).StyleAtomId; // TODO slow
                let appliedValues: StyleValue[] = currentApp.clientData.CaliforniaProject.StyleValues.filter(v => v.StyleAtomId == styleAtomId); // TODO slow
                for (let cssProp of appliedValues) {
                    if (cssProp.CssValue !== "") {
                        styleRule += `${cssProp.CssProperty}: ${cssProp.CssValue};`;
                    }
                }
            }
            styleRule += "}";
            return currentPagePreview.wrapCssMediaQuery(styleRule, responsiveDevice);
        }
        else {
            // pseudo style is target
            // create a css declaration for target pseudo style
            let pseudoStyleRule: string = `${selector}${stateModifier}{`;
            for (let pseudoStyleAtomMapping of styleMolecule.MappedStyleAtoms.filter(styleAtomMap => styleAtomMap.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && styleAtomMap.StateModifier === stateModifier)) {
                let styleAtomId: number = (currentApp.clientData.CaliforniaProject.StyleAtoms.find(a => a.MappedToMoleculeId == pseudoStyleAtomMapping.StyleMoleculeAtomMappingId) as StyleAtom).StyleAtomId; // TODO slow
                let appliedValues: StyleValue[] = currentApp.clientData.CaliforniaProject.StyleValues.filter(v => v.StyleAtomId == styleAtomId); // TODO slow
                for (let cssProp of appliedValues) {
                    if (cssProp.CssValue !== "") {
                        pseudoStyleRule += `${cssProp.CssProperty}: ${cssProp.CssValue};`;
                    }
                }
            }
            pseudoStyleRule += "}";
            return currentPagePreview.wrapCssMediaQuery(pseudoStyleRule, responsiveDevice);
        }
    };

    private wrapCssMediaQuery = (styleRule: string, responsiveDevice: ResponsiveDevice): string => {
        // css for default + smallest device (modIndex 0, 1) are "classic" css rules
        if (responsiveDevice.WidthThreshold !== undefined && responsiveDevice.WidthThreshold > 0) {
            // medium device + ... + largest device "media" css rules
            return `@media(min-width:${currentPagePreview.dynamicClientGridBreakpoints[currentApp.clientData.CaliforniaProject.ResponsiveDevices.indexOf(responsiveDevice)]}px){${styleRule}}`;
        }
        else {
            // dont wrap
            return styleRule;
        }
    };

    public appendStyleRulesFor = (styleMolecule: StyleMolecule, styleSheet: CSSStyleSheet): void => {
        let ruleIndex: number;
        let styleRule: string;
        for (let i = 0; i < currentApp.clientData.CaliforniaProject.ResponsiveDevices.length; i++) {
            let responsiveDevice = currentApp.clientData.CaliforniaProject.ResponsiveDevices[i];
            ruleIndex = styleSheet.cssRules.length;
            styleRule = currentPagePreview.getCssRuleOf(styleMolecule, responsiveDevice, undefined);
            styleSheet.insertRule(styleRule, ruleIndex);
            currentPagePreview.virtualStyleIndex[i][styleMolecule.StyleMoleculeId] = ruleIndex;

            let stateModifiers: string[] = [];
            for (let i = 0; i < styleMolecule.MappedStyleAtoms.length; i++) {
                let styleAtomMap = styleMolecule.MappedStyleAtoms[i];
                if (styleAtomMap.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && styleAtomMap.StateModifier !== undefined)
                if (stateModifiers.findIndex(s => s === styleAtomMap.StateModifier) == -1) {
                    stateModifiers.push(styleAtomMap.StateModifier);
                }
            }
            for (let stateModifier of stateModifiers) {
                //if (stateModifier === ":hover") { // TODO must whitelist acceptable selectors or browser throws exception
                ruleIndex = styleSheet.cssRules.length;
                styleRule = currentPagePreview.getCssRuleOf(styleMolecule, responsiveDevice, stateModifier);
                styleSheet.insertRule(styleRule, ruleIndex);
                currentPagePreview.virtualPseudoStyleIndex[i][`${styleMolecule.StyleMoleculeId}${stateModifier}`] = ruleIndex;
            }
        }
    };

    public reloadCssStyles = (): void => {
        for (let i = currentApp.styleSheet.cssRules.length; i > 0; i--) {
            currentApp.styleSheet.deleteRule(i - 1);
        }
        for (let i = 0; i < currentApp.clientData.CaliforniaProject.StyleMolecules.length; i++) {
            let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules[i];
            currentPagePreview.appendStyleRulesFor(styleMolecule, currentApp.styleSheet);
        }
    };

    public updatePagePreviewDimensions = (): void => {
        // calculate page preview holder target width for overridden responsive device
        let staticMargin: number = currentApp.state.defaultSymmetricPagePreviewHolderMarginPx;
        let targetWidthPx: number = 0;
        if (currentApp.state.overrideResponsiveDeviceId == 0) {
            // default: use available space to fill viewport horizontally
            currentApp.state.targetPagePreviewHolderMarginPx = staticMargin;
            targetWidthPx = currentApp.state.availableSpacePagePreviewPx - 2 * staticMargin;
            currentApp.state.targetPagePreviewWidthPx = targetWidthPx;
            currentApp.state.isEnoughAvailableSpacePagePreview = true;
        }
        else {
            // specific preview size was set by user => modify target width for overridden responsive device
            let overrideWithResponsiveDevice: ResponsiveDevice = currentApp.clientData.CaliforniaProject.ResponsiveDevices.find(r => r.ResponsiveDeviceId == currentApp.state.overrideResponsiveDeviceId) as ResponsiveDevice;
            if (currentApp.state.overrideResponsiveDeviceId == currentApp.state.highestWidthThresholdResponsiveDeviceId) {
                // add 1px to highest responsive setting width threshold, such that targetWidth is higher than highest value
                targetWidthPx = overrideWithResponsiveDevice.WidthThreshold + 1;
            }
            else {
                let targetResponsiveSettingAbove: ResponsiveDevice = currentApp.clientData.CaliforniaProject.ResponsiveDevices[currentApp.clientData.CaliforniaProject.ResponsiveDevices.indexOf(overrideWithResponsiveDevice) + 1]; // relies on sorted devices
                targetWidthPx = targetResponsiveSettingAbove.WidthThreshold - 1;
            }
            if ((currentApp.state.availableSpacePagePreviewPx - 2 * staticMargin) >= targetWidthPx) {
                let remainingSpacePx: number = ((currentApp.state.availableSpacePagePreviewPx - 2 * staticMargin) - targetWidthPx);
                currentApp.state.targetPagePreviewHolderMarginPx = staticMargin + (remainingSpacePx / 2); // TODO do we need floor? 1px error TODO also browser displays width pixels with fraction
                currentApp.state.targetPagePreviewWidthPx = currentApp.state.availableSpacePagePreviewPx - remainingSpacePx;
                currentApp.state.isEnoughAvailableSpacePagePreview = true;
                
            }
            else {
                currentApp.state.isEnoughAvailableSpacePagePreview = false;
                currentApp.state.targetPagePreviewHolderMarginPx = staticMargin;
                currentApp.state.targetPagePreviewWidthPx = targetWidthPx;
            }
        }
        if (currentApp.state.isDataLoaded === true) {
            let currentResponsiveDeviceIndex = currentApp.clientData.CaliforniaProject.ResponsiveDevices.findIndex(r => targetWidthPx < r.WidthThreshold); // depends on sort order
            if (currentResponsiveDeviceIndex == 0) {
                // targetWidthPx == 0
                console.log(DEFAULT_EXCEPTION);
            }
            else if (currentResponsiveDeviceIndex == -1) {
                // not found => larger than highest
                currentApp.state.currentResponsiveDeviceId = currentApp.state.highestWidthThresholdResponsiveDeviceId;
            }
            else {
                currentApp.state.currentResponsiveDeviceId = currentApp.clientData.CaliforniaProject.ResponsiveDevices[currentResponsiveDeviceIndex - 1].ResponsiveDeviceId;
            }
        }

        // adjust media query min-widths of page preview styles, such that page preview shows responsive device view; independent of client width and UI width
        // css media query rendering order of browsers is mobile first (no min-width, min-width_1 < min-width_2 < ...)
        // TODO create dynamic client breakpoints such that a browser resize to smaller view does not trigger smaller breakpoints before recalculating ui size + client breakpoints
        currentPagePreview.dynamicClientGridBreakpoints = [];
        let californiaAppWidth: number | undefined = $(window).width(); // TODO use inner or outer width
        if (californiaAppWidth === undefined) {
            console.log(DEFAULT_EXCEPTION);
            return;
        }
        let californiaUIWidth = californiaAppWidth - currentApp.state.availableSpacePagePreviewPx;
        for (let i = 0; i < currentApp.clientData.CaliforniaProject.ResponsiveDevices.length; i++) {
            let responsiveDevice: ResponsiveDevice = currentApp.clientData.CaliforniaProject.ResponsiveDevices[i];
            if (responsiveDevice.WidthThreshold < 0) {
                // Default + xs are default css (no media query) and always applied (order matters, xs css overwrites default css)
                // Default/None == -1; smallest device == 0
                currentPagePreview.dynamicClientGridBreakpoints.push(0);
            }
            else if (responsiveDevice.WidthThreshold == 0) {
                currentPagePreview.dynamicClientGridBreakpoints.push(0);
            }
            else {
                // responsive devices get an adjusted value for WidthThreshold: lower than current client width for WidthThreshold < currentResponsiveDevice.WidthThreshold; higher than client width for WidthThreshold > currentResponsiveDevice.WidthThreshold; must be strictly monotonically increasing
                let currentDeviceIndex = currentApp.clientData.CaliforniaProject.ResponsiveDevices.findIndex(r => r.ResponsiveDeviceId == currentApp.state.currentResponsiveDeviceId);
                let safetyMarginPx: number = 50; // scroll bar
                if (i <= currentDeviceIndex) {
                    let adjustedBreakPoint = californiaAppWidth - safetyMarginPx - ((currentDeviceIndex + 1 - i));
                    currentPagePreview.dynamicClientGridBreakpoints.push(adjustedBreakPoint);
                }
                else {
                    let adjustedBreakPoint = californiaAppWidth + safetyMarginPx + (i - currentDeviceIndex);
                    currentPagePreview.dynamicClientGridBreakpoints.push(adjustedBreakPoint);
                }
            }
        }
        currentPagePreview.reloadCssStyles();
    };
}