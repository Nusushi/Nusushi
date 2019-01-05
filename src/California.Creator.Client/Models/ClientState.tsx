/// <reference path="../jsx.ts" />
"use strict";
import { CaliforniaApp, DEFAULT_EXCEPTION } from "./CaliforniaApp";
import { LayoutType, CaliforniaView, CaliforniaEvent } from "./CaliforniaGenerated";
import { TransactionMode } from "../ViewModels/PropertyBarVM";

let currentApp: CaliforniaApp;
let currentClientState: ClientState;

export const STATIC_MARGIN_PX: number = 25;

export enum SelectionMode {
    Styles = 0,
    Content = 1    
};

export enum ReadyState {
    Ok = 0,
    Pending = 1
};

export enum EditViewMode {
    Default = 0,
    SidebarOnly = 1,
    PagePreviewOnly = 2
};

export class ClientState {
    public isDataLoaded: boolean = false; // TODO
    public isHideUserInterface: boolean = false; // TODO
    public editViewMode: EditViewMode = EditViewMode.Default; // TODO
    public currentSelectionMode: SelectionMode = SelectionMode.Content;
    public currentReadyState: ReadyState = ReadyState.Ok; // TODO use everywhere
    public isJaxOn: boolean = false;
    // --- responsive selector: shows responsive device for current client size (e.g. 1200px browser - 350px UI width highlights resp_x, where resp_(x-1).WidthThreshold == 800px < resp_(x).WidthThreshold == 1000px <= resp_(x+1).WidthThreshold == 1200px)
    // user can override the target responsive device, for a device between lowestWidthThreshold and highestWidthThreshold; if it is smaller than client size, empty space is filled with UI margin; if it is larger, the preview size is increased (overflow=scroll)
    public lowestWidthThresholdResponsiveDeviceId: number = 0; // TODO is not client state but server data state, which is calculated on client
    public highestWidthThresholdResponsiveDeviceId: number = 0;
    public specialStyleHolder: CaliforniaView | undefined = undefined;
    public currentResponsiveDeviceId: number = 0;
    public noneResponsiveDeviceId: number = 0;
    public defaultSymmetricPagePreviewHolderMarginPx: number = STATIC_MARGIN_PX; // static margin, used in the default view
    public overrideResponsiveDeviceId: number = 0; // can be 0 for no selection, or responsive device id for a device: lowestWidthThreshold < selected device widthThreshold < highestWidthThreshold
    public availableSpacePagePreviewPx: number;
    public isEnoughAvailableSpacePagePreview: boolean = true;
    public targetPagePreviewHolderMarginPx: number = STATIC_MARGIN_PX;
    public targetPagePreviewWidthPx: number;
    public visiblePropertyBarMaxCount: number = 2; // can be 1, 2 or 4
    public popupTargetPropertyBarIndex: number = 0;
    public currentTransactionMode: TransactionMode = TransactionMode.MoveLayoutMoleculeIntoLayoutMolecule;
    public highlightedLayoutBaseId: number = 0; // TODO handling when item is deleted while being highlighted
    public isSelectAllTextArea: boolean = false;
    // --- undo/repeat ---
    public lastCommand: CaliforniaEvent = CaliforniaEvent.ReadInitialClientData;
    public lastCaliforniaEventData: (string|number)[] = [];
    // ---
    // --- hover/selection state ---
    public selectedLayoutBaseId: number = 0;
    public preselectedLayoutBaseId: number = 0;
    public preselectedCaliforniaViewId: number = 0;
    public isDrawHelperLines: boolean = false;
    public hoveredBoxTreeLayoutBaseId: number = 0;
    public hoveredPagePreviewLayoutBaseId: number = 0;
    public hoveredInsertLayoutBaseId: number = 0; // selectedLayoutBaseId is target
    public backupSortOrder: number | undefined = undefined; // hoveredInsertLayoutBaseId gets temporary sort order key (atom in box => maxInt etc.)
    public backupOwnerRowId: number | undefined = undefined; // hoveredInsertLayoutBaseId gets temporary owner row (sometimes)
    public backupPlacedBoxInBoxId: number | undefined = undefined; // hoveredInsertLayoutBaseId gets temporary owner row (sometimes, only when backupOwnerRowId is set)
    // ---
    // --- unique styles ---
    public newBoxStyleMoleculeId: number = 0;
    // ---
    constructor(californiaAppArg: CaliforniaApp) {
        currentClientState = this;
        currentApp = californiaAppArg;
    };
}