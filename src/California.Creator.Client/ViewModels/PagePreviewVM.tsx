"use strict";
import * as maquette from "maquette";
const h = maquette.h;
import { PagePreview } from "./../Models/PagePreview";
import { CaliforniaView, LayoutRow } from "../Models/CaliforniaGenerated";
let currentVM: PagePreviewVM;

export class PagePreviewVM {
    // data
    public tempOriginalContent: string = "";
    public tempContent: string = "";
    public editedLayoutAtomId: number = 0;
    public stylesOfEditedContent: { [key: string]: string } = {};
    public activeCaliforniaViewId: number = 0;
    public activeCaliforniaViewBodyStyleString: string = "";
    public activeCaliforniaViewStyleString: string = "";
    // components
    public californiaViewProjector: maquette.Mapping<CaliforniaView, { renderMaquette: () => maquette.VNode }>;
    public fixedLayoutRowsProjector: maquette.Mapping<LayoutRow, { renderMaquette: () => maquette.VNode }>;

    // client settings

    constructor(pagePreviewArg: PagePreview) {
        currentVM = this;
        this.californiaViewProjector = pagePreviewArg.renderCaliforniaViewArray();
        this.fixedLayoutRowsProjector = pagePreviewArg.renderLayoutRowArray(true);
    };
}