/// <reference path="../jsx.ts" />
import { VNode } from "maquette";
import * as maquette from "maquette";
const h = maquette.h;
import { TokyoController, TrackerClientViewModel, TrackerEvent } from "./TokyoTrackerGenerated";
import * as TokyoTrackerClient from "./TokyoTrackerClient";
import * as TokyoTrackerClientVM from "./../ViewModels/TokyoTrackerClientVM";

export function parseIntFromAttribute(element: EventTarget | null, attributeName: string): number { // can throw // TODO code duplication with california
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
        //console.trace(); // TODO together with new error, and error.stack as debug info, only works compiled in development mode
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

export const DEFAULT_EXCEPTION: string = "unexpected error";
// TODO use strict everywhere?
export class TokyoTrackerApp {
    public static TrackerAppInstance: TokyoTrackerApp;
    public projector: maquette.Projector;
    public controller: TokyoController;
    public client: TokyoTrackerClient.TokyoTrackerClient;
    public clientVM: TokyoTrackerClientVM.TokyoTrackerClientVM;
    public tokyoMainDiv: HTMLElement = document.getElementById("tokyo-main") as HTMLElement;
    public clientData: TrackerClientViewModel = {
        CurrentRevision: 0,
        StatusText: "",
        SelectedTimeTableId: 0,
        SelectedTimeTable: undefined,
        TimeStamp: undefined,
        TrackerEvent: TrackerEvent.ShowInitialClientData,
        TargetId: 0,
        UpdatedName: "",
        UnboundTimeStamps: [],
        UpdatedRating: undefined,
        TimeTables: [],
        TimeTable: undefined,
        TimeTableSecondary: undefined,
        WorkUnitIdSource: 0,
        WorkUnitIdTarget: 0,
        TimeZones: [],
        CultureIds: [],
        WeekDayLetters: [],
        AbbreviatedMonths: [],
        StartDayOfWeekIndex: 0,
        SelectedTimeZoneIdTimeStamps: "",
        SelectedTimeZoneIdView: "",
        SelectedCultureId: "",
        TimeNormTag: undefined,
        TargetWorkUnitId: 0,
        TrackerClientFlags: [],
        TimeNormNoChildren: undefined,
        WorkUnit: undefined,
        UrlToReadOnly: "",
        UrlToReadAndEdit: "",
        ClientTimelineOffset: "",
        WorkUnitDurationString: "",
        TimeNormDurationString: ""
    };
    public constructor() {
        this.projector = maquette.createProjector();
        this.client = new TokyoTrackerClient.TokyoTrackerClient(this);
        this.clientVM = this.client.viewModel;
        this.controller = new TokyoController();
        // get client data TODO send with initial page
        this.controller.InitialClientDataJson(new Date().toString())
            .done(function (data: any): void {
                TokyoTrackerApp.TrackerAppInstance.client.setupInitialDataStore(data);
            }).fail(function (): void {
                console.log("could not get data"); // TODO if negative response => notification
            });
        // initialize projector 
        document.addEventListener("DOMContentLoaded", function (): void {
            TokyoTrackerApp.TrackerAppInstance.projector.append(TokyoTrackerApp.TrackerAppInstance.tokyoMainDiv, TokyoTrackerApp.TrackerAppInstance.renderTrackerApp);
            TokyoTrackerApp.TrackerAppInstance.projector.scheduleRender(); // TODO should not be rendered with items from small device view // TODO might be called too often
        });
        window.addEventListener("resize", function(): void {
            TokyoTrackerApp.TrackerAppInstance.client.recalculateTimelineBounds();
            //TokyoTrackerApp.TrackerAppInstance.client.setupUiForDevice(); TODO
            //TokyoTrackerApp.TrackerAppInstance.projector.scheduleRender(); // TODO might be called too often // TODO document render calls by function
        });
    };
    private renderTrackerApp = (): VNode => {
        return <div afterCreate={TokyoTrackerApp.TrackerAppInstance.createdTrackerAppHandler} styles={{"flex": "1 1 100%", "height": "100%", "max-height": "100%", "display": "flex", "flex-flow": "column nowrap"}}>
            {TokyoTrackerApp.TrackerAppInstance.client.renderMaquette()}
        </div> as VNode;
    };
    private createdTrackerAppHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: maquette.VNode[]): void => {
        //TokyoTrackerApp.TrackerAppInstance.trackerSubDiv = document.getElementById("tracker-sub") as HTMLElement;
        TokyoTrackerApp.TrackerAppInstance.client.recalculateTimelineBounds();
    };
};
