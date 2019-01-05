"use strict";
import * as maquette from "maquette";
const h = maquette.h;
import { PropertyBar } from "./../Models/PropertyBar";
import { StyleQuantum, StyleAtom, StyleAtomType, StyleMolecule, LayoutRow, LayoutBase, CaliforniaView } from "../Models/CaliforniaGenerated";
import { CaliforniaApp } from "../Models/CaliforniaApp";

export enum PropertyBarMode {
    None = 0,
    CaliforniaView = 1,
    LayoutMolecules = 2,
    LayoutAtoms = 3,
    AllStyleQuantums = 4,
    AllStyleAtoms = 5,
    AllStyleMolecules = 6,
    StyleMolecule = 7,
    AllLayoutMolecules = 8,
    LayoutBase = 9,
    AllCaliforniaViews = 10
};

export enum PopupSecondaryMode {
    None = 0,
    SelectBoxIntoBox = 1,
    SelectBoxIntoBoxAtomInPlace = 2
};

export enum PopupMode {
    None = 0,
    AddCssProperty = 1,
    AllCssProperties = 2,
    UpdateCssValue = 3,
    MatchingQuantums = 4,
    UpdateCssQuantum = 5,
    AllCssPropertiesForQuantum = 6,
    InsertLayoutRowIntoView = 7,
    InsertLayoutAtomIntoBox = 8,
    SelectBox = 9, // TODO rename to SelectBoxHasSecondary or how to make sure secondary mode is set?
    MoveStyleAtom = 10,
    ShareCaliforniaProject = 11,
    SelectInteractionTarget = 12,
    SelectInteractionTargetLayoutFilter = 13,
    CaliforniaViewSelection = 14,
    EditUserDefinedCss = 15,
    SuggestedCssValues = 16
};

export enum TransactionMode {
    MoveLayoutMoleculeIntoLayoutMolecule = 0, // TODO need default === none?
    MoveLayoutMoleculeBeforeLayoutMolecule = 1, // TODO differentiate mode
    SyncLayoutStylesImitating = 2
}

let currentApp: CaliforniaApp;

export class PropertyBarVM {
    private propertyBarVMIndex: number = -1;
    public currentPropertyBarMode: PropertyBarMode = PropertyBarMode.None;
    public currentPopupMode: PopupMode = PopupMode.None;
    public currentSecondaryPopupMode: PopupSecondaryMode = PopupSecondaryMode.None;
    // data
    public tempQuantumName: string = "Quantum";
    public tempCssPropertyName: string = "";
    public tempCssValue: string = "";
    public lastUsedTempCssValue: string = "";
    public tempPseudoSelector: string = "";
    public tempCaliforniaViewName: string = "";
    public tempCssValueForInteraction: string = "";
    public selectedStyleAtomId: number = 0;
    public selectedStyleValueId: number = 0;
    public selectedStyleQuantumId: number = 0;
    public selectedStyleMoleculeId: number = 0;
    private _selectedCaliforniaViewId: number = 0; // can be 0
    public selectedResponsiveDeviceId: number = 0;
    public selectedStateModifier: string = "";
    public selectedStyleAtomType: StyleAtomType = StyleAtomType.Generic;
    public selectedLayoutBaseIdForFilter: number = 0;
    public selectedLayoutStyleInteraction: number = 0;
    public tempOriginalContent: string = "";
    public tempContent: string = "";
    public editedLayoutAtomId: number = 0;
    public tempUserDefinedCss: string = "";
    private _deepestLevelActiveView: number = 0; // supposed to be strictly larger equal any layout atom level for current view
    // TODO different selections of same object type possible depending on current UI functionality
    public selectedStyleAtomIdForPopup: number = 0;
    // box tree
    public isSyncedWithBoxTreeToTheLeft: boolean = false;
    public isSyncedWithPagePreview: boolean = false;
    // components
    public styleQuantumProjector: maquette.Mapping<StyleQuantum, { renderMaquette: () => maquette.VNode }>;
    public styleAtomProjector: maquette.Mapping<StyleAtom, { renderMaquette: () => maquette.VNode }>;
    public styleMoleculeProjector: maquette.Mapping<StyleMolecule, { renderMaquette: () => maquette.VNode }>;
    public instanceableAtomProjector: maquette.Mapping<LayoutRow, { renderMaquette: () => maquette.VNode }>;
    public instanceableMoleculeProjector: maquette.Mapping<LayoutRow, { renderMaquette: () => maquette.VNode }>;
    public allLayoutMoleculesProjector: maquette.Mapping<LayoutBase, { renderMaquette: () => maquette.VNode }>;
    public allCaliforniaViewsProjector: maquette.Mapping<CaliforniaView, { renderMaquette: () => maquette.VNode }>;
    public boxTreeProjector: maquette.Mapping<CaliforniaView, { renderMaquette: () => maquette.VNode }>;
    // client settings

