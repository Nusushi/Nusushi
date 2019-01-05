import * as maquette from "maquette";
const h = maquette.h;
import { TimeStamp, WorkUnit, TimeTableViewModel, TrackerTimeZone } from "./../Models/TokyoTrackerGenerated";
import { TokyoTrackerClient, OFFLINE_N } from "./../Models/TokyoTrackerClient";

let currentVM: TokyoTrackerClientVM;

export enum ClientEditMode {
    None = 0,
    Pending = 1,
    WorkUnit = 2,
    TimeNorm = 3,
    TimeTable = 4,
    EditTimeStamp = 5,
    AddManualTimeStamp = 6,
    SelectTimeTable = 7,
    ShareTimeTable = 8,
    GlobalizationSetup = 9,
    DisplayChart = 10
}

export enum TimeZoneSetting {
    Device = 0,
    View = 1,
    TimeStamp = 2
}

export class TokyoTrackerClientVM {
    public isShowButton: boolean = true;
    // data
    public unboundTimeStamps: TimeStamp[] = [];
    public timeTables: TimeTableViewModel[] = [];
    public currentEditMode: ClientEditMode = ClientEditMode.None;
    public tempEditedWorkUnitId: number | undefined = undefined;
    public tempWorkUnitName: string = "";
    public tempEditedTimeNormParentWorkUnitId: number | undefined = undefined;
    public tempEditedTimeNormId: number | undefined = undefined;
    public tempTimeNormName: string = "";
    public tempTimeNormColorR: number = 0; // TODO separate edit functions
    public tempTimeNormColorG: number = 0;
    public tempTimeNormColorB: number = 0;
    public tempTimeTableName: string = "";
    public tempEditedTimeStamp: TimeStamp | undefined = undefined;
    public tempEditedTimeStampHtmlInput: string = "";
    public isTempEditedTimeStampChanged: boolean = false;
    public forMoveWorkUnitId: number | undefined = undefined;
    public hoveredProductivityTimeNormId: number | undefined = undefined;
    public hoveredProductivityStarIndex: number | undefined = undefined;
    public timeZones: TrackerTimeZone[] = [];
    public selectedTimeZoneDevice: number | undefined = undefined;
    public selectedTimeZoneView: number | undefined = undefined;
    public selectedTimeZoneTimeStamp: number | undefined = undefined;    
    public workUnits: WorkUnit[] = [];
    public isToggledTimeline: boolean = true;
    public isExplicitlyToggledTimeline: boolean = false;
    public lastScroll: number = 0;
    public isToggledTableOfContents: boolean = true;
    public isToggledDetailedTimeNorm: boolean = true;
    public isToggledSelectToc: boolean = false;
    public offlineTimeStampCount: number = parseInt(window.localStorage.getItem(OFFLINE_N) as string); // TODO try catch TODO security audit
    public isDragging: boolean = false;
    public draggedWorkUnitId: number = 0;
    public hoveredWorkUnitIdWhileDrag: number = 0;
    public draggedFollowUpWorkUnitId: number = 0;
    public draggedElementPositionStartX: number = 0;
    public draggedElementPositionStartY: number = 0;
    public draggedElementPositionX: number = 0;
    public draggedElementPositionY: number = 0;
    public lastCultureIdDotNet: string = "";
    public lastTimeZoneIdDotNet: string = "";
    // components
    public unboundTimeStampMapping: maquette.Mapping<TimeStamp, { renderMaquette: () => maquette.VNode }>;
    public timeTableSelectionMapping: maquette.Mapping<TimeTableViewModel, { renderMaquette: () => maquette.VNode }>;
    public timeZoneDeviceSelectionMapping: maquette.Mapping<TrackerTimeZone, { renderMaquette: () => maquette.VNode }>;
    public timeZoneViewSelectionMapping: maquette.Mapping<TrackerTimeZone, { renderMaquette: () => maquette.VNode }>;
    public timeZoneTimeStampSelectionMapping: maquette.Mapping<TrackerTimeZone, { renderMaquette: () => maquette.VNode }>;
    public workUnitMapping: maquette.Mapping<WorkUnit, { renderMaquette: () => maquette.VNode }>;
    public tableWorkUnitSelectionMapping: maquette.Mapping<TimeTableViewModel, { renderMaquette: () => maquette.VNode[] }>;
    // client settings
    public isSendLocalTimezone: boolean = true; // TODO
    constructor(clientArg: TokyoTrackerClient) {
        currentVM = this;
        this.unboundTimeStampMapping = clientArg.createUnboundTimeStampMapping();
        this.timeTableSelectionMapping = clientArg.createTimeTableSelectionMapping();
        this.timeZoneDeviceSelectionMapping = clientArg.createTimeZoneMapping(TimeZoneSetting.Device);
        this.timeZoneViewSelectionMapping = clientArg.createTimeZoneMapping(TimeZoneSetting.View);
        this.timeZoneTimeStampSelectionMapping = clientArg.createTimeZoneMapping(TimeZoneSetting.TimeStamp);
        this.workUnitMapping = clientArg.createWorkUnitTableMapping();
        this.tableWorkUnitSelectionMapping = clientArg.createTableWorkUnitSelectionMapping();
    };

    public updateUnboundTimeStampMapping = (timeStamps: TimeStamp[]) => {
        currentVM.unboundTimeStamps = [];
        currentVM.unboundTimeStamps.push(...timeStamps);
        currentVM.unboundTimeStampMapping.map(currentVM.unboundTimeStamps);
    };

    public updateTimeTablesMapping = (timeTables: TimeTableViewModel[]) => {
        currentVM.timeTables = [];
        currentVM.timeTables.push(...timeTables);
        currentVM.timeTableSelectionMapping.map(currentVM.timeTables);
        currentVM.tableWorkUnitSelectionMapping.map(currentVM.timeTables);
    };

    public updateTimeZonesMapping = (timeZones: TrackerTimeZone[]) => {
        currentVM.timeZones = [];
        currentVM.timeZones.push(...timeZones);
        currentVM.timeZoneDeviceSelectionMapping.map(currentVM.timeZones);
        currentVM.timeZoneViewSelectionMapping.map(currentVM.timeZones);
        currentVM.timeZoneTimeStampSelectionMapping.map(currentVM.timeZones);
    };

    public setTimeZone = (timeZoneId: string, setting: TimeZoneSetting) => {
        let timeZoneEntry = currentVM.timeZones.find(tz => tz.IdDotNet == timeZoneId);
        if (timeZoneEntry !== undefined) {
            switch (setting) {
                case TimeZoneSetting.Device:
                    currentVM.selectedTimeZoneDevice = timeZoneEntry.Key;
                    break;
                case TimeZoneSetting.View:
                    currentVM.selectedTimeZoneView = timeZoneEntry.Key;
                    break;
                default:
                    console.log("Not implemented."); // TODO
            }
        }
        else {
            console.log("Invalid time zone."); // TODO
        }
    };

    public updateWorkUnitsForTableMapping = (newWorkUnits: WorkUnit[]) => {
        currentVM.workUnits = [];
        currentVM.workUnits.push(...newWorkUnits.sort((a, b) => { return a.ManualSortOrderKey - b.ManualSortOrderKey; })); //TODO still required?
        currentVM.workUnitMapping.map(currentVM.workUnits);
        /*currentVM.workUnits.map(workUnit => { TODO enable limit data
            if (workUnit.TimeNorms !== undefined) {
                workUnit.TimeNorms.map(timeNorm => {
                    if (timeNorm.StartTime !== undefined) {
                        // if (.) => min / max date from server TODO
                    }
                });
            }
        });*/
    };
};
