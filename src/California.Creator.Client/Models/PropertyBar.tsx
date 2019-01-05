/// <reference path="../jsx.ts" />
"use strict";
import { VNode, VNodeProperties } from "maquette";
declare var require: any;
var velocity: any = require("velocity-animate");
import * as maquette from "maquette";
const h = maquette.h;
import { CaliforniaApp, DEFAULT_EXCEPTION, getArrayForEnum, parseIntFromAttribute, parseStringFromAttribute } from "./CaliforniaApp";
import { PropertyBarMode, PropertyBarVM, PopupMode, PopupSecondaryMode, TransactionMode } from "./../ViewModels/PropertyBarVM";
import { StyleQuantum, StyleAtom, StyleValue, StyleAtomType, StyleMolecule, ResponsiveDevice, StyleMoleculeAtomMapping, LayoutBase, LayoutRow, LayoutBox, LayoutAtom, LayoutType, CaliforniaView, SpecialLayoutBoxType, ContentAtom, CaliforniaEvent } from "./CaliforniaGenerated";
import { EditViewMode, ReadyState, SelectionMode } from "./ClientState";
import * as popperjs from "popper.js";
import { ContentAtomType } from "../Typewriter/ContentAtomType";

let currentApp: CaliforniaApp;

export const VERY_HIGH_VALUE: number = 2300000000;

enum CaliforniaViewSpecialStyle {
    View = 0,
    Body = 1,
    Html = 2
}

export class PropertyBar {
    public viewModel: PropertyBarVM;
    private propertyBarIndex: number = -1;
    private _visibleLayoutAtomDomNodeReferences: HTMLElement[] = [];
    private _activeViewLayoutAtomDomNodeReferences: { [key: string]: HTMLElement } = {};
    private _visibleLayoutAtomKeys: string[] = [];
    private _mostUpperVisibleLayoutAtomId: number = 0;

    constructor(californiaAppArg: CaliforniaApp, targetIndex: number) {
        currentApp = californiaAppArg;
        this.propertyBarIndex = targetIndex;
        this.viewModel = new PropertyBarVM(this, targetIndex, californiaAppArg);
        this.viewModel.isSyncedWithBoxTreeToTheLeft = false;// TODO targetIndex != 0;
        this.viewModel.isSyncedWithPagePreview = false;// TODO targetIndex == 0;
    };

    private get currentPropertyBar(): PropertyBar {
        return currentApp.propertyBars[this.propertyBarIndex];
    };

    private get nextExceptLastPropertyBar(): PropertyBar {
        let nextPropertyBarIndex: number = this.propertyBarIndex + 1;
        if (nextPropertyBarIndex < currentApp.state.visiblePropertyBarMaxCount) {
            return currentApp.propertyBars[nextPropertyBarIndex];
        }
        else {
            return this.currentPropertyBar;
        }
    };

    public get visibleLayoutAtomDomNodeReferences(): HTMLElement[] {
        return this._visibleLayoutAtomDomNodeReferences;
    };

    public get visibleLayoutAtomKeys(): string[] {
        return this._visibleLayoutAtomKeys;
    };

    public get mostUpperVisibleLayoutAtomId(): number {
        return this._mostUpperVisibleLayoutAtomId;
    };

    public get activeViewLayoutAtomDomNodeReferences(): { [key: string]: HTMLElement } {
        return this._activeViewLayoutAtomDomNodeReferences;
    };

    public renderPropertyBar = (): VNode => {
        let divPropertyBarsStyles = {
            // TODO scroll sync
            "flex": currentApp.state.editViewMode === EditViewMode.SidebarOnly ? "1 1 200px" : `1 1 200px`, // property bar width not invariant to browser zoom (%-value would be, but depends on viewport size)
            "display": "flex",
            "flex-flow": "row nowrap",
            "height": "100%",
            "min-width": "100px",
            "width": "200px",
            "z-index": "2" // TODO document
        };
        let propertyBarStyles = {
            "flex": currentApp.state.editViewMode === EditViewMode.SidebarOnly ? "1 1 1px" : `1 1 1px`, // property bar width not invariant to browser zoom (%-value would be, but depends on viewport size)
            //"width": currentApp.state.editViewMode === EditViewMode.SidebarOnly ? "auto" //"100%" : `${currentApp.controlAreaWidthPx}px`,
            //"display": "flex",
            //"flex-flow": "column nowrap",
            //"overflow": "hidden",
            //"margin-top": /*currentApp.state.isShowSidebarOnly ? TODO*/currentApp.navigationHeigthPx + "px", TODO => moved to navigation element
            "border-right": this.propertyBarIndex < (currentApp.propertyBarCount - 1) && this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.CaliforniaView ? "solid 3px black" : undefined, // TODO pattern
            "width": "100%",
            "height": "100%",
            "display": "flex",
            "flex-flow": "column nowrap"
        };
        return <div key={`p${this.propertyBarIndex.toString()}`} styles={divPropertyBarsStyles}>
            <div key="v0" styles={propertyBarStyles}>
                {this.currentPropertyBar.renderPropertyBarNavigation()}
                {this.currentPropertyBar.renderPropertyBarControls()}
                {this.propertyBarIndex != 0 ? this.currentPropertyBar.renderPropertyBarPoppersRenderOnce() : undefined}
            </div>
        </div> as VNode;
    };

    public renderPropertyBarPoppersRenderOnce = (): VNode => { // TODO just render once...
        return <div key="k0">
            {this.currentPropertyBar.renderAddCssPropertyPopup()}
            {this.currentPropertyBar.renderAllCssPropertiesPopup()}
            {this.currentPropertyBar.renderUpdateCssValuePopup()}
            {this.currentPropertyBar.renderMatchingQuantumsPopup()}
            {this.currentPropertyBar.renderUpdateCssQuantumPopup()}
            {this.currentPropertyBar.renderAllCssPropertiesForQuantumPopup()}
            {this.currentPropertyBar.insertLayoutRowIntoViewPopup()}
            {this.currentPropertyBar.insertLayoutAtomIntoBoxPopup()}
            {this.currentPropertyBar.insertLayoutBoxIntoBoxPopup()}
            {this.currentPropertyBar.moveStyleAtomToResponsiveDevicePopup()}
            {/*this.currentPropertyBar.moveLayoutMoleculeIntoPopup()*/}
            {/*this.currentPropertyBar.moveLayoutMoleculeBeforePopup()*/}
            {this.currentPropertyBar.renderSelectInteractionTargetPopup()}
            {this.currentPropertyBar.renderSelectInteractionTargetLayoutFilterPopup()}
            {this.currentPropertyBar.renderShareCaliforniaProjectPopup()}
            {this.currentPropertyBar.renderCaliforniaViewSelectionPopup()}
            {this.currentPropertyBar.renderEditUserDefinedCssPopup()}
            {this.currentPropertyBar.renderSuggestedCssValuesPopup()}
        </div> as VNode;
    };

    public renderPropertyBarNavigation = (): VNode => {
        let propertyBarNavigationStyles = {
            "margin-top": /*currentApp.state.isShowSidebarOnly ? TODO*/currentApp.navigationHeigthPx + "px",
            "display": `flex`,
            "flex-flow": "row nowrap",
            "height": `auto`,
            "width": "100%",
            "flex": "0 0 auto"
        };
        let hiddenModeButtons: number[] = [
            PropertyBarMode.None,
            PropertyBarMode.AllStyleAtoms,
            PropertyBarMode.LayoutAtoms,
            PropertyBarMode.LayoutBase,
            PropertyBarMode.LayoutMolecules,
            PropertyBarMode.StyleMolecule
        ]; // TODO render whole UI static
        let propertyBarModeIconStrings: { [key: number]: string } = {};
        propertyBarModeIconStrings[PropertyBarMode.AllCaliforniaViews] = "V";
        propertyBarModeIconStrings[PropertyBarMode.AllLayoutMolecules] = "L";
        propertyBarModeIconStrings[PropertyBarMode.AllStyleMolecules] = "S";
        propertyBarModeIconStrings[PropertyBarMode.AllStyleQuantums] = "Q";
        propertyBarModeIconStrings[PropertyBarMode.CaliforniaView] = ":)";
        let propertyBarModeButtons: (VNode | undefined)[] = getArrayForEnum(PropertyBarMode).map((type: string, index: number) => {
            let modeButtonStyles = {
                "color": index === this.currentPropertyBar.viewModel.currentPropertyBarMode ? "red" : undefined,
                "width": "1px",
                "margin-right": "5px",
                "margin-left": "5px",
                "flex": "1 1 1px"
            };
            if (hiddenModeButtons.findIndex(el => el == index) != -1) {
                return undefined;
            }
            return <button key={index} role="button" pid={index.toString()} onclick={this.currentPropertyBar.setPropertyBarMode} styles={modeButtonStyles}>{propertyBarModeIconStrings[index] !== undefined ? propertyBarModeIconStrings[index] : type}</button> as VNode;
        });
        return <div key="n0" styles={propertyBarNavigationStyles}>
            {propertyBarModeButtons}
            {this.propertyBarIndex == 0 ? <button key="a" onclick={this.currentPropertyBar.logoutPopupClickHandler} styles={{ "flex": "0 0 auto", "width": "auto" }}>&#9993;&#8230;</button> : undefined}
        </div> as VNode;
    };

    public setPropertyBarMode = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.currentPropertyBarMode = parseIntFromAttribute(evt.target, "pid");
    };

    public renderPropertyBarControls = (): VNode => {
        let divPropertyBarControlsStyles = {
            "flex": "1 1 auto",
            "height": "1px", // TODO was 100% before, but it got cut off on the bottom
            "width": "100%"
        };
        let propertyBarControlsStyles = {
            "width": "100%",
            "height": "100%", // TODO fixme
            "overflow": "auto"
        };
        return <div key={this.currentPropertyBar.viewModel.currentPropertyBarMode} styles={divPropertyBarControlsStyles}>
            {
                this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.AllStyleAtoms ?
                    <div key={PropertyBarMode.AllStyleAtoms} styles={propertyBarControlsStyles}>
                        {this.currentPropertyBar.viewModel.styleAtomProjector.results.map(r => r.renderMaquette())}
                    </div>
                    : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.AllStyleQuantums ?
                        <div key={PropertyBarMode.AllStyleQuantums} styles={propertyBarControlsStyles}>
                            {this.currentPropertyBar.renderStyleQuantumControls()}
                            {this.currentPropertyBar.viewModel.styleQuantumProjector.results.map(r => r.renderMaquette())}
                        </div>
                        : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.AllStyleMolecules ?
                            <div key={PropertyBarMode.AllStyleMolecules} styles={propertyBarControlsStyles}>
                                {this.currentPropertyBar.viewModel.styleMoleculeProjector.results.map(r => r.renderMaquette())}
                            </div>
                            : (this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.StyleMolecule) ?
                                <div key={PropertyBarMode.AllStyleMolecules} styles={propertyBarControlsStyles}>
                                    {this.currentPropertyBar.renderStyleMoleculeControls(this)}
                                </div>
                                : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.LayoutAtoms ?
                                    <div key={PropertyBarMode.LayoutAtoms} styles={propertyBarControlsStyles}>
                                        {this.currentPropertyBar.viewModel.instanceableAtomProjector.results.map(r => r.renderMaquette())}
                                    </div>
                                    : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.LayoutMolecules ?
                                        <div key={PropertyBarMode.LayoutMolecules} styles={propertyBarControlsStyles}>
                                            {this.currentPropertyBar.viewModel.instanceableMoleculeProjector.results.map(r => r.renderMaquette())}
                                        </div>
                                        : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.AllLayoutMolecules ?
                                            <div key={PropertyBarMode.AllLayoutMolecules} styles={propertyBarControlsStyles}>
                                                {this.currentPropertyBar.viewModel.allLayoutMoleculesProjector.results.map(r => r.renderMaquette())}
                                            </div>
                                            : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.LayoutBase ?
                                                <div key={PropertyBarMode.LayoutBase} styles={propertyBarControlsStyles}>
                                                    {this.currentPropertyBar.renderLayoutBaseControls()}
                                                </div>
                                                : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.AllCaliforniaViews ?
                                                    <div key={PropertyBarMode.AllCaliforniaViews} styles={propertyBarControlsStyles}>
                                                        {this.currentPropertyBar.renderCaliforniaViewControlsWhenAll()}
                                                        {this.currentPropertyBar.viewModel.allCaliforniaViewsProjector.results.map(r => r.renderMaquette())}
                                                    </div>
                                                    : this.currentPropertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.CaliforniaView ?
                                                        <div key={PropertyBarMode.CaliforniaView} styles={propertyBarControlsStyles}>
                                                            {this.currentPropertyBar.renderCaliforniaViewControls()}
                                                        </div>
                                                        : undefined
            }
        </div> as VNode;
    };

    /*public scrollTODO = (evt: UIEvent) => {
        console.log("in scroll");
        let scrolledElement: HTMLElement = evt.target as HTMLElement;
        // TODO semi-const
        let scrollVerticalMinPx: number = 0;
        let scrollVerticalMaxPx: number = 222;
        scrollVerticalMaxPx = scrolledElement.scrollHeight - scrolledElement.clientHeight;//scrolledElement.scrollHeight - (((scrolledElement.firstElementChild as HTMLElement).lastElementChild as HTMLElement).firstElementChild as HTMLElement).clientHeight;
        let scrollTargetCssValueMin: number = 0;
        let scrollTargetCssValueMax: number = 222;
        let scrollDeltaStar: number = scrollVerticalMaxPx - scrollVerticalMinPx;
        let isInverted: boolean = true;
        // ---
        if (scrolledElement.scrollTop > scrollVerticalMinPx || scrolledElement.scrollTop < scrollVerticalMaxPx) { //TODO need to save state of previous scroll event for the case when the scroll distance is large
            let scrollDelta: number = scrolledElement.scrollTop - scrollVerticalMinPx;
            let scrollFraction: number = (scrollDelta / scrollDeltaStar);
            if (isInverted === true) {
                scrollFraction = 1.0 - scrollFraction;
            }
            let scrollTargetCssValue: number = scrollFraction * (scrollTargetCssValueMax - scrollTargetCssValueMin) + scrollTargetCssValueMin;
            let scrollTargetCssString: string = `rgb(${scrollTargetCssValue},${scrollTargetCssValue},${scrollTargetCssValue})`;
            scrolledElement.style.backgroundColor = scrollTargetCssString;
        }
    };*/

    /*public scrollTODO = (evt: UIEvent) => {
        console.log("in scroll");
        let scrolledElement: HTMLElement = evt.target as HTMLElement;
        // TODO semi-const
        let scrollVerticalMinPx: number = 0;
        let scrollVerticalMaxPx: number = 222;
        scrollVerticalMaxPx = scrolledElement.scrollHeight - scrolledElement.clientHeight;//scrolledElement.scrollHeight - (((scrolledElement.firstElementChild as HTMLElement).lastElementChild as HTMLElement).firstElementChild as HTMLElement).clientHeight;
        let scrollTargetCssValueMin: number = 0;
        let scrollTargetCssValueMax: number = 40;
        let scrollDeltaStar: number = scrollVerticalMaxPx - scrollVerticalMinPx;
        let isInverted: boolean = true;
        // ---
        if (scrolledElement.scrollTop > scrollVerticalMinPx || scrolledElement.scrollTop < scrollVerticalMaxPx) { //TODO need to save state of previous scroll event for the case when the scroll distance is large
            let scrollDelta: number = scrolledElement.scrollTop - scrollVerticalMinPx;
            let scrollFraction: number = (scrollDelta / scrollDeltaStar);
            if (isInverted === true) {
                scrollFraction = 1.0 - scrollFraction;
            }
            let scrollTargetCssValue: number = scrollFraction * (scrollTargetCssValueMax - scrollTargetCssValueMin) + scrollTargetCssValueMin;
            let paddingTargetCssString: string = `${scrollTargetCssValue}px`;
            scrolledElement.style.paddingLeft = paddingTargetCssString;
        }
    };*/

    /*private isAnimationRunning: boolean = false; TODO code samples animations on scroll position reaching/leaving top
    private isAnimationInSecondState: boolean = false;

    public scrollTODO = (evt: UIEvent) => {
        let scrolledElement: HTMLElement = evt.target as HTMLElement;
        // TODO semi-const
        let scrollVerticalMinPx: number = 0;
        let scrollVerticalMaxPx: number = 222;
        //scrollVerticalMaxPx = scrolledElement.scrollHeight - scrolledElement.clientHeight;//scrolledElement.scrollHeight - (((scrolledElement.firstElementChild as HTMLElement).lastElementChild as HTMLElement).firstElementChild as HTMLElement).clientHeight;
        let scrollTargetCssValueMin: number = 0;
        let scrollTargetCssValueMax: number = 40;
        let scrollDeltaStar: number = scrollVerticalMaxPx - scrollVerticalMinPx;
        let isInverted: boolean = true;
        // ---
        // animation: fold in when scrolling away from top
        let isFirstTransition: boolean = !this.currentPropertyBar.isAnimationInSecondState;
        if (scrolledElement.scrollTop > 0) {
            // transition 2 => 1
            if (this.currentPropertyBar.isAnimationInSecondState) {
                // do nothing
                return;
            }
            else {
                if (this.currentPropertyBar.isAnimationRunning) {
                    velocity.animate(scrolledElement, "stop");
                    this.currentPropertyBar.isAnimationRunning = false;
                }
            }
        }
        else { // scrollTop == 0
            // transition 1 => 2
            if (this.currentPropertyBar.isAnimationInSecondState) {
                if (this.currentPropertyBar.isAnimationRunning) {
                    velocity.animate(scrolledElement, "stop");
                    this.currentPropertyBar.isAnimationRunning = false;
                }
            }
            else {
                // TODO this should never happen
                console.log(DEFAULT_EXCEPTION);
                return;
            }
        }
        let durationMax: number = 100;
        let paddingLeft: string = scrolledElement.style.marginLeft as string;
        let paddingLeftPx: number = parseInt(paddingLeft.substring(0, paddingLeft.length - 2));
        if (isFirstTransition) {
            this.currentPropertyBar.isAnimationInSecondState = true;
            let durationDelta = (paddingLeftPx - 0) / 40.0 * durationMax;
            if (durationDelta > 10) {
                this.currentPropertyBar.isAnimationRunning = true;
                velocity.animate(scrolledElement, { "margin-left": 0 }, { duration: durationDelta, easing: "ease-in", complete: () => { this.currentPropertyBar.isAnimationRunning = false; } });
            }
            else {
                scrolledElement.style.marginLeft = "0px";
            }            
        }
        else {
            this.currentPropertyBar.isAnimationInSecondState = false;
            //domNode.style.overflow = "hidden";
            let durationDelta = (40 - paddingLeftPx) / 40.0 * durationMax;
            if (durationDelta > 10) {
                this.currentPropertyBar.isAnimationRunning = true;
                velocity.animate(scrolledElement, { "margin-left": 40 }, { duration: durationDelta, easing: "ease-out", complete: () => { this.currentPropertyBar.isAnimationRunning = false; } });
            }
            else {
                scrolledElement.style.marginLeft = "40px";
            }
        }
    };*/

