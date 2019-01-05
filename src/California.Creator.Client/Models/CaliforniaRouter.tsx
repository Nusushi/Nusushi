/// <reference path="../jsx.ts" />
"use strict";
import { VNode, VNodeProperties } from "maquette";
declare var require: any;
import * as maquette from "maquette";
const h = maquette.h;
import { CaliforniaApp, DEFAULT_EXCEPTION } from "./CaliforniaApp";
import { CaliforniaClientViewModel, CaliforniaEvent, CaliforniaView, StyleMolecule, ResponsiveDevice, LayoutRow, CaliforniaProject, LayoutAtom, LayoutBox, LayoutType, ContentAtom, StyleMoleculeAtomMapping } from "./CaliforniaGenerated";
import { STATIC_MARGIN_PX, EditViewMode } from "./ClientState";
import { LayoutStyleInteraction } from "../Typewriter/LayoutStyleInteraction";
import { PropertyBar } from "./PropertyBar";
import { PropertyBarMode } from "../ViewModels/PropertyBarVM";

let currentApp: CaliforniaApp;
let currentRouter: CaliforniaRouter;

export class CaliforniaRouter {
    private deepestLevelCurrentRow: number = 0; // TODO optimize / transfer server => concept?
    constructor(californiaAppArg: CaliforniaApp) {
        currentRouter = this;
        currentApp = californiaAppArg;
    };

    /*public updateData = (response: CaliforniaClientViewModel, preventRenderChainedCalls: boolean = false): void => { TODO kept as reference, unused
        CaliforniaApp.CaliforniaAppInstance.projector.scheduleRender();
        // incremental update, use data from response to update native client data store (conversions for dates, object references etc.)
        let isValidResponse: boolean = currentRouter.validateResponse(response);
        if (!isValidResponse) {
            return;
        }
        // TODO can return 404 not found when deleting already deleted objects => page reload
        // TODO selected project change must be tested properly (breakpoint in controller actions and execute 2nd ui action)
        // TODO other constraints like "selected project may not change"?
        // TODO refactor client view model to not contain multiple references to same object => selected project should be californiaProjects[index]
        // TODO test every client interaction with a delay and select project in between
        // temporary solution: reset object references => leads to different bugs
        let isUpdateSuccess = false;

        // unused, always full page update
        switch (response.CaliforniaEvent) {
            case CaliforniaEvent.DuplicateStyleQuantum:
                if (response.StyleQuantum !== undefined
                    && currentApp.clientData.CaliforniaProject !== undefined
                    && currentApp.clientData.CaliforniaProject.StyleQuantums !== undefined) {
                    currentApp.clientData.CaliforniaProject.StyleQuantums.push(response.StyleQuantum);
                    currentApp.propertyBarVM.styleQuantumMapping.map(currentApp.clientData.CaliforniaProject.StyleQuantums);
                    isUpdateSuccess = true;
                }
                break;
            default:
                console.log(`incremental update not implemented for ${CaliforniaEvent[response.CaliforniaEvent]}`); // TODO
        }

        if (!isUpdateSuccess) {
            console.log("invalid client state");
            location.reload(); // TODO audit security...
        }

        if (!preventRenderChainedCalls) {
            currentApp.projector.scheduleRender();
        }
    };*/