    constructor(propertyBarArg: PropertyBar, targetIndex: number, californiaAppArg: CaliforniaApp) {
        currentApp = californiaAppArg;
        this.propertyBarVMIndex = targetIndex;
        this.styleQuantumProjector = propertyBarArg.renderStyleQuantumArray(propertyBarArg);
        this.styleAtomProjector = propertyBarArg.renderStyleAtomArray(propertyBarArg);
        this.styleMoleculeProjector = propertyBarArg.renderStyleMoleculeArray(propertyBarArg);
        this.instanceableAtomProjector = propertyBarArg.renderLayoutRowArray(propertyBarArg);
        this.instanceableMoleculeProjector = propertyBarArg.renderLayoutRowArray(propertyBarArg); // TODO
        this.allLayoutMoleculesProjector = propertyBarArg.renderLayoutMoleculeArray(propertyBarArg);
        this.allCaliforniaViewsProjector = propertyBarArg.renderCaliforniaViewArray(propertyBarArg);
        this.boxTreeProjector = propertyBarArg.renderBoxTreeForCaliforniaView(propertyBarArg);
    };

    private get currentVM(): PropertyBarVM {
        return currentApp.propertyBarVMs[this.propertyBarVMIndex];
    };

    public get selectedCaliforniaViewId(): number {
        return this._selectedCaliforniaViewId;
    };

    public get deepestLevelActiveView(): number {
        return this._deepestLevelActiveView;
    };

    public setSelectedResponsiveDeviceId = (responsiveDeviceId: number, isForce: boolean): void => {
        if (isForce || this.currentVM.selectedResponsiveDeviceId == 0) {
            this.currentVM.selectedResponsiveDeviceId = responsiveDeviceId;
        }
    };

    public clearSelectedCaliforniaView(isClearWhenNonEqual: boolean, clearWhenEqualsCaliforniaViewId: number): void {
        // view models can have no selection
        if (isClearWhenNonEqual === true || this.currentVM.selectedCaliforniaViewId == clearWhenEqualsCaliforniaViewId) {
            this.currentVM._selectedCaliforniaViewId = 0;
            this.currentVM._deepestLevelActiveView = 0;
            this.currentVM.boxTreeProjector.map([]);
        }
    };

    public setSelectedCaliforniaView = (californiaView: CaliforniaView, isForce: boolean): void => {
        if (isForce || this.currentVM.selectedCaliforniaViewId == 0) {
            this.currentVM._selectedCaliforniaViewId = californiaView.CaliforniaViewId;
            this.currentVM._deepestLevelActiveView = californiaView.DeepestLevel;
            this.currentVM.boxTreeProjector.map([californiaView]); // TODO map not compatible with ... ?
        }
        else {
            this.currentVM.boxTreeProjector.map([]);
            this.currentVM._deepestLevelActiveView = 0;
            this.currentVM._selectedCaliforniaViewId = 0;
        }        
    };

    public updateData = (styleQuantums: StyleQuantum[], styleAtoms: StyleAtom[], styleMolecules: StyleMolecule[], layoutMolecules: LayoutBase[], allCaliforniaViews: CaliforniaView[], instanceableAtomsView: CaliforniaView, instanceableRowsView: CaliforniaView): void => {
        this.currentVM.styleQuantumProjector.map(styleQuantums);
        this.currentVM.styleAtomProjector.map(styleAtoms);
        this.currentVM.styleMoleculeProjector.map(styleMolecules);
        this.currentVM.allLayoutMoleculesProjector.map(layoutMolecules); // TODO reverse server side? // TODO only required once for all propertybars
        this.currentVM.allCaliforniaViewsProjector.map(allCaliforniaViews); // TODO no internals
        this.currentVM.instanceableAtomProjector.map(instanceableAtomsView.PlacedLayoutRows);
        this.currentVM.instanceableMoleculeProjector.map(instanceableRowsView.PlacedLayoutRows);
    };
}