    public renderStyleMoleculeControls = (propertyBar: PropertyBar): VNode | undefined => {
        if (propertyBar.viewModel.selectedStyleMoleculeId != 0) {
            let sourceStyleMoleculeIdString: string = propertyBar.viewModel.selectedStyleMoleculeId.toString();
            let styleMolecule: StyleMolecule | undefined = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleMoleculeId == propertyBar.viewModel.selectedStyleMoleculeId); // TODO find used repeatedly for render controls
            if (styleMolecule === undefined) { // can happen if style molecule got deleted in the mean time in other property bars
                return undefined; // TODO document
            }
            let isClonedStyle: boolean = false;
            let cloneRefStyleMoleculeIdString: string | undefined = undefined;
            if (styleMolecule.ClonedFromStyleId !== undefined) {
                isClonedStyle = true;
                cloneRefStyleMoleculeIdString = styleMolecule.ClonedFromStyleId.toString();
            }
            let styledLayoutBaseIdString: string = styleMolecule.StyleForLayoutId.toString();
            let propertyBarControlsStyles = {
                "height": "100%",
                "width": "100%",
                "display": "flex",
                "flex-flow": "column nowrap"
            };

            // TODO clone reference style selector should always be visible (depends on cloneOfStyleId)
            // TODO ref style/clone style
            return <div key={PropertyBarMode.StyleMolecule} styles={propertyBarControlsStyles}>
                <div key="0" styles={{ "flex": "0 0 auto" }}>
                    Selected StyleMolecule #{propertyBar.viewModel.selectedStyleMoleculeId}
                    {isClonedStyle ? <div key="0">
                        <button key="a" role="button" mid={cloneRefStyleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>ref style (#{cloneRefStyleMoleculeIdString})</button>
                        <button disabled key="b" role="button" mid={sourceStyleMoleculeIdString} onclick={propertyBar.createReferenceStyleMoleculeClickHandler}>make ref</button>
                        <button disabled key="c" role="button" mid={sourceStyleMoleculeIdString} onclick={propertyBar.syncToReferenceStyleClickHandler}>sync to ref</button>
                        <button disabled key="d" role="button" mid={sourceStyleMoleculeIdString} onclick={propertyBar.syncFromReferenceStyleClickHandler}>sync from ref</button>
                    </div> : propertyBar.renderStyleMoleculeReferenceSelector()}
                    {propertyBar.renderResponsiveDeviceSelectors()}
                    {propertyBar.renderStateModifierSelectors()}
                    {propertyBar.renderStyleAtomControls()}
                </div>
                <div key="1" styles={{ "flex": "1 1 1px", "overflow": "scroll" }}>
                    {propertyBar.viewModel.styleAtomProjector.results.map(r => r.renderMaquette())}
                    {propertyBar.renderStyleMoleculeChildren(propertyBar)}
                </div>
                <div key="2" styles={{ "flex": "0 0 auto" }}>
                    <button key="a" role="button" lid={styledLayoutBaseIdString} onclick={propertyBar.selectLayoutBaseClickHandler}>layout #{styledLayoutBaseIdString}</button>
                </div>
            </div> as VNode;
        }
        else {
            return undefined;
        }
    };

    public syncToReferenceStyleClickHandler = (evt: MouseEvent) => {
        currentApp.controller.SyncStyleMoleculeToReferenceStyleJson(parseIntFromAttribute(evt.target, "mid")).done(data => currentApp.router.updateData(data));
    };

    public syncFromReferenceStyleClickHandler = (evt: MouseEvent) => {
        currentApp.controller.SyncStyleMoleculeFromReferenceStyleJson(parseIntFromAttribute(evt.target, "mid")).done(data => currentApp.router.updateData(data));
    };

    public createReferenceStyleMoleculeClickHandler = (evt: MouseEvent) => {
        currentApp.controller.SetStyleMoleculeAsReferenceStyleJson(parseIntFromAttribute(evt.target, "mid")).done(data => currentApp.router.updateData(data));
    };

    public renderStyleMoleculeReferenceSelector = (): VNode => {
        // TODO enable when backend functionality is implemented
        return <div key="-1">
            <select disabled onchange={this.currentPropertyBar.styleMoleculeReferenceChangedHandler}>
                {currentApp.clientData.CaliforniaProject.StyleMolecules.map(mol => {
                    // TODO expensive
                    if (mol.ClonedFromStyleId !== undefined) {
                        // ignore styles which are not reference styles
                        return undefined;
                    }
                    let styleMoleculeIdString: string = mol.StyleMoleculeId.toString();
                    if (mol.StyleMoleculeId == this.currentPropertyBar.viewModel.selectedStyleMoleculeId) {
                        return <option selected key={styleMoleculeIdString} value={styleMoleculeIdString}>{mol.Name} #{mol.StyleMoleculeId}</option>;
                    }
                    else {
                        return <option key={styleMoleculeIdString} value={styleMoleculeIdString}>{mol.Name} #{mol.StyleMoleculeId}</option>;
                    }
                })}
            </select>
        </div> as VNode;
    };

    public styleMoleculeReferenceChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        let parsedStyleMoleculeId: number | undefined = undefined;
        if (targetSelect.selectedIndex < targetSelect.childElementCount) {
            let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
            parsedStyleMoleculeId = parseInt(selectOptionElement.value);
        }
        if (parsedStyleMoleculeId !== undefined) {
            currentApp.controller.SetStyleMoleculeReferenceJson(this.currentPropertyBar.viewModel.selectedStyleMoleculeId, parsedStyleMoleculeId).done(data => currentApp.router.updateData(data));
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public renderStyleMoleculeChildren = (propertyBar: PropertyBar): VNode => {
        let childMolecules: StyleMolecule[] = [];
        if (propertyBar.viewModel.selectedStyleMoleculeId != 0) {
            childMolecules = currentApp.clientData.CaliforniaProject.StyleMolecules.filter(s => s.ClonedFromStyleId == this.currentPropertyBar.viewModel.selectedStyleMoleculeId); // TODO expensive // TODO everywhere where something like s.ClonedFromStyleId is used: reset view before elements are deleted.. this can throw undefined
        }
        return <div key="-4"> affects styles:
            {childMolecules.map(s => {
                let styleMoleculeIdString: string = s.StyleMoleculeId.toString();
                return <div key={styleMoleculeIdString}>
                    <button key="a" role="button" mid={styleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>#{styleMoleculeIdString}</button>
                </div>;
            })}
        </div> as VNode;
    };

    public renderBoxTreeForCaliforniaView = (propertyBar: PropertyBar): maquette.Mapping<CaliforniaView, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<CaliforniaView, any>(
            function getSectionSourceKey(source: CaliforniaView) {
                return source.CaliforniaViewId;
            },
            function createSectionTarget(source: CaliforniaView) {
                let sourceCaliforniaViewIdString = source.CaliforniaViewId.toString();
                let layoutRows = propertyBar.renderLayoutRowArray(propertyBar);
                layoutRows.map(source.PlacedLayoutRows);
                // TODO show body+html style molecule in page preview
                return {
                    renderMaquette: function () {
                        let treeViewStyles = {
                            "display": "flex",
                            "flex-direction": "row",
                            "flex-wrap": "wrap",
                            "margin-right": "-15px",
                            "font-family": "sans-serif",
                            //"width": "900px", // TODO workaround elements breaking line...
                            "border-bottom": "solid, 1px, black",
                            "width": "auto",
                            "height": "auto",
                            "padding-bottom": "123px" /*TODO size of n*element for add/create layout elements at end, currently 120+3*/
                        };
                        return (propertyBar.viewModel.selectedCaliforniaViewId == source.CaliforniaViewId) ? <div key={sourceCaliforniaViewIdString} styles={treeViewStyles}>
                            {layoutRows.results.map(r => r.renderMaquette())}
                        </div> : undefined;
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

    public renderLayoutRowArray = (propertyBar: PropertyBar): maquette.Mapping<LayoutRow, { renderMaquette: () => maquette.VNode }> => { // TODO code duplication with page preview
        return maquette.createMapping<LayoutRow, any>(
            function getSectionSourceKey(source: LayoutRow) {
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutRow) {
                let sourceLayoutRowIdString = source.LayoutBaseId.toString();
                let renderedLayoutBoxes = propertyBar.renderLayoutBoxArray(propertyBar);
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
                renderedLayoutBoxes.map(sortedBoxes);
                let styleMoleculeId: number = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule).StyleMoleculeId; // TODO expensive
                let styleMoleculeIdString: string = styleMoleculeId.toString();
                let layoutRowStyleClass: string = `s${styleMoleculeIdString}`;
                return {
                    renderMaquette: function () {
                        let treeRowStyles = {
                            "flex-basis": "100%",
                            "width": "100%",
                            "max-width": "100%",
                            "padding-left": "15px",
                            "padding-right": "15px",
                            "background-color": "rgb(222, 222, 222)"
                        };
                        let captionStyles = {
                            "flex-basis": "auto",
                            "width": "auto",
                            "color": "rgb(78, 78, 78)",
                            "padding-left": "15px",
                            "padding-right": "15px",
                            "margin": "0",
                            "background-color": "rgb(222, 222, 222)",
                            "text-decoration": "underline"
                        };
                        let divButtonStyles = {
                            "display": "flex",
                            "flex-direction": "row",
                            "flex-wrap": "nowrap",
                            //"margin-left": "-15px",
                            "margin-right": "-15px",
                            "width": "auto",
                            "flex": "0 0 auto",
                        };
                        let buttonStyles = {
                            "font-size": "10px",
                            "color": "rgb(78, 78, 78)",
                            "background-color": "rgb(222, 222, 222)", // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": undefined,
                            "outline-offset": undefined
                        };
                        let isPreselectedAny: boolean = currentApp.state.preselectedLayoutBaseId != 0;
                        let isPreselectedCurrent: boolean = isPreselectedAny && currentApp.state.preselectedLayoutBaseId == source.LayoutBaseId;
                        let buttonStylesTarget = { // TODO append array
                            "font-size": "10px",
                            "color": !isPreselectedAny || isPreselectedCurrent ? "rgb(222, 222, 222)" : "rgb(78, 78, 78)",
                            "background-color": "rgb(222, 222, 222)", // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": !isPreselectedAny || isPreselectedCurrent ? undefined : "solid 4px rgb(200,0,0)",
                            "outline-offset": !isPreselectedAny || isPreselectedCurrent ? undefined : "-4px"
                        };
                        let buttonStylesPreselectRow = { // TODO append array
                            "font-size": "10px",
                            "color": isPreselectedCurrent ? "rgb(222,222,222)" : isPreselectedAny ? "rgb(222, 222, 222)" : "rgb(78, 78, 78)",
                            "background-color": isPreselectedCurrent ? "rgb(200,0,0)" : "rgb(222, 222, 222)", // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": isPreselectedCurrent || isPreselectedAny ? undefined : "solid 1px rgb(200,0,0)",
                            "outline-offset": isPreselectedCurrent || isPreselectedAny ? undefined : "-1px"
                        };
                        let divSubBoxStyles = {
                            "flex-basis": "100%",
                            "width": "100%",
                            "max-width": "100%",
                            "padding-left": "15px",
                            "padding-right": "15px"
                        };
                        return <div key={sourceLayoutRowIdString} styles={treeRowStyles} lid={sourceLayoutRowIdString} onmouseenter={propertyBar.layoutBaseMouseEnterHandler} onmouseleave={propertyBar.layoutBaseMouseLeaveHandler}>
                            <div key="-2" styles={divButtonStyles}>
                                <p key="-1" styles={captionStyles}>ROW</p>
                                <button key="a" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.insertLayoutBoxIntoBoxClickHandler}>+(B)</button>
                                <button key="b" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.selectLayoutBaseClickHandler}>&#8230;{/*ellipsis TODO check is this comment removed*/}</button>
                                <button key="c" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.highlightLayoutBaseClickHandler}>?</button>
                                <button key="d" styles={buttonStyles} mid={styleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>S&#8230;</button>
                                <button key="e" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.saveLayoutMoleculeClickHandler}>!!!</button>
                                {isPreselectedCurrent || !isPreselectedAny ? <button key="f" styles={buttonStylesPreselectRow} lid={sourceLayoutRowIdString} onclick={propertyBar.moveLayoutRowBeforeRowClickHandler}>MV(R)</button> :
                                    <button disabled key="f0" styles={buttonStylesPreselectRow} lid={sourceLayoutRowIdString} onclick={propertyBar.moveLayoutRowBeforeRowClickHandler}>MV(R)</button>}
                                {isPreselectedCurrent || !isPreselectedAny ? <button key="g" styles={buttonStylesPreselectRow} lid={sourceLayoutRowIdString} onclick={propertyBar.syncLayoutBaseStylesClickHandler}>ST(R)</button> :
                                    <button disabled key="g0" styles={buttonStylesPreselectRow} lid={sourceLayoutRowIdString} onclick={propertyBar.syncLayoutBaseStylesClickHandler}>ST(R)</button>}
                                {isPreselectedAny && !isPreselectedCurrent ? <button key="h" styles={buttonStylesTarget} lid={sourceLayoutRowIdString} onclick={propertyBar.finalizeLayoutRequest}>$(B:R)</button> :
                                    <button disabled key="h0" styles={buttonStylesTarget} lid={sourceLayoutRowIdString} onclick={propertyBar.finalizeLayoutRequest}>$(B:R)</button>}
                                {!isPreselectedAny ? <button key="i" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.deleteLayoutBaseClickHandler}>X</button> :
                                    <button disabled key="i0" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.deleteLayoutBaseClickHandler}>X</button>}
                                {!isPreselectedAny && sortedBoxes.length > 0 ? <button key="j" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.deleteBelowLayoutBaseClickHandler}>CLR</button> :
                                    <button disabled key="j0" styles={buttonStyles} lid={sourceLayoutRowIdString} onclick={propertyBar.deleteBelowLayoutBaseClickHandler}>CLR</button>}
                            </div>
                            <div key="0" styles={divSubBoxStyles}>{renderedLayoutBoxes.results.map(r => r.renderMaquette())}</div>
                        </div>;
                    },
                    update: function (updatedSource: LayoutRow) {
                        source = updatedSource;
                        sourceLayoutRowIdString = source.LayoutBaseId.toString();
                        unsortedBoxes = source.AllBoxesBelowRow.filter(b => b.PlacedBoxInBoxId === undefined);
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
                        renderedLayoutBoxes.map(sortedBoxes);
                        styleMoleculeId = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule).StyleMoleculeId; // TODO expensive
                        styleMoleculeIdString = styleMoleculeId.toString();
                        layoutRowStyleClass = `s${styleMoleculeId}`;
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutRow, target: { renderMaquette(): any, update(updatedSource: LayoutRow): void }) {
                target.update(updatedSource);
            });
    };

    private renderLayoutBoxArray = (propertyBar: PropertyBar): maquette.Mapping<LayoutBox, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<LayoutBox, any>(
            function getSectionSourceKey(source: LayoutBox) {
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutBox) {
                let sourceLayoutBoxIdString = source.LayoutBaseId.toString();
                let renderedLayoutAtoms = propertyBar.renderLayoutAtomArray(propertyBar);
                let renderedLayoutBoxes = propertyBar.renderLayoutBoxArray(propertyBar);
                let styleMoleculeId: number = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule).StyleMoleculeId; // TODO expensive
                let styleMoleculeIdString: string = styleMoleculeId.toString();
                let layoutBoxStyleClass: string = `s${styleMoleculeId}`;
                // --- only render mode: tree ---
                let deepnessPadding: string = "";
                let calculatedBackgroundColor: string = "";
                let calculatedColor: string = "";
                let calculatedBorderColor: string = "";
                let hasSubAtoms: boolean = false;
                let isOddLevel: boolean = false;
                // ---
                deepnessPadding = `${(source.Level + 1) * 15}px`;
                calculatedBackgroundColor = propertyBar.calculateBackgroundColorForLevel(source.Level);
                calculatedColor = propertyBar.calculateColorForLevel(source.Level);
                calculatedBorderColor = `solid 1px ${propertyBar.calculateBackgroundColorForLevel(source.Level + 1)}`;
                hasSubAtoms = source.PlacedInBoxAtoms.length > 0;
                isOddLevel = (source.Level % 2) != 0;
                return {
                    renderMaquette: function () {
                        let renderedBoxContent: VNode[] = currentApp.pagePreview.mapAndRenderLayoutBoxContent(source, source.PlacedInBoxAtoms, renderedLayoutAtoms, source.PlacedInBoxBoxes, renderedLayoutBoxes);
                        let treeBoxStyles = {
                            "display": "flex",
                            "flex-direction": "row",
                            "flex-wrap": "wrap",
                            //"margin-left": "-15px", TODO stair effect
                            "margin-right": "-15px",
                            "background-color": "rgb(222, 222, 222)",
                            "border-bottom": hasSubAtoms ? calculatedBorderColor : undefined,
                            "border-left": "solid 1px black",// hasSubAtoms ? calculatedBorderColor : undefined // TODO only next to atoms
                            "zoom": "1.05" // TODO caleidoscope / zoom factor
                        };
                        let boxCaptionStyles = {
                            "padding-left": "15px",
                            "padding-right": "15px",
                            "width": "auto",
                            "flex": "0 0 auto",
                            "margin": "0",
                            "text-decoration": "underline",
                            "color": calculatedColor,
                            "background-color": calculatedBackgroundColor,
                            "font-stretch": isOddLevel ? "extra-condensed" : undefined
                        };
                        let divButtonStyles = {
                            "display": "flex",
                            "flex-direction": "row",
                            "flex-wrap": "nowrap",
                            "margin-left": "-15px",
                            "margin-right": "-15px"
                        };
                        let buttonStyles = {
                            "font-size": "10px",
                            "color": calculatedColor,
                            "background-color": calculatedBackgroundColor,
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": undefined,
                            "outline-offset": undefined
                        };
                        let buttonDisabledStyles = {
                            "font-size": "10px",
                            "background-color": "rgb(242,242,242)",
                            "color": calculatedColor,
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": undefined,
                            "outline-offset": undefined
                        };
                        let isPreselectedAny: boolean = currentApp.state.preselectedLayoutBaseId != 0;
                        let isPreselectedCurrent: boolean = isPreselectedAny && currentApp.state.preselectedLayoutBaseId == source.LayoutBaseId;
                        let buttonStylesTarget = { // TODO append array
                            "font-size": "10px",
                            "color": !isPreselectedAny || isPreselectedCurrent ? calculatedBackgroundColor : calculatedColor,
                            "background-color": calculatedBackgroundColor, // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": !isPreselectedAny || isPreselectedCurrent ? undefined : "solid 4px rgb(200,0,0)",
                            "outline-offset": !isPreselectedAny || isPreselectedCurrent ? undefined : "-4px",
                        };
                        let buttonStylesPreselectAny = { // TODO append array // TODO differentiate
                            "font-size": "10px",
                            "color": isPreselectedCurrent ? calculatedBackgroundColor : isPreselectedAny ? calculatedBackgroundColor : calculatedColor,
                            "background-color": isPreselectedCurrent ? "rgb(200,0,0)" : calculatedBackgroundColor, // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": isPreselectedCurrent || isPreselectedAny ? undefined : "solid 1px rgb(200,0,0)",
                            "outline-offset": isPreselectedCurrent || isPreselectedAny ? undefined : "-1px",
                        };
                        let divSubTreeStyles = {
                            "flex-basis": "100%",
                            "width": "100%",
                            "max-width": "100%",
                            "padding-left": "15px",
                            "padding-right": "15px",
                            "background-color": calculatedBackgroundColor
                        };
                        // TODO document: whole client app relies on database keys being strictly positive
                        // TODO concept+do everywhere: in this case many html tags are set when 1 would be enough on parent
                        return <div key={sourceLayoutBoxIdString} styles={treeBoxStyles}>
                            <div key="0" styles={divSubTreeStyles} lid={sourceLayoutBoxIdString} onmouseenter={propertyBar.layoutBaseMouseEnterHandler} onmouseleave={propertyBar.layoutBaseMouseLeaveHandler}>
                                <div key="-2" styles={divButtonStyles}>
                                    {<p key="-1" styles={boxCaptionStyles}>BOX{!isOddLevel ? " |" : undefined}</p>}
                                    <button key="a" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.insertLayoutAtomIntoBoxClickHandler}>+(A)</button>
                                    <button key="b" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.insertLayoutBoxIntoBoxClickHandler}>+(B)</button>
                                    <button key="c" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.selectLayoutBaseClickHandler}>&#8230;{/*Ellipsis*/}</button>
                                    <button key="d" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.highlightLayoutBaseClickHandler}>?</button>
                                    <button key="e" styles={buttonStyles} mid={styleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>S&#8230;{/*Ellipsis*/}</button>
                                    <button key="f" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.saveLayoutMoleculeClickHandler}>!!!</button>
                                    {isPreselectedCurrent || !isPreselectedAny ? <button key="g" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.moveLayoutBoxIntoRowClickHandler}>IN(R)</button> :
                                        <button disabled key="g0" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.moveLayoutBoxIntoRowClickHandler}>IN(R)</button>}
                                    {isPreselectedCurrent || !isPreselectedAny ? <button key="h" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.moveLayoutBoxIntoBoxClickHandler}>IN(B)</button> :
                                        <button disabled key="h0" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.moveLayoutBoxIntoBoxClickHandler}>IN(B)</button>}
                                    {isPreselectedCurrent || !isPreselectedAny ? <button key="i" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.moveLayoutBoxBeforeBoxClickHandler}>MV(A:B)</button> :
                                        <button disabled key="i0" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.moveLayoutBoxBeforeBoxClickHandler}>MV(A:B)</button>}
                                    {isPreselectedCurrent || !isPreselectedAny ? <button key="j" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.syncLayoutBaseStylesClickHandler}>ST(B)</button> :
                                        <button disabled key="j0" styles={buttonStylesPreselectAny} lid={sourceLayoutBoxIdString} onclick={propertyBar.syncLayoutBaseStylesClickHandler}>ST(B)</button>}
                                    {isPreselectedAny && !isPreselectedCurrent ? <button key="k" styles={buttonStylesTarget} lid={sourceLayoutBoxIdString} onclick={propertyBar.finalizeLayoutRequest}>$(A:B)</button> :
                                        <button disabled key="k0" styles={buttonStylesTarget} lid={sourceLayoutBoxIdString} onclick={propertyBar.finalizeLayoutRequest}>$(A:B)</button>}
                                    {!isPreselectedAny ? <button key="l" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.deleteLayoutBaseClickHandler}>X</button> : 
                                        <button disabled key="l0" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.deleteLayoutBaseClickHandler}>X</button>}
                                    {!isPreselectedAny && renderedBoxContent.length > 0 ? <button key="m" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.deleteBelowLayoutBaseClickHandler}>CLR</button> :
                                        <button disabled key="m0" styles={buttonStyles} lid={sourceLayoutBoxIdString} onclick={propertyBar.deleteBelowLayoutBaseClickHandler}>CLR</button>}
                                </div>
                                {renderedBoxContent}
                            </div>
                        </div>;
                    },
                    update: function (updatedSource: LayoutBox) {
                        source = updatedSource;
                        sourceLayoutBoxIdString = source.LayoutBaseId.toString();
                        styleMoleculeId = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule).StyleMoleculeId; // TODO expensive
                        styleMoleculeIdString = styleMoleculeId.toString();
                        layoutBoxStyleClass = `s${styleMoleculeId}`;
                        deepnessPadding = `${(source.Level + 1) * 15}px`;
                        calculatedBackgroundColor = propertyBar.calculateBackgroundColorForLevel(source.Level);
                        calculatedColor = propertyBar.calculateColorForLevel(source.Level);
                        calculatedBorderColor = `solid 1px ${propertyBar.calculateBackgroundColorForLevel(source.Level + 1)}`; // TODO test if rendered too many times by setting break point in updater
                        hasSubAtoms = source.PlacedInBoxAtoms.length > 0;
                        isOddLevel = (source.Level % 2) != 0;
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutBox, target: { renderMaquette(): any, update(updatedSource: LayoutBox): void }) {
                target.update(updatedSource);
            });
    };

    public calculateColorForLevel = (level: number): string => { // TODO hardcoded precalculated table
        level = level < 0 ? 0 : level;
        let colorValue: number = level > 2 ? 222 : 78 + level * 12; // gray tone with limit to white
        return `rgb(${colorValue},${colorValue},${colorValue})`;
    };

    public calculateBackgroundColorForLevel = (level: number): string => {
        level = level < 0 ? 0 : level;
        let colorValue: number = 200 - level * 22;
        colorValue = colorValue < 0 ? 0 : colorValue;
        return `rgb(${colorValue},${colorValue},${colorValue})`;
    };

    private renderLayoutAtomArray = (propertyBar: PropertyBar): maquette.Mapping<LayoutAtom, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<LayoutAtom, any>(
            function getSectionSourceKey(source: LayoutAtom) {
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutAtom) {
                let sourceLayoutAtomIdString = source.LayoutBaseId.toString();
                let sourceContentAtomIdString = source.HostedContentAtom.ContentAtomId.toString();
                let styleMolecule: StyleMolecule = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule); // TODO expensive
                let styleMoleculeId: number = styleMolecule.StyleMoleculeId;
                let styleMoleculeIdString: string = styleMoleculeId.toString();
                let layoutAtomStyleClass: string = `s${styleMoleculeIdString}`; // TODO create all of these constant strings when parsing data => one instance in memory !!!
                // --- only render mode: tree ---
                let calculatedPaddingPx: number = (propertyBar.viewModel.deepestLevelActiveView + 1 - source.Level) * 15;
                let calculatedMargin: string = "";
                let calculatedColor: string = "";
                let calculatedBackgroundColor: string = "";
                // ---
                calculatedMargin = `${(source.Level) * 15 + 15}px`;
                calculatedColor = propertyBar.calculateColorForLevel(source.Level);
                calculatedBackgroundColor = propertyBar.calculateBackgroundColorForLevel(source.Level);
                let hostedContentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.ContentAtomId == source.HostedContentAtom.ContentAtomId) as ContentAtom); // TODO expensive (2 copies of content)
                return {
                    renderMaquette: function () {
                        let isRenderedAtomVisible: boolean = currentApp.pagePreview.visibleLayoutAtomKeys.findIndex(k => k === sourceLayoutAtomIdString) != -1;
                        let isRenderedAtomHovered: boolean = currentApp.state.hoveredPagePreviewLayoutBaseId == source.LayoutBaseId; // TODO wiggle effect
                        // TODO same stair effect right side margin
                        let divAtomStyles = {
                            "display": "flex",
                            "flex-direction": "row",
                            "flex-wrap": "nowrap",
                            //"margin-left": "-15px", TODO stair effect
                            "margin-right": "-15px",
                            "border-left": "solid 1px black"
                        };
                        let atomCaptionStyles = {
                            "text-decoration": "underline",
                            "flex": "0 0 auto",
                            "width": "auto",
                            "margin-left": "15px", // TODO stair effect
                            "padding-left": `${(calculatedPaddingPx + (isRenderedAtomVisible ? -1 : 0)).toString()}px`,
                            "padding-right": "15px",
                            "margin": "0",
                            "color": calculatedColor,
                            "background-color": calculatedBackgroundColor,
                            "font-size": undefined,
                            "min-width": undefined,
                            "border-left": isRenderedAtomHovered ? "solid 3px rgb(200,0,0)" : isRenderedAtomVisible ? "dashed 1px rgb(200,0,0)" :  undefined
                        };
                        let inputStyles = { // TODO input disappears when layoutatom hosted content atom text is: $\left.\right]$
                            "text-decoration": undefined,
                            "flex": "0 0 auto",
                            "width": "auto",
                            "margin-left": "15px", // TODO stair effect
                            "padding-left": undefined,
                            "padding-right": "15px",
                            "margin": "0",
                            "color": undefined,
                            "background-color": undefined,
                            "font-size": "0.8rem",
                            "min-width": "200px"
                        };
                        let buttonStyles = {
                            "font-size": "10px",
                            "color": calculatedColor,
                            "background-color": calculatedBackgroundColor,
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": undefined,
                            "outline-offset": undefined
                        };
                        let isPreselectedAny: boolean = currentApp.state.preselectedLayoutBaseId != 0;
                        let isPreselectedCurrent: boolean = isPreselectedAny && currentApp.state.preselectedLayoutBaseId == source.LayoutBaseId;
                        let buttonStylesTarget = { // TODO append array
                            "font-size": "10px",
                            "color": !isPreselectedAny || isPreselectedCurrent ? calculatedBackgroundColor : calculatedColor,
                            "background-color": calculatedBackgroundColor, // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": !isPreselectedAny || isPreselectedCurrent ? undefined : "solid 4px rgb(200,0,0)",
                            "outline-offset": !isPreselectedAny || isPreselectedCurrent ? undefined : "-4px"
                        };
                        let buttonStylesPreselectAny = { // TODO append array
                            "font-size": "10px",
                            "color": isPreselectedCurrent ? calculatedBackgroundColor : isPreselectedAny ? calculatedBackgroundColor : calculatedColor,
                            "background-color": isPreselectedCurrent ? "rgb(200,0,0)" : calculatedBackgroundColor, // TODO magic strings
                            "width": "auto",
                            "flex": "0 0 auto",
                            "outline": isPreselectedCurrent || isPreselectedAny ? undefined : "solid 1px rgb(200,0,0)",
                            "outline-offset": isPreselectedCurrent || isPreselectedAny ? undefined : "-1px"
                        };
                        let description: string = "";
                        if (hostedContentAtom.ContentAtomType === ContentAtomType.Text && hostedContentAtom.TextContent !== undefined) {
                            description = hostedContentAtom.TextContent.length > 20 ? hostedContentAtom.TextContent.substring(0, 20) + "..." : hostedContentAtom.TextContent; // TODO expensive // TODO ellipsis // TODO multiple places // TODO create when storing in DB? or when loading in client
                        }
                        else if (hostedContentAtom.ContentAtomType === ContentAtomType.Link && hostedContentAtom.Url !== undefined) {
                            description = hostedContentAtom.Url.length > 20 ? hostedContentAtom.Url.substring(0, 20) + "..." : hostedContentAtom.Url; // TODO expensive // TODO ellipsis // TODO multiple places // TODO create when storing in DB? or when loading in client
                        }
                        else {
                            console.log(DEFAULT_EXCEPTION);
                            return undefined;
                        }
                        let renderedInputForContent: VNode | undefined = undefined;
                        let isEditedLayoutAtomId: boolean = source.LayoutBaseId == propertyBar.viewModel.editedLayoutAtomId;
                        if (isEditedLayoutAtomId) {
                            // show input field instead of rendered atom
                            renderedInputForContent = <input key={`inp${sourceLayoutAtomIdString}`}
                                class={layoutAtomStyleClass}
                                value={propertyBar.viewModel.tempContent}
                                oninput={propertyBar.contentAtomInputHandler}
                                onblur={propertyBar.contentAtomLostFocusHandler}
                                onkeydown={propertyBar.contentAtomKeyDownHandler}
                                styles={inputStyles}
                                afterCreate={propertyBar.contentAtomAfterCreateHandler}
                                cid={sourceContentAtomIdString}
                            ></input> as VNode;
                        }
                        return <div key={sourceLayoutAtomIdString}
                            lid={sourceLayoutAtomIdString}
                            styles={divAtomStyles}
                            afterCreate={propertyBar.layoutAtomAfterCreateHandler}
                            onmouseenter={propertyBar.layoutBaseMouseEnterHandler}
                            onmouseleave={propertyBar.layoutBaseMouseLeaveHandler}>
                            {!isEditedLayoutAtomId ? <p key="0" styles={atomCaptionStyles} aid={sourceLayoutAtomIdString} cid={sourceContentAtomIdString} onclick={propertyBar.layoutAtomClickHandler}><small key="0" aid={sourceLayoutAtomIdString} cid={sourceContentAtomIdString}>{description}ATOM</small></p> :
                                renderedInputForContent}
                            <button key="a" styles={buttonStyles} lid={sourceLayoutAtomIdString} onclick={propertyBar.selectLayoutBaseClickHandler}>&#8230;{/*Ellipsis*/}</button>
                            <button key="b" styles={buttonStyles} lid={sourceLayoutAtomIdString} onclick={propertyBar.highlightLayoutBaseClickHandler}>?</button>
                            <button key="c" styles={buttonStyles} mid={styleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>S&#8230;{/*Ellipsis*/}</button>
                            {isPreselectedCurrent || !isPreselectedAny ? <button key="d" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.moveLayoutAtomIntoBoxClickHandler}>IN</button> :
                                <button disabled key="d0" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.moveLayoutAtomIntoBoxClickHandler}>IN</button>}
                            {isPreselectedCurrent || !isPreselectedAny ? <button key="e" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.moveLayoutAtomBeforeAtomClickHandler}>MV</button> :
                                <button disabled key="e0" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.moveLayoutAtomBeforeAtomClickHandler}>MV</button>}
                            {isPreselectedCurrent || !isPreselectedAny ? <button key="f" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.syncLayoutBaseStylesClickHandler}>ST(A)</button> :
                                <button disabled key="f0" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.syncLayoutBaseStylesClickHandler}>ST(A)</button>}
                            {isPreselectedCurrent || !isPreselectedAny ? <button key="g" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.createBoxForAtomInPlaceClickHandler}>+(B).IN</button> :
                                <button disabled key="g0" styles={buttonStylesPreselectAny} lid={sourceLayoutAtomIdString} onclick={propertyBar.createBoxForAtomInPlaceClickHandler}>+(B).IN</button>}
                            {isPreselectedAny && !isPreselectedCurrent ? <button key="h" styles={buttonStylesTarget} lid={sourceLayoutAtomIdString} onclick={propertyBar.finalizeLayoutRequest}>$(A:B)</button> :
                                <button disabled key="h0" styles={buttonStylesTarget} lid={sourceLayoutAtomIdString} onclick={propertyBar.finalizeLayoutRequest}>$(A:B)</button>}
                            {!isPreselectedAny ? <button key="i" styles={buttonStyles} lid={sourceLayoutAtomIdString} onclick={propertyBar.deleteLayoutBaseClickHandler}>X</button> :
                                <button disabled key="i0" styles={buttonStyles} lid={sourceLayoutAtomIdString} onclick={propertyBar.deleteLayoutBaseClickHandler}>X</button>}
                        </div>;
                    },
                    update: function (updatedSource: LayoutAtom) {
                        source = updatedSource;
                        sourceLayoutAtomIdString = source.LayoutBaseId.toString();
                        sourceContentAtomIdString = source.HostedContentAtom.ContentAtomId.toString();
                        styleMolecule = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule); // TODO expensive
                        styleMoleculeId = styleMolecule.StyleMoleculeId;
                        styleMoleculeIdString = styleMoleculeId.toString();
                        layoutAtomStyleClass = `s${styleMoleculeId}`;
                        calculatedPaddingPx = (propertyBar.viewModel.deepestLevelActiveView - source.Level) * 15 + 15;
                        calculatedColor = propertyBar.calculateColorForLevel(source.Level);
                        calculatedMargin = `${(source.Level) * 15 + 15}px`;
                        calculatedBackgroundColor = propertyBar.calculateBackgroundColorForLevel(source.Level);
                        hostedContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.ContentAtomId == source.HostedContentAtom.ContentAtomId) as ContentAtom); // TODO expensive (2 copies of content)
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutAtom, target: { renderMaquette(): any, update(updatedSource: LayoutAtom): void }) {
                target.update(updatedSource);
            });
    };

    private contentAtomAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        let targetElement: HTMLInputElement = element as HTMLInputElement;
        targetElement.focus();
    };

    private layoutBaseMouseEnterHandler = (evt: MouseEvent) => {
        let targetElement: HTMLElement = evt.target as HTMLInputElement;
        currentApp.state.hoveredBoxTreeLayoutBaseId = parseIntFromAttribute(targetElement, "lid");
    };

    private layoutBaseMouseLeaveHandler = (evt: MouseEvent) => {
        currentApp.state.hoveredBoxTreeLayoutBaseId = 0; // TODO remember what was entered before and rehighlight
    };

    private layoutAtomAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        if (this.currentPropertyBar.propertyBarIndex == 0) {
            let targetElement: HTMLElement = element as HTMLElement;
            this.currentPropertyBar._activeViewLayoutAtomDomNodeReferences[properties.key as string] = targetElement;
        }
    };

    private resetContentAtomEditMode = () => {
        currentApp.pagePreview.resetEquationNumbersWhenModifying(false);
        this.currentPropertyBar.viewModel.editedLayoutAtomId = 0;
        this.currentPropertyBar.viewModel.tempContent = "";
        this.currentPropertyBar.viewModel.tempOriginalContent = "";
    };

    private contentAtomLostFocusHandler = (evt: FocusEvent) => {
        this.currentPropertyBar.updateContentAtom(parseIntFromAttribute(evt.target, "cid"));
    };

    private updateContentAtom = (contentAtomId: number) => {
        if (this.currentPropertyBar.viewModel.editedLayoutAtomId != 0) {
            if (this.currentPropertyBar.viewModel.tempContent !== this.currentPropertyBar.viewModel.tempOriginalContent) {
                if (this.currentPropertyBar.viewModel.tempContent !== "") {
                    let contentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(a => a.InstancedOnLayoutId == this.currentPropertyBar.viewModel.editedLayoutAtomId) as ContentAtom); // TODO expensive but feels better
                    if (contentAtom.ContentAtomType === ContentAtomType.Text) {
                        contentAtom.TextContent = this.currentPropertyBar.viewModel.tempContent;
                    }
                    else if (contentAtom.ContentAtomType === ContentAtomType.Link) {
                        contentAtom.Url = this.currentPropertyBar.viewModel.tempContent;
                    }
                    else {
                        console.log(DEFAULT_EXCEPTION);
                        return;
                    }
                    currentApp.state.currentReadyState = ReadyState.Pending;
                    currentApp.controller.UpdateTextContentAtomJson(contentAtomId, this.currentPropertyBar.viewModel.tempContent).done((data: any) => {
                        currentApp.router.updateData(data);
                    }).always((data: any) => currentApp.state.currentReadyState = ReadyState.Ok);
                }
                else {
                    // do nothing TODO document => use button to delete layout atom instead, shows escape behaviour in current implementation
                    (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.InstancedOnLayoutId == this.currentPropertyBar.viewModel.editedLayoutAtomId) as ContentAtom).TextContent = this.currentPropertyBar.viewModel.tempOriginalContent; // TODO expensive
                }
            }
        }
        this.currentPropertyBar.resetContentAtomEditMode();
    };

    private contentAtomKeyDownHandler = (evt: KeyboardEvent) => { // TODO code duplication
        if (evt.keyCode == 13 /*ENTER*/) {
            evt.preventDefault();
            (evt.target as HTMLInputElement).blur();
        }
        else if (evt.keyCode == 27 /*ESC*/) {
            evt.preventDefault();
            (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.InstancedOnLayoutId == this.currentPropertyBar.viewModel.editedLayoutAtomId) as ContentAtom).TextContent = this.currentPropertyBar.viewModel.tempOriginalContent; // TODO expensive
            this.currentPropertyBar.resetContentAtomEditMode();
            (evt.target as HTMLInputElement).blur();
        }
        else if (evt.keyCode == undefined /*input focus lost*/) {
            evt.preventDefault();
        }
        // TODO clean whitespaces
        // TODO autosize
    };

    private contentAtomInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempContent = (evt.target as HTMLInputElement).value;
        (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.InstancedOnLayoutId == this.currentPropertyBar.viewModel.editedLayoutAtomId) as ContentAtom).TextContent = this.currentPropertyBar.viewModel.tempContent; // TODO expensive // TODO a nicer way would be to prevent update in rendered page and trigger formula update; formula is destroyed when rendered content changes
    };

    private layoutAtomClickHandler = (evt: MouseEvent) => {
        evt.preventDefault(); // TODO this is set for every content atom in preview... performance?
        if (currentApp.state.currentReadyState !== ReadyState.Ok) {
            console.log("pending...");
            return;
        }
        if (currentApp.state.currentSelectionMode === SelectionMode.Content) {
            let contentAtomId: number = parseIntFromAttribute(evt.target, "cid");
            let layoutAtomId: number = parseIntFromAttribute(evt.target, "aid");
            let hostedContentAtom: ContentAtom = (currentApp.clientData.CaliforniaProject.ContentAtoms.find(c => c.ContentAtomId == contentAtomId) as ContentAtom); // TODO expensive (2 copies of content)
            this.currentPropertyBar.viewModel.tempContent = "";
            if (hostedContentAtom.ContentAtomType === ContentAtomType.Text) { // TODO code duplication for content selection at multiple places
                this.currentPropertyBar.viewModel.tempContent = hostedContentAtom.TextContent as string;
            }
            else if (hostedContentAtom.ContentAtomType === ContentAtomType.Link) {
                this.currentPropertyBar.viewModel.tempContent = hostedContentAtom.Url as string;
            }
            else {
                console.log(DEFAULT_EXCEPTION);
                return;
            }
            this.currentPropertyBar.viewModel.tempOriginalContent = this.currentPropertyBar.viewModel.tempContent;
            this.currentPropertyBar.viewModel.editedLayoutAtomId = layoutAtomId;
        }
        else {
            // TODO
        }
    };

    public renderStateModifierSelectors = (): VNode => {
        let stateModifierGroupStyles = {
            "display": "flex",
            "flex-flow": "row nowrap"
        };
        let stateModifiers: string[] = [];
        let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleMoleculeId == this.currentPropertyBar.viewModel.selectedStyleMoleculeId) as StyleMolecule; // TODO find used repeatedly for render controls
        for (let i = 0; i < styleMolecule.MappedStyleAtoms.length; i++) {
            let modifier: string | undefined = styleMolecule.MappedStyleAtoms[i].StateModifier;
            if (modifier === undefined) {
                modifier = "";
            }
            if (stateModifiers.findIndex(s => s === modifier) == -1) {
                stateModifiers.push(modifier);
            }
        }
        let renderedModifiers: VNode[] = [];
        for (let i = 0; i < stateModifiers.length; i++) {
            let modifier: string = stateModifiers[i];
            let modifierButtonStyles = {
                "flex": "0 0 auto",
                "background-color": modifier == this.currentPropertyBar.viewModel.selectedStateModifier ? "red" : undefined
            };
            renderedModifiers.push(<button key={modifier} role="button" mid={modifier} onclick={this.currentPropertyBar.stateModifierClickHandler} styles={modifierButtonStyles}>{modifier}</button> as VNode);
        }

        return <div key="-2" styles={stateModifierGroupStyles}>
            {renderedModifiers}
        </div> as VNode;
    };

    public stateModifierClickHandler = (evt: MouseEvent) => {
        let selectedStateModifier: string = parseStringFromAttribute(evt.target, "mid");
        if (selectedStateModifier === this.currentPropertyBar.viewModel.selectedStateModifier) {
            this.currentPropertyBar.viewModel.selectedStateModifier = "";
        }
        else {
            this.currentPropertyBar.viewModel.selectedStateModifier = selectedStateModifier;
        }
    };

    public renderResponsiveDeviceSelectors = (): VNode => {
        let responsiveGroupStyles = {
            "display": "flex",
            "flex-flow": "row wrap"
        };
        return <div key="-3" styles={responsiveGroupStyles}>
            {(currentApp.clientData.CaliforniaProject.ResponsiveDevices !== undefined) ? currentApp.clientData.CaliforniaProject.ResponsiveDevices.map(r => {
                let responsiveButtonStyles = {
                    "flex": "0 0 auto",
                    "background-color": r.ResponsiveDeviceId == this.currentPropertyBar.viewModel.selectedResponsiveDeviceId ? "red" : undefined
                };
                let responsiveDeviceIdString: string = r.ResponsiveDeviceId.toString();
                return <button key={responsiveDeviceIdString} role="button" rid={responsiveDeviceIdString} onclick={this.currentPropertyBar.selectResponsiveDeviceClickHandler} styles={responsiveButtonStyles}>{r.NameShort}</button> as VNode;
            }) : undefined}
        </div> as VNode;
    };

    public selectResponsiveDeviceClickHandler = (evt: MouseEvent) => {
        let selectedResponsiveId: number = parseIntFromAttribute(evt.target, "rid");
        if (this.currentPropertyBar.viewModel.selectedResponsiveDeviceId == selectedResponsiveId) {
            this.currentPropertyBar.viewModel.selectedResponsiveDeviceId = currentApp.state.noneResponsiveDeviceId;
        }
        else {
            this.currentPropertyBar.viewModel.selectedResponsiveDeviceId = selectedResponsiveId;
        }
    };

    public renderStyleAtomControls = (): VNode => {
        return <div key="-1">
            <select key="0"
                onchange={this.currentPropertyBar.styleAtomTypeChangedHandler}>
                {getArrayForEnum(StyleAtomType).map((type: string, index: number) => {
                    let isSelected: boolean = index === this.currentPropertyBar.viewModel.selectedStyleAtomType;
                    return isSelected ? <option selected key={index} value={index.toString()}>{type}</option> : <option key={index} value={index.toString()}>{type}</option>;
                })}
            </select>
            <input key="-1"
                placeholder={"optional :hover,:before,..."}
                value={this.currentPropertyBar.viewModel.tempPseudoSelector}
                oninput={this.currentPropertyBar.pseudoSelectorInputHandler}>
            </input>
            <button key="a" role="button" onclick={this.currentPropertyBar.createStyleAtomForMoleculeClickHandler}>+</button>
        </div> as VNode;
    };

    public pseudoSelectorInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempPseudoSelector = (evt.target as HTMLInputElement).value;
    };

    public styleAtomTypeChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        let parsedStyleAtomType: number | undefined = undefined;
        if (targetSelect.selectedIndex < targetSelect.childElementCount) {
            let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
            parsedStyleAtomType = parseInt(selectOptionElement.value);
        }
        if (parsedStyleAtomType !== undefined) {
            this.currentPropertyBar.viewModel.selectedStyleAtomType = parsedStyleAtomType;
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public createStyleAtomForMoleculeClickHandler = (evt: MouseEvent) => {
        if (this.currentPropertyBar.viewModel.tempPseudoSelector !== "") {
            this.currentPropertyBar.viewModel.selectedStateModifier = this.currentPropertyBar.viewModel.tempPseudoSelector;
        }
        currentApp.controller.CreateStyleAtomForMoleculeJson(this.currentPropertyBar.viewModel.selectedStyleMoleculeId, this.currentPropertyBar.viewModel.selectedStyleAtomType, this.currentPropertyBar.viewModel.selectedResponsiveDeviceId, this.currentPropertyBar.viewModel.selectedStateModifier).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.viewModel.tempPseudoSelector = "";
    };

    public renderStyleQuantumControls = (): VNode => {
        return <div key="0">
            <input key="-3"
                value={this.currentPropertyBar.viewModel.tempQuantumName}
                oninput={this.currentPropertyBar.quantumNameInputHandler}>
            </input>
            <input key="-2"
                value={this.currentPropertyBar.viewModel.tempCssPropertyName}
                oninput={this.currentPropertyBar.cssPropertyNameInputHandler}>
            </input>
            <input key="-1"
                value={this.currentPropertyBar.viewModel.tempCssValue}
                oninput={this.currentPropertyBar.cssValueInputHandler}>
            </input>
            <button key="a" role="button" onclick={this.currentPropertyBar.createStyleQuantumClickHandler}>&#10004;</button>
            <button key="b" role="button" onclick={this.currentPropertyBar.showAllCssPropertiesForQuantumClickHandler}>?</button>
        </div> as VNode;
    };

    public showAllCssPropertiesForQuantumClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.AllCssPropertiesForQuantum);
    };

    public createStyleQuantumClickHandler = (evt: MouseEvent) => {
        currentApp.controller.CreateStyleQuantumJson(currentApp.clientData.CaliforniaProject.CaliforniaProjectId, this.currentPropertyBar.viewModel.tempQuantumName, this.currentPropertyBar.viewModel.tempCssPropertyName, this.currentPropertyBar.viewModel.tempCssValue).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.resetAddQuantumState();
    };

    public resetAddQuantumState = (): void => {
        this.currentPropertyBar.viewModel.tempQuantumName = "Quantum";
        this.currentPropertyBar.viewModel.tempCssPropertyName = "";
        this.currentPropertyBar.viewModel.tempCssValue = "";
    };

    public quantumNameInputHandler = (evt: KeyboardEvent): void => {
        this.currentPropertyBar.viewModel.tempQuantumName = (evt.target as HTMLInputElement).value;
    };

    public renderStyleValueArray = (propertyBar: PropertyBar): maquette.Mapping<StyleValue, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<StyleValue, any>(
            function getSectionSourceKey(source: StyleValue) {
                return source.StyleValueId;
            },
            function createSectionTarget(source: StyleValue) {
                let sourceIdString = source.StyleValueId.toString();
                let styleValueButtonStyle = {
                    "flex": "0 0 auto",
                    "width": "auto",
                    "height": "1rem"
                };
                let styleValueTextStyle = {
                    "outline": source.CssValue === "" ? "solid white 1px" : undefined,
                    "outline-offset": source.CssValue === "" ? "-1px" : undefined,
                    "flex": "0 0 auto",
                    "width": "auto",
                    "margin": "0"
                };
                return {
                    renderMaquette: function () {
                        return <div key={sourceIdString} exitAnimation={propertyBar.styleElementExitAnimation} styles={{ "display":"flex", "flex-flow": "row nowrap"}}>
                            <p styles={styleValueTextStyle}>{source.CssProperty}: {source.CssValue}</p>
                            <button key="a" role="button" vid={sourceIdString} onclick={propertyBar.deleteStyleValueClickHandler} styles={styleValueButtonStyle}>X</button>
                            <button key="b" role="button" aid={source.StyleAtomId.toString()} vid={sourceIdString} onclick={propertyBar.updateCssValueClickHandler} styles={styleValueButtonStyle}>Edit</button>
                        </div> as VNode;
                    },
                    update: function (updatedSource: StyleValue) {
                        source = updatedSource;
                        sourceIdString = source.StyleValueId.toString();
                    }
                };
            },
            function updateSectionTarget(updatedSource: StyleValue, target: { renderMaquette(): any, update(updatedSource: StyleValue): void }) {
                target.update(updatedSource);
            });
    };

    public renderStyleQuantumArrayForStyleAtom = (): maquette.Mapping<StyleQuantum, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<StyleQuantum, any>(
            function getSectionSourceKey(source: StyleQuantum) {
                return source.StyleQuantumId;
            },
            function createSectionTarget(source: StyleQuantum) {
                let sourceIdString = source.StyleQuantumId.toString();
                return {
                    renderMaquette: function () {
                        return <div key={sourceIdString}>
                            <p styles={{"margin": "0"}}>{source.Name}: {source.CssProperty} ({source.CssValue})</p>
                        </div> as VNode;
                    },
                    update: function (updatedSource: StyleQuantum) {
                        source = updatedSource;
                        sourceIdString = source.StyleQuantumId.toString();
                    }
                };
            },
            function updateSectionTarget(updatedSource: StyleQuantum, target: { renderMaquette(): any, update(updatedSource: StyleQuantum): void }) {
                target.update(updatedSource);
            });
    };

    public renderStyleAtomArray = (propertyBar: PropertyBar): maquette.Mapping<StyleAtom, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<StyleAtom, any>(
            function getSectionSourceKey(source: StyleAtom) {
                return source.StyleAtomId;
            },
            function createSectionTarget(source: StyleAtom) {
                let styleAtomIdString = source.StyleAtomId.toString();
                let appliedValuesMap = propertyBar.renderStyleValueArray(propertyBar);
                let appliedQuantumsMap = propertyBar.renderStyleQuantumArrayForStyleAtom();
                if (source.AppliedValues !== undefined) {
                    appliedValuesMap.map(source.AppliedValues);
                }
                else {
                    appliedValuesMap.map([]);
                }
                if (source.MappedQuantums !== undefined) {
                    appliedQuantumsMap.map(source.MappedQuantums.map(qm => qm.StyleQuantum));
                }
                else {
                    appliedQuantumsMap.map([]);
                }
                return {
                    renderMaquette: function () {
                        let isDisplayStyleAtom: boolean = true;
                        if (propertyBar.viewModel.currentPropertyBarMode === PropertyBarMode.StyleMolecule) {
                            let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleMoleculeId == propertyBar.viewModel.selectedStyleMoleculeId) as StyleMolecule;// TODO slow very expensive
                            let targetMappingIndex: number = styleMolecule.MappedStyleAtoms.findIndex(m => m.ResponsiveDeviceId == propertyBar.viewModel.selectedResponsiveDeviceId && m.StyleMoleculeAtomMappingId == source.MappedToMoleculeId && ((m.StateModifier === undefined && propertyBar.viewModel.selectedStateModifier === "") || (m.StateModifier === propertyBar.viewModel.selectedStateModifier)));
                            isDisplayStyleAtom = targetMappingIndex != -1;
                        }
                        let divStyleAtomStyles = {
                            "display": !isDisplayStyleAtom ? "none" : undefined,
                            "width": "100%",
                            "height": "auto"
                        };
                        return <div key={styleAtomIdString}
                            exitAnimation={propertyBar.styleElementExitAnimation}
                            styles={divStyleAtomStyles}>
                            <p key="0" styles={{ "margin": "0" }}>(#{styleAtomIdString}){source.Name}:</p>
                            {appliedValuesMap.results.map(r => r.renderMaquette())}
                            <button key="a" role="button" aid={styleAtomIdString} onclick={propertyBar.createCssPropertyForAtomClickHandler}>+</button>
                            <button key="b" role="button" aid={styleAtomIdString} onclick={propertyBar.moveStyleAtomPopupClickHandler}>=></button>
                            {source.IsDeletable ? <button key="b0" role="button" aid={styleAtomIdString} onclick={propertyBar.deleteStyleAtomClickHandler}>X</button> : <button disabled key="b1" role="button" aid={styleAtomIdString}>X</button>}
                            {source.MappedQuantums.length > 0 ? <p key="-1">quantums:</p> : undefined} {appliedQuantumsMap.results.map(r => r.renderMaquette())}
                        </div>;
                    },
                    update: function (updatedSource: StyleAtom) {
                        source = updatedSource;
                        appliedValuesMap.map(updatedSource.AppliedValues);
                        appliedQuantumsMap.map(updatedSource.MappedQuantums.map(qm => qm.StyleQuantum));
                    }
                };
            },
            function updateSectionTarget(updatedSource: StyleAtom, target: { renderMaquette(): any, update(updatedSource: StyleAtom): void }) {
                target.update(updatedSource);
            });
    };

    public moveStyleAtomPopupClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.selectedStyleAtomIdForPopup = parseIntFromAttribute(evt.target, "aid");
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveStyleAtom);
    };

    public createCssPropertyForAtomClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.selectedStyleAtomId = parseIntFromAttribute(evt.target, "aid");
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.AddCssProperty);
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.AllCssProperties); // TODO render first popup, then use renderd question mark button as target (intermediate popup dom)
    };

    public deleteStyleAtomClickHandler = (evt: MouseEvent) => {
        currentApp.controller.DeleteStyleAtomJson(parseIntFromAttribute(evt.target, "aid")).done(data => currentApp.router.updateData(data));
    };
    
    public updateCssValueClickHandler = (evt: MouseEvent) => {
        let styleValueId: number = parseIntFromAttribute(evt.target, "vid");
        let styleAtomId: number = parseIntFromAttribute(evt.target, "aid");
        this.currentPropertyBar.viewModel.selectedStyleValueId = styleValueId;
        this.currentPropertyBar.viewModel.selectedStyleAtomId = styleAtomId;
        let targetStyleValue: StyleValue = currentApp.clientData.CaliforniaProject.StyleValues.find(val => val.StyleValueId == styleValueId) as StyleValue;
        this.currentPropertyBar.viewModel.tempCssValue = targetStyleValue.CssValue;
        this.currentPropertyBar.viewModel.tempCssPropertyName = targetStyleValue.CssProperty;
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.UpdateCssValue);
    };

    public deleteStyleValueClickHandler = (evt: MouseEvent) => {
        currentApp.controller.DeleteStyleValueJson(parseIntFromAttribute(evt.target, "vid")).done(data => currentApp.router.updateData(data));
    };

    public renderAddCssPropertyPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.AddCssProperty || this.viewModel.currentPopupMode === PopupMode.AllCssProperties;
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.AddCssProperty]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.saveCssPropertyForAtomClickHandler}>&#10004;</button>
                <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelAddCssPropertyForAtomClickHandler}>x</button>
            </div>
            <div>
                <input key="-1"
                    value={this.currentPropertyBar.viewModel.tempCssPropertyName}
                    oninput={this.currentPropertyBar.cssPropertyNameInputHandler}>
                </input>
                <button key="a" role="button" onclick={this.currentPropertyBar.showAllCssPropertiesClickHandler}>?</button>
            </div>
        </div> as VNode;
    };

    public renderAllCssPropertiesForQuantumPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.AllCssPropertiesForQuantum;
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.AllCssPropertiesForQuantum]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31" /*TODO document*/, "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelUpdateCssPropertyForQuantumClickHandler}>x</button>
            </div>
            {currentApp.clientData.AllCssProperties.map((prop: string) => {
                return <div key={prop}>
                    {prop}<button key="a" role="button" cid={prop} onclick={this.currentPropertyBar.setSelectedCssPropertyForQuantumClickHandler}>&#10004;</button>
                </div>;
            })}
        </div> as VNode;
    };

    public setSelectedCssPropertyForQuantumClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.tempCssPropertyName = parseStringFromAttribute(evt.target, "cid");
        this.currentPropertyBar.closePopup();
    };

    public cancelUpdateCssPropertyForQuantumClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };

    public insertLayoutRowIntoViewPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.InsertLayoutRowIntoView;
        let instanceableLayoutRows: LayoutRow[] = [];
        if (isPopupVisible) { // TODO will break when data is not supplied
            let instanceableRowsView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] Instanceable Layout Rows") as CaliforniaView; // TODO magic string => const export
            instanceableLayoutRows.push(...instanceableRowsView.PlacedLayoutRows);

            let userInstanceableRowsView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] User Layout Molecules") as CaliforniaView; // TODO magic string => const export
            for (let i = 1; i < userInstanceableRowsView.PlacedLayoutRows.length; i++) { // skip box holder
                instanceableLayoutRows.push(userInstanceableRowsView.PlacedLayoutRows[i]);
            }
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.InsertLayoutRowIntoView]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelInsertLayoutRowIntoViewClickHandler}>x</button>
            </div>
            {instanceableLayoutRows.map((prop: LayoutRow) => {
                return <div key={prop.LayoutBaseId}>
                    <button styles={{ "width": "auto", "margin": "0" }} key="a"
                        lid={prop.LayoutBaseId.toString()}
                        onclick={this.currentPropertyBar.insertSelectedLayoutRowIntoViewClickHandler}
                        onmouseenter={this.currentPropertyBar.insertRowShowPreviewHandler}
                        onmouseleave={this.currentPropertyBar.insertRowHidePreviewHandler}>&#10004;
                    </button>
                    <p key="0" styles={{ "-webkit-user-select": "none", "width": "auto", "margin": "0", "float": "left" }} lid={prop.LayoutBaseId.toString()} ontouchstart={this.currentPropertyBar.insertRowShowPreviewHandler} ontouchend={this.currentPropertyBar.insertRowHidePreviewHandler}>{prop.LayoutBaseId}</p>
                </div>;
            })}
        </div> as VNode;
    };

    public insertSelectedLayoutRowIntoViewClickHandler = (evt: MouseEvent) => {
        let layoutId: number = parseIntFromAttribute(evt.target, "lid");
        currentApp.controller.CreateLayoutRowForViewJson(this.currentPropertyBar.viewModel.selectedCaliforniaViewId, layoutId).done(data => currentApp.router.updateData(data));
        currentApp.state.lastCommand = CaliforniaEvent.CreateLayoutRowForView;
        currentApp.state.lastCaliforniaEventData = [this.currentPropertyBar.viewModel.selectedCaliforniaViewId, layoutId];
        this.currentPropertyBar.closePopup();
    };

    public cancelInsertLayoutRowIntoViewClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };

    public insertLayoutAtomIntoBoxPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.InsertLayoutAtomIntoBox;
        let instanceableLayoutAtoms: LayoutAtom[] = [];
        if (isPopupVisible) { // TODO will break when data is not supplied
            let instanceableAtomsView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] Instanceable Layout Atoms") as CaliforniaView; // TODO magic string => const export
            let atomContainerBox: LayoutBox = instanceableAtomsView.PlacedLayoutRows[0].AllBoxesBelowRow.find(b => b.PlacedInBoxAtoms.length > 0) as LayoutBox;
            instanceableLayoutAtoms.push(...atomContainerBox.PlacedInBoxAtoms);
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.InsertLayoutAtomIntoBox]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelInsertLayoutAtomIntoBoxClickHandler}>x</button>
            </div>
            {instanceableLayoutAtoms.map((prop: LayoutAtom) => {
                let textPreview: string = "";
                if (prop.HostedContentAtom.ContentAtomType === ContentAtomType.Text) { // TODO code duplication for content selection at multiple places
                    textPreview = prop.HostedContentAtom.TextContent as string;
                }
                else if (prop.HostedContentAtom.ContentAtomType === ContentAtomType.Link) {
                    textPreview = prop.HostedContentAtom.Url as string;
                }
                else {
                    console.log(DEFAULT_EXCEPTION);
                    return;
                }
                return <div key={prop.LayoutBaseId}>
                    <button styles={{ "width": "auto", "margin": "0" }} key="a"
                        lid={prop.LayoutBaseId.toString()}
                        onclick={this.currentPropertyBar.insertSelectedLayoutAtomIntoBoxClickHandler}
                        onmouseenter={this.currentPropertyBar.insertAtomShowPreviewHandler}
                        onmouseleave={this.currentPropertyBar.insertAtomHidePreviewHandler}>&#10004;
                    </button>
                    <p key="0" styles={{ "-webkit-user-select": "none", "width": "auto", "margin": "0", "float": "left" }} lid={prop.LayoutBaseId.toString()} ontouchstart={this.currentPropertyBar.insertAtomShowPreviewHandler} ontouchend={this.currentPropertyBar.insertAtomHidePreviewHandler}>{prop.LayoutBaseId} {textPreview}</p>
                </div>;
            })}
        </div> as VNode;
    };

    public insertSelectedLayoutAtomIntoBoxClickHandler = (evt: MouseEvent) => {
        let targetBoxId: number = currentApp.state.selectedLayoutBaseId;
        currentApp.controller.CreateLayoutAtomForBoxJson(targetBoxId, parseIntFromAttribute(evt.target, "lid")).done(data => {
            currentApp.router.updateData(data);
            currentApp.projector.renderNow(); // TODO
            let updatedSubAtoms: LayoutAtom[] = (currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == targetBoxId) as LayoutBox).PlacedInBoxAtoms;
            this.currentPropertyBar.viewModel.editedLayoutAtomId = updatedSubAtoms[updatedSubAtoms.length - 1].LayoutBaseId; // TODO just guessing, save all existing and compare to all new for safe variant
        });
        this.currentPropertyBar.closePopup();
    };

    public cancelInsertLayoutAtomIntoBoxClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };

    public displayPopup = (targetPosition: HTMLElement, popupMode: PopupMode) => {
        /*if (CaliforniaApp.CaliforniaAppInstance.state.currentPopupMode !== PopupMode.None) { TODO
            return;
        }*/
        let popupElement: HTMLElement | null = null;
        popupElement = document.getElementById(`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[popupMode]}`) as HTMLElement;
        if (popupElement !== null) {
            this.viewModel.currentPopupMode = popupMode;
            var displayPopup = new popperjs.default(targetPosition, popupElement, {
                placement: 'bottom-end',
                modifiers: {
                    /*flip: {
                        behavior: ['left', 'bottom', 'top']
                    },*/
                    preventOverflow: {
                        boundariesElement: document.body,
                    }
                },
            });
            //document.body.style.backgroundColor = "rgb(245, 245, 245)"; TODO
            currentApp.projector.renderNow(); // required to update popup position to be contained by boundaries element
            return;
        }
        console.log(DEFAULT_EXCEPTION);
    };

    public closePopup = () => {
        //document.body.style.background = "white"; TODO
        this.viewModel.currentPopupMode = PopupMode.None;
        this.viewModel.currentSecondaryPopupMode = PopupSecondaryMode.None;
    };

    public insertLayoutBoxIntoBoxPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.SelectBox;
        let instanceableLayoutBoxes: LayoutBox[] = [];
        if (isPopupVisible) { // TODO will break when data is not supplied
            let instanceableRowsView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] Instanceable Layout Rows") as CaliforniaView; // TODO magic string => const export
            let allBoxes: LayoutBox[] = instanceableRowsView.PlacedLayoutRows[0].AllBoxesBelowRow;
            let firstSubBox: LayoutBox = allBoxes.find(b => b.PlacedBoxInBoxId === undefined) as LayoutBox;
            //let targetBox: LayoutBox = allBoxes.find(b => b.PlacedBoxInBoxId == firstSubBox.LayoutBaseId) as LayoutBox; TODO kept as reference for previous project default with 2x2 boxes (now 1x2)
            instanceableLayoutBoxes.push(firstSubBox);

            let userInstanceableView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] User Layout Molecules") as CaliforniaView; // TODO magic string => const export
            let userBoxes: LayoutBox[] = userInstanceableView.PlacedLayoutRows[0].AllBoxesBelowRow.filter(b => b.PlacedBoxInBoxId === undefined);
            instanceableLayoutBoxes.push(...userBoxes);
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.SelectBox]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelInsertLayoutBoxIntoBoxClickHandler}>x</button>
            </div>
            {instanceableLayoutBoxes.map((prop: LayoutBox) => {
                return <div key={prop.LayoutBaseId}>
                    <button styles={{ "width": "auto", "margin": "0" }}
                        key="a"
                        role="button"
                        lid={prop.LayoutBaseId.toString()}
                        onclick={this.currentPropertyBar.insertSelectedLayoutBoxIntoBoxOrRowClickHandler}
                        onmouseenter={this.currentPropertyBar.insertBoxShowPreviewHandler}
                        onmouseleave={this.currentPropertyBar.insertBoxHidePreviewHandler}>
                        &#10004;
                    </button>
                    <p key="0" styles={{ "-webkit-user-select": "none", "width": "auto", "margin": "0", "float": "left" }} lid={prop.LayoutBaseId.toString()} ontouchstart={this.currentPropertyBar.insertBoxShowPreviewHandler} ontouchend={this.currentPropertyBar.insertBoxHidePreviewHandler}>{prop.LayoutBaseId}</p>
                </div>;
            })}
        </div> as VNode;
    };

    private insertRowShowPreviewHandler = (evt: MouseEvent | TouchEvent) => { // TODO code duplication
        let targetElement: HTMLElement = evt.target as HTMLElement;
        let hoveredLayoutId: number = parseIntFromAttribute(targetElement, "lid");
        currentApp.state.hoveredInsertLayoutBaseId = hoveredLayoutId;
        let tempRow: LayoutRow = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == hoveredLayoutId) as LayoutRow;
        currentApp.state.backupSortOrder = tempRow.LayoutSortOrderKey;
        tempRow.LayoutSortOrderKey = VERY_HIGH_VALUE; // TODO very high value
        let californiaView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == this.currentPropertyBar.viewModel.selectedCaliforniaViewId) as CaliforniaView;
        californiaView.PlacedLayoutRows.push(tempRow);
        currentApp.router.setActiveCaliforniaView(californiaView); // TODO inconsistent data model leads to this manual refresh / boxtree not in sync
    };

    private insertRowHidePreviewHandler = (evt: MouseEvent | TouchEvent) => { // TODO code duplication
        let californiaView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == this.currentPropertyBar.viewModel.selectedCaliforniaViewId) as CaliforniaView;
        let tempRowIndex: number = californiaView.PlacedLayoutRows.findIndex(r => r.LayoutBaseId == currentApp.state.hoveredInsertLayoutBaseId);
        if (tempRowIndex != -1) {
            let tempRow: LayoutRow = californiaView.PlacedLayoutRows.splice(tempRowIndex, 1)[0];
            if (currentApp.state.backupSortOrder !== undefined) {
                tempRow.LayoutSortOrderKey = currentApp.state.backupSortOrder;
            }
            else {
                console.log(DEFAULT_EXCEPTION);
            }
            currentApp.state.backupSortOrder = undefined;
            currentApp.state.hoveredInsertLayoutBaseId = 0;
            currentApp.router.setActiveCaliforniaView(californiaView);
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    private insertBoxShowPreviewHandler = (evt: MouseEvent | TouchEvent) => { // TODO code duplication
        // TODO test / check is it necessary to add subbox references to parentRow=>allBoxesBelowRow for this temporary update?
        let targetElement: HTMLElement = evt.target as HTMLElement;
        let hoveredLayoutId: number = parseIntFromAttribute(targetElement, "lid");
        currentApp.state.hoveredInsertLayoutBaseId = hoveredLayoutId;
        let tempBox: LayoutBox = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == hoveredLayoutId) as LayoutBox;
        currentApp.state.backupSortOrder = tempBox.LayoutSortOrderKey;
        tempBox.LayoutSortOrderKey = VERY_HIGH_VALUE; // TODO very high value
        let selectedBoxOrRow: LayoutBase = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == currentApp.state.selectedLayoutBaseId) as LayoutBase;
        if (selectedBoxOrRow.LayoutType === LayoutType.Box) {
            // seems to work without changing layout box owner row, unlike when inserting directly into layout row
            (selectedBoxOrRow as LayoutBox).PlacedInBoxBoxes.push(tempBox);
        }
        else if (selectedBoxOrRow.LayoutType === LayoutType.Row) {
            //let tempNewOwnerRow: LayoutRow = selectedBoxOrRow as LayoutRow;
            let tempNewOwnerRow: LayoutRow = (currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == currentApp.pagePreviewVM.activeCaliforniaViewId) as CaliforniaView).PlacedLayoutRows.find(ro => ro.LayoutBaseId == currentApp.state.selectedLayoutBaseId) as LayoutRow;
            tempNewOwnerRow.AllBoxesBelowRow.push(tempBox);
            currentApp.state.backupOwnerRowId = tempBox.BoxOwnerRowId;
            tempBox.BoxOwnerRowId = tempNewOwnerRow.LayoutBaseId;
            tempBox.BoxOwnerRow = tempNewOwnerRow;
            currentApp.state.backupPlacedBoxInBoxId = tempBox.PlacedBoxInBoxId;
            tempBox.PlacedBoxInBoxId = undefined;
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
        currentApp.router.setActiveCaliforniaView(currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == currentApp.pagePreviewVM.activeCaliforniaViewId) as CaliforniaView); // TODO inconsistent data model leads to this manual refresh / boxtree not in sync
    };

    private insertBoxHidePreviewHandler = (evt: MouseEvent | TouchEvent) => { // TODO code duplication
        let selectedBoxOrRow: LayoutBase = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == currentApp.state.selectedLayoutBaseId) as LayoutBase;
        if (selectedBoxOrRow.LayoutType === LayoutType.Box) {
            let layoutBox: LayoutBox = selectedBoxOrRow as LayoutBox;
            let tempBoxIndex: number = layoutBox.PlacedInBoxBoxes.findIndex(b => b.LayoutBaseId == currentApp.state.hoveredInsertLayoutBaseId);
            if (tempBoxIndex != -1) {
                let tempBox: LayoutBox = layoutBox.PlacedInBoxBoxes.splice(tempBoxIndex, 1)[0];
                if (currentApp.state.backupSortOrder !== undefined) {
                    tempBox.LayoutSortOrderKey = currentApp.state.backupSortOrder;
                }
                else {
                    // value always set
                    console.log(DEFAULT_EXCEPTION);
                }
                currentApp.state.backupSortOrder = undefined;
                currentApp.state.hoveredInsertLayoutBaseId = 0;
            }
            else {
                console.log(DEFAULT_EXCEPTION);
            }
        }
        else if (selectedBoxOrRow.LayoutType === LayoutType.Row) {
            let layoutRow: LayoutRow = selectedBoxOrRow as LayoutRow;
            let tempBoxIndex: number = layoutRow.AllBoxesBelowRow.findIndex(b => b.LayoutBaseId == currentApp.state.hoveredInsertLayoutBaseId);
            if (tempBoxIndex != -1) {
                let tempBox: LayoutBox = layoutRow.AllBoxesBelowRow.splice(tempBoxIndex, 1)[0];
                if (currentApp.state.backupSortOrder !== undefined) {
                    tempBox.LayoutSortOrderKey = currentApp.state.backupSortOrder;
                }
                else {
                    console.log(DEFAULT_EXCEPTION);
                }
                if (currentApp.state.backupOwnerRowId !== undefined) {
                    // value set only in certain cases
                    let backupRow = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(r => r.LayoutBaseId == currentApp.state.backupOwnerRowId) as LayoutRow;
                    tempBox.BoxOwnerRowId = currentApp.state.backupOwnerRowId;
                    tempBox.BoxOwnerRow = backupRow;
                    if (currentApp.state.backupPlacedBoxInBoxId !== undefined) {
                        tempBox.PlacedBoxInBoxId = currentApp.state.backupPlacedBoxInBoxId;
                    }
                    currentApp.state.backupOwnerRowId = undefined;
                    currentApp.state.backupPlacedBoxInBoxId = undefined;
                }
                currentApp.state.backupSortOrder = undefined;
                currentApp.state.hoveredInsertLayoutBaseId = 0;
            }
            else {
                console.log(DEFAULT_EXCEPTION);
            }
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
        currentApp.router.setActiveCaliforniaView(currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == currentApp.pagePreviewVM.activeCaliforniaViewId) as CaliforniaView); // TODO inconsistent data model leads to this manual refresh / boxtree not in sync
    };

    private insertAtomShowPreviewHandler = (evt: MouseEvent | TouchEvent) => { // TODO code duplication
        // TODO everywhere make use of int8 and fix enum/bool to int32 default coercions
        let targetElement: HTMLElement = evt.target as HTMLElement;
        let hoveredLayoutId: number = parseIntFromAttribute(targetElement, "lid");
        currentApp.state.hoveredInsertLayoutBaseId = hoveredLayoutId;
        let tempAtom: LayoutAtom = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == hoveredLayoutId) as LayoutAtom;
        currentApp.state.backupSortOrder = tempAtom.LayoutSortOrderKey;
        tempAtom.LayoutSortOrderKey = VERY_HIGH_VALUE; // TODO very high value
        let layoutBox: LayoutBox = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == currentApp.state.selectedLayoutBaseId) as LayoutBox;
        layoutBox.PlacedInBoxAtoms.push(tempAtom);
    };

    private insertAtomHidePreviewHandler = (evt: MouseEvent | TouchEvent) => { // TODO code duplication
        let layoutBox: LayoutBox = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == currentApp.state.selectedLayoutBaseId) as LayoutBox;
        let tempAtomIndex: number = layoutBox.PlacedInBoxAtoms.findIndex(a => a.LayoutBaseId == currentApp.state.hoveredInsertLayoutBaseId);
        if (tempAtomIndex != -1) {
            let tempAtom: LayoutAtom = layoutBox.PlacedInBoxAtoms.splice(tempAtomIndex, 1)[0];
            if (currentApp.state.backupSortOrder !== undefined) {
                tempAtom.LayoutSortOrderKey = currentApp.state.backupSortOrder;
            }
            else {
                console.log(DEFAULT_EXCEPTION);
            }
            currentApp.state.backupSortOrder = undefined;
            currentApp.state.hoveredInsertLayoutBaseId = 0;
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public insertSelectedLayoutBoxIntoBoxOrRowClickHandler = (evt: MouseEvent) => {
        // TODO document popup is reused
        if (this.viewModel.currentSecondaryPopupMode === PopupSecondaryMode.SelectBoxIntoBox) {
            let layoutId: number = parseIntFromAttribute(evt.target, "lid");
            let targetLayoutId: number = currentApp.state.selectedLayoutBaseId;
            currentApp.controller.CreateLayoutBoxForBoxOrRowJson(targetLayoutId, layoutId).done(data => {
                currentApp.router.updateData(data);
                // TODO focus last atom if one was created
            });
            currentApp.state.lastCommand = CaliforniaEvent.CreateLayoutBoxForBoxOrRow;
            currentApp.state.lastCaliforniaEventData = [currentApp.state.selectedLayoutBaseId, layoutId];
        }
        else if (this.viewModel.currentSecondaryPopupMode === PopupSecondaryMode.SelectBoxIntoBoxAtomInPlace) {
            currentApp.controller.CreateLayoutBoxForAtomInPlaceJson(currentApp.state.selectedLayoutBaseId, parseIntFromAttribute(evt.target, "lid")).done(data => {
                currentApp.router.updateData(data);
                // TODO focus last atom if one was created
            });
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }        
        this.currentPropertyBar.closePopup();
    };

    public cancelInsertLayoutBoxIntoBoxClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };

    /*public moveLayoutMoleculeIntoPopup = (): VNode => {
        let thisPopupMode: PopupMode = PopupMode.MoveLayoutMoleculeIntoLayoutMolecule;
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === thisPopupMode;
        let isDataLoaded: boolean = currentApp.clientData.CaliforniaProject !== undefined && currentApp.clientData.CaliforniaProject.ResponsiveDevices !== undefined; // TODO code duplication / state
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[thisPopupMode]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelLayoutMoleculeIntoClickHandler}>x</button>
            </div>
            {isDataLoaded ? currentApp.clientData.CaliforniaProject.LayoutMolecules.map((layoutMolecule: LayoutBase) => {
                if (layoutMolecule.LayoutType === LayoutType.Atom) {
                    return undefined;
                }
                let layoutBaseIdString: string = layoutMolecule.LayoutBaseId.toString();
                let sourceStyleMoleculeIdString = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutMolecule.LayoutBaseId) as StyleMolecule).StyleMoleculeId.toString(); // TODO expensive
                return <div key={layoutBaseIdString}>
                    <button key="a" lid={layoutBaseIdString} onclick={this.currentPropertyBar.moveLayoutMoleculeIntoLayoutMoleculeClickHandler}>#{layoutBaseIdString} {LayoutType[layoutMolecule.LayoutType]} style #{sourceStyleMoleculeIdString}</button>
                </div>
            }) : undefined}
        </div> as VNode;
    };
    
    public moveLayoutMoleculeIntoLayoutMoleculeClickHandler = (evt: MouseEvent) => {
        currentApp.controller.MoveLayoutMoleculeIntoLayoutMoleculeJson(currentApp.state.selectedLayoutBaseId, parseIntFromAttribute(evt.target, "lid")).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.closePopup();
    };

    public cancelLayoutMoleculeIntoClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };*/

    public moveLayoutMoleculeIntoLayoutMolecule = () => {
        currentApp.controller.MoveLayoutMoleculeIntoLayoutMoleculeJson(currentApp.state.preselectedLayoutBaseId, currentApp.state.selectedLayoutBaseId).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.closePopup();
    };

    /*public moveLayoutMoleculeBeforePopup = (): VNode => { TODO unused
        let thisPopupMode: PopupMode = PopupMode.MoveLayoutMoleculeBeforeLayoutMolecule;
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === thisPopupMode;
        let isDataLoaded: boolean = currentApp.clientData.CaliforniaProject !== undefined && currentApp.clientData.CaliforniaProject.ResponsiveDevices !== undefined; // TODO code duplication / state
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[thisPopupMode]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelLayoutMoleculeBeforeClickHandler}>x</button>
            </div>
            {isDataLoaded ? currentApp.clientData.CaliforniaProject.LayoutMolecules.map((layoutMolecule: LayoutBase) => {
                let layoutBaseIdString: string = layoutMolecule.LayoutBaseId.toString();
                let sourceStyleMoleculeIdString = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutMolecule.LayoutBaseId) as StyleMolecule).StyleMoleculeId.toString(); // TODO expensive
                return <div key={layoutBaseIdString}>
                    <button key="a" lid={layoutBaseIdString} onclick={this.currentPropertyBar.moveLayoutMoleculeBeforeLayoutMoleculeClickHandler}>#{layoutBaseIdString} {LayoutType[layoutMolecule.LayoutType]} style #{sourceStyleMoleculeIdString}</button>
                </div>
            }) : undefined}
        </div> as VNode;
    };

    public moveLayoutMoleculeBeforeLayoutMoleculeClickHandler = (evt: MouseEvent) => {
        currentApp.controller.MoveLayoutMoleculeNextToLayoutMoleculeJson(currentApp.state.selectedLayoutBaseId, parseIntFromAttribute(evt.target, "lid"), true).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.closePopup();
    };

    public cancelLayoutMoleculeBeforeClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };*/

    public moveLayoutMoleculeBeforeLayoutMolecule = () => {
        currentApp.controller.MoveLayoutMoleculeNextToLayoutMoleculeJson(currentApp.state.preselectedLayoutBaseId, currentApp.state.selectedLayoutBaseId, true).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.closePopup();
    };

    public syncLayoutMoleculeStylesImitatingReferenceLayout = () => {
        // TODO everywhere checks move/duplicate to client
        currentApp.controller.SyncLayoutStylesImitatingReferenceLayoutJson(currentApp.state.selectedLayoutBaseId, currentApp.state.preselectedLayoutBaseId).done(data => currentApp.router.updateData(data)); // TODO concept: target/preselect order changes
        this.currentPropertyBar.closePopup();
    };

    public moveStyleAtomToResponsiveDevicePopup = (): VNode => {
        let thisPopupMode: PopupMode = PopupMode.MoveStyleAtom;
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === thisPopupMode;
        let isDataLoaded: boolean = currentApp.clientData.CaliforniaProject !== undefined && currentApp.clientData.CaliforniaProject.ResponsiveDevices !== undefined; // TODO code duplication / state
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[thisPopupMode]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelMoveStyleAtomClickHandler}>x</button>
            </div>
            {isDataLoaded ? currentApp.clientData.CaliforniaProject.ResponsiveDevices.map((responsiveDevice: ResponsiveDevice) => {
                let responsiveDeviceIdString: string = responsiveDevice.ResponsiveDeviceId.toString();
                let isSelectedResponsiveDeviceInPropertyBar: boolean = this.currentPropertyBar.viewModel.selectedResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId;
                return <div key={responsiveDeviceIdString}>
                    {!isSelectedResponsiveDeviceInPropertyBar ? <button key="a" rid={responsiveDeviceIdString} onclick={this.currentPropertyBar.moveStyleAtomToResponsiveDeviceClickHandler}>{responsiveDevice.NameShort}</button> :
                        <button disabled key="a0" rid={responsiveDeviceIdString} onclick={this.currentPropertyBar.moveStyleAtomToResponsiveDeviceClickHandler}>{responsiveDevice.NameShort}</button>
                    }
                </div>
            }) : undefined}
        </div> as VNode;
    };

    public moveStyleAtomToResponsiveDeviceClickHandler = (evt: MouseEvent) => {
        let targetResponsiveDeviceId: number = parseIntFromAttribute(evt.target, "rid");
        currentApp.controller.MoveStyleAtomToResponsiveDeviceJson(this.currentPropertyBar.viewModel.selectedStyleAtomIdForPopup, targetResponsiveDeviceId).done(data => {
            currentApp.router.updateData(data);
            this.currentPropertyBar.viewModel.selectedResponsiveDeviceId = targetResponsiveDeviceId;
        });
        this.currentPropertyBar.viewModel.selectedStyleAtomIdForPopup = 0;
        this.currentPropertyBar.closePopup();
    };

    public cancelMoveStyleAtomClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.selectedStyleAtomIdForPopup = 0;
        this.currentPropertyBar.closePopup();
    };

    public renderAllCssPropertiesPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.AllCssProperties;
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.AllCssProperties]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelUpdateCssPropertylickHandler}>x</button>
            </div>
            {currentApp.clientData.AllCssProperties.map((prop: string) => {
                let isPropertyUnmapped: boolean = true;
                let isPropertyVisible: boolean = false;
                if (this.currentPropertyBar.viewModel.selectedStyleAtomId != 0 && currentApp.clientData.CaliforniaProject !== undefined
                    && currentApp.clientData.CaliforniaProject.StyleAtoms !== undefined) {
                    let targetAtom: StyleAtom | undefined = currentApp.clientData.CaliforniaProject.StyleAtoms.find(s => s.StyleAtomId == this.currentPropertyBar.viewModel.selectedStyleAtomId);
                    if (targetAtom !== undefined && targetAtom.AppliedValues !== undefined) {
                        isPropertyVisible = currentApp.clientData.StyleAtomCssPropertyMapping[StyleAtomType[targetAtom.StyleAtomType]].findIndex(p => p === prop) != -1;
                        isPropertyUnmapped = targetAtom.AppliedValues.findIndex(v => v.CssProperty === prop) == -1;
                    }
                }
                return isPropertyVisible ? <div key={prop}>
                    {prop}{ isPropertyUnmapped ? <button key="a0" role="button" cid={prop} onclick={this.currentPropertyBar.setSelectedCssPropertyClickHandler}>&#10004;</button>
                        : <button disabled key="a1" role="button" cid={prop} onclick={this.currentPropertyBar.setSelectedCssPropertyClickHandler}>&#10004;</button>}
                </div> : undefined;
            })}
        </div> as VNode;
    };

    public cancelUpdateCssPropertylickHandler = (evt: MouseEvent) => {
        this.viewModel.currentPopupMode = PopupMode.AddCssProperty;
    };

    public showAllCssPropertiesClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.AllCssProperties);
    };

    public setSelectedCssPropertyClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.tempCssPropertyName = parseStringFromAttribute(evt.target, "cid");
        this.currentPropertyBar.saveCssPropertyForAtom();
    };

    public saveCssPropertyForAtomClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.saveCssPropertyForAtom();
    };

    private saveCssPropertyForAtom = (): void => {
        currentApp.controller.CreateStyleValueForAtomJson(this.currentPropertyBar.viewModel.selectedStyleAtomId, this.currentPropertyBar.viewModel.tempCssPropertyName).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.resetTempCssPropertyState();
    };

    public cancelAddCssPropertyForAtomClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.resetTempCssPropertyState();
    };

    public resetTempCssPropertyState = (): void => {
        this.currentPropertyBar.viewModel.tempCssPropertyName = "";
        this.currentPropertyBar.viewModel.tempCssValue = "";
        this.currentPropertyBar.viewModel.selectedStyleAtomId = 0;
        this.currentPropertyBar.viewModel.selectedStyleValueId = 0;
        this.currentPropertyBar.viewModel.selectedStyleQuantumId = 0;
        this.currentPropertyBar.closePopup();
    };

    public cssPropertyNameInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempCssPropertyName = (evt.target as HTMLInputElement).value;
    };

    public renderUpdateCssValuePopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.UpdateCssValue || this.viewModel.currentPopupMode === PopupMode.MatchingQuantums;
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.UpdateCssValue]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.saveUpdatedCssValueClickHandler}>&#10004;</button>
                <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelUpdateCssValueClickHandler}>x</button>
            </div>
            <div>
                <input key="-1"
                    value={this.currentPropertyBar.viewModel.tempCssValue}
                    oninput={this.currentPropertyBar.cssValueInputHandler}>
                </input>
                <button key="a" role="button" onclick={this.currentPropertyBar.showMatchingQuantumsClickHandler}>?</button>
                <button key="b" role="button" onclick={this.currentPropertyBar.showSuggestedCssValuesClickHandler}>??</button>
                <button key="c" role="button" onclick={this.currentPropertyBar.setTempCssToZeroClickHandler}>0</button>
                <button key="d" role="button" onclick={this.currentPropertyBar.setTempCssToNoneClickHandler}>none</button>
                <button key="e" role="button" onclick={this.currentPropertyBar.setTempCssToNullClickHandler}>null</button>
                <button key="f" role="button" onclick={this.currentPropertyBar.setTempCssToAutoClickHandler}>auto</button>
                {this.currentPropertyBar.viewModel.lastUsedTempCssValue !== "" ? <button key="g" role="button" onclick={this.currentPropertyBar.setTempCssAppendLastUsedClickHandler}>+{this.currentPropertyBar.viewModel.lastUsedTempCssValue.length > 10 ? this.currentPropertyBar.viewModel.lastUsedTempCssValue.substring(0, 10) + "..." : this.currentPropertyBar.viewModel.lastUsedTempCssValue}</button> : undefined}
            </div>
        </div> as VNode;
    };

    public showSuggestedCssValuesClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.SuggestedCssValues);
    };

    public setTempCssToZeroClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.viewModel.tempCssValue = "0";
    };

    public setTempCssToNullClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.viewModel.tempCssValue = "null";
    };

    public setTempCssToNoneClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.viewModel.tempCssValue = "none";
    };

    public setTempCssToAutoClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.viewModel.tempCssValue = "auto";
    };

    public setTempCssAppendLastUsedClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.viewModel.tempCssValue = this.currentPropertyBar.viewModel.tempCssValue + this.currentPropertyBar.viewModel.lastUsedTempCssValue;
    };

    public renderUpdateCssQuantumPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.UpdateCssQuantum;
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.UpdateCssQuantum]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.saveUpdatedCssQuantumClickHandler}>&#10004;</button>
                <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelUpdateCssQuantumClickHandler}>x</button>
            </div>
            <div>
                <input key="-1"
                    value={this.currentPropertyBar.viewModel.tempCssValue}
                    oninput={this.currentPropertyBar.cssValueInputHandler}>
                </input>
            </div>
        </div> as VNode;
    };

    public showMatchingQuantumsClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MatchingQuantums);
    };

    public saveUpdatedCssQuantumClickHandler = (evt: MouseEvent) => {
        currentApp.controller.UpdateStyleQuantumJson(this.currentPropertyBar.viewModel.selectedStyleQuantumId, this.currentPropertyBar.viewModel.tempCssValue).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.viewModel.lastUsedTempCssValue = this.currentPropertyBar.viewModel.tempCssValue;
        this.currentPropertyBar.resetTempCssPropertyState();
    };

    public cancelUpdateCssQuantumClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.resetTempCssPropertyState();
    };

    public saveUpdatedCssValueFromAttrClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.tempCssValue = parseStringFromAttribute(evt.target, "fid");
        this.currentPropertyBar.saveUpdatedCssValue();
    };

    public saveUpdatedCssValueClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.saveUpdatedCssValue();
    };

    public saveUpdatedCssValue = () => {
        currentApp.controller.UpdateStyleValueJson(this.currentPropertyBar.viewModel.selectedStyleValueId, this.currentPropertyBar.viewModel.tempCssValue).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.viewModel.lastUsedTempCssValue = this.currentPropertyBar.viewModel.tempCssValue;
        this.currentPropertyBar.resetTempCssPropertyState();
    };

    public cancelUpdateCssValueClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.resetTempCssPropertyState();
    };

    public cssValueInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempCssValue = (evt.target as HTMLInputElement).value;
    };

    public cssValueForInteractionInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempCssValueForInteraction = (evt.target as HTMLInputElement).value;
    };

    public renderSelectInteractionTargetPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.SelectInteractionTarget;
        let renderedOptions: VNode[] = [];
        if (isPopupVisible === true) {
            currentApp.clientData.CaliforniaProject.LayoutMolecules.map(m => {
                let layoutBaseIdString: string = m.LayoutBaseId.toString();
                renderedOptions.push(<div key={layoutBaseIdString} styles={{ "flex": "0 0 100%", "width": "100%", "min-width": "100%" }}>
                    layout #{layoutBaseIdString} <button key="a" role="button" bid={layoutBaseIdString} onclick={this.currentPropertyBar.selectLayoutBaseForInteractionTargetClickHandler}>&#10004;</button>
                </div> as VNode);
            });
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.SelectInteractionTarget]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div key="0" styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelSelectInteractionTargetClickHandler}>x</button>
            </div>
            <div key="1" styles={{ "display": "flex", "flex-flow": "row wrap" }}>
                {renderedOptions}
            </div>
        </div> as VNode;
    };

    public renderEditUserDefinedCssPopup = (): VNode => {
        // TODO prevent closing popup before user discarded/confirmed changes
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.EditUserDefinedCss;
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.EditUserDefinedCss]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index": "31", "background-color": "white", "border": "solid black 1px" }}>
            <div key="0" styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.confirmEditUserDefinedCssClickHandler}>&#10004;</button>
                <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelEditUserDefinedCssClickHandler}>x</button>
            </div>
            <div key="1">
                <textarea
                    value={this.currentPropertyBar.viewModel.tempUserDefinedCss}
                    oninput={this.currentPropertyBar.userDefinedCssInputHandler}>
                </textarea>
            </div>
        </div> as VNode;
    };

    private userDefinedCssInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempUserDefinedCss = (evt.target as HTMLTextAreaElement).value;
    };

    public updateUserDefinedCss = () => {
        if (this.currentPropertyBar.viewModel.tempUserDefinedCss !== undefined) {
            currentApp.controller.UpdateUserDefinedCssForViewJson(currentApp.state.preselectedCaliforniaViewId, this.currentPropertyBar.viewModel.tempUserDefinedCss).done(data => currentApp.router.updateData(data));
            this.currentPropertyBar.cancelEditUserDefinedCss();
        }
    };

    public cancelEditUserDefinedCssClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.cancelEditUserDefinedCss();
    };

    public confirmEditUserDefinedCssClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.updateUserDefinedCss();
    };

    public cancelEditUserDefinedCss = () => {
        this.currentPropertyBar.viewModel.tempUserDefinedCss = "";
        currentApp.state.preselectedCaliforniaViewId = 0;
        this.currentPropertyBar.closePopup();
    };

    public selectLayoutBaseForInteractionTargetClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.viewModel.selectedLayoutBaseIdForFilter = parseIntFromAttribute(evt.target, "bid");
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.SelectInteractionTargetLayoutFilter);
    };

    public cancelSelectInteractionTargetClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
        this.currentPropertyBar.viewModel.selectedLayoutBaseIdForFilter = 0;
        this.currentPropertyBar.viewModel.selectedLayoutStyleInteraction = 0;
    };

    public renderSelectInteractionTargetLayoutFilterPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.SelectInteractionTargetLayoutFilter;
        let renderedOptions: VNode[] = [];
        if (isPopupVisible === true) {
            currentApp.clientData.CaliforniaProject.StyleValues.map(v => {
                let styleAtom: StyleAtom = currentApp.clientData.CaliforniaProject.StyleAtoms.find(a => a.AppliedValues.findIndex(map => map.StyleValueId == v.StyleValueId) != -1) as StyleAtom; // TODO expensive
                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.MappedStyleAtoms.findIndex(map => map.StyleMoleculeAtomMappingId == styleAtom.MappedToMoleculeId) != -1) as StyleMolecule; // TODO expensive
                if (styleMolecule.StyleForLayoutId == this.currentPropertyBar.viewModel.selectedLayoutBaseIdForFilter) {
                    let styleValueIdString: string = v.StyleValueId.toString();
                    renderedOptions.push(<div key={styleValueIdString} styles={{ "flex": "0 0 100%", "width": "100%", "min-width": "100%" }}>
                        value #{styleValueIdString}: {v.CssProperty}:{v.CssValue} <button key="a" role="button" vid={styleValueIdString} onclick={this.currentPropertyBar.selectStyleValueForInteractionTargetClickHandler}>&#10004;</button>
                    </div> as VNode);
                }
            });
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.SelectInteractionTargetLayoutFilter]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div key="0" styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelSelectStyleValueForInteractionTargetClickHandler}>x</button>
            </div>
            <div key="1" styles={{ "display": "flex", "flex-flow": "row wrap" }}>
                {renderedOptions}
            </div>
        </div> as VNode;
    };

    public selectStyleValueForInteractionTargetClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
        currentApp.controller.CreateStyleValueInteractionJson(this.currentPropertyBar.viewModel.selectedLayoutStyleInteraction, parseIntFromAttribute(evt.target, "vid"), this.currentPropertyBar.viewModel.tempCssValueForInteraction).done(data => currentApp.router.updateData(data));
    };

    public cancelSelectStyleValueForInteractionTargetClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
        this.currentPropertyBar.viewModel.selectedLayoutBaseIdForFilter = 0;
        this.currentPropertyBar.viewModel.selectedLayoutStyleInteraction = 0;
    };

    public renderMatchingQuantumsPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.MatchingQuantums;
        let renderedOptions: VNode[] = [];
        if (isPopupVisible === true) {
            currentApp.clientData.CaliforniaProject.StyleQuantums.map(quantum => {
                let isMatchingProperty: boolean = quantum.CssProperty === this.currentPropertyBar.viewModel.tempCssPropertyName;
                if (isMatchingProperty === true) {
                    renderedOptions.push(<div key={quantum.StyleQuantumId}>
                        {quantum.Name} = {quantum.CssValue} <button key="a" role="button" qid={quantum.StyleQuantumId.toString()} onclick={this.currentPropertyBar.setQuantumOnAtomClickHandler}>&#10004;</button>
                    </div> as VNode);
                }
            });
            if (renderedOptions.length == 0) {
                renderedOptions.push(<div key="0">No quantums available.</div> as VNode);
            }
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.MatchingQuantums]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelSelectMatchingCssQuantumClickHandler}>x</button>
            </div>
            {renderedOptions}
        </div> as VNode;
    };

    public cancelSelectMatchingCssQuantumClickHandler = (evt: MouseEvent) => {
        this.viewModel.currentPopupMode = PopupMode.UpdateCssValue;
    };

    public renderSuggestedCssValuesPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.SuggestedCssValues;
        let renderedOptions: VNode[] = [];
        if (isPopupVisible === true) {
            if (this.currentPropertyBar.viewModel.tempCssPropertyName === "font-family") {
                currentApp.clientData.ThirdPartyFonts.map((family: string, index: number) => {
                    renderedOptions.push(<div key={index}>
                        Google Font: {family} <button key="a" role="button" fid={family} onclick={this.currentPropertyBar.saveUpdatedCssValueFromAttrClickHandler}>&#10004;</button>
                    </div> as VNode);
                });
            }
            if (renderedOptions.length == 0) {
                renderedOptions.push(<div key="0">No suggestions available.</div> as VNode); // TODO disable popup button when no suggestions available for property
            }
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.SuggestedCssValues]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index": "31", "background-color": "white", "border": "solid black 1px", "height": "300px", "overflow": "scroll" }}>
            <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelSuggestedCssValuesClickHandler}>x</button>
            </div>
            {renderedOptions}
        </div> as VNode;
    };
    
    public cancelSuggestedCssValuesClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };
    
    public setQuantumOnAtomClickHandler = (evt: MouseEvent) => {
        let quantumId: number = parseIntFromAttribute(evt.target, "qid");
        currentApp.controller.ApplyStyleQuantumToAtomJson(this.currentPropertyBar.viewModel.selectedStyleAtomId, quantumId).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.resetTempCssPropertyState();
        this.currentPropertyBar.viewModel.lastUsedTempCssValue = (currentApp.clientData.CaliforniaProject.StyleQuantums.find(q => q.StyleQuantumId == quantumId) as StyleQuantum).CssValue; // TODO expensive
    };

    public renderStyleMoleculeArray = (propertyBar: PropertyBar): maquette.Mapping<StyleMolecule, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<StyleMolecule, any>(
            function getSectionSourceKey(source: StyleMolecule) {
                return source.StyleMoleculeId;
            },
            function createSectionTarget(source: StyleMolecule) {
                let sourceStyleMoleculeIdString = source.StyleMoleculeId.toString();
                return {
                    renderMaquette: function () {
                        return <div key={sourceStyleMoleculeIdString}
                            exitAnimation={propertyBar.styleElementExitAnimation}>
                            <p>(#{sourceStyleMoleculeIdString}){source.Name}</p>
                            <button key="a" role="button" mid={sourceStyleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>Edit</button>
                        </div>;
                    },
                    update: function (updatedSource: StyleMolecule) {
                        sourceStyleMoleculeIdString = updatedSource.StyleMoleculeId.toString();
                        source = updatedSource;
                    }
                };
            },
            function updateSectionTarget(updatedSource: StyleMolecule, target: { renderMaquette(): any, update(updatedSource: StyleMolecule): void }) {
                target.update(updatedSource);
            });
    };

    public selectStyleMoleculeClickHandler = (evt: MouseEvent) => {
        this.nextExceptLastPropertyBar.viewModel.selectedStyleMoleculeId = parseIntFromAttribute(evt.target, "mid");
        this.nextExceptLastPropertyBar.viewModel.selectedStateModifier = ""; // TODO reset state only when not available in new selected molecule
        this.nextExceptLastPropertyBar.viewModel.currentPropertyBarMode = PropertyBarMode.StyleMolecule;
    };

    public showEditUserDefinedCssClickHandler = (evt: MouseEvent) => {
        let californiaViewId: number = parseIntFromAttribute(evt.target, "vid");
        let californiaViewCss: string | undefined = (currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == californiaViewId) as CaliforniaView).UserDefinedCss;
        this.currentPropertyBar.viewModel.tempUserDefinedCss = californiaViewCss !== undefined ? californiaViewCss : "";
        currentApp.state.preselectedCaliforniaViewId = californiaViewId;
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.EditUserDefinedCss);
    };

    public highlightLayoutBaseClickHandler = (evt: MouseEvent) => {
        let targetLayoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.highlightedLayoutBaseId != targetLayoutBaseId) {
            currentApp.state.highlightedLayoutBaseId = targetLayoutBaseId;
        }
        else {
            currentApp.state.highlightedLayoutBaseId = 0;
        }
    };

    public renderStyleQuantumArray = (propertyBar: PropertyBar): maquette.Mapping<StyleQuantum, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<StyleQuantum, any>(
            function getSectionSourceKey(source: StyleQuantum) {
                return source.StyleQuantumId;
            },
            function createSectionTarget(source: StyleQuantum) {
                let sourceIdString = source.StyleQuantumId.toString();
                return {
                    renderMaquette: function () {
                        return <div key={sourceIdString}
                            exitAnimation={propertyBar.styleElementExitAnimation}>
                            <p key="0" styles={{"margin":"0"}}>(#{sourceIdString}){source.Name}: {source.CssProperty} => {source.CssValue}</p>
                            <button key="a" role="button" qid={sourceIdString} onclick={propertyBar.duplicateStyleQuantumClickHandler}>DD</button>
                            {source.IsDeletable ? <button key="b0" role="button" qid={sourceIdString} onclick={propertyBar.deleteStyleQuantumClickHandler}>X</button> : <button disabled key="b1" role="button">X</button>}
                            <button key="c" role="button" qid={sourceIdString} onclick={propertyBar.updateCssQuantumClickHandler}>Edit</button>
                        </div>;
                    },
                    update: function (updatedSource: StyleQuantum) {
                        sourceIdString = updatedSource.StyleQuantumId.toString();
                        source = updatedSource;
                    }
                };
            },
            function updateSectionTarget(updatedSource: StyleQuantum, target: { renderMaquette(): any, update(updatedSource: StyleQuantum): void }) {
                target.update(updatedSource);
            });
    };

    public updateCssQuantumClickHandler = (evt: MouseEvent) => {
        let styleQuantumId: number = parseIntFromAttribute(evt.target, "qid");
        this.currentPropertyBar.viewModel.selectedStyleQuantumId = styleQuantumId;
        let targetStyleQuantum: StyleQuantum = currentApp.clientData.CaliforniaProject.StyleQuantums.find(val => val.StyleQuantumId == styleQuantumId) as StyleQuantum;
        this.currentPropertyBar.viewModel.tempCssValue = targetStyleQuantum.CssValue;
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.UpdateCssQuantum);
    };

    public deleteStyleQuantumClickHandler = (evt: MouseEvent): void => {
        currentApp.controller.DeleteStyleQuantumJson(parseIntFromAttribute(evt.target, "qid")).done(data => currentApp.router.updateData(data));
    };

    public duplicateStyleQuantumClickHandler = (evt: MouseEvent): void => {
        currentApp.controller.DuplicateStyleQuantumJson(parseIntFromAttribute(evt.target, "qid")).done(data => currentApp.router.updateData(data));
    };

    public styleElementExitAnimation = (domNode: HTMLElement, removeElement: () => void, properties?: maquette.VNodeProperties) => {
        domNode.style.overflow = "hidden";
        velocity.animate(domNode, { opacity: 0.5, height: 0 }, { duration: 100, easing: "ease-out", complete: removeElement });
    };
    
    public renderLayoutMoleculeArray = (propertyBar: PropertyBar): maquette.Mapping<LayoutBase, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<LayoutBase, any>(
            function getSectionSourceKey(source: LayoutBase) {
                // function that returns a key to uniquely identify each item in the data
                return source.LayoutBaseId;
            },
            function createSectionTarget(source: LayoutBase) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let sourceLayoutBaseIdString = source.LayoutBaseId.toString();
                let sourceStyleMoleculeIdString = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule).StyleMoleculeId.toString(); // TODO expensive
                let layoutControlButtonStyles = {
                    "margin-right": "5px"
                };
                return {
                    renderMaquette: function () {
                        let description: string | undefined = "";
                        if (source.LayoutType === LayoutType.Atom) {
                            let sourceLayoutAtom: LayoutAtom = (source as LayoutAtom);
                            let textContentString: string = "";
                            if (sourceLayoutAtom.HostedContentAtom.ContentAtomType === ContentAtomType.Text) {
                                textContentString = sourceLayoutAtom.HostedContentAtom.TextContent as string;
                            }
                            else if (sourceLayoutAtom.HostedContentAtom.ContentAtomType === ContentAtomType.Link) {
                                textContentString = sourceLayoutAtom.HostedContentAtom.Url as string;
                            }
                            else {
                                console.log(DEFAULT_EXCEPTION);
                            }
                            description = textContentString.length > 20 ? textContentString.substring(0, 20) + "..." : textContentString; // TODO expensive // TODO ellipsis // TODO multiple places // TODO create when storing in DB? or when loading in client
                            description += ` in box #${(source as LayoutAtom).PlacedAtomInBoxId}`
                        }
                        // TODO hide layout molecules, where style molecule is internal style
                        return <div key={sourceLayoutBaseIdString}> {LayoutType[source.LayoutType].toString()} #{sourceLayoutBaseIdString} {description}
                            <button key="a" role="button" lid={sourceLayoutBaseIdString} onclick={propertyBar.selectLayoutBaseClickHandler} styles={layoutControlButtonStyles}>?</button>
                            <button key="b" role="button" mid={sourceStyleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler} styles={layoutControlButtonStyles}>S</button>
                            <button key="c" role="button" lid={sourceLayoutBaseIdString} onclick={propertyBar.deleteLayoutBaseClickHandler} styles={layoutControlButtonStyles}>X</button>
                        </div>;
                    },
                    update: function (updatedSource: LayoutBase) {
                        source = updatedSource;
                        sourceLayoutBaseIdString = source.LayoutBaseId.toString();
                        let sourceStyleMoleculeIdString = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == source.LayoutBaseId) as StyleMolecule).StyleMoleculeId.toString(); // TODO expensive
                    }
                };
            },
            function updateSectionTarget(updatedSource: LayoutBase, target: { renderMaquette(): any, update(updatedSource: LayoutBase): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public renderCaliforniaViewArray = (propertyBar: PropertyBar): maquette.Mapping<CaliforniaView, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<CaliforniaView, any>(
            function getSectionSourceKey(source: CaliforniaView) {
                // function that returns a key to uniquely identify each item in the data
                return source.CaliforniaViewId;
            },
            function createSectionTarget(source: CaliforniaView) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let sourceCaliforniaViewIdString = source.CaliforniaViewId.toString();
                return {
                    renderMaquette: function () {
                        let isDeleteButtonEnabled: boolean = source.PlacedLayoutRows.length == 0;
                        return <div key={sourceCaliforniaViewIdString}>{source.Name} View #{sourceCaliforniaViewIdString}
                            <button key="a" role="button" vid={sourceCaliforniaViewIdString} onclick={propertyBar.selectCaliforniaViewClickHandler}>:)</button>
                            {(!source.IsInternal && source.CaliforniaViewId != currentApp.pagePreview.viewModel.activeCaliforniaViewId) ? <button key="b" role="button" vid={sourceCaliforniaViewIdString} onclick={propertyBar.activateCaliforniaViewClickHandler}>&#10004;</button> : undefined}
                            : {source.IsInternal ? "internal" : undefined} {source.Name} hosted by {source.HostedByLayoutMappings.length} layouts
                            <button key="c" role="button" mid={source.SpecialStyleViewStyleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>style #{source.SpecialStyleViewStyleMoleculeIdString}</button>
                            <button key="d" role="button" mid={source.SpecialStyleBodyStyleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>body style #{source.SpecialStyleBodyStyleMoleculeIdString}</button>
                            <button key="e" role="button" mid={source.SpecialStyleHtmlStyleMoleculeIdString} onclick={propertyBar.selectStyleMoleculeClickHandler}>HTML style #{source.SpecialStyleHtmlStyleMoleculeIdString}</button>
                            <button key="f" role="button" vid={sourceCaliforniaViewIdString} onclick={propertyBar.showEditUserDefinedCssClickHandler}>CSS</button>
                            {isDeleteButtonEnabled ? <button key="g" role="button" vid={sourceCaliforniaViewIdString} onclick={propertyBar.deleteCaliforniaViewClickHandler}>X</button> : <button disabled key="g0" role="button" onclick={propertyBar.deleteCaliforniaViewClickHandler}>X</button>}
                        </div>;
                    },
                    update: function (updatedSource: CaliforniaView) {
                        source = updatedSource;
                        sourceCaliforniaViewIdString = source.CaliforniaViewId.toString();
                    }
                };
            },
            function updateSectionTarget(updatedSource: CaliforniaView, target: { renderMaquette(): any, update(updatedSource: CaliforniaView): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public logoutPopupClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.ShareCaliforniaProject);
    };

    public renderShareCaliforniaProjectPopup = (): VNode => {
        // TODO multilanguage where text strings are everywhere
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.ShareCaliforniaProject; // TODO shorten ids everywhere
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.ShareCaliforniaProject]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index" : "31", "background-color": "white", "border": "solid black 1px" }}>
            <div key="0" styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="b" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelShareCaliforniaProjectClickHandler}>x</button>
            </div>
            <div key="1" styles={{ "display": "flex", "flex-flow": "row wrap" }}>
                <p key="a" styles={{ "flex": "0 0 100%", "width": "100%", "min-width": "100%" }}>{currentApp.clientData.UrlToReadOnly}</p>
                {/* TODO <p key="b">{currentApp.clientData.UrlToReadAndEdit}</p>*/}
                Bookmark! Clear browser history!
                <button key="c" type="button" onclick={this.currentPropertyBar.logoutClickHandler} styles={{ "flex": "0 0 10%", "width": "10%", "min-width": "10%" }}>&#128274;</button>
                <button key="d" type="button" onclick={this.currentPropertyBar.tokyoClickHandler} styles={{ "flex": "0 0 10%", "width": "10%", "min-width": "10%" }}>TOKYO</button>
            </div>
        </div> as VNode;
    };

    public tokyoClickHandler = (evt: MouseEvent) => {
        window.location.assign(window.location.origin + "/tokyo/"); // TODO hardcoded link
    };

    public cancelShareCaliforniaProjectClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };

    public logoutClickHandler = (evt: MouseEvent) => {
        currentApp.controller.LogoutAction().done((response: any) => {
            window.location.assign(window.location.origin + "/california/"); // TODO hardcoded link
        });
    };

    public activateCaliforniaViewClickHandler = (evt: MouseEvent) => {
        let californiaViewId: number = parseIntFromAttribute(evt.target, "vid");
        let userPages: CaliforniaView[] = currentApp.clientData.CaliforniaProject.CaliforniaViews.filter(view => !view.IsInternal);
        let activeView: CaliforniaView | undefined = undefined;
        let activePageIndex: number = userPages.findIndex(v => v.CaliforniaViewId == californiaViewId);
        if (activePageIndex > -1) {
            activeView = userPages[activePageIndex];
            currentApp.router.setActiveCaliforniaView(activeView);
            currentApp.pagePreview.resetEquationNumbersWhenModifying(true); // TODO test
            this.currentPropertyBar.viewModel.setSelectedCaliforniaView(activeView, true);
            this.viewModel.currentPropertyBarMode = PropertyBarMode.CaliforniaView; // TODO everywhere: this.vieModel or this.currentPropertyBar.viewModel
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public selectLayoutBaseClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid"); // TODO everywhere: use backend value instead of parsing where possible => saves multiple strings
        currentApp.state.selectedLayoutBaseId = layoutBaseId;
        this.currentPropertyBar.viewModel.currentPropertyBarMode = PropertyBarMode.LayoutBase;
    };

    public selectCaliforniaViewClickHandler = (evt: MouseEvent) => {
        let californiaViewId: number = parseIntFromAttribute(evt.target, "vid");
        let userPages: CaliforniaView[] = currentApp.clientData.CaliforniaProject.CaliforniaViews.filter(view => !view.IsInternal);
        let activeView: CaliforniaView | undefined = undefined;
        let activePageIndex: number = userPages.findIndex(v => v.CaliforniaViewId == californiaViewId);
        if (activePageIndex > -1) {
            activeView = userPages[activePageIndex];
            if (this.currentPropertyBar.propertyBarIndex == 0) {
                currentApp.router.setActiveCaliforniaView(activeView);
                currentApp.pagePreview.resetEquationNumbersWhenModifying(true); // TODO test
            }
            else {
                this.currentPropertyBar.viewModel.isSyncedWithBoxTreeToTheLeft = false;
            }
            this.currentPropertyBar.viewModel.setSelectedCaliforniaView(activeView, true);
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
        this.currentPropertyBar.viewModel.currentPropertyBarMode = PropertyBarMode.CaliforniaView;
    };

    public deleteLayoutBaseClickHandler = (evt: MouseEvent) => {
        if (currentApp.state.preselectedLayoutBaseId != 0) { // TODO document // TODO disable button
            return;
        }
        currentApp.state.selectedLayoutBaseId = 0;
        currentApp.controller.DeleteLayoutJson(parseIntFromAttribute(evt.target, "lid"), false).done(data => currentApp.router.updateData(data));
    };

    public deleteBelowLayoutBaseClickHandler = (evt: MouseEvent) => {
        if (currentApp.state.preselectedLayoutBaseId != 0) { // TODO document // TODO disable button
            return;
        }
        currentApp.state.selectedLayoutBaseId = 0;
        currentApp.controller.DeleteLayoutJson(parseIntFromAttribute(evt.target, "lid"), true).done(data => currentApp.router.updateData(data));
    };

    public renderLayoutBaseControls = (): VNode | undefined => {
        if (currentApp.state.selectedLayoutBaseId == 0) {
            return undefined;
        }
        let selectedLayoutBase: LayoutBase = currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == currentApp.state.selectedLayoutBaseId) as LayoutBase;
        let layoutBaseIdString: string = selectedLayoutBase.LayoutBaseId.toString();
        let sourceStyleMoleculeIdString: string = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == selectedLayoutBase.LayoutBaseId) as StyleMolecule).StyleMoleculeId.toString(); // TODO expensive
        if (selectedLayoutBase.LayoutType === LayoutType.Atom) {
            let selectedLayoutAtom: LayoutAtom = selectedLayoutBase as LayoutAtom;
            let isPictureContent: boolean = selectedLayoutAtom.HostedContentAtom.ContentAtomType === ContentAtomType.Picture;
            let pictureContentIdString: string | undefined = isPictureContent ? selectedLayoutAtom.HostedContentAtom.PictureContent.PictureContentId.toString() : undefined;
            return <div key={LayoutType.Atom}> Atom:
                <button key="a" role="button" mid={sourceStyleMoleculeIdString} onclick={this.currentPropertyBar.selectStyleMoleculeClickHandler}>style #{sourceStyleMoleculeIdString}</button>
                <button key="b" role="button" lid={layoutBaseIdString} onclick={this.currentPropertyBar.deleteLayoutBaseClickHandler}>X</button>
                <button key="c" role="button" aid={layoutBaseIdString} onclick={this.currentPropertyBar.createLayoutStyleInteraction}>+ Interaction</button>
                {selectedLayoutAtom.LayoutStyleInteractions.map(interaction => {
                    let interactionIdString: string = interaction.LayoutStyleInteractionId.toString();
                    return <div key={`i${interactionIdString}`}>
                        <p key="0">Interaction #{interaction.LayoutStyleInteractionId}</p>
                        <input key="1"
                            value={this.currentPropertyBar.viewModel.tempCssValueForInteraction}
                            oninput={this.currentPropertyBar.cssValueForInteractionInputHandler}>
                        </input>
                        {this.currentPropertyBar.viewModel.tempCssValueForInteraction !== "" ? <button key="a" role="button" lid={interactionIdString} onclick={this.currentPropertyBar.selectInteractionTargetClickHandler}>?</button> : <button disabled key="a0" role="button" lid={interactionIdString} onclick={this.currentPropertyBar.selectInteractionTargetClickHandler}>?</button>}
                        <button key="b" role="button" lid={interactionIdString} onclick={this.currentPropertyBar.deleteLayoutStyleInteractionClickHandler}>X</button>
                        {interaction.StyleValueInteractions.map(map => {
                            let mappingIdString: string = map.StyleValueInteractionMappingId.toString();
                            return <div key={mappingIdString}>
                                <p key="0">#{mappingIdString}: {map.CssValue}</p>
                                <button key="a" role="button" vid={map.StyleValueId.toString()} lid={interactionIdString} onclick={this.currentPropertyBar.deleteStyleValueInteractionClickHandler}>X</button>
                            </div>
                        })}
                    </div>
                })}
                <form key="0" action="UploadFiles" method="post" enctype="multipart/form-data">
                    <p key="0">picture id #{pictureContentIdString}</p>
                    <input multiple key="1" type="file" name="formFiles" onchange={this.currentPropertyBar.uploadFileChangeHandler}></input>
                    <button key="a" role="button" pid={pictureContentIdString} onclick={this.currentPropertyBar.uploadFileClickHandler}>...</button>
                </form>
            </div> as VNode;
        }
        else if (selectedLayoutBase.LayoutType === LayoutType.Box) {
            let selectedLayoutBox: LayoutBox = selectedLayoutBase as LayoutBox;
            let specialLayoutBoxTypeSelectors: VNode[] = [];
            getArrayForEnum(SpecialLayoutBoxType).map((type: string, index: number) => {
                let isLayoutBoxType: boolean = index == selectedLayoutBox.SpecialLayoutBoxType;
                let layoutBoxTypeString: string = index.toString();
                specialLayoutBoxTypeSelectors.push(isLayoutBoxType ? <option selected key={layoutBoxTypeString} value={layoutBoxTypeString}>{type}</option> as VNode : <option key={layoutBoxTypeString} value={layoutBoxTypeString}>{type}</option> as VNode);
            });
            return <div key={LayoutType.Box}> Box:
                <button key="a" role="button" mid={sourceStyleMoleculeIdString} onclick={this.currentPropertyBar.selectStyleMoleculeClickHandler}>style #{sourceStyleMoleculeIdString}</button>
                <button disabled key="b" role="button" onclick={this.currentPropertyBar.createViewForBoxClickHandler}>Create View</button>
                <button key="c" role="button" lid={layoutBaseIdString} onclick={this.currentPropertyBar.deleteLayoutBaseClickHandler}>X</button>
                <select key="0" bid={layoutBaseIdString}
                    onchange={this.currentPropertyBar.specialLayoutBoxTypeChangedHandler}>
                    {specialLayoutBoxTypeSelectors}
                </select>
            </div> as VNode;
        }
        else if (selectedLayoutBase.LayoutType === LayoutType.Row) {
            let selectedLayoutRow: LayoutRow = selectedLayoutBase as LayoutRow;
            let currentBoxCount: number = selectedLayoutRow.AllBoxesBelowRow.filter(b => b.PlacedBoxInBoxId === undefined).length;
            let boxCountSelectors: VNode[] = [];
            for (let i = 0; i <= 12; i++) {
                let isSelected: boolean = i == currentBoxCount;
                let boxCountString: string = i.toString();
                if (i == 0) {
                    boxCountSelectors.push(isSelected ? <option disabled selected key={boxCountString} value={boxCountString}>{boxCountString}</option> as VNode : <option disabled key={boxCountString} value={boxCountString}>{boxCountString}</option> as VNode);
                }
                else {
                    boxCountSelectors.push(isSelected ? <option selected key={boxCountString} value={boxCountString}>{boxCountString}</option> as VNode : <option key={boxCountString} value={boxCountString}>{boxCountString}</option> as VNode);
                }
            }
            return <div key={LayoutType.Row}> Row:
                <button key="a" role="button" mid={sourceStyleMoleculeIdString} onclick={this.currentPropertyBar.selectStyleMoleculeClickHandler}>style #{sourceStyleMoleculeIdString}</button>
                <button key="b" role="button" lid={layoutBaseIdString} onclick={this.currentPropertyBar.deleteLayoutBaseClickHandler}>X</button>
                <select key="c" rid={layoutBaseIdString}
                    onchange={this.currentPropertyBar.boxCountInRowChangedHandler}>
                    {boxCountSelectors}
                </select>
            </div> as VNode;
        }
        console.log(DEFAULT_EXCEPTION);
        return undefined;
    };

    public uploadFileChangeHandler = (evt: Event): void => {
        let fileSelector: HTMLInputElement = evt.target as HTMLInputElement;
        if (fileSelector.files !== null) {
            let fileArray: File[] = [];
            for (let index in fileSelector.files) {
                let file: File = fileSelector.files[index];
                fileArray.push(file);
                console.log(file);
                let fileReader: FileReader = new FileReader();
                fileReader.addEventListener("loadend", this.currentPropertyBar.fileProcessingLoadEndHandler);
                //fileReader.readAsText(new Blob([file]), undefined);
            }
            if (fileSelector.files.length == 0) {
                console.log("empty");
            }
            //currentApp.controller.UploadFilesAction(fileArray);
        }
        else {
            console.log("undefined");
        }
    };

    public fileProcessingLoadEndHandler = (evt: ProgressEvent): void => {
        console.log(evt.total);
        console.log((evt.target as FileReader).result);
    };

    public uploadFileClickHandler = (evt: MouseEvent): void => {
        evt.preventDefault();
        let targetForm: HTMLFormElement = (evt.target as HTMLButtonElement).form as HTMLFormElement;
        console.log("upload dialog TODO");
        jQuery.ajax(targetForm.action, {
            method: targetForm.method,
            contentType: "multipart/form-data",
            data: $(targetForm).serialize()
        } as JQueryAjaxSettings);
    };

    public selectInteractionTargetClickHandler = (evt: MouseEvent): void => {
        this.currentPropertyBar.viewModel.selectedLayoutStyleInteraction = parseIntFromAttribute(evt.target, "lid"); // TODO hack
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.SelectInteractionTarget);
    };

    public deleteStyleValueInteractionClickHandler = (evt: MouseEvent): void => {
        currentApp.controller.DeleteStyleValueInteractionJson(parseIntFromAttribute(evt.target, "lid"), parseIntFromAttribute(evt.target, "vid")).done(data => currentApp.router.updateData(data));
    };
    
    public deleteLayoutStyleInteractionClickHandler = (evt: MouseEvent): void => {
        currentApp.controller.DeleteLayoutStyleInteractionJson(parseIntFromAttribute(evt.target, "lid")).done(data => currentApp.router.updateData(data));
    };

    public createLayoutStyleInteraction = (evt: MouseEvent): void => {
        currentApp.controller.CreateLayoutStyleInteractionForLayoutAtomJson(parseIntFromAttribute(evt.target, "aid")).done(data => currentApp.router.updateData(data));
    };

    public specialLayoutBoxTypeChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        let selectedSpecialLayoutBoxType: number | undefined = undefined;
        if (targetSelect.selectedIndex < targetSelect.childElementCount) {
            let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
            selectedSpecialLayoutBoxType = parseInt(selectOptionElement.value);
        }
        if (selectedSpecialLayoutBoxType !== undefined) {
            currentApp.controller.SetSpecialLayoutBoxTypeJson(parseIntFromAttribute(targetSelect, "bid"), selectedSpecialLayoutBoxType).done(data => currentApp.router.updateData(data));
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public boxCountInRowChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        let parsedBoxCount: number | undefined = undefined;
        if (targetSelect.selectedIndex < targetSelect.childElementCount) {
            let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
            parsedBoxCount = parseInt(selectOptionElement.value);
        }
        if (parsedBoxCount !== undefined) {
            currentApp.controller.SetLayoutBoxCountForRowOrBoxJson(parseIntFromAttribute(targetSelect, "rid"), currentApp.state.newBoxStyleMoleculeId, parsedBoxCount, false).done(data => currentApp.router.updateData(data));
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public finalizeLayoutRequest = (evt: MouseEvent) => {
        // TODO differentiate mode
        currentApp.state.selectedLayoutBaseId = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.currentTransactionMode === TransactionMode.MoveLayoutMoleculeIntoLayoutMolecule) {
            this.currentPropertyBar.moveLayoutMoleculeIntoLayoutMolecule();
            currentApp.state.preselectedLayoutBaseId = 0;
        }
        else if (currentApp.state.currentTransactionMode === TransactionMode.MoveLayoutMoleculeBeforeLayoutMolecule) {
            this.currentPropertyBar.moveLayoutMoleculeBeforeLayoutMolecule();
            currentApp.state.preselectedLayoutBaseId = 0;
        }
        else if (currentApp.state.currentTransactionMode === TransactionMode.SyncLayoutStylesImitating) {
            this.currentPropertyBar.syncLayoutMoleculeStylesImitatingReferenceLayout(); // TODO document or rework sticky preselection
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
            console.log(DEFAULT_EXCEPTION);
            return;
        }
    };

    public moveLayoutBoxIntoRowClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeIntoLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeIntoLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public moveLayoutBoxIntoBoxClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeIntoLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeIntoLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public moveLayoutBoxBeforeBoxClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeBeforeLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeBeforeLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public moveLayoutRowBeforeRowClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeBeforeLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeBeforeLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public syncLayoutBaseStylesClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.SyncLayoutStylesImitating;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.UNDEFINED);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public moveLayoutAtomIntoBoxClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeIntoLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeIntoLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public createBoxForAtomInPlaceClickHandler = (evt: MouseEvent) => {
        currentApp.state.selectedLayoutBaseId = parseIntFromAttribute(evt.target, "lid");
        // TODO document: foreign popup is used, controller request differentiation by state
        this.viewModel.currentSecondaryPopupMode = PopupSecondaryMode.SelectBoxIntoBoxAtomInPlace;
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.SelectBox);
    };

    public moveLayoutAtomBeforeAtomClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeBeforeLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeBeforeLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public moveLayoutAtomBeforeBoxClickHandler = (evt: MouseEvent) => {
        let layoutBaseId: number = parseIntFromAttribute(evt.target, "lid");
        if (currentApp.state.preselectedLayoutBaseId != layoutBaseId) {
            currentApp.state.preselectedLayoutBaseId = layoutBaseId;
            currentApp.state.currentTransactionMode = TransactionMode.MoveLayoutMoleculeBeforeLayoutMolecule;
            //this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.MoveLayoutMoleculeBeforeLayoutMolecule);
        }
        else {
            currentApp.state.preselectedLayoutBaseId = 0;
        }
    };

    public saveLayoutMoleculeClickHandler = (evt: MouseEvent) => {
        currentApp.controller.SetLayoutRowOrBoxAsInstanceableJson(currentApp.clientData.CaliforniaProject.CaliforniaProjectId, parseIntFromAttribute(evt.target, "lid")).done(data => currentApp.router.updateData(data));
    };

    public createViewForBoxClickHandler = (evt: MouseEvent) => {
        console.log("TODO");
    };

    public insertLayoutAtomIntoBoxClickHandler = (evt: MouseEvent) => {
        currentApp.state.selectedLayoutBaseId = parseIntFromAttribute(evt.target, "lid");
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.InsertLayoutAtomIntoBox);
    };

    public insertLayoutBoxIntoBoxClickHandler = (evt: MouseEvent) => {
        currentApp.state.selectedLayoutBaseId = parseIntFromAttribute(evt.target, "lid");
        this.viewModel.currentSecondaryPopupMode = PopupSecondaryMode.SelectBoxIntoBox;
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.SelectBox);
    };

    public renderCaliforniaViewControlsWhenAll = (): VNode => {
        let isAddButtonEnabled: boolean = this.currentPropertyBar.viewModel.tempCaliforniaViewName !== "";
        return <div key="-1">
            <input key="0"
                value={this.currentPropertyBar.viewModel.tempCaliforniaViewName}
                oninput={this.currentPropertyBar.californiaViewNameInputHandler}>
            </input>
            {isAddButtonEnabled ? <button key="a" role="button" onclick={this.currentPropertyBar.createCaliforniaViewClickHandler}>&#10004;</button> : <button disabled key="a0" role="button" onclick={this.currentPropertyBar.createCaliforniaViewClickHandler}>&#10004;</button>}
            {isAddButtonEnabled ? <button key="b" role="button" onclick={this.currentPropertyBar.createCaliforniaViewFromReferenceClickHandler}>x2</button> : <button disabled key="b0" role="button" onclick={this.currentPropertyBar.createCaliforniaViewFromReferenceClickHandler}>x2</button>}
        </div> as VNode;
    };

    public createCaliforniaViewFromReferenceClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.CaliforniaViewSelection);
    };

    public createCaliforniaViewClickHandler = (evt: MouseEvent) => {
        currentApp.controller.CreateCaliforniaViewJson(currentApp.clientData.CaliforniaProject.CaliforniaProjectId, this.currentPropertyBar.viewModel.tempCaliforniaViewName).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.viewModel.tempCaliforniaViewName = "";
    };
    
    public californiaViewNameInputHandler = (evt: KeyboardEvent) => {
        this.currentPropertyBar.viewModel.tempCaliforniaViewName = (evt.target as HTMLInputElement).value;
    };

    public renderCaliforniaViewSelectionPopup = (): VNode => {
        let isPopupVisible: boolean = this.viewModel.currentPopupMode === PopupMode.CaliforniaViewSelection; // TODO shorten ids everywhere
        let renderedOptions: VNode[] = [];
        if (isPopupVisible === true) {
            currentApp.clientData.CaliforniaProject.CaliforniaViews.filter(m => !m.IsInternal).map(m => {
                let californiaViewIdString: string = m.CaliforniaViewId.toString();
                renderedOptions.push(<div key={californiaViewIdString} styles={{ "flex": "0 0 100%", "width": "100%", "min-width": "100%" }}>
                    view #{californiaViewIdString}: {m.Name} <button key="a" role="button" vid={californiaViewIdString} onclick={this.currentPropertyBar.selectCaliforniaViewInPopupClickHandler}>&#10004;</button>
                </div> as VNode);
            });
        }
        return <div id={`${this.currentPropertyBar.propertyBarIndex}PopupMode${PopupMode[PopupMode.CaliforniaViewSelection]}`} styles={{ "display": isPopupVisible ? "block" : "none", "z-index": "31", "background-color": "white", "border": "solid black 1px" }}>
            <div key="0" styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                <button key="b" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} onclick={this.currentPropertyBar.cancelSelectCaliforniaViewPopupClickHandler}>x</button>
            </div>
            <div key="1" styles={{ "display": "flex", "flex-flow": "row wrap" }}>
                {renderedOptions}
            </div>
        </div> as VNode;
    };

    public selectCaliforniaViewInPopupClickHandler = (evt: MouseEvent) => {
        currentApp.controller.CreateCaliforniaViewFromReferenceViewJson(currentApp.clientData.CaliforniaProject.CaliforniaProjectId, this.currentPropertyBar.viewModel.tempCaliforniaViewName, parseIntFromAttribute(evt.target, "vid")).done(data => currentApp.router.updateData(data));
        this.currentPropertyBar.viewModel.tempCaliforniaViewName = "";
        this.currentPropertyBar.closePopup();
    };

    public cancelSelectCaliforniaViewPopupClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.closePopup();
    };

    public renderCaliforniaViewControls = (): VNode | undefined => {
        if (this.currentPropertyBar.viewModel.selectedCaliforniaViewId == 0) {
            return undefined;
        }
        let selectedCaliforniaView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.CaliforniaViewId == this.currentPropertyBar.viewModel.selectedCaliforniaViewId) as CaliforniaView; // TODO potentially slow
        let californiaViewIdString: string = selectedCaliforniaView.CaliforniaViewId.toString();
        let viewControlsButtonHolderStyles = {
            "flex": "0 0 auto",
            "height": "auto"
        };
        let viewControlsBoxTreeHolderStyles = {
            /*TODO need set all div styles width/height in this box to either undefined or value // TODO do everywhere!!*/
            "flex": "1 1 1px",
            "width": "100%",
            "height": "auto",
            "overflow": "scroll"
        };// TODO applied at many places: code sense for usage of boxTreeProjector should be coupled with renderBoxTree routine
        let isSyncWithPreviewActive: boolean = this.currentPropertyBar.propertyBarIndex == 0;
        let isSyncWithLeftActive: boolean = this.currentPropertyBar.propertyBarIndex != 0;
        let isDrawHelperLinesActive: boolean = this.currentPropertyBar.propertyBarIndex == 0;
        let syncWithLeftBoxTreeButtonStyles = {
            "outline": !isSyncWithLeftActive ? undefined : this.currentPropertyBar.viewModel.isSyncedWithBoxTreeToTheLeft ? "solid 1px rgb(200,0,0)" : "solid 1px rgb(0,242,0)",
            "outline-offset": !isSyncWithLeftActive ? undefined : "-1px"
        };
        let syncWithPreviewButtonStyles = {
            "outline": !isSyncWithPreviewActive ? undefined : this.currentPropertyBar.viewModel.isSyncedWithPagePreview ? "solid 1px rgb(200,0,0)" : "solid 1px rgb(0,242,0)",
            "outline-offset": !isSyncWithPreviewActive ? undefined : "-1px"
        };
        let drawHelperLinesButtonStyles = {
            "outline": !isDrawHelperLinesActive ? undefined : currentApp.state.isDrawHelperLines ? "solid 1px rgb(200,0,0)" : undefined,
            "outline-offset": !isDrawHelperLinesActive ? undefined : "-1px"
        };
        return <div styles={{ "width": "100%", "height": "100%", "display": "flex", "flex-flow": "column nowrap" }}> View #{californiaViewIdString}
            <div key="0" styles={viewControlsButtonHolderStyles}>
                <button key="a" onclick={this.currentPropertyBar.insertLayoutRowIntoViewClickHandler}>+(R)</button>
                <button key="b" mid={selectedCaliforniaView.SpecialStyleViewStyleMoleculeIdString} onclick={this.currentPropertyBar.selectStyleMoleculeClickHandler}>style #{selectedCaliforniaView.SpecialStyleViewStyleMoleculeIdString}</button>
                <button key="c" mid={selectedCaliforniaView.SpecialStyleBodyStyleMoleculeIdString} onclick={this.currentPropertyBar.selectStyleMoleculeClickHandler}>body style #{selectedCaliforniaView.SpecialStyleBodyStyleMoleculeIdString}</button>
                <button key="d" mid={selectedCaliforniaView.SpecialStyleHtmlStyleMoleculeIdString} onclick={this.currentPropertyBar.selectStyleMoleculeClickHandler}>HTML style #{selectedCaliforniaView.SpecialStyleHtmlStyleMoleculeIdString}</button>
                <button key="e" onclick={this.currentPropertyBar.resetPreselectedLayoutClickHandler}>o</button>
                {isDrawHelperLinesActive ? <button key="f" onclick={this.currentPropertyBar.drawHelperLinesClickHandler} styles={drawHelperLinesButtonStyles}>\-\</button> : <button disabled key="f0" onclick={this.currentPropertyBar.drawHelperLinesClickHandler} styles={drawHelperLinesButtonStyles}>\-\</button>}
                {isSyncWithPreviewActive ? <button key="g" onclick={this.currentPropertyBar.syncWithPagePreviewClickHandler} styles={syncWithPreviewButtonStyles}>-=-</button> : <button disabled key="g0" onclick={this.currentPropertyBar.syncWithPagePreviewClickHandler} styles={syncWithPreviewButtonStyles}>-=-</button>}
                {isSyncWithLeftActive ? <button key="h" onclick={this.currentPropertyBar.syncWithLeftPropertyBarClickHandler} styles={syncWithLeftBoxTreeButtonStyles}>==</button> : <button disabled key="h0" onclick={this.currentPropertyBar.syncWithLeftPropertyBarClickHandler} styles={syncWithLeftBoxTreeButtonStyles}>==</button>}
            </div>
            <div key="1"
                styles={viewControlsBoxTreeHolderStyles}
                onscroll={this.currentPropertyBar.boxTreeScrollHandler}
                afterCreate={this.currentPropertyBar.boxTreeAfterCreateHandler}>
                {this.currentPropertyBar.viewModel.boxTreeProjector.results.map(r => r.renderMaquette())}
            </div>
        </div> as VNode;
    };

    private boxTreeScrollHandler = (evt: UIEvent) => {
        // TODO instead: render in same div to synchronize scroll
        // TODO fix bug => bad handling when using scroll bars in edge...
        // TODO kaleidoscope effect selection / boxtree display range
        // TODO test case: show 4 box trees #1-#4, move #2, sync to left (#1 reads from #2), move #4, sync to left (#3 reads from #4), then activate sync to left in #3 => all movements should be synced
        // --- sync scroll with other property bars ---
        let currentPropertyBarIndex: number = this.currentPropertyBar.propertyBarIndex;
        let currentScrollDom: HTMLDivElement | undefined = currentApp.propertyBarBoxTreeDomReferences[currentPropertyBarIndex] as HTMLDivElement /*TODO cast unsafe*/;
        if (currentApp.state.visiblePropertyBarMaxCount > 1 && currentApp.propertyBarBoxTreeScrollHandled[currentPropertyBarIndex] === false) {
            currentApp.propertyBarBoxTreeScrollHandled[currentPropertyBarIndex] = true;
            //console.log("onscroll for property bar #" + this.currentPropertyBar.propertyBarIndex + " to" + (evt.target as HTMLDivElement).scrollTop + " from max height" + (evt.target as HTMLDivElement).scrollHeight + ", diff: " + ((evt.target as HTMLDivElement).scrollHeight - (evt.target as HTMLDivElement).scrollTop).toString() + ", expected at max scroll: " + (evt.target as HTMLDivElement).clientHeight);
            let currentViewModel: PropertyBarVM = this.currentPropertyBar.viewModel;
            if (currentScrollDom === undefined) {
                console.log(DEFAULT_EXCEPTION);
                return;
            }
            let progressingPropertyBarIndex: number = currentPropertyBarIndex;
            // sync with left + progression to left
            let isKeepGoingLeft: boolean = currentPropertyBarIndex > 0;
            let currentIteration: number = 0;
            let maxIteration: number = currentPropertyBarIndex - 1;
            while (isKeepGoingLeft === true && progressingPropertyBarIndex > 0) {
                if (currentApp.propertyBarVMs[progressingPropertyBarIndex].isSyncedWithBoxTreeToTheLeft) {
                    let targetScrollDom: HTMLDivElement | undefined = currentApp.propertyBarBoxTreeDomReferences[progressingPropertyBarIndex - 1];
                    //console.log("LEFT: current: " + currentScrollDom.scrollTop + ", prev: " + (targetScrollDom as HTMLDivElement).scrollTop);
                    if (targetScrollDom !== undefined && targetScrollDom.scrollTop != currentScrollDom.scrollTop) {
                        currentApp.propertyBarBoxTreeScrollHandled[progressingPropertyBarIndex - 1] = true;
                        targetScrollDom.scrollTop = currentScrollDom.scrollTop;
                    }
                }
                else {
                    isKeepGoingLeft = false;
                    break;
                }
                if (currentIteration > maxIteration) {
                    console.log(DEFAULT_EXCEPTION);
                    break;
                }
                progressingPropertyBarIndex--;
                currentIteration++;
            }
            // sync with right + progression to right
            progressingPropertyBarIndex = currentPropertyBarIndex + 1;
            let isKeepGoingRight: boolean = true;
            currentIteration = 0;
            maxIteration = (currentApp.state.visiblePropertyBarMaxCount - 1) - currentPropertyBarIndex;
            while (isKeepGoingRight === true && progressingPropertyBarIndex < currentApp.state.visiblePropertyBarMaxCount) {
                if (currentApp.propertyBarVMs[progressingPropertyBarIndex].isSyncedWithBoxTreeToTheLeft) {
                    let targetScrollDom: HTMLDivElement | undefined = currentApp.propertyBarBoxTreeDomReferences[progressingPropertyBarIndex];
                    //console.log("RIGHT: current: " + currentScrollDom.scrollTop + ", prev: " + (targetScrollDom as HTMLDivElement).scrollTop);
                    if (targetScrollDom !== undefined && targetScrollDom.scrollTop != currentScrollDom.scrollTop) {
                        currentApp.propertyBarBoxTreeScrollHandled[progressingPropertyBarIndex] = true;
                        targetScrollDom.scrollTop = currentScrollDom.scrollTop;
                    }
                }
                else {
                    isKeepGoingRight = false;
                    break;
                }
                if (currentIteration > maxIteration) {
                    console.log(DEFAULT_EXCEPTION);
                    break;
                }
                progressingPropertyBarIndex++;
                currentIteration++;
            }
            //console.log("onscroll end for property bar #" + currentPropertyBarIndex);
        }
        else {
            for (let i = 0; i < currentApp.state.visiblePropertyBarMaxCount; i++) {
                currentApp.propertyBarBoxTreeScrollHandled[i] = false;
            }
        }
        // --- sync visible elements ---
        if (currentPropertyBarIndex == 0 && this.currentPropertyBar.viewModel.isSyncedWithPagePreview) {
            // TODO called too often TODO initial render
            // update visible layout atom dom node references
            this.currentPropertyBar._visibleLayoutAtomDomNodeReferences = [];
            this.currentPropertyBar._visibleLayoutAtomKeys = [];
            this.currentPropertyBar._mostUpperVisibleLayoutAtomId = 0;
            let processedElementCount: number = 0;
            let mostUpperVisibleIndex: number = -1;
            let mostUpperVisibleLayoutAtomId: number = 0;
            let mostUpperVisibleDeltaTopLeft: number = currentScrollDom.clientHeight + 1; // otherwise element is below visible area
            let staticOffsetPx: number = currentScrollDom.getBoundingClientRect().top; // TODO everywhere: pixel aliasing are maybe because comparing not numerically, but strictly (eps)
            let currentScrollTop: number = currentScrollDom.scrollTop;
            let minXPreview: number = 0; // 0 based for top left corner in viewport/pagepreview
            let maxXPreview: number = currentScrollDom.clientHeight;
            //console.log("scrolled boxtree to" + currentScrollTop + " at client height " + pagePreviewHolder.clientHeight + " from max height" + pagePreviewHolder.scrollHeight  + " minX " + minXPreview + " maxX" + maxXPreview);
            for (let elementKey in this.currentPropertyBar._activeViewLayoutAtomDomNodeReferences) {
                // TODO order all layout elements and process only specific range or move processing range with scroll
                let domNode: HTMLElement = this.currentPropertyBar._activeViewLayoutAtomDomNodeReferences[elementKey];
                let isDomNodeVisible: boolean = false;
                //console.log("processing element: clientTop " + domNode.clientHeight + ", offsetTop " + domNode.offsetTop);
                let boundingRectElement: ClientRect = domNode.getBoundingClientRect();
                //let firstClientRectElement: ClientRect = domNode.getClientRects()[0];
                let minXElementDeltaTopLeft: number = boundingRectElement.top - staticOffsetPx;
                let maxXElementDeltaBottomLeft: number = currentScrollDom.clientHeight - (boundingRectElement.top - staticOffsetPx + currentScrollTop + boundingRectElement.height) + currentScrollTop;
                if (boundingRectElement.height > 0) { // height can be 0 => invisible
                    if (minXElementDeltaTopLeft >= 0.0 && minXElementDeltaTopLeft <= currentScrollDom.clientHeight) {
                        isDomNodeVisible = true;
                    }
                    else if (maxXElementDeltaBottomLeft >= 0.0 && maxXElementDeltaBottomLeft <= currentScrollDom.clientHeight) {
                        isDomNodeVisible = true;
                    }
                    else if (minXElementDeltaTopLeft <= 0.0 && maxXElementDeltaBottomLeft <= 0.0) {
                        isDomNodeVisible = true;
                    }
                }
                //console.log("element: bounding rect top " + boundingRectElement.top + ", rect bottom " + boundingRectElement.bottom + ", minXElementDeltaTopLeft: " + minXElementDeltaTopLeft + ", maxXElementDeltaBottomLeft: " + maxXElementDeltaBottomLeft);
                if (isDomNodeVisible) {
                    this.currentPropertyBar._visibleLayoutAtomDomNodeReferences.push(domNode);
                    this.currentPropertyBar._visibleLayoutAtomKeys.push(elementKey);
                    //console.log("visible element: first client rect top " + firstClientRectElement.top + ", rect bottom " + firstClientRectElement.bottom);
                    if (minXElementDeltaTopLeft < mostUpperVisibleDeltaTopLeft) {
                        mostUpperVisibleDeltaTopLeft = minXElementDeltaTopLeft;
                        mostUpperVisibleIndex = this.currentPropertyBar._visibleLayoutAtomKeys.length;
                        mostUpperVisibleLayoutAtomId = parseIntFromAttribute(domNode, "lid");
                    }
                }
                processedElementCount++;
            }
            if (mostUpperVisibleLayoutAtomId != this.currentPropertyBar._mostUpperVisibleLayoutAtomId) {
                this.currentPropertyBar._mostUpperVisibleLayoutAtomId = mostUpperVisibleLayoutAtomId;
                if (mostUpperVisibleLayoutAtomId != 0 && this.currentPropertyBar.viewModel.isSyncedWithPagePreview) {
                    currentApp.pagePreview.syncScrollPositionFromBoxTree();
                }
            }
            //console.log("boxTree scroll: processed " + processedElementCount + " object positions, visible: " + this.currentPropertyBar._visibleLayoutAtomDomNodeReferences.length.toString() + " most upper visible index: " + mostUpperVisibleIndex + " most upper visible layout id: " + this.currentPropertyBar._mostUpperVisibleLayoutAtomId);
        }
    };

    public syncScrollPositionFromPagePreview = (): void => {
        if (this.currentPropertyBar.viewModel.isSyncedWithPagePreview) {
            let currentPropertyBarIndex: number = this.currentPropertyBar.propertyBarIndex;
            let currentScrollDom: HTMLDivElement | undefined = currentApp.propertyBarBoxTreeDomReferences[currentPropertyBarIndex];
            if (currentScrollDom !== undefined) {
                let staticOffsetPx: number = currentScrollDom.getBoundingClientRect().top;
                let targetLayoutAtomId: number = currentApp.pagePreview.mostUpperVisibleLayoutAtomId;
                console.log("tree from preview for target layout #" + targetLayoutAtomId);
                let domNodeOfTargetLayout: HTMLElement | undefined = this.currentPropertyBar._visibleLayoutAtomDomNodeReferences.find(r => parseIntFromAttribute(r, "lid" /*TODO use dict*/) == targetLayoutAtomId);
                if (domNodeOfTargetLayout === undefined) {
                    domNodeOfTargetLayout = this.currentPropertyBar._activeViewLayoutAtomDomNodeReferences[targetLayoutAtomId];
                }
                if (domNodeOfTargetLayout !== undefined) {
                    currentScrollDom.scrollTop = currentScrollDom.scrollTop + (domNodeOfTargetLayout.getBoundingClientRect().top - staticOffsetPx);
                }
                else {
                    console.log(DEFAULT_EXCEPTION);
                }
            }
            else {
                // TODO test when this happens and fix
            }
        }
    };

    private boxTreeAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        currentApp.propertyBarBoxTreeDomReferences[this.currentPropertyBar.propertyBarIndex] = element as HTMLDivElement;
    };

    public deleteCaliforniaViewClickHandler = (evt: MouseEvent) => {
        let deleteCaliforniaViewId: number = parseIntFromAttribute(evt.target, "vid");
        // TODO also need to clear stuff that is connected to view.. selected special style etc.
        currentApp.router.clearCaliforniaPropertyBars(false, deleteCaliforniaViewId);
        currentApp.controller.DeleteCaliforniaViewJson(deleteCaliforniaViewId).done(data => currentApp.router.updateData(data));
    };

    public insertLayoutRowIntoViewClickHandler = (evt: MouseEvent) => {
        this.currentPropertyBar.displayPopup(evt.target as HTMLElement, PopupMode.InsertLayoutRowIntoView);
    };

    private resetPreselectedLayoutClickHandler = (evt: MouseEvent) => {
        currentApp.state.preselectedLayoutBaseId = 0;
    };

    private drawHelperLinesClickHandler = (evt: MouseEvent) => {
        // TODO smooth line update when scrolling: use css transform to change start/end node dependent on scrolled distance+window
        currentApp.state.isDrawHelperLines = !currentApp.state.isDrawHelperLines;
    };

    private syncWithPagePreviewClickHandler = (evt: MouseEvent) => {
        // only for the first property bar, toggle // TODO document
        let currentPropertyBarIndex: number = this.currentPropertyBar.propertyBarIndex;
        let currentViewModel: PropertyBarVM = this.currentPropertyBar.viewModel;
        if (currentPropertyBarIndex == 0) {
            currentViewModel.isSyncedWithPagePreview = !currentViewModel.isSyncedWithPagePreview;
            if (currentViewModel.isSyncedWithPagePreview) {
                // initial sync scroll position
                let currentBoxTreeDomReference: HTMLElement | undefined = currentApp.propertyBarBoxTreeDomReferences[currentPropertyBarIndex];
                if (currentBoxTreeDomReference !== undefined) {
                    if (currentBoxTreeDomReference.scrollTop <= 1.0/*px*/) {// TODO document
                        // sync boxtree scroll originating in pagepreview
                        this.currentPropertyBar.syncScrollPositionFromPagePreview();
                    }
                    else {
                        // sync pagepreview scroll originating in boxtree
                        currentApp.pagePreview.syncScrollPositionFromBoxTree();
                    }
                }
            }
        }
    };

    private syncWithLeftPropertyBarClickHandler = (evt: MouseEvent) => {
        // default when property bar immediately to the left displays equivalent box tree: toggle sync // TODO document
        // TODO instead: render in same div to synchronize scroll
        let currentPropertyBarIndex: number = this.currentPropertyBar.propertyBarIndex;
        let currentViewModel: PropertyBarVM = this.currentPropertyBar.viewModel;
        if (currentPropertyBarIndex != 0
            && currentApp.propertyBarVMs[currentPropertyBarIndex - 1].currentPropertyBarMode === PropertyBarMode.CaliforniaView
            && currentApp.propertyBarVMs[currentPropertyBarIndex - 1].selectedCaliforniaViewId == currentViewModel.selectedCaliforniaViewId) {
            currentViewModel.isSyncedWithBoxTreeToTheLeft = !currentViewModel.isSyncedWithBoxTreeToTheLeft;
            if (currentViewModel.isSyncedWithBoxTreeToTheLeft === true) {
                // initial sync scroll position
                let currentBoxTreeDomReference: HTMLDivElement | undefined = currentApp.propertyBarBoxTreeDomReferences[currentPropertyBarIndex];
                let otherBoxTreeDomReference: HTMLDivElement | undefined = currentApp.propertyBarBoxTreeDomReferences[currentPropertyBarIndex - 1];
                if (currentBoxTreeDomReference !== undefined
                    && otherBoxTreeDomReference !== undefined) {
                    otherBoxTreeDomReference.scrollTop = currentBoxTreeDomReference.scrollTop;
                }
                else {
                    console.log(DEFAULT_EXCEPTION);
                    currentViewModel.isSyncedWithBoxTreeToTheLeft = false;
                }
            }
        }
        else {
            currentViewModel.isSyncedWithBoxTreeToTheLeft = false;
        }
    };
}