    public updateData = (response: CaliforniaClientViewModel, isInitial = false, preventRenderChainedCalls: boolean = false): void => {
        // copy data from response to native client data store (conversions for dates, object references etc.)
        let isValidResponse: boolean = currentRouter.validateResponse(response);
        if (!isValidResponse) {
            return;
        }
        // TODO more initial client check if required data, certificates, ... is loaded etc. test basic setup and clear view again
        let isFullDataUpdate: boolean = false;
        let isUpdateSuccess: boolean = false;
        switch (response.CaliforniaEvent) {
            case CaliforniaEvent.UpdateContentAtom:
                if (response.PartialUpdate.ContentAtom !== undefined) {
                    let currentContentAtomIndex: number = currentApp.clientData.CaliforniaProject.ContentAtoms.findIndex(c => c.ContentAtomId == response.PartialUpdate.ContentAtom.ContentAtomId);
                    // TODO test: does update routines of rendered layout atoms have to be called to refresh "local" copy of content/rendering
                    currentApp.clientData.CaliforniaProject.ContentAtoms.splice(currentContentAtomIndex, 1, ...[response.PartialUpdate.ContentAtom]);
                    (currentApp.clientData.CaliforniaProject.LayoutMolecules.find(l => l.LayoutBaseId == response.PartialUpdate.ContentAtom.InstancedOnLayoutId) as LayoutAtom).HostedContentAtom = response.PartialUpdate.ContentAtom;
                    isUpdateSuccess = true;
                }                
                break;
            default:
                isFullDataUpdate = true;
                break;
        }
        if (isFullDataUpdate === true) {
            if (response.CaliforniaProject !== undefined) {
                currentApp.clientData.CaliforniaProject = response.CaliforniaProject;

                currentApp.clientData.UrlToReadOnly = response.UrlToReadOnly; // TODO only initial
                currentApp.clientData.UrlToReadAndEdit = response.UrlToReadAndEdit;

                // sort responsive devices: -1, 0, ...>0
                if (response.CaliforniaProject.ResponsiveDevices.length !== undefined && response.CaliforniaProject.ResponsiveDevices.length > 0) { // check if responsive devices are already seeded
                    currentApp.clientData.CaliforniaProject.ResponsiveDevices = currentApp.clientData.CaliforniaProject.ResponsiveDevices.sort((r1, r2) => (r1.WidthThreshold !== undefined && r2.WidthThreshold !== undefined) ? (r1.WidthThreshold < r2.WidthThreshold ? -1 : r1.WidthThreshold == r2.WidthThreshold ? 0 : 1) : 0);
                    currentApp.state.lowestWidthThresholdResponsiveDeviceId = currentApp.clientData.CaliforniaProject.ResponsiveDevices[1].ResponsiveDeviceId;
                    currentApp.state.highestWidthThresholdResponsiveDeviceId = currentApp.clientData.CaliforniaProject.ResponsiveDevices[currentApp.clientData.CaliforniaProject.ResponsiveDevices.length - 1].ResponsiveDeviceId;
                    currentApp.state.noneResponsiveDeviceId = currentApp.clientData.CaliforniaProject.ResponsiveDevices[0].ResponsiveDeviceId;
                    if (isInitial) { // TODO what if initial data is erroneous
                        for (let i = 0; i < currentApp.propertyBars.length; i++) {
                            let propertyBar: PropertyBar = currentApp.propertyBars[i];
                            propertyBar.viewModel.setSelectedResponsiveDeviceId(currentApp.state.noneResponsiveDeviceId, true);
                        }
                    }

                    if (currentApp.pagePreview.virtualStyleIndex.length == 0) {
                        currentApp.pagePreview.virtualStyleIndex = [];
                        currentApp.pagePreview.virtualPseudoStyleIndex = [];
                        for (let responsiveDevice of currentApp.clientData.CaliforniaProject.ResponsiveDevices) {
                            currentApp.pagePreview.virtualStyleIndex.push([]);
                            currentApp.pagePreview.virtualPseudoStyleIndex.push({});
                        }
                    }
                }
                if (isInitial === true) {
                    currentApp.clientData.AllCssProperties = response.AllCssProperties;
                    currentApp.clientData.StyleAtomCssPropertyMapping = response.StyleAtomCssPropertyMapping;
                    currentApp.clientData.ThirdPartyFonts = response.ThirdPartyFonts;
                }

                if (currentApp.clientData.CaliforniaProject.StyleQuantums !== undefined &&
                    currentApp.clientData.CaliforniaProject.StyleAtoms !== undefined &&
                    currentApp.clientData.CaliforniaProject.StyleMolecules !== undefined &&
                    currentApp.clientData.CaliforniaProject.LayoutMolecules !== undefined &&
                    currentApp.clientData.CaliforniaProject.CaliforniaViews !== undefined &&
                    currentApp.clientData.CaliforniaProject.CaliforniaViews.length > 0) { // TODO duplicate numbering
                    // TODO only map when property bar was used at least once // TODO sort and reordering is happening at different placed
                    currentRouter.restoreLayoutMoleculeAndStyleReferences();
                    currentApp.state.specialStyleHolder = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(v => v.IsInternal === true && v.Name === "[Internal] Special Styles");
                    if (currentApp.state.specialStyleHolder === undefined) {
                        console.log(DEFAULT_EXCEPTION);  // TODO will break when data changes
                        return;
                    }
                    currentApp.clientData.CaliforniaProject.CaliforniaViews.filter(v => !v.IsInternal).map(v => {
                        if (currentApp.state.specialStyleHolder !== undefined) {
                            let californiaViewStyleHolderRow: LayoutRow = currentApp.state.specialStyleHolder.PlacedLayoutRows.find(layoutRow => {
                                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutRow.LayoutBaseId) as StyleMolecule; // TODO expensive
                                if (styleMolecule.Name === `[Internal] ${v.Name} View Style`) { // TODO will break when data changes
                                    v.SpecialStyleViewStyleMoleculeId = styleMolecule.StyleMoleculeId;
                                    return true;
                                }
                                return false;
                            }) as LayoutRow;
                            let californiaViewBodyStyleHolderRow: LayoutRow = currentApp.state.specialStyleHolder.PlacedLayoutRows.find(layoutRow => {
                                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutRow.LayoutBaseId) as StyleMolecule; // TODO expensive
                                if (styleMolecule.Name === `[Internal] ${v.Name} Body Style`) { // TODO will break when data changes
                                    v.SpecialStyleBodyStyleMoleculeId = styleMolecule.StyleMoleculeId;
                                    return true;
                                }
                                return false;
                            }) as LayoutRow;
                            let californiaViewHtmlStyleHolderRow: LayoutRow = currentApp.state.specialStyleHolder.PlacedLayoutRows.find(layoutRow => {
                                let styleMolecule: StyleMolecule = currentApp.clientData.CaliforniaProject.StyleMolecules.find(m => m.StyleForLayoutId == layoutRow.LayoutBaseId) as StyleMolecule; // TODO expensive
                                if (styleMolecule.Name === `[Internal] ${v.Name} Html Style`) { // TODO will break when data changes
                                    v.SpecialStyleHtmlStyleMoleculeId = styleMolecule.StyleMoleculeId;
                                    return true;
                                }
                                return false;
                            }) as LayoutRow;
                            v.SpecialStyleViewStyleMoleculeIdString = v.SpecialStyleViewStyleMoleculeId.toString();
                            v.SpecialStyleViewStyleString = `s${v.SpecialStyleViewStyleMoleculeIdString}`;
                            v.SpecialStyleBodyStyleMoleculeIdString = v.SpecialStyleBodyStyleMoleculeId.toString();
                            v.SpecialStyleBodyStyleString = `s${v.SpecialStyleBodyStyleMoleculeIdString}`;
                            v.SpecialStyleHtmlStyleMoleculeIdString = v.SpecialStyleHtmlStyleMoleculeId.toString();
                            v.SpecialStyleHtmlStyleString = `s${v.SpecialStyleHtmlStyleMoleculeIdString}`;
                        }
                        else {
                            v.SpecialStyleViewStyleString = "";
                            v.SpecialStyleBodyStyleString = "";
                            v.SpecialStyleHtmlStyleString = "";
                        }
                    });

                    let instanceableAtomsView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] Instanceable Layout Atoms") as CaliforniaView; // TODO magic string => const export
                    let instanceableRowsView: CaliforniaView = currentApp.clientData.CaliforniaProject.CaliforniaViews.find(view => view.IsInternal && view.Name === "[Internal] Instanceable Layout Rows") as CaliforniaView; // TODO magic string => const export
                    currentRouter.setActiveCaliforniaViewId(currentApp.pagePreviewVM.activeCaliforniaViewId != 0 ? currentApp.pagePreviewVM.activeCaliforniaViewId : 0, true, isInitial);
                    for (let i = 0; i < currentApp.propertyBars.length; i++) {
                        currentApp.propertyBars[i].viewModel.updateData(currentApp.clientData.CaliforniaProject.StyleQuantums,
                            currentApp.clientData.CaliforniaProject.StyleAtoms,
                            currentApp.clientData.CaliforniaProject.StyleMolecules,
                            currentApp.clientData.CaliforniaProject.LayoutMolecules,
                            currentApp.clientData.CaliforniaProject.CaliforniaViews,
                            instanceableAtomsView,
                            instanceableRowsView);
                    }
                    let firstRow: LayoutRow = instanceableRowsView.PlacedLayoutRows[0];
                    currentApp.state.newBoxStyleMoleculeId = (currentApp.clientData.CaliforniaProject.StyleMolecules.find(s => s.StyleForLayoutId == firstRow.AllBoxesBelowRow[0].LayoutBaseId) as StyleMolecule).StyleMoleculeId;
                }
                else {
                    currentApp.state.specialStyleHolder = undefined;
                    console.log(DEFAULT_EXCEPTION);
                    // TODO
                }

                if (isInitial === true) {
                    let i: number = 0;
                    if (i < currentApp.propertyBarCount) { // TODO example snippet for code guidelines => layout helps to write correct code / not forget stuff
                        currentApp.propertyBarVMs[i++].currentPropertyBarMode = PropertyBarMode.CaliforniaView;
                        if (i < currentApp.propertyBarCount) {
                            currentApp.propertyBarVMs[i++].currentPropertyBarMode = PropertyBarMode.AllCaliforniaViews;
                            if (i < currentApp.propertyBarCount) {
                                currentApp.propertyBarVMs[i++].currentPropertyBarMode = PropertyBarMode.StyleMolecule;
                                if (i < currentApp.propertyBarCount) {
                                    currentApp.propertyBarVMs[i++].currentPropertyBarMode = PropertyBarMode.AllStyleQuantums;
                                }
                            }
                        }
                    }
                }

                currentRouter.setupUiForDevice(); // TODO what if first render happens after initial client data update? is this possible at all

                isUpdateSuccess = true;
            }
        }

        if (isUpdateSuccess === false) {
            console.log("missing client data");
            location.reload(); // TODO audit security... TEST
        }
        currentApp.state.isDataLoaded = true;
        if (!preventRenderChainedCalls) {
            currentApp.projector.scheduleRender();
        }
    };

    public clearCaliforniaPropertyBars(isClearWhenNonEqual: boolean, clearWhenEqualsCaliforniaViewId: number): void {
        // view models can have no selection
        for (let i = 0; i < currentApp.propertyBars.length; i++) {
            currentApp.propertyBars[i].viewModel.clearSelectedCaliforniaView(isClearWhenNonEqual, clearWhenEqualsCaliforniaViewId);
        }
    };
    
    public setActiveCaliforniaViewId(californiaViewId: number, isDefaultToHome: boolean, isSetAllPropertyBars: boolean): void {
        let userPages: CaliforniaView[] = currentApp.clientData.CaliforniaProject.CaliforniaViews.filter(view => !view.IsInternal);
        let activeView: CaliforniaView | undefined = undefined;
        let activePageIndex: number = -1;
        if (californiaViewId != 0) {
            activePageIndex = userPages.findIndex(v => v.CaliforniaViewId == californiaViewId);
        }
        if (activePageIndex == -1 && isDefaultToHome === true) {
            activePageIndex = userPages.findIndex(v => v.Name === "Home"); // TODO will break if data is not supplied
        }
        if (activePageIndex > -1) {
            activeView = userPages[activePageIndex];
            currentRouter.setActiveCaliforniaView(activeView);
            if (isSetAllPropertyBars === true) {
                for (let i: number = 0; i < currentApp.propertyBars.length; i++) {
                    currentApp.propertyBars[i].viewModel.setSelectedCaliforniaView(activeView, true);
                }
            }
            else {
                // TODO complex update view models
                currentApp.propertyBars[0].viewModel.setSelectedCaliforniaView(activeView, true);
                for (let i: number = 1; i < currentApp.propertyBars.length; i++) {
                    let propertyBarViewId: number = currentApp.propertyBars[i].viewModel.selectedCaliforniaViewId;
                    if (propertyBarViewId != 0) {
                        let propertyBarView: CaliforniaView = userPages.find(v => v.CaliforniaViewId === propertyBarViewId) as CaliforniaView;
                        currentApp.propertyBars[i].viewModel.setSelectedCaliforniaView(propertyBarView, true);
                    }
                }
            }            
        }
        else {
            console.log(DEFAULT_EXCEPTION);
        }
    };

    public setActiveCaliforniaView(californiaView: CaliforniaView): void { // TODO should be setter method for activeCaliforniaView
        currentApp.pagePreview.viewModel.activeCaliforniaViewId = californiaView.CaliforniaViewId;
        currentApp.pagePreviewVM.californiaViewProjector.map([californiaView]);
        currentApp.pagePreviewVM.fixedLayoutRowsProjector.map(californiaView.PlacedLayoutRows);
    };

    private restoreLayoutMoleculeAndStyleReferences = (): void => {
        // restore layout molecule object references: view => row => boxes => boxes/atoms => atoms => content
        let project: CaliforniaProject = currentApp.clientData.CaliforniaProject;
        let allRows: LayoutRow[] = [];
        let allBoxes: LayoutBox[] = [];
        let allAtoms: LayoutAtom[] = [];
        project.LayoutMolecules.map(mol => {
            switch (mol.LayoutType) {
                case LayoutType.Row:
                    allRows.push(mol as LayoutRow);
                    break;
                case LayoutType.Box:
                    allBoxes.push(mol as LayoutBox);
                    break;
                case LayoutType.Atom:
                    allAtoms.push(mol as LayoutAtom);
                    break;
                default:
                    console.log(DEFAULT_EXCEPTION);
                    break;
            }
        });
        let allContentAtoms: ContentAtom[] = project.ContentAtoms;
        let allInteractions: LayoutStyleInteraction[] = project.LayoutStyleInteractions;
        // layout references
        // TODO sort rows here? strange data model // TODO sorted not only when updating data // TODO is sort order guaranteed to be generated layout id in generate project (probably yes)
        project.CaliforniaViews.map(view => {
            view.PlacedLayoutRows = allRows.filter(row => row.PlacedOnViewId == view.CaliforniaViewId).sort((rowA: LayoutRow, rowB: LayoutRow) => {
                if (rowA.LayoutSortOrderKey < rowB.LayoutSortOrderKey) {
                    return -1;
                }
                else if (rowA.LayoutSortOrderKey == rowB.LayoutSortOrderKey) {
                    return 0;
                }
                else { // rowA.LayoutSortOrderKey > rowB.LayoutSortOrderKey
                    return 1;
                }
            });
        });
        let deepestLevelCurrentView: number = 0;
        project.CaliforniaViews.map(view => {
            deepestLevelCurrentView = 0;
            for (let iRow: number = 0; iRow < view.PlacedLayoutRows.length; iRow++) { // TODO are array limits recognized as immutable by javascript runtimes? if not, declare limit before loop everywhere
                currentRouter.deepestLevelCurrentRow = 0;
                let row: LayoutRow = view.PlacedLayoutRows[iRow];
                row.AllBoxesBelowRow = allBoxes.filter(box => box.BoxOwnerRowId == row.LayoutBaseId && box.PlacedBoxInBoxId == undefined);
                row.AllBoxesBelowRow.map(box => {
                    currentRouter.restoreLayoutBoxReferencesRecursive(0, box, row, allBoxes, allAtoms, allContentAtoms, allInteractions);
                    box.BoxOwnerRow = row;
                });
                row.DeepestLevel = currentRouter.deepestLevelCurrentRow;
                if (currentRouter.deepestLevelCurrentRow > deepestLevelCurrentView) {
                    deepestLevelCurrentView = currentRouter.deepestLevelCurrentRow;
                }
            }
            view.DeepestLevel = deepestLevelCurrentView;
        });
        // style references
        /*project.StyleAtoms.map(atom => {
            atom.AppliedValues = project.StyleValues.filter(val => val.StyleAtomId == atom.StyleAtomId);
        });*/
        //console.log("after restore:");console.log(currentApp.clientData);
    };

    private restoreLayoutBoxReferencesRecursive = (boxLevel: number, box: LayoutBox, boxOwnerRow: LayoutRow, allBoxes: LayoutBox[], allAtoms: LayoutAtom[], allContentAtoms: ContentAtom[], allInteractions: LayoutStyleInteraction[]): void => {
        box.Level = boxLevel;
        box.PlacedInBoxBoxes = allBoxes.filter(subBox => subBox.PlacedBoxInBoxId !== undefined && subBox.PlacedBoxInBoxId == box.LayoutBaseId);
        box.PlacedInBoxBoxes.map(subBox => {
            currentRouter.restoreLayoutBoxReferencesRecursive(boxLevel + 1, subBox, boxOwnerRow, allBoxes, allAtoms, allContentAtoms, allInteractions);
            subBox.BoxOwnerRow = boxOwnerRow;
            subBox.PlacedBoxInBox = box;
        });
        boxOwnerRow.AllBoxesBelowRow.push(...box.PlacedInBoxBoxes);
        box.PlacedInBoxAtoms = allAtoms.filter(subAtom => subAtom.PlacedAtomInBoxId !== undefined && subAtom.PlacedAtomInBoxId == box.LayoutBaseId);
        box.PlacedInBoxAtoms.map(subAtom => {
            subAtom.HostedContentAtom = allContentAtoms.find(contentAtom => contentAtom.InstancedOnLayoutId == subAtom.LayoutBaseId) as ContentAtom;
            subAtom.PlacedAtomInBox = box;
            subAtom.LayoutStyleInteractions = allInteractions.filter(map => map.LayoutAtomId == subAtom.LayoutBaseId);
            subAtom.Level = boxLevel + 1;
        });
        if (box.PlacedInBoxAtoms.length > 0) {
            if ((boxLevel + 1) > currentRouter.deepestLevelCurrentRow) {
                currentRouter.deepestLevelCurrentRow = boxLevel + 1;
            }
        }
        else if (boxLevel > currentRouter.deepestLevelCurrentRow) {
            currentRouter.deepestLevelCurrentRow = boxLevel;
        }
    };

    public setupUiForDevice = (): void => {
        // TODO use window.innerWidth to adjust UI state, called on page load and window resize TODO iphone seems to have viewport larger than available space...
        let californiaAppHeight: number | undefined = $(window).height(); // TODO use inner or outer height
        if (californiaAppHeight === undefined) {
            console.log(DEFAULT_EXCEPTION);
            return;
        }
        document.body.style.height = `${californiaAppHeight}px`; // 100% height on iPad, with and without rendered browser controls
        // clientWidth includes margin
        if (currentApp.state.isHideUserInterface === true || currentApp.state.editViewMode === EditViewMode.PagePreviewOnly) {
            // TODO verify this works everywhere
            currentApp.state.availableSpacePagePreviewPx = currentApp.pagePreviewHolder !== undefined ? window.innerWidth + 2 * currentApp.state.targetPagePreviewHolderMarginPx : 0;
        }
        else {
            currentApp.state.availableSpacePagePreviewPx = currentApp.pagePreviewHolder !== undefined ? currentApp.pagePreviewHolder.clientWidth + (2 * currentApp.state.targetPagePreviewHolderMarginPx) : 0; // TODO test resulting value changes depending on client browser/system architecture?
        }
        if (currentApp.clientData.CaliforniaProject !== undefined && currentApp.clientData.CaliforniaProject.ResponsiveDevices !== undefined && currentApp.clientData.CaliforniaProject.ResponsiveDevices.length > 0) {
            if (currentApp.pagePreviewVM.editedLayoutAtomId == 0) { // TODO
                // TODO disable resize when editing text areas in other controls (e.g. value in property bar)
                currentApp.pagePreview.updatePagePreviewDimensions();
            }
        }
    };

    private validateResponse = (response: CaliforniaClientViewModel): boolean => {
        //console.log(response); // TODO exception management: when app throws, hangs, etc. print current data state (or save on send(!!))
        if (response == null) {
            console.log("invalid client data"); // TODO
            return false;
        } else if (response.StatusText !== null && response.StatusText !== "") {
            console.log(response.StatusText);
            return false;
        }
        // check revision
        if ((currentApp.clientData !== undefined) && (response.CurrentRevision < currentApp.clientData.CurrentRevision)) {
            console.log("Ignoring client data: revision is lower");
            // TODO make sure all other stuff that calls this function handles case correctly
            return false;
        }
        return true;
    };
}