/// <reference path="../jsx.ts" />
declare var require: any;
var velocity: any = require("velocity-animate");
import { VNode, VNodeProperties } from "maquette";
import * as maquette from "maquette";
const h = maquette.h;
import { TokyoTrackerApp, DEFAULT_EXCEPTION, parseIntFromAttribute, parseStringFromAttribute } from "./TokyoTrackerApp";
import { Timeline, TimelineTimeSpan, TIMELINE_MIN_WIDTH } from "./Timeline"; // TODO fix constant imports such that always needs module name.CONSTANT 
import { TokyoTrackerClientVM, ClientEditMode, TimeZoneSetting } from "./../ViewModels/TokyoTrackerClientVM";
import { TrackerClientViewModel, TrackerEvent, TimeStamp, WorkUnit, TimeNorm, TimeTable, CreateTimeStampPostViewModel, EditTimeStampPostViewModel, TimeTableViewModel, TrackerTimeZone } from "./TokyoTrackerGenerated";
import * as timelineConstants from "./TimelineConstants";
import { TableOfContents, ClientSectionViewModel } from "./TableOfContents";
import * as popperjs from "popper.js";
import { ProductivityRating } from "../Typewriter/ProductivityRating";
import { TIMELINE_MIN_WIDTH_PX, TIMELINE_COLOR_STRING, INTERACTION_AREA_BG_COLOR_STRING, TIMELINE_HEIGHT_PX, TIMELINE_INTERMEDIATE_COLOR_STRING, TIMELINE_HIGHLIGHT_COLOR_STRING } from "./TimelineConstants";

require("./../third_party/colorpicker/js/colorpicker.js"); // jquery plugin
require("./../third_party/colorpicker/css/colorpicker.css");

// TODO webpack/uglifyJS warnings: review webpack output warnings/errors when compiling for release (side effects, unused variables, ...)
// TODO document / automate sha hash generation and link creation (integrity check vs. version)
let currentApp: TokyoTrackerApp;
let currentClient: TokyoTrackerClient;
let currentTimeline: Timeline;
let currentTableOfContents: TableOfContents;
const UNSET: string = "UNSET";
export const OFFLINE_N: string = "Tn";

export class TokyoTrackerClient {
    public viewModel: TokyoTrackerClientVM;
    public timeTableScrollDivElement: HTMLDivElement | undefined = undefined;
    public topBarGlobalizationButtonElement: HTMLElement | undefined = undefined;
    public timeTableScrollPaddingBottom: string = "300px";
    private activeSensorHandler: EventListenerOrEventListenerObject | undefined = undefined;

    constructor(currentAppArg: TokyoTrackerApp) {
        currentClient = this;
        currentApp = currentAppArg;
        currentTimeline = new Timeline(currentAppArg, 0);
        currentTableOfContents = new TableOfContents(currentAppArg);
        this.viewModel = new TokyoTrackerClientVM(this);
    };

    public renderMaquette = (): VNode => {
        /*TODO temp version for alpha release*/
        let unboundTimeStampCount: number = 0;
        let unboundTimestampCountString: string = "";
        let isOddNumber: boolean = false;
        if (currentApp.clientData.UnboundTimeStamps !== undefined) {
            unboundTimeStampCount = currentApp.clientData.UnboundTimeStamps.length;
            isOddNumber = unboundTimeStampCount % 2 == 1;
            unboundTimestampCountString = isOddNumber ? ((unboundTimeStampCount - 1) / 2).toString() : (unboundTimeStampCount / 2).toString();
        }
        // TODO make export to csv only enabled when bound timestamps > 0
        let isWarningTriggered: boolean = currentApp.clientData.ClientTimelineOffset !== "";
        let isSensorEnabled: boolean = currentTimeline.viewModel.isSensorEnabled;
        let buttonGroupStyles = { "flex": "1 1 50px", "border-color": "white", "align-items": "center" };
        let buttonGroupStylesCreateTimestamp = { "flex": "1 1 50px", "align-items": "center", "border-color": "white" };
        let buttonGroupStylesCreateTimeTable = { "flex": "1 1 50px", "border-color": "white", "align-items": "center" };
        let buttonGroupStylesEnableSensor = { "flex": "1 1 50px", "border-color": currentTimeline.viewModel.isSensorEnabled ? "brown" : "white", "align-items": "center" };
        let buttonGroupStylesTimelineEnabler = { "flex": "1 1 50px", "border-color": "white", "align-items": "center" };
        let buttonGroupStylesEnableSensors = { "flex": "1 1 50px", "border-color": isSensorEnabled ? "red" : undefined, "align-items": "center" };
        let buttonGroupStyleProcessTimestamps = { "flex": "1 1 50px", "border-color": TIMELINE_COLOR_STRING, "align-items": "center", "letter-spacing": "0.3px" /*"transition": "all ease-in 0.2s"TODO fake transition with afterEnter etc*/ };
        let buttonGroupStyleStatusChangesBottomBar = { "flex": "1 1 50px", "border-color": TIMELINE_COLOR_STRING, "align-items": "center", "margin-top": "-6px", "padding": "0", "text-align": "center", "letter-spacing": "0.3px" /*"transition": "all ease-in 0.2s"TODO fake transition with afterEnter etc*/ };
        let buttonGlobalizationStyles = { "flex": "1 1 50px", "align-items": "center", "opacity": isWarningTriggered ? "1" : "0.2", "border-color": isWarningTriggered ? "brown" : "white" };
        let buttonGlobalizationWarningText: string;
        if (isWarningTriggered) {
            buttonGlobalizationWarningText = `(${currentApp.clientData.ClientTimelineOffset})`;
        }
        else {
            buttonGlobalizationWarningText = "";
        }
        let isRemoveTableEnabled = false;
        let isTimeZonePreviousActive = false;
        let isTimeZoneNextActive = false;
        let isSyncButtonActive = currentClient.viewModel.offlineTimeStampCount > 0;
        if (currentApp.clientData.SelectedTimeTable !== undefined &&
            currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits !== undefined &&
            currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
            isRemoveTableEnabled = currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits.length == 0;
        }
        let tableNameStyles = { "flex": "0 0 auto", "padding-left": "12px" /*as in table*/, "text-decoration": "underline", "margin-right": "20px", "margin-bottom": "0" };
        let selectedTableIdString: string = currentApp.clientData.SelectedTimeTableId.toString();
        
        /*TODO audit security for how strings are created in javascript and maquette, everywhere*/
        return <div key="client" styles={{ "position": "relative", "flex": "1 1 99%", "height": "99%", "max-height": "99%", "max-width": "100%", "width": "100%", "display": "flex", "flex-flow": "column nowrap" }}
            ontouchend={currentClient.mouseUpGlobalTouchHandler}
            onmouseup={currentClient.mouseUpGlobalPointerHandler}
            onmousemove={currentClient.mouseGlobalMoveHandler}>
            <div key="2" styles={{ "flex": "0 0 auto", "display": "flex", "flex-flow": "row nowrap", "background-color": INTERACTION_AREA_BG_COLOR_STRING, "margin-bottom": "4px" }}>
                <button key="a" type="button" class="btn btn-sm" onclick={currentClient.createTimeStampClickHandler} styles={buttonGroupStylesCreateTimestamp}>&#8986;</button>
                <button key="b" type="button" class="btn btn-sm" onclick={currentClient.createManualTimeStampClickHandler} styles={buttonGroupStyles}>&#8986;&#8230;</button>
                {(unboundTimeStampCount > 1 && isOddNumber) ? <button key="c" type="button" class="btn btn-sm" onclick={currentClient.processTimeStampsClickHandler} styles={buttonGroupStyleProcessTimestamps}>&#9783; (<b>{unboundTimestampCountString}</b>+&#189;)</button>
                    : (unboundTimeStampCount > 1 && !isOddNumber) ? <button key="c0" type="button" class="btn btn-sm" onclick={currentClient.processTimeStampsClickHandler} styles={buttonGroupStyleProcessTimestamps}>&#9783; (<b>{unboundTimestampCountString}</b>+0)</button>
                        : unboundTimeStampCount == 1 ? <button disabled key="c00" type="button" class="btn btn-sm" onclick={currentClient.processTimeStampsClickHandler} styles={buttonGroupStyleProcessTimestamps}>&#9783; <span styles={{"opacity": "0"}}>{unboundTimestampCountString}</span>(+&#189;)</button>
                        : undefined}
                {!(currentClient.viewModel.currentEditMode === ClientEditMode.TimeTable) ? <button key="e" type="button" class="btn btn-sm" onclick={currentClient.createTimeTableClickHandler} styles={buttonGroupStylesCreateTimeTable}><p styles={{ "transform": "rotate(90deg)", "margin-bottom": "0" }}>&#8631;</p></button> : undefined}
                {!(currentClient.viewModel.currentEditMode === ClientEditMode.TimeTable) ? <button key="f" type="button" class="btn btn-sm" onclick={currentClient.toggleEnableSensorTimelime} styles={buttonGroupStylesEnableSensor}><p styles={{ "margin-bottom": "0" }}>&lt;-&gt;</p></button> : undefined}
                { /* <button disabled key="g" type="button" class="btn btn-sm" onclick={currentClient.displayChartClickHandler} styles={buttonGroupStyles}>&#128200;...</button> */}
                <button key="h" type="button" class="btn btn-sm" onclick={currentClient.globalizationSetupClickHandler} styles={buttonGlobalizationStyles} afterCreate={currentClient.globalizationButtonTopBarAfterCreateHandler}>&#127760;... {buttonGlobalizationWarningText}</button>
                <button key="i" type="button" class="btn btn-sm" onclick={currentClient.toggleSmallDeviceView} styles={buttonGroupStylesTimelineEnabler}><span styles={{ "transform": currentClient.viewModel.isToggledTimeline ? undefined : "rotate(-90deg)", "display": "inline-block", "transition": "all ease-in .2s", "font-weight": "bold" }}>&nabla;</span></button>
                <button key="j" type="button" class="btn btn-sm" onclick={currentClient.shareTimeTableClickHandler} styles={buttonGroupStyles}>&#9993;&#8230;</button>
                {isSyncButtonActive ? <button key="k" type="button" class="btn btn-sm" onclick={currentClient.sendTimeStampsOfflineClickHandler} styles={buttonGroupStylesCreateTimestamp}>&#8593; ({currentClient.viewModel.offlineTimeStampCount/*TODO security audit: how to make sure its always just a string? .toString()? and that number.prototype isnt changed*/})</button> : undefined}
            </div>
            {currentClient.viewModel.isToggledTimeline ? <div key="t3" styles={{ "display": "flex", "flex-flow": "row nowrap", "padding-bottom": "20px", "margin-bottom": "5px", "padding-top": "5px", "border-bottom": "1px solid rgba(0, 0, 0, 0.1)", "border-top": "1px solid rgba(0, 0, 0, 0.1)" }}>
                <div key="0" styles={{ "flex": "1 1 100%", "width": "100%", "position": "relative", "max-width": "100%" }}>
                    {currentTimeline.renderMaquette()}
                </div>
                {/*<div key="1" styles={{ "height": "75px", "flex": "0 0 30px", "width": "30px", "display": "flex", "flex-flow": "column nowrap", "padding-bottom": "1px" }}>
                    {isTimeZonePreviousActive ? <button key="a" type="button" class="btn btn-sm" onclick={currentClient.timeZoneViewUpClickHandler} styles={buttonGroupStylesSide}>{"<-"}</button> : <button disabled key="aa" type="button" class="btn btn-sm" onclick={currentClient.timeZoneViewUpClickHandler} styles={buttonGroupStylesSide}>{"<-"}</button>}
                    {isTimeZoneNextActive ? <button key="b" type="button" class="btn btn-sm" onclick={currentClient.timeZoneViewDownClickHandler} styles={buttonGroupStylesSide}>{"->"}</button> : <button disabled key="bb" type="button" class="btn btn-sm" onclick={currentClient.timeZoneViewDownClickHandler} styles={buttonGroupStylesSide}>{"->"}</button>}
                </div>*/}
            </div> : undefined /*TODO module timeline / TOC not required if not supported by device*/}
            <div key="3" styles={{ "flex": "1 1 1px", "display": "flex", "flex-flow": "row nowrap", "height": "1px" }}>
                {currentClient.viewModel.isToggledTableOfContents ? <div key="0" styles={{ "flex": "0 0 15%", "height": "auto", "width": "15%", "max-width": "15%", "overflow-x": "hidden", "overflow-y": "scroll" }}>
                    {currentTableOfContents.renderMaquette()}
                </div> : undefined}
                <div key="1" styles={{ "flex": "1 1 100%", "width": "100%", "overflow-x": "scroll", "overflow-y": "scroll" }} onscroll={currentTableOfContents.onContentPositionScrolledHandler}
                    afterCreate={currentClient.timeTableAfterCreateHandler}>
                    <div key="2" styles={{ "flex": "0 0 auto", "display": "flex", "flex-flow": "row nowrap" }}>
                        <a key="0" class="anchor" name={`a_${currentApp.clientData.SelectedTimeTableId}`} tabindex="-1"></a>
                        {
                            currentClient.viewModel.currentEditMode === ClientEditMode.TimeTable ?
                                <input key="-1"
                                    value={currentClient.viewModel.tempTimeTableName}
                                    oninput={currentClient.timeTableNameChangedHandler}
                                    onblur={currentClient.timeTableNameLostFocusHandler}
                                    onkeydown={currentClient.timeTableNameKeyDownHandler}
                                    afterCreate={currentClient.inputNameAfterCreateHandler}
                                    styles={tableNameStyles}>
                                </input> :
                                <h3 key="-2"
                                    tid={selectedTableIdString}
                                    onclick={currentClient.tableNameClickedHandler}
                                    styles={tableNameStyles}>
                                    {(currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits) ?
                                        `${currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.Name}` : undefined}
                                </h3>
                        }
                        {(!(currentClient.viewModel.currentEditMode === ClientEditMode.TimeTable) && isRemoveTableEnabled) ? <button key="3" type="button" class="btn btn-sm" onclick={currentClient.removeTimeTableClickHandler} styles={{ "margin-left": "20px", "width": "35px" }}>&#10008;</button> : undefined /*TODO test throws exception when called anyway*/}
                    </div>
                    <div key="-3" styles={{ "flex": "1 1 auto", "height": "auto", "position": "relative" /*for drag&drop*/ }}>
                        {currentClient.viewModel.workUnitMapping.results.map(r => r.renderMaquette())}
                        {!(currentClient.viewModel.currentEditMode === ClientEditMode.TimeTable) ? <button key="1" type="button" class="btn btn-sm" onclick={currentClient.createWorkUnitClickHandler} styles={{ "width": "35px", "margin-left": "15px", "margin-top": "1rem", "margin-bottom": currentClient.timeTableScrollPaddingBottom }}>+</button> : undefined}
                    </div>
                </div>
            </div>
            {currentClient.viewModel.isToggledTimeline && unboundTimeStampCount > 0 ?
                <hr key="hr1" styles={{"margin-bottom": "5px"}}></hr> : undefined}
            {currentClient.viewModel.isToggledTimeline && unboundTimeStampCount > 0 ?
                <div key="4" styles={{ "display": "flex", "flex-flow": "row wrap", "flex": "0 0 auto", "height": "auto", "align-self": "flex-end", "margin-bottom": "3px" }}>
                    {currentClient.viewModel.unboundTimeStampMapping.results.map(r => r.renderMaquette())}
                </div> : undefined}
            {currentClient.viewModel.isToggledTimeline && unboundTimeStampCount > 1 ?
                <div key="6" styles={{ "flex": "0 0 auto", "align-self": "flex-end" }}>
                    <select key="a" onchange={currentClient.activeWorkUnitChangedHandler}>
                        {<option disabled key="a" styles={{ "font-weight": "bold" }}>Add new times to ...</option>}
                        {currentClient.viewModel.tableWorkUnitSelectionMapping.results.map(r => r.renderMaquette())}
                    </select>
                    {isOddNumber ? <button key="b" type="button" class="btn btn-sm" onclick={currentClient.processTimeStampsClickHandler} styles={buttonGroupStyleStatusChangesBottomBar}>&#9783; (<b>{unboundTimestampCountString}</b>+&#189;)</button>
                        : <button key="b0" type="button" class="btn btn-sm" onclick={currentClient.processTimeStampsClickHandler} styles={buttonGroupStyleStatusChangesBottomBar}>&#9783; (<b>{unboundTimestampCountString}</b>+0)</button>}
                </div> : undefined}
            <div key="5">
                {currentClient.renderEditTimeStampPopup()}
            </div>
            <div key="7">
                {currentClient.renderShareTimeTablePopup()}
            </div>
            <div key="8">
                {currentClient.renderGlobalizationPopup()}
            </div>
            <div key="9">
                {currentClient.renderChartPopup()}
            </div>
            {currentClient.viewModel.isToggledSelectToc ? <div key="10" styles={{ "flex": "0 0 auto" }}>
                <select key="a" onchange={currentClient.selectedTimeTableChangedHandler}>
                    {currentClient.viewModel.timeTableSelectionMapping.results.map(r => r.renderMaquette())}
                </select>
            </div> : undefined}
        </div> as VNode;
    };

    public timelineShowAnimation = () => {
        if (currentTimeline.timelineDivElement === undefined) {
            console.log(DEFAULT_EXCEPTION);
            return;
        }
        velocity.animate(currentTimeline.timelineDivElement, "stop", true);
        let animationTime = 911 * (1 - (currentTimeline.timelineDivElement.clientHeight / TIMELINE_HEIGHT_PX));
        velocity.animate(currentTimeline.timelineDivElement, {
            height: TIMELINE_HEIGHT_PX,
        }, {
                duration: animationTime,
                easing: "ease-in"
            }, );
    };

    public timelineHideAnimation = () => {
        if (currentTimeline.timelineDivElement === undefined) {
            console.log(DEFAULT_EXCEPTION);
            return;
        }
        velocity.animate(currentTimeline.timelineDivElement, "stop", true);
        if (currentClient.timeTableScrollDivElement !== undefined
            && currentClient.timeTableScrollDivElement.scrollHeight > (document.body.clientHeight + TIMELINE_HEIGHT_PX)) {
            // prevent bounce forward<=>backward when animating, occurs during or after the hide-animation is running, which leads to increased space => less scrollHeight => eventually the hide animation leads to scroll=0 again => loop // TODO check still required in final version
            let animationTime = 911 * (currentTimeline.timelineDivElement.clientHeight / TIMELINE_HEIGHT_PX);
            velocity.animate(currentTimeline.timelineDivElement, {
                height: 0,
            }, {
                    duration: animationTime,
                    easing: "ease-out"
                });
        }
    };
    
    public timeStampTimeChangedHandler = (evt: KeyboardEvent) => {
        let dateStringInputFormat = (evt.target as HTMLInputElement).value;
        if (dateStringInputFormat === "") {
            // invalid date => empty value but browser displays something
            return;
        }
        let extractedYear = parseInt(dateStringInputFormat.slice(0, 4));
        let extractedMonth = parseInt(dateStringInputFormat.slice(5, 7));
        let extractedDayOfMonth = parseInt(dateStringInputFormat.slice(8, 10));
        let extractedHours = parseInt(dateStringInputFormat.slice(11, 13));
        let extractedMinutes = parseInt(dateStringInputFormat.slice(14, 16));
        currentClient.viewModel.tempEditedTimeStampHtmlInput = currentClient.convertTimeForHtmlInput(extractedYear, extractedMonth, extractedDayOfMonth, extractedHours, extractedMinutes);
        currentClient.viewModel.isTempEditedTimeStampChanged = true;
    };

    public convertHtmlInputToEditTimeStampVM = (htmlInputValue: string, timeStampId: number): EditTimeStampPostViewModel => {
        let extractedYear = parseInt(htmlInputValue.slice(0, 4));
        let extractedMonth = parseInt(htmlInputValue.slice(5, 7));
        let extractedDayOfMonth = parseInt(htmlInputValue.slice(8, 10));
        let extractedHours = parseInt(htmlInputValue.slice(11, 13));
        let extractedMinutes = parseInt(htmlInputValue.slice(14, 16));
        return {
            Year: extractedYear,
            Month: extractedMonth,
            Day: extractedDayOfMonth,
            Hour: extractedHours,
            Minute: extractedMinutes,
            Second: 0,
            Millisecond: 0,
            StatusMessage: "",
            JsTimeString: new Date(extractedYear, extractedMonth, extractedDayOfMonth, extractedHours, extractedMinutes).toString(),  // TODO cannot use new Date because it's forced to local time zone
            TimeStampName: "UNSET_TIMESTAMP",
            ExactTicks: 0,
            YearOld: extractedYear,
            MonthOld: extractedMonth,
            DayOld: extractedDayOfMonth,
            HourOld: extractedHours,
            MinuteOld: extractedMinutes,
            SecondOld: 0,
            MillisecondOld: 0,
            TimeStampId: timeStampId
        };
    };

    public recalculateTimelineBounds = () => {
        currentTimeline.viewModel.actualSpacePx = document.body.clientWidth; // TODO subtract body margins for portability / use renderedtimeline div
        currentTimeline.viewModel.isEnoughSpace = document.body.clientWidth > TIMELINE_MIN_WIDTH_PX;
        if (currentTimeline.viewModel.isEnoughSpace) {
            currentTimeline.viewModel.timelineMaxDayElements = 14;
            currentTimeline.viewModel.timelineMinZoomLevel = -1;
            currentTimeline.viewModel.timelineMaxZoomLevel = 20;
            currentTimeline.viewModel.timelineZoomLevel = currentTimeline.viewModel.timelineZoomLevel > currentTimeline.viewModel.timelineMaxZoomLevel ? currentTimeline.viewModel.timelineMaxZoomLevel : currentTimeline.viewModel.timelineZoomLevel < currentTimeline.viewModel.timelineMinZoomLevel ? currentTimeline.viewModel.timelineMinZoomLevel : currentTimeline.viewModel.timelineZoomLevel; // TODO dont double calculate in initial render
        }
        else if (currentTimeline.viewModel.actualSpacePx > 500) { // TODO
            currentTimeline.viewModel.timelineMaxDayElements = 8;
            currentTimeline.viewModel.timelineMinZoomLevel = -1;
            currentTimeline.viewModel.timelineMaxZoomLevel = 10;
            currentTimeline.viewModel.timelineZoomLevel = currentTimeline.viewModel.timelineZoomLevel > currentTimeline.viewModel.timelineMaxZoomLevel ? currentTimeline.viewModel.timelineMaxZoomLevel : currentTimeline.viewModel.timelineZoomLevel < currentTimeline.viewModel.timelineMinZoomLevel ? currentTimeline.viewModel.timelineMinZoomLevel : currentTimeline.viewModel.timelineZoomLevel; // TODO dont double calculate in initial render
        }
        else { // smallest possible view
            currentTimeline.viewModel.timelineMaxDayElements = 4;
            currentTimeline.viewModel.timelineMinZoomLevel = -1;
            currentTimeline.viewModel.timelineMaxZoomLevel = 1;
            currentTimeline.viewModel.timelineZoomLevel = currentTimeline.viewModel.timelineZoomLevel > currentTimeline.viewModel.timelineMaxZoomLevel ? currentTimeline.viewModel.timelineMaxZoomLevel : currentTimeline.viewModel.timelineZoomLevel < currentTimeline.viewModel.timelineMinZoomLevel ? currentTimeline.viewModel.timelineMinZoomLevel : currentTimeline.viewModel.timelineZoomLevel; // TODO dont double calculate in initial render
        }
        currentApp.projector.scheduleRender(); // TODO
    };

    public convertHtmlInputToTimeStampVM = (htmlInputValue: string): CreateTimeStampPostViewModel => {
        let extractedYear = parseInt(htmlInputValue.slice(0, 4));
        let extractedMonth = parseInt(htmlInputValue.slice(5, 7));
        let extractedDayOfMonth = parseInt(htmlInputValue.slice(8, 10));
        let extractedHours = parseInt(htmlInputValue.slice(11, 13));
        let extractedMinutes = parseInt(htmlInputValue.slice(14, 16));
        return {
            Year: extractedYear,
            Month: extractedMonth,
            Day: extractedDayOfMonth,
            Hour: extractedHours,
            Minute: extractedMinutes,
            Second: 0, // TODO second milisecond not supported with native inputs
            Millisecond: 0,
            StatusMessage: "",
            ManualDateTimeString: "",
            JsTimeString: new Date(extractedYear, extractedMonth, extractedDayOfMonth, extractedHours, extractedMinutes).toString(),  // TODO cannot use new Date because it's forced to local time zone
            TimeStampName: "UNSET_TIMESTAMP"
        } as CreateTimeStampPostViewModel;
    };

    public convertTimeStampForHtmlInput = (timeStampOrString: Date | string): string => {
        let timeStamp: Date;
        if (typeof timeStampOrString as string === "Date") {
            timeStamp = timeStampOrString as Date;
        }
        else { // TODO conversion should happen when applying data to client VM
            timeStamp = new Date(timeStampOrString as string); // TODO cannot use new Date because it's forced to local time zone
        }
        let editTimeYear = timeStamp.getFullYear();
        let editTimeMonth = timeStamp.getMonth();
        let editTimeDayOfMonth = timeStamp.getDate();
        let editTimeHour = timeStamp.getHours();
        let editTimeMinute = timeStamp.getMinutes();
        return currentClient.convertTimeForHtmlInput(editTimeYear, editTimeMonth + 1, editTimeDayOfMonth, editTimeHour, editTimeMinute);
    }

    public convertTimeForHtmlInput = (year: number, month: number, dayOfMonth: number, hour: number, minute: number): string => {
        let editTimeYearString: string = year.toString();
        let editTimeMonthString: string = month < 10 ? "0" + month : month.toString();
        let editTimeDayString: string = dayOfMonth < 10 ? "0" + dayOfMonth : dayOfMonth.toString();
        let editTimeHourString: string = hour < 10 ? "0" + hour : hour.toString();
        let editTimeMinuteString: string = minute < 10 ? "0" + minute : minute.toString();
        return `${editTimeYearString}-${editTimeMonthString}-${editTimeDayString}T${editTimeHourString}:${editTimeMinuteString}`;
    };

    public renderEditTimeStampPopup = () => {
        // similar popup is used for 1) add manual timestamp 2) edit timestamp
        let isPopupVisible: boolean = currentClient.viewModel.currentEditMode === ClientEditMode.EditTimeStamp || currentClient.viewModel.currentEditMode === ClientEditMode.AddManualTimeStamp;
        let editTimeValue: string = isPopupVisible ? currentClient.viewModel.tempEditedTimeStampHtmlInput : "";
        let isShowDuplicateButton: boolean = (!currentClient.viewModel.isTempEditedTimeStampChanged && (currentClient.viewModel.currentEditMode !== ClientEditMode.AddManualTimeStamp));
        let isDisplayWarning: boolean = currentApp.clientData.ClientTimelineOffset !== "";
        let buttonTopBarStyles = { "flex": "1 0 10%", "width": "10%", "min-width": "10%" };
        let buttonGlobalizationStyles: { [key: string]: string | undefined } /*TODO everywhere */ = { "display": isDisplayWarning ? undefined : "none", "flex": "1 0 10%", "width": "10%", "min-width": "10%", "border-color": "brown" };
        let buttonGlobalizationWarningText: string;
        if (isDisplayWarning) {
            buttonGlobalizationWarningText = `(${currentApp.clientData.ClientTimelineOffset})`;
        }
        else {
            buttonGlobalizationWarningText = "";
        }
        // TODO take current time button
        // TODO accept changes not necessary on edge? has integrated button => needs to confirm twice
        return <div id={ClientEditMode[ClientEditMode.EditTimeStamp]} styles={{ "display": isPopupVisible ? "block" : "none", "background-color": "white", "border": "solid black 1px", "z-index": "2" }}>
            <form>
                <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                    {isShowDuplicateButton ? <button key="a" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} class="btn btn-sm" onclick={currentClient.duplicateTimeStampClickHandler}>x2</button> : <button disabled key="a0" role="button" styles={{ "flex": "10% 1 0", "width": "10%", "min-width": "10%" }} class="btn btn-sm">x2</button>}
                    <button key="b" type="button" class="btn btn-sm" onclick={currentClient.globalizationSetupClickHandler} styles={buttonGlobalizationStyles}>&#127760; {buttonGlobalizationWarningText}</button>
                    <button key="c" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%", "background-color": currentClient.viewModel.isTempEditedTimeStampChanged ? TIMELINE_INTERMEDIATE_COLOR_STRING : undefined }} class="btn btn-sm" onclick={currentClient.saveEditTimeStampClickHandler}>&#10004;</button>
                    <button key="d" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%", "background-color": currentClient.viewModel.isTempEditedTimeStampChanged ? TIMELINE_HIGHLIGHT_COLOR_STRING : undefined }} class="btn btn-sm" onclick={currentClient.cancelEditTimeStampClickHandler}>x</button>
                </div>
                <div class="form-group">
                    <input type="datetime-local" class="form-control" id="editLocalTimeStamp1" aria-describedby="timeStampHelp" value={editTimeValue}
                        oninput={currentClient.timeStampTimeChangedHandler}>
                        <small id="timeStampHelp" class="form-text text-muted">When does this show TODO.</small>
                    </input>
                </div>
            </form>
        </div>;
    };

    /*public renderSelectTimeTablePopup = () => { TODO UNUSED
        // TODO open local file?
        let isSelectMode: boolean = currentClient.viewModel.currentEditMode === ClientEditMode.SelectTimeTable;
        // TODO set role=button instead type=button everywhere
        return <div id={ClientEditMode[ClientEditMode.SelectTimeTable]} styles={{ "display": isSelectMode ? "block" : "none", "background-color": "white", "border": "solid black 1px", "z-index": "2" }}>
            <form>
                <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                    <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} class="btn btn-sm" onclick={currentClient.createTimeTableClickHandler}>&#10010;</button>
                    <button key="c" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} class="btn btn-sm" onclick={currentClient.cancelSelectTimeTableClickHandler}>x</button>
                </div>
                <div class="form-group">
                    <select key="a" onchange={currentClient.selectedTimeTableChangedHandler}>
                        {currentClient.viewModel.timeTableSelectionMapping.results.map(r => r.renderMaquette())}
                    </select>
                    <button key="b" role="button" class="btn btn-sm" onclick={currentClient.removeTimeTableClickHandler}>&#10008;</button>
                </div>
            </form>
        </div>
    };*/

    public renderShareTimeTablePopup = () => {
        // TODO multilanguage
        let isShareMode: boolean = currentClient.viewModel.currentEditMode === ClientEditMode.ShareTimeTable;
        return <div id={ClientEditMode[ClientEditMode.ShareTimeTable]} styles={{ "display": isShareMode ? "block" : "none", "background-color": "white", "border": "solid black 1px", "z-index": "2" }}>
            <form>
                <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                    <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} class="btn btn-sm" onclick={currentClient.cancelShareTimeTableClickHandler}>x</button>
                </div>
                <div class="form-group">
                    <p key="0" styles={{ "margin-top": "1rem" }}>{currentApp.clientData.UrlToReadOnly}<span><button key="a" type="button" class="btn btn-sm" onclick={currentClient.logoutClickHandler}>&#128274;</button></span></p>
                    <button key="b" type="button" class="btn btn-sm" onclick={currentClient.exportToCSVClickHandler}>&#128190;</button>
                    <button key="c" type="button" class="btn btn-sm" onclick={/*currentClient.enableShortLinkTODO*/undefined}>Device Switch</button>
                    {/* TODO <p key="b">{currentApp.clientData.UrlToReadAndEdit}</p>*/}
                    <button key="d" type="button" class="btn btn-sm" onclick={currentClient.californiaClickHandler}>CALIFORNIA</button>
                </div>
            </form>
        </div>;
    };

    public californiaClickHandler = (evt: MouseEvent) => {
        window.location.assign(window.location.origin + "/california/"); // TODO hardcoded link
    };

    public renderGlobalizationPopup = () => {
        let isGlobalizationMode: boolean = currentClient.viewModel.currentEditMode === ClientEditMode.GlobalizationSetup; // TODO popups need ids for divs => is there a better way to manage many popups
        let buttonLastCultureStyles = {
            "width": "auto",
            "height": "35px"
        };
        let buttonLastCultureCaptionStyles = {
            "transform": "matrix(0, -1, -1, 0, 1, 1)",
            "margin": "0"
        };
        let buttonLastCultureCaptionStylesInverse = {
            "transform": "matrix(1, 0, 0, 1, 1, 1)",
            "margin": "0"
        };
        return <div id={ClientEditMode[ClientEditMode.GlobalizationSetup]} styles={{ "display": isGlobalizationMode ? "block" : "none", "background-color": "white", "border": "solid black 1px", "z-index": "2" }}>
            <form>
                <div key="0" styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                    <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} class="btn btn-sm" onclick={currentClient.cancelGlobalizationSetupClickHandler}>x</button>
                </div>
                <div key="-1" styles={{ "flex": "0 0 auto", "height": "auto", "width": "33.33%", "display": "flex" }}>
                    <select key="0" onchange={currentClient.cultureChangedHandler} styles={{ "width": "auto", "height": "35px" }}>
                        {currentApp.clientData.CultureIds !== undefined ? currentApp.clientData.CultureIds.map(culture => {
                            let selectedCulture: string = currentApp.clientData.SelectedCultureId;
                            return (culture.IdDotNet === selectedCulture) ? <option selected key={culture.Key} value={culture.Key.toString()}>{culture.DisplayName}</option> :
                                <option key={culture.Key} value={culture.Key.toString()}>{culture.DisplayName}</option>;
                        }) : undefined}
                    </select>
                    {currentClient.viewModel.lastCultureIdDotNet !== "" ? <button key="a" type="button" class="btn btn-sm" onclick={currentClient.selectLastSelectedCultureClickHandler} styles={buttonLastCultureStyles}><span key="0"><p styles={buttonLastCultureCaptionStyles}>&#8631;</p></span>{/*TODO revert transform in :before pseudo element, <span key="1"><p styles={buttonLastCultureCaptionStylesInverse}>&#8631;</p></span>*/}</button>
                        : <button disabled key="a0" type="button" class="btn btn-sm" onclick={currentClient.selectLastSelectedCultureClickHandler} styles={buttonLastCultureStyles}><span key="0"><p styles={buttonLastCultureCaptionStyles}>&#8631;</p></span>{/*TODO revert transform in :before pseudo element, <span key="1"><p styles={buttonLastCultureCaptionStylesInverse}>&#8631;</p></span>*/}</button>}
                </div>
                {/*TODO changed both with one command temporarily <div key="-2" styles={{"flex": "0 0 auto", "height": "auto", "width": "33.33%"}}>
                    <h5>device (cookie):</h5>
                    <select onchange={currentClient.timeZoneDeviceChangedHandler}>
                        {(currentClient.viewModel.selectedTimeZoneDevice === undefined) ? <option key="a" selected>Select device time zone...</option> : undefined}
                        {currentClient.viewModel.timeZoneDeviceSelectionMapping.results.map(r => r.renderMaquette())}
                    </select>
                    </div>*/}
                <div key="-3" styles={{"flex": "0 0 auto", "height": "auto", "width": "33.33%"}}>
                    <select onchange={currentClient.timeZoneViewChangedHandler}>
                        {(currentClient.viewModel.selectedTimeZoneView === undefined) ? <option key="a" selected>Select view time zone...</option> : undefined}
                        {currentClient.viewModel.timeZoneViewSelectionMapping.results.map(r => r.renderMaquette())}
                    </select>
                    {currentClient.viewModel.lastTimeZoneIdDotNet !== "" ? <button key="a" type="button" class="btn btn-sm" onclick={currentClient.selectLastSelectedTimeZoneClickHandler} styles={buttonLastCultureStyles}><span key="0"><p styles={buttonLastCultureCaptionStyles}>&#8631;</p></span>{/*TODO revert transform in :before pseudo element, <span key="1"><p styles={buttonLastCultureCaptionStylesInverse}>&#8631;</p></span>*/}</button>
                        : <button disabled key="a0" type="button" class="btn btn-sm" onclick={currentClient.selectLastSelectedTimeZoneClickHandler} styles={buttonLastCultureStyles}><span key="0"><p styles={buttonLastCultureCaptionStyles}>&#8631;</p></span>{/*TODO revert transform in :before pseudo element, <span key="1"><p styles={buttonLastCultureCaptionStylesInverse}>&#8631;</p></span>*/}</button>}
                </div>
            </form>
        </div>
    };

    public renderChartPopup = () => {
        let isDisplayChart: boolean = currentClient.viewModel.currentEditMode === ClientEditMode.DisplayChart;
        return <div id={ClientEditMode[ClientEditMode.DisplayChart]} styles={{ "display": isDisplayChart ? "block" : "none", "background-color": "white", "border": "solid black 1px", "z-index": "2" }}>
            <form>
                <div styles={{ "display": "flex", "flex-flow": "row nowrap", "min-width": "250px" }}>
                    <button key="b" role="button" styles={{ "flex": "1 0 10%", "width": "10%", "min-width": "10%" }} class="btn btn-sm" onclick={currentClient.cancelChartClickHandler}>x</button>
                </div>
                <div key="-4" styles={{"flex": "0 0 auto", "height": "auto", "width": "auto", "min-height": "200px"}}>{"GraphTODO"}</div>
            </form>
        </div>
    };
    

    public renderProductivityRating = (timeNormId: number, productivityPercentage: number): VNode[] => {
        if (productivityPercentage < 0.0 || productivityPercentage > 100.0) {
            console.log("Invalid productivity rating.");
        }
        let totalStarCount: number = 3; // 0 star = 0%, 1 star = 33.3%, 2 stars = 66.7%, 3 stars = 100%
        let percentagePerStar: number = 100.0 / totalStarCount;
        let decimalPercentage: number = (productivityPercentage + 0.1) / 100.0;
        let floatFilledStarCount: number = decimalPercentage * totalStarCount;
        let filledStarCount: number = Math.floor(floatFilledStarCount); // TODO move to backend
        let whiteStarCount: number = 3 - filledStarCount;
        let ratingElements: VNode[] = [];
        let isHoveredGroup: boolean = false;
        if (currentClient.viewModel.hoveredProductivityStarIndex != undefined && currentClient.viewModel.hoveredProductivityTimeNormId != undefined) {
            isHoveredGroup = timeNormId == currentClient.viewModel.hoveredProductivityTimeNormId;
        }
        for (let i = 0; i < filledStarCount; i++) {
            let currentRating: number = (i + 1) * percentagePerStar;
            let isHoveredStar: boolean = false;
            if (isHoveredGroup && currentClient.viewModel.hoveredProductivityStarIndex != undefined) {
                isHoveredStar = (currentClient.viewModel.hoveredProductivityStarIndex >= i && currentClient.viewModel.hoveredProductivityStarIndex < filledStarCount) || (currentClient.viewModel.hoveredProductivityStarIndex == i);
            }
            ratingElements.push(<span key={`a${i.toString()}`}
                tid={timeNormId.toString()}
                wid={currentRating.toPrecision(3)}
                zid={(i == filledStarCount - 1) ? "1" : "0"}
                xid={i.toString()}
                onclick={currentClient.editProductivityRatingClickHandler}
                onmouseenter={currentClient.enterProductivityRatingHandler}
                onmouseleave={currentClient.exitProductivityRatingHandler}
                styles={{"color": isHoveredStar ? "rgb(42, 178, 191)" : "rgb(14, 30, 86)"}}>&#9733;</span > as VNode);
        }
        for (let j = 0; j < whiteStarCount; j++) {
            let currentRating: number = (filledStarCount + j + 1) * percentagePerStar;
            let isHoveredStar: boolean = false;
            if (isHoveredGroup && currentClient.viewModel.hoveredProductivityStarIndex != undefined) { // TODO number|undefined everywhere potentially wrong
                isHoveredStar = (filledStarCount + j) <= currentClient.viewModel.hoveredProductivityStarIndex;
            }
            ratingElements.push(<span key={`b${(filledStarCount + j).toString()}`}
                tid={timeNormId.toString()}
                wid={currentRating.toPrecision(3)}
                zid="0"
                xid={(filledStarCount + j).toString()}
                onclick={currentClient.editProductivityRatingClickHandler}
                onmouseenter={currentClient.enterProductivityRatingHandler}
                onmouseleave={currentClient.exitProductivityRatingHandler}
                styles={{ "color": isHoveredStar ? "rgb(42, 178, 191)" : "rgb(14, 30, 86)",  "-webkit-user-select": "none" }}
                innerHTML={!isHoveredStar ? "&#9734;" : "&#9733;"}></span> as VNode);
        }
        return ratingElements;
    }

    public mouseUpGlobalTouchHandler = (evt: TouchEvent) => {
        if (currentTimeline.viewModel.isTimelinePanning || currentTimeline.viewModel.isTimelineInteracting) {
            currentTimeline.endTimelineInteraction();
        }
        if (currentClient.viewModel.isDragging) {
            currentClient.endDrag();
        }
    };

    public mouseUpGlobalPointerHandler = (evt: MouseEvent) => {
        if (currentClient.viewModel.isDragging) {
            currentClient.endDrag();
        }
    };

    public logoutClickHandler = (evt: MouseEvent) => {
        currentApp.controller.LogoutAction().done((response: any) => {
            window.location.assign(window.location.origin + "/tokyo/"); // TODO hardcoded link
        });
    }

    public toggleSmallDeviceView = (evt: MouseEvent): void => {
        if (currentClient.viewModel.isToggledTimeline === false) {
            currentClient.viewModel.isExplicitlyToggledTimeline = true;
        }
        currentClient.viewModel.isToggledTimeline = !currentClient.viewModel.isToggledTimeline;
        currentClient.viewModel.isToggledTableOfContents = !currentClient.viewModel.isToggledTableOfContents;
        currentClient.viewModel.isToggledDetailedTimeNorm = !currentClient.viewModel.isToggledDetailedTimeNorm;
        currentClient.viewModel.isToggledSelectToc = !currentClient.viewModel.isToggledSelectToc;
    };

    public createTimeNormClickHandler = (event: MouseEvent): void => {
        let workUnitId: number = parseIntFromAttribute(event.target, "wid");
        let localTime = ""; // TODO code duplication
        if (currentClient.viewModel.isSendLocalTimezone) {
            localTime = new Date(Date.now()).toString(); // TODO cannot use new Date because it's forced to local time zone => probably not a problem since timestamp is only used for timezone auto-recognition, stamp is created server side
        }
        currentApp.controller.CreateTimeNormJson(workUnitId, localTime).done(data => currentClient.UpdateData(data));
    };

    public workUnitDragClickHandler = (event: MouseEvent): void => {
        let targetElement = event.target as HTMLElement;
        // TODO at drag start, scroll to position such that when timenorms are temporary hidden, the dragged element is in between its -1/+1 neighbors => immediately releasing button results in no change
        if (event.buttons != 1) {
            return;
        }
        // TODO doesnt work on ipad
        let workUnitId: number = parseIntFromAttribute(targetElement, "wid");
        currentClient.viewModel.isDragging = true;
		currentClient.viewModel.draggedElementPositionStartX = event.clientX;
		currentClient.viewModel.draggedElementPositionStartY = event.clientY;
        currentClient.dragUpdate(event.clientX, event.clientY);
        currentClient.viewModel.draggedWorkUnitId = workUnitId;

        // 4th parent is scrollable
        if (currentClient.timeTableScrollDivElement !== undefined) {
            currentClient.timeTableScrollDivElement.scrollTop = 0;
        }
        
        // find next work unit to prevent drag operation that has no effect
        let followUpId: number | undefined = undefined;
        let targetWorkUnitIndex: number = -1;
        if (currentApp.clientData.TimeTables !== undefined) {
            let timeTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                let currentWorkUnitIndex: number = -1;
                if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                    currentWorkUnitIndex = t.TimeTableAndWorkUnits.WorkUnits.findIndex(w => w.WorkUnitId == workUnitId);
                }
                if (currentWorkUnitIndex != -1) {
                    targetWorkUnitIndex = currentWorkUnitIndex;
                    return true;
                }
                return false;
            });
            if (timeTable !== undefined && timeTable.TimeTableAndWorkUnits !== undefined && timeTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                if (targetWorkUnitIndex < (timeTable.TimeTableAndWorkUnits.WorkUnits.length - 1)) {
                    followUpId = timeTable.TimeTableAndWorkUnits.WorkUnits[targetWorkUnitIndex + 1].WorkUnitId;
                }
            }
        }
        currentClient.viewModel.draggedFollowUpWorkUnitId = followUpId !== undefined ? followUpId : 0;
    };

    public mouseGlobalMoveHandler = (evt: MouseEvent) => {
        if (currentClient.viewModel.isDragging) {
            currentClient.dragUpdate(evt.clientX, evt.clientY);
            
        }
    };
	
	public dragUpdate = (newClientX: number, newClientY: number) => {
		currentClient.viewModel.draggedElementPositionX = newClientX - currentClient.viewModel.draggedElementPositionStartX + 15; // display element next to mouse cursor
		currentClient.viewModel.draggedElementPositionY = newClientY - currentClient.viewModel.draggedElementPositionStartY + 15;
	};

    public workUnitMouseEnterHandler = (event: MouseEvent): void => {
        if (currentClient.viewModel.isDragging) {
            let workUnitId: number = parseIntFromAttribute(event.target, "wid");
            if (currentClient.viewModel.draggedWorkUnitId != workUnitId
                && currentClient.viewModel.draggedFollowUpWorkUnitId != workUnitId) {
                currentClient.viewModel.hoveredWorkUnitIdWhileDrag = workUnitId;
            }
        }
    };

    public workUnitMouseLeaveHandler = (event: MouseEvent): void => {
        currentClient.viewModel.hoveredWorkUnitIdWhileDrag = 0;
    };

    public workUnitMouseUpHandler = (event: MouseEvent): void => {
        if (currentClient.viewModel.draggedWorkUnitId != 0 && currentClient.viewModel.hoveredWorkUnitIdWhileDrag != 0) {
            currentApp.controller.MoveWorkUnitBeforeWorkUnitJson(currentClient.viewModel.draggedWorkUnitId, currentClient.viewModel.hoveredWorkUnitIdWhileDrag)
                .done((data: any) => currentClient.UpdateData(data));
        }
        currentClient.endDrag();
    };

    public endDrag = (): void => {
        currentClient.viewModel.isDragging = false;
        currentClient.viewModel.draggedWorkUnitId = 0;
        currentClient.viewModel.draggedFollowUpWorkUnitId = 0;
        currentClient.viewModel.hoveredWorkUnitIdWhileDrag = 0;
    };

    public createWorkUnitTableMapping = (): maquette.Mapping<WorkUnit, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<WorkUnit, any>(
            function getSectionSourceKey(source: WorkUnit) {
                // function that returns a key to uniquely identify each item in the data
                return source.WorkUnitId;
            },
            function createSectionTarget(source: WorkUnit) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let timeNorms: TimeNorm[] = [];
                if (source.TimeNorms !== undefined) {
                    timeNorms = source.TimeNorms;
                }
                let timeNormMapping = currentClient.createTimeNormTableMapping();
                timeNormMapping.map(timeNorms);
                let isAtLeastOneTimeNorm: boolean = timeNorms.length > 0;
                let isActiveWorkUnit: boolean = source.WorkUnitId == currentApp.clientData.TargetWorkUnitId;
                let sourceId: number = source.WorkUnitId;
                let sourceIdString: string = source.WorkUnitId.toString();
                let sourceNameString: string = source.Name;
                let durationString: string = source.DurationString;
                return {
                    renderMaquette: function () {
                        let isUnboundTimeStamps: boolean = false; // TODO convert to UI state, used in multiple spots
                        if (currentApp.clientData.UnboundTimeStamps !== undefined && currentApp.clientData.UnboundTimeStamps.length > 0) {
                            isUnboundTimeStamps = true;
                        }
                        let isEditedWorkUnit: boolean = sourceId == currentClient.viewModel.tempEditedWorkUnitId;
                        isActiveWorkUnit = sourceId == currentApp.clientData.TargetWorkUnitId;
                        let isHoveredWorkUnit: boolean = sourceId == currentClient.viewModel.hoveredWorkUnitIdWhileDrag;
                        let isDraggedWorkUnit: boolean = sourceId == currentClient.viewModel.draggedWorkUnitId;
                        let workUnitHeadingString: string = `${sourceNameString}`;
                        if (durationString !== "< 1s") { // TODO compare to constant default string used in backend
                            workUnitHeadingString = `${sourceNameString} (${durationString})`
                        }
                        // TODO doesnt work on freshly added workunits because object references are different
                        // TODO NO TABLE
                        return <div key={sourceIdString} wid={sourceIdString} styles={{"flex": "0 0 auto", "position": isDraggedWorkUnit ? "absolute" : "relative", "left": isDraggedWorkUnit ? `${currentClient.viewModel.draggedElementPositionX}px`: undefined, "top": isDraggedWorkUnit ? `${currentClient.viewModel.draggedElementPositionY}px` : undefined}} onmouseenter={currentClient.workUnitMouseEnterHandler} onmouseleave={currentClient.workUnitMouseLeaveHandler} onmouseup={currentClient.workUnitMouseUpHandler}>
                            { isHoveredWorkUnit ? <div key="-13" styles={{"width": "20px", "height": "2px", "background-color": "black"}}></div> : undefined}
                            <hr key="-11"></hr>
                            {isEditedWorkUnit ?
                                <div key="10">
                                    <input
                                        value={currentClient.viewModel.tempWorkUnitName}
                                        oninput={currentClient.workUnitNameChangedHandler}
                                        onblur={currentClient.workUnitNameLostFocusHandler}
                                        onkeydown={currentClient.workUnitNameKeyDownHandler}
                                        afterCreate={currentClient.inputNameAfterCreateHandler}>
                                    </input>
                                </div>
                                : <div key="0" styles={{"display": "flex", "flex-flow": "row nowrap"}}>
                                    <h4 key="h1" tid={sourceIdString}
                                        onclick={currentClient.workUnitNameClickedHandler} 
                                        styles={{ "flex": "0 0 auto", "padding-left": "12px" }}><a class="anchor" name={`a_${currentTableOfContents.viewModel.createSectionNumberingWorkUnit(currentApp.clientData.SelectedTimeTableId, sourceId)}`} tabindex="-1"></a>{isActiveWorkUnit ? "\u2022" : ""}{workUnitHeadingString}</h4>
                                    { /* TODO isAtLeastOneTimeNorm ? <button key="c" type="button" class="btn btn-sm" wid={source.WorkUnitId.toString()} onclick={currentClient.focusWorkUnitClickHandler}>&#128269;</button> : undefined */ }
                                    {(!isActiveWorkUnit) ? <button key="a" type="button" class="btn btn-sm" wid={sourceIdString} onclick={currentClient.activateWorkUnitClickHandler} styles={{ "margin-left": "20px", "width": "35px"}}>&#10004;</button> : undefined }
                                    {!isUnboundTimeStamps ? <button key="b" type="button" class="btn btn-sm" wid={sourceIdString} onmousedown={currentClient.createTimeNormClickHandler} styles={{ "margin-left": "20px", "width": "35px" }}>+</button> : <button disabled key="b0" type="button" class="btn btn-sm" wid={sourceIdString} onmousedown={currentClient.createTimeNormClickHandler} styles={{ "margin-left": "20px", "width": "35px", "font-size": "1.2", "font-weight": "bold" }}>+</button>}
                                    <button key="c" type="button" class="btn btn-sm" wid={sourceIdString} onmousedown={currentClient.workUnitDragClickHandler} styles={{ "margin-left": "20px", "width": "35px", "cursor": "move" }}>&equiv;</button>
                                    {(!isAtLeastOneTimeNorm && !isActiveWorkUnit) ? <button key="d" type="button" class="btn btn-sm" wid={sourceIdString} onclick={currentClient.removeWorkUnitClickHandler} styles={{ "margin-left": "20px", "width": "35px" }}>&#10008;</button> : undefined /*TODO instead of showing activate button always, on delete active change active workunit to previous and show delete button even when active*/}
                                </div>/*TODO improve click area => only name, not col?*/}
                            <hr key="-12" styles={{"margin-bottom": "0"}}></hr>
                            {currentClient.viewModel.isDragging ? undefined : timeNormMapping.results.map(comp => comp.renderMaquette())}
                            {!isUnboundTimeStamps ? <button key="a" type="button" class="btn btn-sm" wid={sourceIdString} onmousedown={currentClient.createTimeNormClickHandler} styles={{ "margin-left": "20px", "width": "35px" }}>+</button> : <button disabled key="a0" type="button" class="btn btn-sm" wid={sourceIdString} onmousedown={currentClient.createTimeNormClickHandler} styles={{ "margin-left": "20px", "width": "35px", "font-size": "1.2", "font-weight": "bold" }}>+</button>}
                        </div>;
                    },
                    update: function (updatedSource: WorkUnit) {
                        if (updatedSource.TimeNorms !== undefined) {
                            timeNorms = updatedSource.TimeNorms;
                        }
                        else {
                            timeNorms = [];
                        }
                        timeNormMapping.map(timeNorms);
                        isAtLeastOneTimeNorm = timeNorms.length > 0;
                        isActiveWorkUnit = updatedSource.WorkUnitId == currentApp.clientData.TargetWorkUnitId;
                        sourceId = updatedSource.WorkUnitId;
                        sourceIdString = updatedSource.WorkUnitId.toString();
                        sourceNameString = updatedSource.Name;
                        durationString = updatedSource.DurationString;
                    }
                };
            },
            function updateSectionTarget(updatedSource: WorkUnit, target: { renderMaquette(): any, update(updatedSource: WorkUnit): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public createTimeNormTableMapping = (): maquette.Mapping<TimeNorm, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<TimeNorm, any>(
            function getSectionSourceKey(source: TimeNorm) {
                // function that returns a key to uniquely identify each item in the data
                return source.TimeNormId;
            },
            function createSectionTarget(source: TimeNorm) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                // TODO create placeholder timenorm => unscheduled
                let startTime: Date;
                let endTime: Date;
                let dateStringStart: string = "";
                let dateStringEnd: string = "";
                let timeStringStart: string = "";
                let timeStringEnd: string = "";
                if (source.StartTime !== undefined) {
                    if (typeof source.StartTime.TrackedTimeForView as string === "Date") { // TODO should be converted when initializing client with data the first time
                        startTime = source.StartTime.TrackedTimeForView as Date;
                    }
                    else {
                        startTime = new Date(source.StartTime.TrackedTimeForView as string); // TODO cannot use new Date because it's forced to local time zone
                    }
                    dateStringStart = source.StartTime.DateString;
                    timeStringStart = source.StartTime.TimeString;
                }
                if (source.EndTime !== undefined) {
                    if (typeof source.EndTime.TrackedTimeForView as string === "Date") {
                        endTime = source.EndTime.TrackedTimeForView as Date;
                    }
                    else {
                        endTime = new Date(source.EndTime.TrackedTimeForView as string); // TODO cannot use new Date because it's forced to local time zone
                    }
                    dateStringEnd = source.EndTime.DateString;
                    timeStringEnd = source.EndTime.TimeString;
                }
                let timeNormIdString = source.TimeNormId.toString();
                let workUnitIdString = source.WorkUnitId.toString();
                let productivityRating: ProductivityRating | undefined = source.ProductivityRating;
                let productivityPercentage: number = 0;
                let durationString = source.DurationString;
                let name = source.Name;
                let colorString: string = `rgb(${source.ColorR},${source.ColorG},${source.ColorB})`;
                return {
                    renderMaquette: function () {
                        let timeColumnStyles = {"flex": "1 1 auto", "display": "flex", "flex-flow": "row wrap"};
                        let columnContentTimeInformation: VNode | undefined = undefined;
                        if (source.StartTime === undefined || source.EndTime === undefined) {
                            return undefined;
                        }
                        else {
                            columnContentTimeInformation = <div styles={timeColumnStyles} key="4">
                                <p key="0" styles={{"flex": "0 0 auto", "margin-right": ".5rem", "margin-bottom": "0"}} tid={source.StartTime.TimeStampId.toString()} wid={timeNormIdString} zid="a" onclick={currentClient.editTimeStampClickHandler}>
                                    {dateStringStart} {timeStringStart}
                                </p> - <p key="1" styles={{ "flex": "0 0 auto", "margin-left": ".5rem", "margin-bottom": "0"}} tid={source.EndTime.TimeStampId.toString()} wid={timeNormIdString} zid="b" onclick={currentClient.editTimeStampClickHandler}>
                                    {dateStringStart !== dateStringEnd ? `${dateStringEnd} ${timeStringEnd}` : timeStringEnd}
                                </p>
                            </div> as VNode;
                        }
                        let inputOrNameColumnStyles = {"flex": "0 0 30%", "max-width": "30%", "padding-left": "12px"};
                        let colorSelectColumnStyles = {"flex": "0 0 20px", "max-width": "20px"};
                        let durationColumnStyles = { "flex": "0 0 100px", "width": "100px", "padding-left": "5px", "padding-right": "5px"};
                        let ratingColumnStyles = { "flex": "0 0 60px", "width": "60px"};
                        let manipulationColumnStyles = {"flex": "0 0 90px", "width": "90px", "margin-right": "20px"};
                        let isEditedTimeNorm = source.TimeNormId == currentClient.viewModel.tempEditedTimeNormId;
                        if (productivityRating != undefined) { // TODO why can productivity rating be null? after process timestamps
                            productivityPercentage = productivityRating.ProductivityPercentage;
                        }
                        else {
                            productivityPercentage = 0;
                        }
                        // TODO row animation when unbinding/removing timenorm
                        return <div key={source.TimeNormId} styles={{"display": "flex", "flex-flow": "row nowrap", "width": "100%", "border-bottom": "1px solid rgba(14, 30, 86, 0.2)", "padding-top": "5px"}}>
                            {
                                isEditedTimeNorm ? <div key="10" styles={inputOrNameColumnStyles}>
                                    <input
                                        value={currentClient.viewModel.tempTimeNormName}
                                        oninput={currentClient.timeNormNameChangedHandler}
                                        onblur={currentClient.timeNormNameLostFocusHandler}
                                        onkeydown={currentClient.timeNormNameKeyDownHandler}
                                        afterCreate={currentClient.inputNameAfterCreateHandler}>
                                    </input>
                                </div>
                                    : <div tid={timeNormIdString} wid={workUnitIdString} key="0"  styles={inputOrNameColumnStyles}><div>
                                        <p tid={timeNormIdString} wid={workUnitIdString} onclick={currentClient.timeNormNameClickedHandler}><a class="anchor" name={`a_${currentTableOfContents.viewModel.createSectionNumberingTimeNorm(currentApp.clientData.SelectedTimeTableId, source.WorkUnitId, source.TimeNormId)}`} tabindex="-1"></a>{name}</p>
                                    </div></div>
                            }
                            {currentClient.viewModel.isToggledDetailedTimeNorm ? <div key="1" styles={colorSelectColumnStyles}>{currentClient.renderColorPickerSelector(source.TimeNormId, colorString)}</div> : undefined}
                            <div key="2" styles={durationColumnStyles}>{durationString}</div>
                            <div key="3" styles={ratingColumnStyles}>{currentClient.renderProductivityRating(source.TimeNormId, productivityPercentage)}</div>
                            {currentClient.viewModel.isToggledDetailedTimeNorm ? <div key="4" styles={manipulationColumnStyles}>
                                <button key="a" type="button" class="btn btn-sm" tid={timeNormIdString} onclick={currentClient.focusTimeNormClickHandler} styles={{ "height": "35px", "width": "35px"}}>&#128269;</button>
                                <button key="b" type="button" class="btn btn-sm" tid={timeNormIdString} onclick={currentClient.unbindTimeNormClickHandler} styles={{ "height": "35px", "margin-left": "20px", "width": "35px"}}>&#8630;</button>
                                {/*<button key="c" type="button" class="btn btn-sm" tid={timeNormIdString} onclick={currentClient.removeTimeNormClickHandler}>&#10008;</button>*/}
                            </div> : undefined }
                            {columnContentTimeInformation}
                        </div>;
                    },
                    update: function (updatedSource: TimeNorm) {
                        durationString = updatedSource.DurationString;
                        if (updatedSource.StartTime) {
                            dateStringStart = updatedSource.StartTime.DateString;
                            timeStringStart = updatedSource.StartTime.TimeString;
                        }
                        if (updatedSource.EndTime) {
                            dateStringEnd = updatedSource.EndTime.DateString;
                            timeStringEnd = updatedSource.EndTime.TimeString;
                        }
                        productivityRating = updatedSource.ProductivityRating;
                        if (!(productivityRating !== undefined && updatedSource.ProductivityRating != undefined)) {
                            productivityRating = undefined; // TODO audit
                        }
                        name = updatedSource.Name;
                        colorString = `rgb(${updatedSource.ColorR},${updatedSource.ColorG},${updatedSource.ColorB})`;
                    }
                };
            },
            function updateSectionTarget(updatedSource: TimeNorm, target: { renderMaquette(): any, update(updatedSource: TimeNorm): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public focusTimeNormClickHandler = (event: MouseEvent) => {
        let timeNormId = parseIntFromAttribute(event.target, "tid");
        let timeRange: TimelineTimeSpan | undefined = currentTimeline.viewModel.timeNormTimeSpans.find(el => el.timeNormId == timeNormId);
        if (timeRange !== undefined && currentTimeline.viewModel.timelineDateCenter !== undefined) {
            let centerDateNewOffset: number = currentTimeline.viewModel.timelineDateCenter.getTime(); // TODO check UTC
            let startDateRightToCenter: boolean = timeRange.startTimeNumber > centerDateNewOffset;
            let isBiasFound: boolean = false;
            let maxDaysDifference: number = 1000; // TODO
            let newOffset: number = 0;
            while (!isBiasFound) {
                centerDateNewOffset = currentTimeline.viewModel.addFullDaysSameTime(new Date(centerDateNewOffset), startDateRightToCenter ? +1 : -1).getTime();
                if (startDateRightToCenter) {
                    if (timeRange.startTimeNumber < centerDateNewOffset) {
                        isBiasFound = true;
                        break;
                    }
                    else {
                        newOffset = newOffset + 1;
                    }
                }
                else if (!startDateRightToCenter) {
                    newOffset = newOffset - 1; // offset must be updated at least once when scrolling left
                    if (timeRange.startTimeNumber > centerDateNewOffset) {
                        isBiasFound = true;
                        break;
                    }
                }
                if (Math.abs(newOffset) > maxDaysDifference) {
                    break; // TODO hardcoded limit
                }
            }
            currentTimeline.viewModel.timelineCenterBias = currentTimeline.viewModel.timelineCenterBias + newOffset;
            

            if (newOffset == 0) {
                // adjust zoom level when time norm is already focused horizontally
                let startZoomLevel: number = currentTimeline.viewModel.timelineZoomLevel;
                let newZoomOffset: number = 0;
                let endTimeNumberOfTimeline: number = currentTimeline.viewModel.addFullDaysSameTime(currentTimeline.viewModel.timelineDateCenter, currentTimeline.viewModel.elementCountAwayFromCenter).getTime();
                let isEndTimeInTimeline: boolean = timeRange.endTimeNumber < endTimeNumberOfTimeline;
                if (!isEndTimeInTimeline) { // TODO this can be zoom out to max zoom if long event; and zoom in to zoom such that end time is in center of right half
                    console.log("end not in timeline TODO");
                    //for (let i: number = startZoomLevel; )
                }
                currentTimeline.viewModel.timelineZoomLevel = startZoomLevel + newZoomOffset;
            }
            currentTimeline.viewModel.timelineElements = currentTimeline.createTimelineElements();
            currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
        }
    }

    public focusWorkUnitClickHandler = (event: MouseEvent) => {
        event.preventDefault();
        // TODO
    }

    public renderColorPickerSelector = (timeNormId: number, colorString: string) => {
        // TODO make color picker ipad compatible, https://www.npmjs.com/package/md-color-picker
        let currentColorValue = colorString;
        return <div 
            tid={`${timeNormId}`} 
            afterCreate={currentClient.divWithColorpickerAfterCreate}
            styles={{"width": "20px", "height": "20px", "background-color": colorString}}>
        </div>;
        // TODO reuse color picker
    };

    public divWithColorpickerAfterCreate = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]): void => {
        let timeNormId = parseIntFromAttribute(element, "tid");
        let isSubmittedValue: boolean = false;
        ($(element) as any).ColorPicker({
            onSubmit: function (hsb: any, hex: string, rgb: {r: number, g: number, b: number}, el: Element) {
                currentTimeline.stopMarkerEditAnimation(timeNormId);
                currentApp.controller.EditTimeNormJson(timeNormId, "", rgb.r, rgb.g, rgb.b).done((data: any): void => {
                    currentApp.client.UpdateData(data);
                });
                isSubmittedValue = true;
                ($(el) as any).ColorPickerHide();
            },
            onChange: function (hsb: any, hex: string, rgb: any) {
                // do nothing
            },
            onHide: function (colorPicker: any) {
                if (!isSubmittedValue) {
                    // reset change
                    // do nothing
                    currentTimeline.stopMarkerEditAnimation(timeNormId);
                }
            },
            onBeforeShow: function () {
                isSubmittedValue = false;
                let timeSpan: TimelineTimeSpan | undefined = currentTimeline.viewModel.timeNormTimeSpans.find(w => w.timeNormId == timeNormId);
                if (timeSpan === undefined) {
                    console.log("animation element undefined"); // TODO
                    return;
                }
                let currentR: number = timeSpan.colorR;
                let currentG: number = timeSpan.colorG;
                let currentB: number = timeSpan.colorB;
                currentTimeline.startMarkerEditAnimation(timeNormId);
                // TODO start edit mode and block timeline interactions
                ($(this) as any).ColorPickerSetColor({r: currentR, g: currentG, b: currentB});
            },
        });
        //$('.colorpickerHolder').css({ 'color': $('.colorpickerHolder').val() });
    };

    /*public renderMaquetteOldTODO = (): VNode => {
        let buttonGroupStyles = { "width": "auto", "flex": "0 0 auto" }
        let isRemoveTableEnabled = false;
        if (currentApp.clientData.TimeTables) {
            isRemoveTableEnabled = currentApp.clientData.TimeTables.length > 1;
        }
        let selectedTableIdString = currentApp.clientData.SelectedTimeTableId.toString();
        return <div key="client" styles={{"position" : "relative"}}>
            <div key="-5" styles={{"position": "absolute", "left": "50%", "top": "0px", "background-color": "black", "height": "10px", "width": "1px"}}></div>
            {currentClient.renderTimeline(0)}
            
            
            <div key="-1">
                <select key="b" onchange={currentClient.workUnitForMoveChangedHandler}>
                    {<option key="a" selected>Select work unit to move</option>}
                    {currentClient.viewModel.workUnitSelectionMapping.results.map(r => r.renderMaquette())}
                </select>
                <select key="c" onchange={currentClient.workUnitPositionChangedHandler}>
                    {<option key="a" selected>Move work unit before...</option>}
                    {currentClient.viewModel.workUnitSelectionMapping.results.map(r => r.renderMaquette())}
                </select>
            </div>
            <div key="1">
                {currentClient.viewModel.workUnitMapping.results.map(r => r.renderMaquette())}
            </div>
            {TODO (img).src = "data:image/png;base64," + byteArray;
                <div key="2">
                <img src="/Tracker/TimeTablePlot?timeTableId=8" width="500px" height="500px"></img>
            </div>}
            <ul key="4" class="list-group">
                { currentClient.viewModel.unboundTimeStampMapping.results.map(r => r.renderMaquette()) }
            </ul>
            <div key="5" styles={{"display": "flex", "flex-flow": "row wrap"}}>
				<button key="d" type="button" class="btn btn-sm" onclick={currentClient.exportToCSVClickHandler} styles={buttonGroupStyles}>Export to .csv</button>
				<button key="e" type="button" class="btn btn-sm" onclick={currentClient.lockTableClickHandler} styles={buttonGroupStyles}>Lock table</button>
				<button key="f" type="button" class="btn btn-sm" onclick={currentClient.toggleAgendaViewClickHandler} styles={buttonGroupStyles}>Toggle agenda view</button>
				<button key="g" type="button" class="btn btn-sm" onclick={currentClient.shareTableClickHandler} styles={buttonGroupStyles}>Share</button>
                <button key="h" type="button" class="btn btn-sm" onclick={currentClient.toggleReorderElementsClickHandler} styles={buttonGroupStyles}>Toggle reorder</button>
                <button key="i" type="button" class="btn btn-sm" onclick={currentClient.newTableClickHandler} styles={buttonGroupStyles}>New table</button>
                {isRemoveTableEnabled ? <button key="j1" type="button" class="btn btn-sm" onclick={currentClient.removeTableClickHandler} styles={buttonGroupStyles}>Delete table</button>
                    : <button key="j2" type="button" class="btn btn-sm" onclick={currentClient.removeTableClickHandler} styles={buttonGroupStyles} disabled>Delete table</button>}
            </div>
        </div> as VNode;
    };*/

    public createUnboundTimeStampMapping = (): maquette.Mapping<TimeStamp, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<TimeStamp, any>(
            function getSectionSourceKey(source: TimeStamp) {
                // function that returns a key to uniquely identify each item in the data
                return source.TimeStampId;
            },
            function createSectionTarget(source: TimeStamp) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let time: Date;
                let dateString: string = "";
                let timeString: string = "";
                if (typeof source.TrackedTimeForView as string === "Date") { // TODO should be converted when initializing client with data the first time
                    time = source.TrackedTimeForView as Date;
                }
                else {
                    time = new Date(source.TrackedTimeForView as string); // TODO cannot use new Date because it's forced to local time zone
                }
                dateString = source.DateString;
                timeString = source.TimeString;
                return {
                    renderMaquette: function () {
                        let timeInformation: VNode = <div styles={{"display": "flex", "flex-flow": "row nowrap"}} key="0">
                            <div styles={{ "flex": "0 0 auto", "padding-right": "5px", "padding-left": "5px"}} tid={source.TimeStampId.toString()} wid="0" zid="c" onclick={currentClient.editTimeStampClickHandler}>
                                {dateString} {timeString}
                            </div>
                            <button styles={{ "flex": "0 0 auto", "width": "35px" }} key="1" type="button" tid={`${source.TimeStampId}`} class="btn btn-sm btn-danger" onclick={currentClient.removeTimeStampButtonClickHandler}>
                            X
                            </button>
                        </div> as VNode;
                        return <div key={source.TimeStampId} styles={{
                            "flex": "0 0 auto",
                            "color": source.IsBound ? "blue" : "red"
                        }}>
                        {timeInformation}
                        </div>; // TODO calculate local time
                    },
                    update: function (updatedSource: TimeStamp) {
                        dateString = updatedSource.DateString;
                        timeString = updatedSource.TimeString;
                    }
                };
            },
            function updateSectionTarget(updatedSource: TimeStamp, target: { renderMaquette(): any, update(updatedSource: TimeStamp): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public unbindTimeNormClickHandler = (event: MouseEvent) => {
        let timeNormId = parseIntFromAttribute(event.target, "tid");
        currentApp.controller.UnbindTimeNormJson(timeNormId).done((response: any) => currentClient.UpdateData(response));
    };

    /* public removeTimeNormClickHandler = (event: MouseEvent) => { // TODO unused
        let timeNormId = parseIntFromAttribute(event.target, "tid");
        currentApp.controller.RemoveTimeNormJson(timeNormId).done((response: any) => currentClient.UpdateData(response));
    };*/

    public inputNameAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        (element as HTMLInputElement).focus();
    };

    public globalizationButtonTopBarAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        currentClient.topBarGlobalizationButtonElement = (element as HTMLElement); // TODO make sure its button, not embedded p div
    };

    public timeTableAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        currentClient.timeTableScrollDivElement = element as HTMLDivElement;
        let fixedWidthPartsOfUI: number = 290; // TODO should be calculated properly (depends on UI)
        currentClient.timeTableScrollPaddingBottom = `${document.body.clientHeight - fixedWidthPartsOfUI}px`; // TODO should update on window resize
    };

    public createTimeTableSelectionMapping = (): maquette.Mapping<TimeTableViewModel, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<TimeTableViewModel, any>(
            function getSectionSourceKey(source: TimeTableViewModel) {
                // function that returns a key to uniquely identify each item in the data
                if (source.TimeTableAndWorkUnits) {
                    return source.TimeTableAndWorkUnits.TimeTableId;
                }
                return 0; // TODO
            },
            function createSectionTarget(source: TimeTableViewModel) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let timeTableIdString = "0";
                if (source.TimeTableAndWorkUnits) {
                    timeTableIdString = source.TimeTableAndWorkUnits.TimeTableId.toString();
                }
                return {
                    renderMaquette: function () {
                        let tableName = "";
                        let isSelectedTable = false;
                        if (source.TimeTableAndWorkUnits) {
                            tableName = source.TimeTableAndWorkUnits.Name;
                            if (currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits) {
                                isSelectedTable = currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.TimeTableId == source.TimeTableAndWorkUnits.TimeTableId;
                            }
                        }
                        return isSelectedTable ? <option key={timeTableIdString} value={timeTableIdString} selected>{tableName}</option>
                            : <option key={timeTableIdString} value={timeTableIdString}>{tableName}</option>;
                    },
                    update: function (updatedSource: TimeTableViewModel) {
                        // do nothing
                    }
                };
            },
            function updateSectionTarget(updatedSource: TimeTableViewModel, target: { renderMaquette(): any, update(updatedSource: TimeTableViewModel): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    }
    
    public createTableWorkUnitSelectionMapping = (): maquette.Mapping<TimeTableViewModel, { renderMaquette: () => maquette.VNode[] }> => {
        return maquette.createMapping<TimeTableViewModel, any>(
            function getSectionSourceKey(source: TimeTableViewModel) {
                // function that returns a key to uniquely identify each item in the data
                if (source.TimeTableAndWorkUnits) {
                    return source.TimeTableAndWorkUnits.TimeTableId;
                }
                return 0; // TODO
            },
            function createSectionTarget(source: TimeTableViewModel) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let timeTableIdString = "0";
                let workUnits: WorkUnit[] = []
                if (source.TimeTableAndWorkUnits !== undefined) {
                    timeTableIdString = source.TimeTableAndWorkUnits.TimeTableId.toString();
                    if (source.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        workUnits = source.TimeTableAndWorkUnits.WorkUnits;
                    }
                }
                let workUnitSelectionMapping: maquette.Mapping<WorkUnit, { renderMaquette: () => maquette.VNode }> = currentClient.createWorkUnitSelectionMapping();
                workUnitSelectionMapping.map(workUnits);
                return {
                    renderMaquette: function () {
                        let tableAndWorkUnitOptions: VNode[] = [];
                        let tableName = "";
                        if (source.TimeTableAndWorkUnits) {
                            tableName = source.TimeTableAndWorkUnits.Name;
                        }
                        tableAndWorkUnitOptions.push(<option disabled key={`t${timeTableIdString}`} value={`t${timeTableIdString}`}>{tableName}</option> as VNode);
                        tableAndWorkUnitOptions.push(...workUnitSelectionMapping.results.map(r => r.renderMaquette()));
                        return tableAndWorkUnitOptions;
                    },
                    update: function (updatedSource: TimeTableViewModel) {
                        workUnits = [];
                        if (updatedSource.TimeTableAndWorkUnits !== undefined &&
                            updatedSource.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                                workUnits = updatedSource.TimeTableAndWorkUnits.WorkUnits;
                        }
                        workUnitSelectionMapping.map(workUnits);
                    }
                };
            },
            function updateSectionTarget(updatedSource: TimeTableViewModel, target: { renderMaquette(): any, update(updatedSource: TimeTableViewModel): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    }

    public createWorkUnitSelectionMapping = (): maquette.Mapping<WorkUnit, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<WorkUnit, any>(
            function getSectionSourceKey(source: WorkUnit) {
                // function that returns a key to uniquely identify each item in the data
                return source.WorkUnitId;
            },
            function createSectionTarget(source: WorkUnit) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let sourceIdString: string = source.WorkUnitId.toString();
                let sourceNameString: string = `...${source.Name}`;
                return {
                    renderMaquette: function () {
                        let isActiveWorkUnit: boolean = source.WorkUnitId == currentApp.clientData.TargetWorkUnitId;
                        return isActiveWorkUnit ? <option selected key={`w${sourceIdString}`} value={sourceIdString}>{sourceNameString}</option> : <option key={`w${sourceIdString}`} value={sourceIdString}>{sourceNameString}</option>;
                    },
                    update: function (updatedSource: WorkUnit) {
                        // TODO sub item duplicated?? => need to update values
                        sourceNameString = `...${updatedSource.Name}`;
                    }
                };
            },
            function updateSectionTarget(updatedSource: WorkUnit, target: { renderMaquette(): any, update(updatedSource: WorkUnit): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public removeWorkUnitClickHandler = (event: MouseEvent) => {
        let workUnitId = parseIntFromAttribute(event.target, "wid");
        currentApp.controller.RemoveWorkUnitJson(workUnitId).done((response: any) => currentClient.UpdateData(response));
    }

    public activateWorkUnitClickHandler = (event: MouseEvent) => {
        let workUnitId = parseIntFromAttribute(event.target, "wid");
        currentApp.controller.SetWorkUnitAsDefaultTargetJson(workUnitId).done((response: any) => currentClient.UpdateData(response));
    }

    public createTimeZoneMapping = (tzSetting: TimeZoneSetting): maquette.Mapping<TrackerTimeZone, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<TrackerTimeZone, any>(
            function getSectionSourceKey(source: TrackerTimeZone) {
                // function that returns a key to uniquely identify each item in the data
                return source.Key;
            },
            function createSectionTarget(source: TrackerTimeZone) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                return {
                    renderMaquette: function () {
                        let isSelectedTimeZone = false;
                        switch (tzSetting) {
                            case TimeZoneSetting.Device:
                                if (currentClient.viewModel.selectedTimeZoneDevice != undefined) {
                                    isSelectedTimeZone = currentClient.viewModel.selectedTimeZoneDevice == source.Key;
                                }
                                break;
                            case TimeZoneSetting.View:
                                if (currentClient.viewModel.selectedTimeZoneView != undefined) {
                                    isSelectedTimeZone = currentClient.viewModel.selectedTimeZoneView == source.Key;
                                }
                                break;
                            case TimeZoneSetting.TimeStamp:
                                if (currentClient.viewModel.selectedTimeZoneTimeStamp != undefined) {
                                    isSelectedTimeZone = currentClient.viewModel.selectedTimeZoneTimeStamp == source.Key;
                                }
                                break;
                        }
                        return isSelectedTimeZone ? <option key={source.Key} value={source.Key.toString()} selected>{source.DisplayName}</option>
                            : <option key={source.Key} value={source.Key.toString()}>{source.DisplayName}</option>;
                    },
                    update: function (updatedSource: TrackerTimeZone) {
                        // do nothing
                    }
                };
            },
            function updateSectionTarget(updatedSource: TrackerTimeZone, target: { renderMaquette(): any, update(updatedSource: TrackerTimeZone): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public createTimeStampClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        let localTime = "";
        if (currentClient.viewModel.isSendLocalTimezone) {
            localTime = new Date(Date.now()).toString(); // TODO cannot use new Date because it's forced to local time zone => probably not a problem since timestamp is only used for timezone auto-recognition, stamp is created server side
        }
        currentApp.controller.CreateTimeStampJson(localTime).done((data: any) => { TokyoTrackerApp.TrackerAppInstance.client.UpdateData(data); })
            .fail((jqXhr, err) => {
                if (jqXhr.status == 0) {
                    currentClient.createOfflineTimeStamp(new Date(Date.now()).getTime().toString()); // TODO cannot use new Date because it's forced to local time zone
                    currentApp.projector.scheduleRender();
                }
            });
    };

    /*public createTimeStampOfflineClickHandler = (evt: MouseEvent) => { // TODO unused
        evt.preventDefault();
        // TODO what if user relogs? eg. delete on logout; display in user area that offline stamps exist
        // TODO make sure user is offline
        let localTime: string = new Date(Date.now()).getTime().toString(); // TODO cannot use new Date because it's forced to local time zone
        currentClient.createOfflineTimeStamp(localTime);
    };*/

    public createOfflineTimeStamp = (timeNumberString: string) => { // TODO document
        // TODO doesnt work on ipad
        let savedTimeStampCountString = window.localStorage.getItem(OFFLINE_N);
        let localTimeStampCount = 0;
        if (savedTimeStampCountString) {
            // TODO try catch
            localTimeStampCount = parseInt(savedTimeStampCountString);
        }
        window.localStorage.setItem("T" + localTimeStampCount.toString(), timeNumberString);
        window.localStorage.setItem(OFFLINE_N, (localTimeStampCount + 1).toString());
        currentClient.viewModel.offlineTimeStampCount = parseInt(window.localStorage.getItem(OFFLINE_N) as string); // TODO try catch
    }

    public sendTimeStampsOfflineClickHandler = (evt: MouseEvent) => {
        // TODO store time stamps with store id for multi store usage
        // TODO what if format/API changes while values are stored on device?
        let localTimeStampCountString = window.localStorage.getItem(OFFLINE_N);
        if (localTimeStampCountString) {
            let offlineStampCount = parseInt(localTimeStampCountString);
            for (let i = 0; i < offlineStampCount; i++) {
                let storageString = `T${i}`;
                let localTimeNumberString: string | null = window.localStorage.getItem(storageString);
                let storedDate: Date | undefined = undefined;
                if (localTimeNumberString !== null) {
                    storedDate = new Date(parseInt(localTimeNumberString));
                }
                if (storedDate !== undefined) {
                    currentApp.controller.CreateTimeStampManuallyPostJson(currentClient.convertHtmlInputToTimeStampVM(currentClient.convertTimeStampForHtmlInput(storedDate))) // TODO time->seconds info gets lost
                        .done((data: any) => {
                            currentClient.UpdateData(data);
                        })
                        .fail((jqXhr, err) => {
                            if (jqXhr.status == 0) { // TODO assume status: offline // TODO problems when only some stamps fail
                                if (storedDate !== undefined) {
                                    currentClient.createOfflineTimeStamp(storedDate.getTime().toString()); // TODO audit
                                    currentApp.projector.scheduleRender();
                                }
                            }
                        });
                }
                window.localStorage.removeItem(storageString);
            }
            window.localStorage.removeItem(OFFLINE_N); // TODO assume: not async actions to store
        }
        currentClient.viewModel.offlineTimeStampCount = parseInt(window.localStorage.getItem(OFFLINE_N) as string); // TODO try catch
    };
	
	public exportToCSVClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits) {
            window.location.assign(`/tokyo/ExportTimeTableToCSV?timeTableId=${currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.TimeTableId}`); // TODO dangerous when api changes => export URLs as well?
        }
    };
	
	public toggleAgendaViewClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        // TODO
    };
	
	public toggleReorderElementsClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        // TODO
    };

    public toggleEnableSensorTimelime = (evt: MouseEvent) => {
        currentTimeline.viewModel.isSensorEnabled = !currentTimeline.viewModel.isSensorEnabled;
        if (currentTimeline.viewModel.isSensorEnabled) {
            currentTimeline.viewModel.isSensorCalibrated = false;
            currentClient.activeSensorHandler = currentClient.processAccelerometer;
            console.log("registering motion sensor reader");
            window.addEventListener("devicemotion", currentClient.activeSensorHandler);
        }
        else {
            console.log("stopping event...");
            if (currentClient.activeSensorHandler !== undefined) {
                window.removeEventListener("devicemotion", currentClient.activeSensorHandler as EventListenerObject)
            }
            currentTimeline.viewModel.isSensorCalibrated = false;
            currentTimeline.viewModel.sensorDataCounter = 0;
            currentTimeline.viewModel.sensorDataCounterLastBiasChange = 0;
            currentTimeline.viewModel.sensorDataCounterLastZoomChange = 0;
        }
    };

    public processAccelerometer = (evt: DeviceMotionEvent) => {
        let UPDATE_RATE = 1;
        // TODO erroneously (?) assuming constant time interval for sensor polling
        let status: string | undefined = undefined;
        if (evt.accelerationIncludingGravity === null) {
            status = "no accelerometer available";
            window.removeEventListener("devicemotion", currentClient.activeSensorHandler as EventListenerObject)
            return;
        }
        else {
            if (!currentTimeline.viewModel.isSensorCalibrated) {
                status = "calibrating sensor...";
                if (evt.accelerationIncludingGravity.x !== null && evt.accelerationIncludingGravity.y !== null) {
                    currentTimeline.viewModel.sensorXStart = evt.accelerationIncludingGravity.x;
                    currentTimeline.viewModel.sensorYStart = evt.accelerationIncludingGravity.y;
                    if (evt.accelerationIncludingGravity.z !== null) {
                        currentTimeline.viewModel.sensorZStart = evt.accelerationIncludingGravity.z;
                    }
                    currentTimeline.viewModel.isSensorCalibrated = true;
                }
                else {
                    status = "calibration failed";
                }
            }
            else {
                status = "reading sensor data...";
                if (evt.accelerationIncludingGravity.x !== null && evt.accelerationIncludingGravity.y !== null) {
                    let DEADZONE: number = 0.3;
                    currentTimeline.viewModel.sensorDataCounter = currentTimeline.viewModel.sensorDataCounter + 1;
                    currentTimeline.viewModel.sensorXPrevious = currentTimeline.viewModel.sensorXCurrent;
                    currentTimeline.viewModel.sensorXCurrent = evt.accelerationIncludingGravity.x - currentTimeline.viewModel.sensorXStart;
                    currentTimeline.viewModel.sensorXCurrent = currentTimeline.viewModel.sensorXCurrent > DEADZONE ? currentTimeline.viewModel.sensorXCurrent - DEADZONE : currentTimeline.viewModel.sensorXCurrent < -DEADZONE ? currentTimeline.viewModel.sensorXCurrent + DEADZONE : currentTimeline.viewModel.sensorXCurrent;
                    currentTimeline.viewModel.sensorYPrevious = currentTimeline.viewModel.sensorYCurrent;
                    currentTimeline.viewModel.sensorYCurrent = evt.accelerationIncludingGravity.y - currentTimeline.viewModel.sensorYStart;
                    currentTimeline.viewModel.sensorYCurrent = currentTimeline.viewModel.sensorYCurrent > DEADZONE ? currentTimeline.viewModel.sensorYCurrent - DEADZONE : currentTimeline.viewModel.sensorYCurrent < -DEADZONE ? currentTimeline.viewModel.sensorYCurrent + DEADZONE : currentTimeline.viewModel.sensorYCurrent;
                    if (evt.accelerationIncludingGravity.z !== null) {
                        currentTimeline.viewModel.sensorZPrevious = currentTimeline.viewModel.sensorZCurrent;
                        currentTimeline.viewModel.sensorZCurrent = evt.accelerationIncludingGravity.z - currentTimeline.viewModel.sensorZStart;
                    }
                    if (currentTimeline.viewModel.sensorDataCounter % UPDATE_RATE == 0) {
                        let isScheduleUpdate: boolean = false;
                        let biasDiff: number = Math.trunc(currentTimeline.viewModel.sensorYCurrent); // TODO deadzone
                        if (biasDiff > 0.5 || biasDiff < -0.5) {
                            if ((currentTimeline.viewModel.sensorDataCounter - currentTimeline.viewModel.sensorDataCounterLastBiasChange) < 20) {
                                biasDiff = 0;
                            }
                            else {
                                currentTimeline.viewModel.sensorDataCounterLastBiasChange = currentTimeline.viewModel.sensorDataCounter;
                                isScheduleUpdate = true;
                            }
                        }
                        currentTimeline.viewModel.timelineCenterBias += biasDiff;
                        let zoomDiff: number = Math.trunc(currentTimeline.viewModel.sensorXCurrent);
                        if (zoomDiff > 0.5 || zoomDiff < -0.5) {
                            if ((currentTimeline.viewModel.sensorDataCounter - currentTimeline.viewModel.sensorDataCounterLastZoomChange) < 50) {
                                zoomDiff = 0;
                            }
                            else {
                                currentTimeline.viewModel.sensorDataCounterLastZoomChange = currentTimeline.viewModel.sensorDataCounter;
                                isScheduleUpdate = true;
                            }
                        }
                        currentTimeline.viewModel.timelineZoomLevel += zoomDiff;
                        currentTimeline.viewModel.timelineZoomLevel = currentTimeline.viewModel.timelineZoomLevel > currentTimeline.viewModel.timelineMaxZoomLevel ? currentTimeline.viewModel.timelineMaxZoomLevel : currentTimeline.viewModel.timelineZoomLevel < currentTimeline.viewModel.timelineMinZoomLevel ? currentTimeline.viewModel.timelineMinZoomLevel : currentTimeline.viewModel.timelineZoomLevel;
                        if (isScheduleUpdate) {
                            currentTimeline.viewModel.timelineElements = currentTimeline.createTimelineElements();
                            currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
                            currentApp.projector.scheduleRender();
                        }
                    }                    
                }
                else {
                    status = "accelerationIncludingGravity data undefined";
                }
            }
        }
        currentTimeline.viewModel.sensorStatus = status as string;
    };

    public createTimeTableClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        currentApp.controller.CreateTimeTableJson().done((data: any) => {
            TokyoTrackerApp.TrackerAppInstance.client.UpdateData(data);
        });
    };

    public createWorkUnitClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits) {
            // when wid attribute is set, move created workunit in front of target workunit
            let workUnitAttribute: Attr | null = (evt.target as HTMLElement).attributes.getNamedItem("wid");
            if (workUnitAttribute !== null) {
                let beforeWorkUnitId: number = parseInt(workUnitAttribute.value);
                currentApp.controller.CreateWorkUnitJson(currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.TimeTableId).done((data: TrackerClientViewModel) => {
                    // TODO document: response data used for next request
                    // TODO prevent UI update in between chained calls
                    let workUnitIdForChainCall: number = 0;
                    if (data.WorkUnit !== undefined) {
                        workUnitIdForChainCall = data.WorkUnit.WorkUnitId;
                    }
                    TokyoTrackerApp.TrackerAppInstance.client.UpdateData(data, true); // TODO bugs on chrome, still gets rendered on end then at target position
                    currentApp.controller.MoveWorkUnitBeforeWorkUnitJson(workUnitIdForChainCall, beforeWorkUnitId).done((data: any) => {
                        TokyoTrackerApp.TrackerAppInstance.client.UpdateData(data);
                    });
                });
            }
            else {
                currentApp.controller.CreateWorkUnitJson(currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.TimeTableId)
                    .done((data: TrackerClientViewModel) => currentClient.UpdateData(data));
            }
        }
        else {
            console.log("Error: No table selected."); // TODO
        }
    };

    public removeTimeTableClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits) {
            currentApp.controller.RemoveTimeTableJson(currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.TimeTableId).done((data: any) => {
                TokyoTrackerApp.TrackerAppInstance.client.UpdateData(data);
            });
        }
        else {
            console.log("Error: No table selected."); // TODO
        }
    };

    public processTimeStampsClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        currentApp.controller.ProcessTimeStampsJson().done((response: any) => currentClient.UpdateData(response));
    };

    public removeTimeStampButtonClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        let timeStampId = parseIntFromAttribute(evt.target, "tid");
        currentApp.controller.RemoveTimeStampJson(timeStampId).done((data: any) => {
            TokyoTrackerApp.TrackerAppInstance.client.UpdateData(data);
        });
    };

    public updateWorkUnitMappingForSelectedTable = () => {
        if (currentApp.clientData.SelectedTimeTable !== undefined
            && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits !== undefined
            && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                currentClient.updateWorkUnitMapping(currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits);
            }
    };

    public updateWorkUnitMapping = (workUnits: WorkUnit[]) => {
        currentClient.viewModel.updateWorkUnitsForTableMapping(workUnits);
        currentTimeline.updateWorkUnitMapping(workUnits);
    };

    public workUnitNameChangedHandler = (evt: KeyboardEvent) => {
        currentClient.viewModel.tempWorkUnitName = (evt.target as HTMLInputElement).value;
    };

    public workUnitNameLostFocusHandler = (evt: FocusEvent) => {
        currentClient.updateWorkUnitNameHandler();
    };

    public workUnitNameKeyDownHandler = (evt: KeyboardEvent) => {
        if (evt.keyCode == 13 /*ENTER*/) {
            evt.preventDefault();
            (evt.target as HTMLInputElement).blur();
        }
        else if (evt.keyCode == 27 /*ESC*/) {
            evt.preventDefault();
            currentClient.viewModel.tempWorkUnitName = "";
            currentClient.viewModel.tempEditedWorkUnitId = undefined;
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
            (evt.target as HTMLInputElement).blur();
        }
    };

    public workUnitNameClickedHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode !== ClientEditMode.None) {
            return;
        }
        let tid = parseStringFromAttribute(evt.target, "tid");
        let workUnitId = parseInt(tid);
        if (currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits
            && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits) {
            let targetWorkUnit = currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits.find(w => w.WorkUnitId == workUnitId);
            if (targetWorkUnit != undefined) {
                currentClient.viewModel.currentEditMode = ClientEditMode.WorkUnit;
                currentClient.viewModel.tempWorkUnitName = targetWorkUnit.Name;
                currentClient.viewModel.tempEditedWorkUnitId = workUnitId;
            }
            else {
                // error TODO
            }
        }
    };

    public timeTableNameChangedHandler = (event: KeyboardEvent) => {
        currentClient.viewModel.tempTimeTableName = (event.target as HTMLInputElement).value;
    };

    public activeWorkUnitChangedHandler = (event: KeyboardEvent) => {
        // TODO only track value client side / in cookie; needed for background tasks?
        let workUnitId: number = parseInt((event.target as HTMLInputElement).value);
        currentApp.controller.SetWorkUnitAsDefaultTargetJson(workUnitId).done((response: any) => currentClient.UpdateData(response));
    }

    public timeNormNameChangedHandler = (event: KeyboardEvent) => {
        currentClient.viewModel.tempTimeNormName = (event.target as HTMLInputElement).value;
    };

    public timeZoneViewChangedHandler = (event: UIEvent) => {  // TODO code duplication at all selects
        let targetSelect = event.target as HTMLSelectElement;
        let parsedTimeZoneKey: number | undefined = undefined;
        if (targetSelect.selectedOptions !== undefined) {
            if (targetSelect.selectedOptions.length == 1) { // TODO workaround internet explorer 11 everywhere where select is used
                parsedTimeZoneKey = parseInt(targetSelect.selectedOptions[0].value);
            }
        }
        else {
            // workaround iexplore 11
            if (targetSelect.selectedIndex < targetSelect.childElementCount) {
                let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
                parsedTimeZoneKey = parseInt(selectOptionElement.value);
            }
        }
        if (parsedTimeZoneKey !== undefined && currentApp.clientData.TimeZones !== undefined) {
            let trackerTimeZone = currentApp.clientData.TimeZones.find(tz => tz.Key == parsedTimeZoneKey);  // TODO code duplication with culture
            if (trackerTimeZone !== undefined) {
                let resetTimeZoneId: string = currentClient.viewModel.lastTimeZoneIdDotNet;
                currentClient.viewModel.lastTimeZoneIdDotNet = currentApp.clientData.SelectedTimeZoneIdView;
                currentApp.clientData.SelectedTimeZoneIdView = trackerTimeZone.IdDotNet;
                currentApp.controller.SetViewTimeZoneJson(trackerTimeZone.IdDotNet).done((data: any) => currentClient.UpdateData(data)).fail((data: any) => { currentClient.viewModel.lastTimeZoneIdDotNet = resetTimeZoneId; }); // TODO bugged // TODO fail for everywhere select change handlers
                return;
            }
        }
        console.log("Error: reload page."); // TODO
    };

    public cultureChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        let parsedCultureKey: number | undefined = undefined;
        if (targetSelect.selectedOptions !== undefined) {
            if (targetSelect.selectedOptions.length == 1) {
                parsedCultureKey = parseInt(targetSelect.selectedOptions[0].value);
            }
        }
        else {
            // workaround iexplore 11
            if (targetSelect.selectedIndex < targetSelect.childElementCount) {
                let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
                parsedCultureKey = parseInt(selectOptionElement.value); // TODO need try catch? or is parseInt API():number|undefined?
            }
        }
        if (parsedCultureKey !== undefined && currentApp.clientData.CultureIds !== undefined) {
            let cultureId = currentApp.clientData.CultureIds.find(culture => culture.Key == parsedCultureKey);  // TODO code duplication with timezone
            if (cultureId !== undefined) {
                let resetCultureId: string = currentClient.viewModel.lastCultureIdDotNet;
                currentClient.viewModel.lastCultureIdDotNet = currentApp.clientData.SelectedCultureId;
                currentApp.clientData.SelectedCultureId = cultureId.IdDotNet;
                currentApp.controller.SetCultureJson(cultureId.IdDotNet).done((data: any) => currentClient.UpdateData(data)).fail((data: any) => { currentClient.viewModel.lastCultureIdDotNet = resetCultureId; }); // TODO bugged // TODO fail for everywhere select change handlers
                currentClient.viewModel.currentEditMode = ClientEditMode.None;
                return;
            }
        }
        console.log("Error: reload page."); // TODO
    };

    public selectLastSelectedCultureClickHandler = () => {
        if (currentClient.viewModel.lastCultureIdDotNet !== "" && currentApp.clientData.SelectedCultureId !== "") {
            let resetCultureId: string = currentClient.viewModel.lastCultureIdDotNet;
            currentClient.viewModel.lastCultureIdDotNet = currentApp.clientData.SelectedCultureId;
            currentApp.clientData.SelectedCultureId = resetCultureId;
            currentApp.controller.SetCultureJson(resetCultureId).done((data: any) => currentClient.UpdateData(data)).fail((data: any) => { currentClient.viewModel.lastCultureIdDotNet = resetCultureId; }); // TODO bugged // TODO fail for everywhere select change handlers
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
            return;
        }
    };

    public selectLastSelectedTimeZoneClickHandler = () => {
        if (currentClient.viewModel.lastTimeZoneIdDotNet !== "" && currentApp.clientData.SelectedTimeZoneIdTimeStamps !== "") {
            let resetTimeZoneId: string = currentClient.viewModel.lastTimeZoneIdDotNet;
            currentClient.viewModel.lastTimeZoneIdDotNet = currentApp.clientData.SelectedTimeZoneIdView;
            currentApp.clientData.SelectedTimeZoneIdView = resetTimeZoneId;
            currentApp.controller.SetViewTimeZoneJson(resetTimeZoneId).done((data: any) => currentClient.UpdateData(data)).fail((data: any) => { currentClient.viewModel.lastTimeZoneIdDotNet = resetTimeZoneId; }); // TODO bugged // TODO fail for everywhere select change handlers
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
            return;
        }
    };

    /* TODO
    public defaultWorkUnitChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        if (targetSelect.selectedOptions.length == 1) {
            let parsedWorkUnitId = parseInt(targetSelect.selectedOptions[0].value);
            currentApp.controller.SetWorkUnitAsDefaultTargetJson(parsedWorkUnitId)
                .done((response: any) => currentClient.UpdateData(response, false));
            return;
        }        
        console.log("Error: reload page."); // TODO
    };*/

    /*public workUnitForMoveChangedHandler = (evt: UIEvent) => { TODO unused
        let targetSelect = evt.target as HTMLSelectElement;
        if (targetSelect.selectedOptions.length == 1) {
            let parsedWorkUnitId = parseInt(targetSelect.selectedOptions[0].value);
            currentClient.viewModel.forMoveWorkUnitId = parsedWorkUnitId;
        }
    };*/

    /*public workUnitPositionChangedHandler = (evt: UIEvent) => { TODO unused
        if (currentClient.viewModel.forMoveWorkUnitId != undefined) {
            let targetSelect = evt.target as HTMLSelectElement;
            if (targetSelect.selectedOptions.length == 1) {
                let parsedWorkUnitId = parseInt(targetSelect.selectedOptions[0].value);
                currentApp.controller.MoveWorkUnitBeforeWorkUnitJson(currentClient.viewModel.forMoveWorkUnitId, parsedWorkUnitId)
                    .done((response: any) => currentClient.UpdateData(response));
                return;
            }
            console.log("Error: reload page."); // TODO
        }
    };*/

    public jumpToTimeTableClickHandler = (evt: MouseEvent) => {
        let targetLink = evt.target as HTMLLinkElement;
        let timeTableId: number = parseIntFromAttribute(targetLink, "tid");
        // TODO document may not be called while update is running and data could still be updated
        if (currentApp.clientData.SelectedTimeTableId != timeTableId) {
            evt.preventDefault(); // prevent link action
            currentClient.changeSelectedTimeTableAndUpdateTableOfContents(timeTableId);
        }    
    }

    private changeSelectedTimeTableAndUpdateTableOfContents = (timeTableId: number) => {
        // may only be called from updateData routine
        if (currentApp.clientData.TimeTables !== undefined) {
            if (currentApp.clientData.SelectedTimeTableId != timeTableId) {
                let selectedTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                    if (t.TimeTableAndWorkUnits) {
                        return t.TimeTableAndWorkUnits.TimeTableId == timeTableId;
                    }
                    return false;
                });
                if (selectedTable !== undefined 
                    && selectedTable.TimeTableAndWorkUnits !== undefined  
                    && selectedTable.TimeTableAndWorkUnits.WorkUnits !== undefined ) {
                    currentApp.clientData.SelectedTimeTable = selectedTable;
                    currentApp.clientData.SelectedTimeTableId = timeTableId;
                    currentClient.updateWorkUnitMapping(selectedTable.TimeTableAndWorkUnits.WorkUnits);
                    currentTableOfContents.viewModel.activeSectionName = currentApp.clientData.SelectedTimeTableId.toString();
                    if (currentClient.timeTableScrollDivElement !== undefined && (Object.getPrototypeOf(currentClient.timeTableScrollDivElement) as any).scrollTo !== undefined) {
                        currentClient.timeTableScrollDivElement.scrollTop = 0; // TODO not supported by edge
                    }
                    currentApp.projector.renderNow();
                    currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId); // TODO should not be called here
                    return;
                }
                else {
                    console.log("Invalid table id.")
                }
            }
        };
    }

    // TODO security audit: how to make sure object prototypes are not altered / create all on app start?

    public selectedTimeTableChangedHandler = (evt: UIEvent) => {
        let targetSelect = evt.target as HTMLSelectElement;
        let parsedTimeTableId: number | undefined = undefined;
        if (targetSelect.selectedOptions !== undefined) {
            if (targetSelect.selectedOptions.length == 1) {
                parsedTimeTableId = parseInt(targetSelect.selectedOptions[0].value);
            }
        }
        else {
            // workaround iexplore 11
            if (targetSelect.selectedIndex < targetSelect.childElementCount) {
                let selectOptionElement: HTMLOptionElement = targetSelect.options[targetSelect.selectedIndex];
                parsedTimeTableId = parseInt(selectOptionElement.value);
            }
        }
        if (parsedTimeTableId !== undefined && parsedTimeTableId != currentApp.clientData.SelectedTimeTableId) {
            currentClient.changeSelectedTimeTableAndUpdateTableOfContents(parsedTimeTableId);
        }
    };

    public timeTableNameLostFocusHandler = (evt: FocusEvent) => {
        currentClient.updateTimeTableNameHandler();
    };

    public timeNormNameLostFocusHandler = (evt: FocusEvent) => {
        currentClient.updateTimeNormNameHandler();
    };

    public updateTimeNormNameHandler = () => {
        if (currentClient.viewModel.currentEditMode === ClientEditMode.TimeNorm) {
            if (currentClient.viewModel.tempEditedTimeNormId) {
                if (currentClient.viewModel.tempTimeNormName !== "") {
                    currentClient.viewModel.currentEditMode = ClientEditMode.Pending; // TODO buggy when response slow
                    currentApp.controller.EditTimeNormJson(currentClient.viewModel.tempEditedTimeNormId, currentClient.viewModel.tempTimeNormName, currentClient.viewModel.tempTimeNormColorR, currentClient.viewModel.tempTimeNormColorG, currentClient.viewModel.tempTimeNormColorB).done((data: any) => {
                        currentClient.UpdateData(data);
                        currentClient.viewModel.currentEditMode = ClientEditMode.None;
                    }).fail((data: any) => currentClient.viewModel.currentEditMode = ClientEditMode.None);
                }
                else {
                    currentClient.viewModel.currentEditMode = ClientEditMode.None;
                    currentClient.viewModel.tempEditedTimeNormId = undefined;
                    currentClient.viewModel.tempEditedTimeNormParentWorkUnitId = undefined;
                    currentClient.viewModel.tempTimeNormName = "";
                    currentClient.viewModel.tempTimeNormColorR = 0;
                    currentClient.viewModel.tempTimeNormColorG = 0;
                    currentClient.viewModel.tempTimeNormColorB = 0;
                    return;
                }
                currentClient.viewModel.tempTimeNormName = "";
                currentClient.viewModel.tempTimeNormColorR = 0;
                currentClient.viewModel.tempTimeNormColorG = 0;
                currentClient.viewModel.tempTimeNormColorB = 0;
                currentClient.viewModel.tempEditedTimeNormId = undefined;
                currentClient.viewModel.tempEditedTimeNormParentWorkUnitId = undefined;
            }
            else {
                // error TODO
            }
        }
    };

    public updateWorkUnitNameHandler = () => {
        if (currentClient.viewModel.currentEditMode === ClientEditMode.WorkUnit) {
            if (currentClient.viewModel.tempEditedWorkUnitId) {
                if (currentClient.viewModel.tempWorkUnitName !== "") {
                    currentClient.viewModel.currentEditMode = ClientEditMode.Pending; // TODO buggy
                    currentApp.controller.EditWorkUnitJson(currentClient.viewModel.tempEditedWorkUnitId, currentClient.viewModel.tempWorkUnitName).done((data: any) => {
                        currentClient.UpdateData(data);
                        currentClient.viewModel.currentEditMode = ClientEditMode.None;
                    }).fail((data: any) => currentClient.viewModel.currentEditMode = ClientEditMode.None);
                }
                else {
                    currentClient.viewModel.currentEditMode = ClientEditMode.None;
                    currentClient.viewModel.tempWorkUnitName = "";
                    currentClient.viewModel.tempEditedWorkUnitId = undefined;
                    return;
                }
                currentClient.viewModel.tempEditedWorkUnitId = undefined;
                currentClient.viewModel.tempWorkUnitName = "";
            }
            else {
                // error TODO
            }
        }
    };

    public updateTimeTableNameHandler = () => {
        if (currentClient.viewModel.currentEditMode === ClientEditMode.TimeTable) {
            if (currentClient.viewModel.tempTimeTableName !== "") {
                if (currentApp.clientData.SelectedTimeTable !== undefined && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits !== undefined) {
                    currentClient.viewModel.currentEditMode = ClientEditMode.Pending;
                    currentApp.controller.EditTimeTableJson(currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.TimeTableId, currentClient.viewModel.tempTimeTableName).done((data: any) => {
                        currentClient.UpdateData(data);
                        currentClient.viewModel.currentEditMode = ClientEditMode.None;
                    }).fail((data: any) => currentClient.viewModel.currentEditMode = ClientEditMode.None);
                }
            }
            else {
                currentClient.viewModel.currentEditMode = ClientEditMode.None;
                currentClient.viewModel.tempTimeTableName = "";
                return;
            }
            currentClient.viewModel.tempTimeTableName = "";
        }
        else {
            // error TODO
        }
    };

    public timeTableNameKeyDownHandler = (evt: KeyboardEvent) => { // TODO code duplication
        if (evt.keyCode == 13 /*ENTER*/) {
            evt.preventDefault();
            (evt.target as HTMLInputElement).blur();
        }
        else if (evt.keyCode == 27 /*ESC*/) {
            evt.preventDefault();
            currentClient.viewModel.tempTimeTableName = "";
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
            (evt.target as HTMLInputElement).blur();
        }
    };

    public timeNormNameKeyDownHandler = (evt: KeyboardEvent) => { // TODO code duplication
        if (evt.keyCode == 13 /*ENTER*/) {
            evt.preventDefault();
            (evt.target as HTMLInputElement).blur();
        }
        else if (evt.keyCode == 27 /*ESC*/) {
            evt.preventDefault();
            currentClient.viewModel.tempTimeNormName = "";
            currentClient.viewModel.tempEditedTimeNormId = undefined;
            currentClient.viewModel.tempEditedTimeNormParentWorkUnitId = undefined;
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
            (evt.target as HTMLInputElement).blur();
        }
    };

    public cancelEditTimeStampClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode === ClientEditMode.EditTimeStamp || currentClient.viewModel.currentEditMode === ClientEditMode.AddManualTimeStamp) {
            currentClient.viewModel.tempEditedTimeStamp = undefined;
            currentClient.viewModel.tempEditedTimeStampHtmlInput = "";
            document.body.style.background = "white"; // TODO when entering popup mode background color is changed => use viewmodel
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
            currentClient.viewModel.isTempEditedTimeStampChanged = false;
        }
    }

    public cancelShareTimeTableClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode === ClientEditMode.ShareTimeTable) {
            document.body.style.background = "white"; // TODO when entering popup mode background color is changed => use viewmodel
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
        }
    }
    
    public cancelGlobalizationSetupClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode === ClientEditMode.GlobalizationSetup) {
            document.body.style.background = "white"; // TODO when entering popup mode background color is changed => use viewmodel
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
        }
    }

    public cancelChartClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode === ClientEditMode.DisplayChart) {
            document.body.style.background = "white"; // TODO when entering popup mode background color is changed => use viewmodel
            currentClient.viewModel.currentEditMode = ClientEditMode.None;
        }
    }

    public editProductivityRatingClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        let timeNormId = parseIntFromAttribute(evt.target, "tid");
        let ratingAttr: Attr | null = (evt.target as HTMLElement).attributes.getNamedItem("wid");
        let ratingValue: number = 0.0;
        if (ratingAttr === null) {
            console.log(DEFAULT_EXCEPTION);
        }
        else {
            ratingValue = parseFloat(ratingAttr.value);
        }
        let isActiveRating = parseIntFromAttribute(evt.target, "zid") == 1;
        if (isActiveRating) {
            ratingValue = 0.0;
        }
        currentApp.controller.EditProductivityRatingJson(timeNormId, ratingValue)
            .done((data: any) => {
                currentClient.UpdateData(data);
            });

        currentClient.viewModel.hoveredProductivityStarIndex = undefined;
        currentClient.viewModel.hoveredProductivityTimeNormId = undefined; // TODO refactor: should be called when interaction ends for every possible ui state
    };

    public enterProductivityRatingHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        let timeNormId = parseIntFromAttribute(evt.target, "tid");
        let starIndex = parseIntFromAttribute(evt.target, "xid");
        currentClient.viewModel.hoveredProductivityTimeNormId = timeNormId;
        currentClient.viewModel.hoveredProductivityStarIndex = starIndex;
    };

    public exitProductivityRatingHandler = (evt: MouseEvent) => {
        currentClient.viewModel.hoveredProductivityStarIndex = undefined;
        currentClient.viewModel.hoveredProductivityTimeNormId = undefined;
    };

    public duplicateTimeStampClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode === ClientEditMode.EditTimeStamp) {
            if (currentClient.viewModel.tempEditedTimeStamp !== undefined) {
                currentApp.controller.DuplicateTimeStampJson(currentClient.viewModel.tempEditedTimeStamp.TimeStampId).done((data: any) => {
                        currentClient.UpdateData(data);
                    });
            }
        }
        currentClient.viewModel.tempEditedTimeStamp = undefined; // TODO code duplication
        currentClient.viewModel.tempEditedTimeStampHtmlInput = "";
        document.body.style.background = "white";
        currentClient.viewModel.currentEditMode = ClientEditMode.None;
        currentClient.viewModel.isTempEditedTimeStampChanged = false;
    }

    public saveEditTimeStampClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode === ClientEditMode.EditTimeStamp) {
            if (currentClient.viewModel.tempEditedTimeStamp !== undefined) {
                currentApp.controller.EditTimeStampPostJson(currentClient.convertHtmlInputToEditTimeStampVM(currentClient.viewModel.tempEditedTimeStampHtmlInput, currentClient.viewModel.tempEditedTimeStamp.TimeStampId))
                    .done((data: any) => {
                        currentClient.UpdateData(data);
                    });
            }
        }
        else if (currentClient.viewModel.currentEditMode === ClientEditMode.AddManualTimeStamp) {
            currentApp.controller.CreateTimeStampManuallyPostJson(currentClient.convertHtmlInputToTimeStampVM(currentClient.viewModel.tempEditedTimeStampHtmlInput))
                .done((data: any) =>
                {
                    currentClient.UpdateData(data);
                });
        }
        currentClient.viewModel.tempEditedTimeStamp = undefined; // TODO code duplication
        currentClient.viewModel.tempEditedTimeStampHtmlInput = "";
        document.body.style.background = "white";
        currentClient.viewModel.currentEditMode = ClientEditMode.None;
        currentClient.viewModel.isTempEditedTimeStampChanged = false;
    }

    public tableNameClickedHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode !== ClientEditMode.None) {
            return;
        }
        let tid = parseStringFromAttribute(evt.target, "tid");
        let tableId = parseInt(tid); // TODO not needed ?
        if (currentApp.clientData.SelectedTimeTable !== undefined && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits !== undefined) {
            currentClient.viewModel.currentEditMode = ClientEditMode.TimeTable;
            currentClient.viewModel.tempTimeTableName = currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.Name;
        }
        // TODO if we make it this far throw error
    };

    public timeNormNameClickedHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode !== ClientEditMode.None) {
            return;
        }
        let timeNormId = parseIntFromAttribute(evt.target, "tid");
        let workUnitId = parseIntFromAttribute(evt.target, "wid");
        if (currentApp.clientData.SelectedTimeTable && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits
            && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits) {
            let targetWorkUnit = currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits.find(w => w.WorkUnitId == workUnitId);
            if (targetWorkUnit !== undefined && targetWorkUnit.TimeNorms !== undefined) {
                let targetTimeNorm = targetWorkUnit.TimeNorms.find(t => t.TimeNormId == timeNormId);
                if (targetTimeNorm) {
                    currentClient.viewModel.currentEditMode = ClientEditMode.TimeNorm;
                    currentClient.viewModel.tempTimeNormName = targetTimeNorm.Name;
                    currentClient.viewModel.tempEditedTimeNormId = timeNormId;
                    currentClient.viewModel.tempEditedTimeNormParentWorkUnitId = workUnitId;
                    currentClient.viewModel.tempTimeNormColorR = targetTimeNorm.ColorR;
                    currentClient.viewModel.tempTimeNormColorG = targetTimeNorm.ColorG;
                    currentClient.viewModel.tempTimeNormColorB = targetTimeNorm.ColorB;
                    return;
                }
            }
        }
        // TODO if we make it this far throw error
    };

    public createManualTimeStampClickHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (currentClient.viewModel.currentEditMode !== ClientEditMode.None) {
            return;
        }
        
        let currentDateTimeLocal = new Date(Date.now()); // TODO cannot use new Date because it's forced to local time zone
        currentClient.viewModel.tempEditedTimeStamp = {
            TrackedTimeAtCreation: currentDateTimeLocal,
            TrackedTimeForView: currentDateTimeLocal,
            TimeStampId: 0,
            BoundNormStart: undefined,
            BoundNormEnd: undefined,
            IsBound: false,
            Name: "MANUAL_TIMESTAMP",
            TrackerStoreId: "",
            TimeZoneIdAtCreation: "UTC",
            UtcOffsetAtCreation: "0",
            IsNegativeUtcOffset: false,
            AbsoluteOfUtcOffset: "0",
            TimeString: "UNSET_TIME",
            DateString: "UNSET_DATE"
        };
        currentClient.viewModel.tempEditedTimeStampHtmlInput = currentClient.convertTimeStampForHtmlInput(currentTimeline.viewModel.timelineDateCenter !== undefined ? currentTimeline.viewModel.timelineDateCenter : currentClient.viewModel.tempEditedTimeStamp.TrackedTimeForView);
        currentClient.displayPopup(ClientEditMode.AddManualTimeStamp, evt.target as HTMLElement);
    };

    public timeZoneViewUpClickHandler = (evt: MouseEvent) => {
        // TODO currentTimeline.;
    }

    public timeZoneViewDownClickHandler = (evt: MouseEvent) => {
        // TODO currentTimeline.;
    }

    public shareTimeTableClickHandler = (evt: MouseEvent) => {
        currentClient.displayPopup(ClientEditMode.ShareTimeTable, evt.target as HTMLElement);
    }

    public displayChartClickHandler = (evt: MouseEvent) => {
        currentClient.displayPopup(ClientEditMode.DisplayChart, evt.target as HTMLElement);
    }

    public displayPopup = (editMode: ClientEditMode, targetPosition: HTMLElement) => {
        if (currentClient.viewModel.currentEditMode !== ClientEditMode.None
            && /*one exception */ currentClient.viewModel.currentEditMode !== ClientEditMode.EditTimeStamp) {
            return;
        }
        
        let popupElement: HTMLElement | null = null;
        if (editMode === ClientEditMode.AddManualTimeStamp) {
            popupElement = document.getElementById(ClientEditMode[ClientEditMode.EditTimeStamp]) as HTMLElement;
        }
        else {
            popupElement = document.getElementById(ClientEditMode[editMode]) as HTMLElement;
        }

        if (popupElement !== null) {
            currentClient.viewModel.currentEditMode = editMode;
            var displayPopup = new popperjs.default(targetPosition, popupElement, {
                placement: 'bottom-start',
                modifiers: {
                    /*flip: {
                        behavior: ['left', 'bottom', 'top']
                    },*/
                    preventOverflow: {
                        boundariesElement: document.body,
                    }
                },
            });
            document.body.style.backgroundColor = "rgb(245, 245, 245)";
            currentApp.projector.renderNow(); // required to update popup position to be contained by boundaries element
            return;
        }
        console.log("reload page...");// TODO display error
    };

    public globalizationSetupClickHandler = (evt: MouseEvent) => {
        currentClient.displayPopup(ClientEditMode.GlobalizationSetup, currentClient.topBarGlobalizationButtonElement as HTMLElement);
    }

    public editTimeStampClickHandler = (evt: MouseEvent) => {
        // TODO edit in device timezone or edit in stamp timezone
        if (currentClient.viewModel.currentEditMode !== ClientEditMode.None) {
            return;
        }
        let timeStampId = parseIntFromAttribute(evt.target, "tid");
        let timeNormId = parseIntFromAttribute(evt.target, "wid");
        let zid: string = parseStringFromAttribute(evt.target, "zid");
        let isStartTimeStamp = zid === "a";
        let targetTimeStamp: TimeStamp | undefined = undefined;
        if (zid === "c") {
            // unbound timestamp
            if (currentApp.clientData.UnboundTimeStamps !== undefined) {
                targetTimeStamp = currentApp.clientData.UnboundTimeStamps.find(t => t.TimeStampId == timeStampId);
            }
        }
        else {
            if (currentApp.clientData.SelectedTimeTable
                && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits
                && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits) {
                let targetIndex: number = -1;
                let targetWorkUnit: WorkUnit | undefined = currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits.find(w => {
                    if (w.TimeNorms !== undefined) {
                        let resultIndex: number = w.TimeNorms.findIndex(t => t.TimeNormId == timeNormId);
                        if (resultIndex != -1) {
                            targetIndex = resultIndex;
                            return true; // TODO code duplication find time norm by id
                        }
                    }
                    return false;
                });
                if (targetIndex != -1 && targetWorkUnit && targetWorkUnit.TimeNorms) {
                    let targetTimeNorm = targetWorkUnit.TimeNorms[targetIndex];
                    if (targetTimeNorm) {
                        targetTimeStamp = isStartTimeStamp ? targetTimeNorm.StartTime : targetTimeNorm.EndTime;
                    } 
                } // TODO ? targetTimeStamp
            }
        }
        if (targetTimeStamp !== undefined) {
            currentClient.viewModel.tempEditedTimeStamp = {
                TrackedTimeAtCreation: targetTimeStamp.TrackedTimeAtCreation,
                TrackedTimeForView: targetTimeStamp.TrackedTimeForView,
                TimeStampId: targetTimeStamp.TimeStampId,
                BoundNormStart: targetTimeStamp.BoundNormStart,
                BoundNormEnd: targetTimeStamp.BoundNormEnd,
                IsBound: targetTimeStamp.IsBound,
                Name: targetTimeStamp.Name,
                TrackerStoreId: targetTimeStamp.TrackerStoreId,
                TimeZoneIdAtCreation: "UTC",
                UtcOffsetAtCreation: "0",
                IsNegativeUtcOffset: false, // TODO
                AbsoluteOfUtcOffset: "0",
                TimeString: targetTimeStamp.TimeString,
                DateString: targetTimeStamp.DateString
            };
            currentClient.viewModel.tempEditedTimeStampHtmlInput = currentClient.convertTimeStampForHtmlInput(currentClient.viewModel.tempEditedTimeStamp.TrackedTimeForView);
            currentClient.displayPopup(ClientEditMode.EditTimeStamp, evt.target as HTMLElement);
            return;
        }
        // TODO throw error if we made it this far
    };

    private dataResponseCheck = (response: TrackerClientViewModel): boolean => {
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
    }

    public updateCultureAfterDataChanged = (weekDayLetters: string[], startDayOfWeekIndex: number, abbreviatedMonths: string[]): void => {
        // day names from server start with monday, start week index depends on language, day of week depends on device timezone
        let referenceDateMonday: Date = new Date(2018, 0, 1, 0, 0, 0, 1);
        let mondayIndexOfWeekForDevice: number = referenceDateMonday.getDay();
        // day of week depends on device timezone => use reference start day of week from server, compare to device timezone and offset array
        // TODO how to test this? need independent devices, configurable languages, time zones, on server and client side etc.
        // e.g. client device set to EN, located in EN [Mon, Tue, Wen, Thu, Fri, Sat, Sun], startWeekIndex == 6, mondayIndexDevice == 1
        // e.g. client device set to EN, located in DE [Mon, Tue, Wen, Thu, Fri, Sat, Sun], startWeekIndex == 6, mondayIndexDevice == 0
        // e.g. client device set to DE, located in DE [Mon, Die, Mit, Don, Fre, Sam, Son], startWeekIndex == 0, mondayIndexDevice == 0
        // e.g. client device set to DE, located in EN [Mon, Die, Mit, Don, Fre, Sam, Son], startWeekIndex == 0, mondayIndexDevice == 1
        currentApp.clientData.WeekDayLetters = [];
        let offsetDay: number = 7 - mondayIndexOfWeekForDevice;
        for (let i = offsetDay; i < 7; i++) {
            currentApp.clientData.WeekDayLetters.push(weekDayLetters[i]);
        }
        for (let i = 0; i < offsetDay; i++) {
            currentApp.clientData.WeekDayLetters.push(weekDayLetters[i]);
        }
        currentApp.clientData.StartDayOfWeekIndex = startDayOfWeekIndex + mondayIndexOfWeekForDevice; // TODO this must be double checked
        if (currentApp.clientData.StartDayOfWeekIndex > 6) {
            currentApp.clientData.StartDayOfWeekIndex -= 7;
        }

        currentApp.clientData.AbbreviatedMonths = abbreviatedMonths;
    }

    public setupUiForDevice = (): void => {
        if (window.innerWidth < TIMELINE_MIN_WIDTH) {
            currentClient.viewModel.isToggledTimeline = false;
            currentClient.viewModel.isToggledTableOfContents = false;
            currentClient.viewModel.isToggledDetailedTimeNorm = false;
            currentClient.viewModel.isToggledSelectToc = true;
        }
        else {
            currentClient.viewModel.isToggledTimeline = true;
            currentClient.viewModel.isToggledTableOfContents = true;
            currentClient.viewModel.isToggledDetailedTimeNorm = true;
            currentClient.viewModel.isToggledSelectToc = false;
        }
    };

    public setupInitialDataStore = (response: TrackerClientViewModel): void => {
        // copy data from response to native client data store (conversions for dates, object references etc.)
        let isValidResponse: boolean = currentClient.dataResponseCheck(response);
        if (!isValidResponse) {
            return;
        }
        // TODO more initial client check if timezones are loaded etc. test basic setup and clear view again
        currentClient.setupUiForDevice(); // TODO workaround for iexplore/android required double render to get innerwidth?
        if (response.TimeTables !== undefined 
            && response.UnboundTimeStamps !== undefined 
            && response.TimeZones !== undefined) {
            // TODO it's error prone to copy every member manually
            currentApp.clientData.TimeZones = response.TimeZones;
            currentApp.clientData.CultureIds = response.CultureIds;
            currentApp.clientData.SelectedCultureId = response.SelectedCultureId;
            currentApp.clientData.SelectedTimeZoneIdTimeStamps = response.SelectedTimeZoneIdTimeStamps; // TODO
            currentApp.clientData.SelectedTimeZoneIdView = response.SelectedTimeZoneIdView;
            currentApp.clientData.ClientTimelineOffset = response.ClientTimelineOffset;

            currentClient.updateCultureAfterDataChanged(response.WeekDayLetters, response.StartDayOfWeekIndex, response.AbbreviatedMonths)

            currentClient.viewModel.updateTimeZonesMapping(currentApp.clientData.TimeZones);
            currentClient.viewModel.setTimeZone(currentApp.clientData.SelectedTimeZoneIdTimeStamps, TimeZoneSetting.Device); // TODO
            currentClient.viewModel.setTimeZone(currentApp.clientData.SelectedTimeZoneIdView, TimeZoneSetting.View);

            
            currentApp.clientData.UrlToReadOnly = response.UrlToReadOnly;
            currentApp.clientData.UrlToReadAndEdit = response.UrlToReadAndEdit;

            // update selected object references / ids
            currentApp.clientData.TimeTables = response.TimeTables;
            currentApp.clientData.TimeTables.map(t => currentClient.convertTableDataForClient(t));
            currentApp.clientData.UnboundTimeStamps = response.UnboundTimeStamps;
            currentApp.clientData.SelectedTimeTableId = response.SelectedTimeTableId;
            currentApp.clientData.TargetWorkUnitId = response.TargetWorkUnitId;

            currentTableOfContents.viewModel.activeSectionName = currentApp.clientData.SelectedTimeTableId.toString(); // TODO are there more such bugs? where initializer is only used in other sub function => activeSectionName

            currentClient.updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged();

            // window.location.hash = "";//`#${(initialActiveAnchor[0] as HTMLAnchorElement).name.toString()}`; clear hash from previous navigation TODO
            currentApp.projector.scheduleRender();
        }
        else {
            console.log("missing client data");
        }
    }

    public convertTableDataForClient = (timeTable: TimeTableViewModel): void => {
        // may only be called once per table
        // TODO add flag to mark updated tables?
        // convert timedates to native javascript objects TODO only correct for native timezone
        timeTable.EarliestDateTimeInData = new Date(timeTable.EarliestDateTimeInData as string);
        timeTable.LatestDateTimeInData = new Date(timeTable.LatestDateTimeInData as string);
        if (timeTable.TimeTableAndWorkUnits !== undefined && timeTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
            // TODO time conversion => make sure all stamps are converted only once (either check typeof string or only call function when whole data is updated)
            timeTable.TimeTableAndWorkUnits.WorkUnits.map(wu => {
                if (wu.TimeNorms !== undefined) {
                    wu.TimeNorms.map(tin => {
                        if (tin.StartTime !== undefined) {
                                tin.StartTime.TrackedTimeForView = new Date(tin.StartTime.TrackedTimeForView as string);
                                tin.StartTime.TrackedTimeAtCreation = new Date(tin.StartTime.TrackedTimeAtCreation as string);
                        }
                        if (tin.EndTime !== undefined) {
                                tin.EndTime.TrackedTimeForView = new Date(tin.EndTime.TrackedTimeForView as string);
                                tin.EndTime.TrackedTimeAtCreation = new Date(tin.EndTime.TrackedTimeAtCreation as string);
                        }
                    });
                }
            });
        }
    }

    public updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged = (): void => {
        if (currentApp.clientData.TimeTables !== undefined) {
            let selectedTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                if (t.TimeTableAndWorkUnits) {
                    return t.TimeTableAndWorkUnits.TimeTableId == currentApp.clientData.SelectedTimeTableId;
                }
                return false;
            });
            currentApp.clientData.SelectedTimeTable = selectedTable;
    
            // map time tables
            currentClient.viewModel.updateTimeTablesMapping(currentApp.clientData.TimeTables);
            
            // map selected time table work units
            if (currentApp.clientData.SelectedTimeTable !== undefined 
                && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits !== undefined 
                && currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                    currentClient.updateWorkUnitMappingForSelectedTable();
                    if (currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits.length > 0) {
                        // TODO audit
                        currentTableOfContents.viewModel.updateTreeForActiveSection(currentApp.clientData.SelectedTimeTableId.toString()); //currentTableOfContents.viewModel.createSectionNumberingWorkUnit(currentApp.clientData.SelectedTimeTableId, currentApp.clientData.SelectedTimeTable.TimeTableAndWorkUnits.WorkUnits[0].WorkUnitId));
                    }                
            }
            else {
                currentClient.updateWorkUnitMapping([]);
            }

            currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId); // TODO should this come after update tree?
        }
        if (currentApp.clientData.UnboundTimeStamps !== undefined) {
            currentApp.clientData.UnboundTimeStamps.map(t => {
                t.TrackedTimeForView = new Date(t.TrackedTimeForView as string);
                t.TrackedTimeAtCreation = new Date(t.TrackedTimeAtCreation as string);
            });
            currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
        }
    }

    public UpdateData = (response: TrackerClientViewModel, preventRenderChainedCalls: boolean = false): void => {
        // incremental update, use data from response to update native client data store (conversions for dates, object references etc.)
        let isValidResponse: boolean = currentClient.dataResponseCheck(response);
        if (!isValidResponse) {
            return;
        }
        // TODO can return 404 not found when deleting already deleted objects => page reload
        // TODO selected table change must be tested properly (breakpoint in controller actions and execute 2nd ui action)
        // TODO other constraints like "selected table may not change"?
        // TODO refactor client view model to not contain multiple references to same object => selected time table should be timetables[index]
        // TODO test every client interaction with a delay and select table in between
        // temporary solution: reset object references => leads to different bugs
        let isUpdateSuccess = false;
        switch (response.TrackerEvent) {
            case TrackerEvent.CreateTimeStamp:
                if (response.TimeStamp !== undefined && currentApp.clientData.UnboundTimeStamps !== undefined) {
                    if (currentApp.clientData.UnboundTimeStamps.findIndex(t => response.TimeStamp !== undefined && t.TimeStampId == response.TimeStamp.TimeStampId) == -1) { // TODO workaround to prevent usage of cached response on ipad
                        response.TimeStamp.TrackedTimeForView = new Date(response.TimeStamp.TrackedTimeForView as string);
                        response.TimeStamp.TrackedTimeAtCreation = new Date(response.TimeStamp.TrackedTimeAtCreation as string);
                        currentApp.clientData.UnboundTimeStamps.push(response.TimeStamp);
                        currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.CreateTimeStampManually:
                if (response.TimeStamp !== undefined && currentApp.clientData.UnboundTimeStamps !== undefined) {
                    if (currentApp.clientData.UnboundTimeStamps.findIndex(t => response.TimeStamp !== undefined && t.TimeStampId == response.TimeStamp.TimeStampId) == -1) { // TODO workaround to prevent usage of cached response on ipad
                        response.TimeStamp.TrackedTimeForView = new Date(response.TimeStamp.TrackedTimeForView as string);
                        response.TimeStamp.TrackedTimeAtCreation = new Date(response.TimeStamp.TrackedTimeAtCreation as string);
                        currentApp.clientData.UnboundTimeStamps.push(response.TimeStamp);
                        currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.DuplicateTimeStamp:
                if (response.TimeStamp !== undefined
                    && currentApp.clientData.UnboundTimeStamps !== undefined) {
                    response.TimeStamp.TrackedTimeForView = new Date(response.TimeStamp.TrackedTimeForView as string);
                    response.TimeStamp.TrackedTimeAtCreation = new Date(response.TimeStamp.TrackedTimeAtCreation as string);
                    currentApp.clientData.UnboundTimeStamps.push(response.TimeStamp);
                    currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
                    isUpdateSuccess = true;
                }
                break;
            case TrackerEvent.RemoveTimeStamp:
                if (currentApp.clientData.UnboundTimeStamps !== undefined) {
                    let removeIndex = currentApp.clientData.UnboundTimeStamps.findIndex(timeStamp => timeStamp.TimeStampId == response.TargetId);
                    currentApp.clientData.UnboundTimeStamps.splice(removeIndex, 1);
                    currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
                    isUpdateSuccess = true;
                }
                break;
            case TrackerEvent.CreateWorkUnit:
                if (response.WorkUnit !== undefined
                    && currentApp.clientData.TimeTables !== undefined) {
                    // find
                    let targetTableId: number = response.WorkUnit.TimeTableId;
                    let targetWorkUnitId: number = response.WorkUnit.WorkUnitId;
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.TimeTableId == targetTableId);
                    // update 
                    if (targetTable !== undefined && targetTable.TimeTableAndWorkUnits !== undefined && targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        if (targetTable.TimeTableAndWorkUnits.WorkUnits.findIndex(w => response.WorkUnit !== undefined && w.WorkUnitId == response.WorkUnit.WorkUnitId) == -1) { // TODO workaround to prevent usage of cached response on ipad // TODO needed for edit actions as well?
                            targetTable.TimeTableAndWorkUnits.WorkUnits.push(response.WorkUnit);
                            currentClient.viewModel.updateTimeTablesMapping(currentClient.viewModel.timeTables);
                            currentClient.updateWorkUnitMappingForSelectedTable();
                            currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                            isUpdateSuccess = true;
                        }
                    }
                }
                break;
            case TrackerEvent.RemoveWorkUnit:
                if (response.TargetId != 0
                    && currentApp.clientData.TimeTables !== undefined) {
                    // find
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                        if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                            let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(w => w.WorkUnitId == response.TargetId)
                            if (currentWorkUnitIndex != -1) {
                                targetWorkUnitIndex = currentWorkUnitIndex;
                                return true;
                            }
                        }
                        return false;
                    });
                    // update
                    if (targetTable !== undefined &&
                        targetTable.TimeTableAndWorkUnits !== undefined &&
                        targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        if (targetWorkUnitIndex > -1) {
                            targetTable.TimeTableAndWorkUnits.WorkUnits.splice(targetWorkUnitIndex, 1);
                            currentClient.viewModel.updateTimeTablesMapping(currentClient.viewModel.timeTables);
                            currentClient.updateWorkUnitMappingForSelectedTable();
                            currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                            isUpdateSuccess = true;
                        }
                    }
                }
                break;
            case TrackerEvent.EditWorkUnit:
                if (response.UpdatedName !== undefined
                    && response.TargetId != 0 
                    && currentApp.clientData.TimeTables !== undefined) {
                    // find
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                        if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                            let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(w => w.WorkUnitId == response.TargetId)
                            if (currentWorkUnitIndex != -1) {
                                targetWorkUnitIndex = currentWorkUnitIndex;
                                return true;
                            }
                        }
                        return false;
                    });
                    // update
                    if (targetTable !== undefined &&
                        targetTable.TimeTableAndWorkUnits !== undefined &&
                        targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        let targetWorkUnit: WorkUnit | undefined = targetTable.TimeTableAndWorkUnits.WorkUnits[targetWorkUnitIndex];
                        targetWorkUnit.Name = response.UpdatedName;
                        currentClient.viewModel.updateTimeTablesMapping(currentClient.viewModel.timeTables);
                        currentClient.updateWorkUnitMappingForSelectedTable();
                        currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                        isUpdateSuccess = true;
                    }
                }
                currentClient.viewModel.currentEditMode = ClientEditMode.None; // TODO client edit mode must reset everywhere on failure as well
                break;
            case TrackerEvent.EditTimeStamp:
                // TODO notify user of invalid/ambiguous times
                // TODO duplicate code with addTimeStamp (partially)
                // TODO swap time stamp order + update IDs if required/notified
                if (response.TimeStamp !== undefined) {
                    response.TimeStamp.TrackedTimeForView = new Date(response.TimeStamp.TrackedTimeForView as string);
                    response.TimeStamp.TrackedTimeAtCreation = new Date(response.TimeStamp.TrackedTimeAtCreation as string);
                    // find + update timestamp: timestamp can be bound or unbound
                    let targetTimeStampId: number = response.TimeStamp.TimeStampId;
                    let targetWorkUnitIndex: number = -1;
                    let isTimeStampFound: boolean = false;
                    let targetTimeNorm: TimeNorm | undefined;
                    let targetWorkUnit: WorkUnit | undefined;
                    if (currentApp.clientData.TimeTables !== undefined) {
                        let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                            if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                                let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(wu => {
                                    if (wu.TimeNorms !== undefined) {
                                        let currentTimeNormIndex: number = -1
                                        wu.TimeNorms.map((tin: TimeNorm, index: number) => {
                                            if (!isTimeStampFound) {
                                                if (tin.StartTimeId == targetTimeStampId) {
                                                    tin.StartTime = response.TimeStamp;
                                                    isTimeStampFound = true;
                                                    currentTimeNormIndex = index;
                                                }
                                                else if (tin.EndTimeId == targetTimeStampId) {
                                                    tin.EndTime = response.TimeStamp;
                                                    isTimeStampFound = true;
                                                    currentTimeNormIndex = index;
                                                }
                                            }
                                        });
                                        if (isTimeStampFound) {
                                            targetTimeNorm = wu.TimeNorms[currentTimeNormIndex];
                                            return true;
                                        }
                                    }
                                    return false;
                                });
                                if (currentWorkUnitIndex != -1) {
                                    targetWorkUnitIndex = currentWorkUnitIndex;
                                    targetWorkUnit = t.TimeTableAndWorkUnits.WorkUnits[currentWorkUnitIndex];
                                    return true;
                                }
                            }
                            return false;
                        });
                        // update timenorm manually where could be affected by timestamp change if found in table // TODO documentation
                        if (isTimeStampFound && targetTimeNorm !== undefined && response.TimeNormNoChildren !== undefined /*TODO test parallelization issues => TimeNorm changed in the meantime?*/) {
                            if (response.WorkUnitDurationString !== undefined && targetWorkUnit !== undefined) {
                                targetWorkUnit.DurationString = response.WorkUnitDurationString;
                            }
                            if (response.TimeNormDurationString !== undefined) {
                                targetTimeNorm.DurationString = response.TimeNormDurationString;
                            }
                            currentClient.updateWorkUnitMappingForSelectedTable();
                            isUpdateSuccess = true;
                        }
                    }

                    if (!isTimeStampFound
                        && currentApp.clientData.UnboundTimeStamps !== undefined) {
                        let targetTimeStampIndex: number = currentApp.clientData.UnboundTimeStamps.findIndex(t => t.TimeStampId == targetTimeStampId);
                        if (targetTimeStampIndex != -1) {
                            currentApp.clientData.UnboundTimeStamps[targetTimeStampIndex] = response.TimeStamp;
                            currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
                            isUpdateSuccess = true;
                        }
                    }
                }
                break;
            case TrackerEvent.EditTimeNorm:
                if (response.TimeNormNoChildren !== undefined && 
                    currentApp.clientData.TimeTables !== undefined) {
                    // find
                    let targetTimeNormId: number = response.TimeNormNoChildren.TimeNormId;
                    let targetTimeNormIndex: number = -1;
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                        if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                            let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(wu => {
                                if (wu.TimeNorms !== undefined) {
                                    let currentTimeNormIndex: number = wu.TimeNorms.findIndex(tin => tin.TimeNormId == targetTimeNormId);
                                    if (currentTimeNormIndex != -1) {
                                        targetTimeNormIndex = currentTimeNormIndex;
                                        return true;
                                    }
                                }
                                return false;
                            });
                            if (currentWorkUnitIndex != -1) {
                                targetWorkUnitIndex = currentWorkUnitIndex;
                                return true;
                            }
                        }
                        return false;
                    });
                    let targetTimeNorm: TimeNorm | undefined = undefined;
                    if (targetTable !== undefined && targetTable.TimeTableAndWorkUnits !== undefined && targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        let targetWorkUnit: WorkUnit | undefined = targetTable.TimeTableAndWorkUnits.WorkUnits[targetWorkUnitIndex];
                        if (targetWorkUnit !== undefined && targetWorkUnit.TimeNorms !== undefined) { // TODO can arrays ever be undefined...
                            targetTimeNorm = targetWorkUnit.TimeNorms[targetTimeNormIndex];
                        }
                    }
                    // update
                    if (targetTimeNorm !== undefined) {
                        targetTimeNorm.Name = response.TimeNormNoChildren.Name;
                        targetTimeNorm.ColorR = response.TimeNormNoChildren.ColorR;
                        targetTimeNorm.ColorG = response.TimeNormNoChildren.ColorG;
                        targetTimeNorm.ColorB = response.TimeNormNoChildren.ColorB;
                        currentClient.updateWorkUnitMappingForSelectedTable();
                        currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                        isUpdateSuccess = true;
                    }
                }
                currentClient.viewModel.currentEditMode = ClientEditMode.None; // TODO
                break;
            case TrackerEvent.UnbindTimeNorm:
                if (response.TargetId != 0
                    && currentApp.clientData.TimeTables !== undefined
                    && response.UnboundTimeStamps !== undefined) {
                    // find
                    let targetTimeNormIndex: number = -1;
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                        if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                            let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(wu => {
                                if (wu.TimeNorms !== undefined) {
                                    let currentTimeNormIndex: number = wu.TimeNorms.findIndex(tin => tin.TimeNormId == response.TargetId);
                                    if (currentTimeNormIndex != -1) {
                                        targetTimeNormIndex = currentTimeNormIndex;
                                        return true;
                                    }
                                }
                                return false;
                            });
                            if (currentWorkUnitIndex != -1) {
                                targetWorkUnitIndex = currentWorkUnitIndex;
                                return true;
                            }
                        }
                        return false;
                    });
                    let targetWorkUnit: WorkUnit | undefined = undefined;
                    if (targetTable !== undefined && targetTable.TimeTableAndWorkUnits !== undefined && targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        targetWorkUnit = targetTable.TimeTableAndWorkUnits.WorkUnits[targetWorkUnitIndex];
                    }
                    // update  
                    if (targetWorkUnit !== undefined && targetWorkUnit.TimeNorms !== undefined) {
                        if (response.WorkUnitDurationString !== undefined) {
                            targetWorkUnit.DurationString = response.WorkUnitDurationString;
                        }
                        targetWorkUnit.TimeNorms.splice(targetTimeNormIndex, 1);
                        currentClient.viewModel.updateTimeTablesMapping(currentClient.viewModel.timeTables);
                        currentClient.updateWorkUnitMappingForSelectedTable();
                        currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                        currentApp.clientData.UnboundTimeStamps = response.UnboundTimeStamps;
                        currentClient.viewModel.updateUnboundTimeStampMapping(currentApp.clientData.UnboundTimeStamps);
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.EditTimeTable:
                if (response.UpdatedName !== undefined
                    && response.TargetId != 0
                    && currentApp.clientData.TimeTables !== undefined) {
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.TimeTableId == response.TargetId);
                    if (targetTable !== undefined && targetTable.TimeTableAndWorkUnits !== undefined) {
                        targetTable.TimeTableAndWorkUnits.Name = response.UpdatedName;
                        currentClient.viewModel.updateTimeTablesMapping(currentClient.viewModel.timeTables);
                        currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                        isUpdateSuccess = true;
                    }
                }
                currentClient.viewModel.currentEditMode = ClientEditMode.None;
                break;
            case TrackerEvent.SetDefaultTargetWorkUnit:
                currentApp.clientData.TargetWorkUnitId = response.TargetWorkUnitId;
                isUpdateSuccess = true;
                break;
            case TrackerEvent.EditProductivityRating:
                // target ID is timenorm id that owns productivity rating // TODO document
                if (response.UpdatedRating !== undefined
                    && response.TargetId != 0
                    && currentApp.clientData.TimeTables !== undefined) {
                    // find
                    let targetTimeNormId: number = response.TargetId;
                    let targetTimeNormIndex: number = -1;
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                        if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                            let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(wu => {
                                if (wu.TimeNorms !== undefined) {
                                    let currentTimeNormIndex: number = wu.TimeNorms.findIndex(tin => tin.TimeNormId == targetTimeNormId);
                                    if (currentTimeNormIndex != -1) {
                                        targetTimeNormIndex = currentTimeNormIndex;
                                        return true;
                                    }
                                }
                                return false;
                            });
                            if (currentWorkUnitIndex != -1) {
                                targetWorkUnitIndex = currentWorkUnitIndex;
                                return true;
                            }
                        }
                        return false;
                    });
                    let targetTimeNorm: TimeNorm | undefined = undefined;
                    if (targetTable !== undefined && targetTable.TimeTableAndWorkUnits !== undefined && targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        let targetWorkUnit: WorkUnit | undefined = targetTable.TimeTableAndWorkUnits.WorkUnits[targetWorkUnitIndex];
                        if (targetWorkUnit !== undefined && targetWorkUnit.TimeNorms !== undefined) { // TODO can arrays ever be undefined...
                            targetTimeNorm = targetWorkUnit.TimeNorms[targetTimeNormIndex];
                        }
                    }
                    // update
                    if (targetTimeNorm !== undefined) {
                        targetTimeNorm.ProductivityRatingId = response.UpdatedRating.ProductivityRatingId;
                        targetTimeNorm.ProductivityRating = response.UpdatedRating;
                        currentClient.updateWorkUnitMappingForSelectedTable();
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.CreateTimeTable:
                if (response.TimeTable !== undefined && response.TimeTable.TimeTableAndWorkUnits !== undefined && currentApp.clientData.TimeTables !== undefined) {
                    if (currentApp.clientData.TimeTables.findIndex(t => response.TimeTable !== undefined && response.TimeTable.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.TimeTableId == response.TimeTable.TimeTableAndWorkUnits.TimeTableId) == -1) { // TODO workaround to prevent usage of cached response on ipad
                        currentApp.clientData.TimeTables.push(response.TimeTable);
                        currentApp.client.viewModel.updateTimeTablesMapping(currentApp.clientData.TimeTables);
                        currentClient.changeSelectedTimeTableAndUpdateTableOfContents(response.TimeTable.TimeTableAndWorkUnits.TimeTableId);
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.RemoveTimeTable:
                if (currentApp.clientData.TimeTables !== undefined) {
                    var removedTableIndex = currentApp.clientData.TimeTables.findIndex(t => {
                        if (t.TimeTableAndWorkUnits) {
                            return t.TimeTableAndWorkUnits.TimeTableId == response.TargetId;
                        }
                        return false;
                    });
                    if (removedTableIndex > -1) {
                        currentApp.clientData.TimeTables.splice(removedTableIndex, 1);
                        currentApp.client.viewModel.updateTimeTablesMapping(currentApp.clientData.TimeTables);
                        // TODO expected timetables > 0 always
                        if (currentApp.clientData.TimeTables.length > 0) {
                            let newDefaultTable = currentApp.clientData.TimeTables[0];
                            if (newDefaultTable.TimeTableAndWorkUnits !== undefined) {
                                currentClient.changeSelectedTimeTableAndUpdateTableOfContents(newDefaultTable.TimeTableAndWorkUnits.TimeTableId);
                            }
                        }
                        else {
                            // TODO error
                        }
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.MoveWorkUnit:
                if (response.TimeTable !== undefined
                    && response.TimeTable.TimeTableAndWorkUnits !== undefined
                    && currentApp.clientData.TimeTables !== undefined) {
                    currentClient.convertTableDataForClient(response.TimeTable);
                    let firstTableId: number = response.TimeTable.TimeTableAndWorkUnits.TimeTableId;
                    let firstTableIndex: number = currentApp.clientData.TimeTables.findIndex(t => t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.TimeTableId == firstTableId);
                    if (firstTableIndex != -1) {
                        currentApp.clientData.TimeTables[firstTableIndex] = response.TimeTable;
                        if (response.TimeTableSecondary !== undefined) {
                            // moved to different table
                            currentClient.convertTableDataForClient(response.TimeTableSecondary);
                            if (response.TimeTableSecondary.TimeTableAndWorkUnits !== undefined) {
                                let secondTableId: number = response.TimeTableSecondary.TimeTableAndWorkUnits.TimeTableId;
                                let secondTableIndex: number = currentApp.clientData.TimeTables.findIndex(t => t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.TimeTableId == secondTableId);
                                if (secondTableIndex != -1) {
                                    currentApp.clientData.TimeTables[secondTableIndex] = response.TimeTableSecondary;
                                }
                            }
                        }
                        currentClient.updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged();
                        currentClient.updateWorkUnitMappingForSelectedTable();
                        currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                        isUpdateSuccess = true;
                    }
                }
                break;
            case TrackerEvent.SetDeviceTimeZone:
                if (response.SelectedTimeZoneIdTimeStamps !== undefined && response.SelectedTimeZoneIdTimeStamps !== "") {
                    currentApp.clientData.SelectedTimeZoneIdTimeStamps = response.SelectedTimeZoneIdTimeStamps;
                    currentClient.viewModel.setTimeZone(response.SelectedTimeZoneIdTimeStamps, TimeZoneSetting.Device);
                    isUpdateSuccess = true;
                }
                break;
            case TrackerEvent.SetViewTimeZone:
                if (response.SelectedTimeZoneIdView !== undefined && 
                    response.SelectedTimeZoneIdView !== "" &&
                    response.TimeTables !== undefined && 
                    response.UnboundTimeStamps !== undefined) {
                    currentApp.clientData.SelectedTimeZoneIdView = response.SelectedTimeZoneIdView;
                    currentClient.viewModel.setTimeZone(response.SelectedTimeZoneIdView, TimeZoneSetting.View);
                    currentApp.clientData.TimeTables = response.TimeTables;
                    currentApp.clientData.TimeTables.map(t => currentClient.convertTableDataForClient(t));
                    currentApp.clientData.UnboundTimeStamps = response.UnboundTimeStamps;
                    currentClient.updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged();
                    currentApp.clientData.ClientTimelineOffset = response.ClientTimelineOffset; // TODO check if required also when changing device time zone
                    isUpdateSuccess = true;
                }
                break;
            case TrackerEvent.RemoveTimeNorm:
                if (response.TargetId != 0
                    && currentApp.clientData.TimeTables !== undefined) {
                    // find
                    let targetTimeNormId: number = response.TargetId;
                    let targetTimeNormIndex: number = -1;
                    let targetWorkUnitIndex: number = -1;
                    let targetTable: TimeTableViewModel | undefined = currentApp.clientData.TimeTables.find(t => {
                        if (t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                            let currentWorkUnitIndex: number = t.TimeTableAndWorkUnits.WorkUnits.findIndex(wu => {
                                if (wu.TimeNorms !== undefined) {
                                    let currentTimeNormIndex: number = wu.TimeNorms.findIndex(tin => tin.TimeNormId == targetTimeNormId);
                                    if (currentTimeNormIndex != -1) {
                                        targetTimeNormIndex = currentTimeNormIndex;
                                        return true;
                                    }
                                }
                                return false;
                            });
                            if (currentWorkUnitIndex != -1) {
                                targetWorkUnitIndex = currentWorkUnitIndex;
                                return true;
                            }
                        }
                        return false;
                    });
                    let targetWorkUnit: WorkUnit | undefined = undefined;
                    if (targetTable !== undefined && targetTable.TimeTableAndWorkUnits !== undefined && targetTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                        targetWorkUnit = targetTable.TimeTableAndWorkUnits.WorkUnits[targetWorkUnitIndex];
                    }
                    // update
                    if (targetWorkUnit !== undefined && targetWorkUnit.TimeNorms !== undefined) {
                        targetWorkUnit.TimeNorms.splice(targetTimeNormIndex, 1);
                        targetWorkUnit.DurationString = response.WorkUnitDurationString !== undefined ? response.WorkUnitDurationString : "";
                        isUpdateSuccess = true;
                        currentClient.updateWorkUnitMappingForSelectedTable();
                        currentTableOfContents.viewModel.updateTableOfContents(currentApp.clientData.TimeTables, currentApp.clientData.SelectedTimeTableId);
                        break;    
                    }
                }
                break;
            case TrackerEvent.SetCulture:
                if (response.TimeTables !== undefined &&
                    response.UnboundTimeStamps !== undefined &&
                    response.WeekDayLetters !== undefined &&
                    response.StartDayOfWeekIndex !== undefined &&
                    response.AbbreviatedMonths !== undefined) {
                    currentClient.updateCultureAfterDataChanged(response.WeekDayLetters, response.StartDayOfWeekIndex, response.AbbreviatedMonths);
                    currentApp.clientData.TimeTables = response.TimeTables;
                    currentApp.clientData.TimeTables.map(t => currentClient.convertTableDataForClient(t));
                    currentApp.clientData.UnboundTimeStamps = response.UnboundTimeStamps;
                    currentApp.clientData.SelectedTimeTable = currentApp.clientData.TimeTables.find(t => (t.TimeTableAndWorkUnits !== undefined) && (t.TimeTableAndWorkUnits.TimeTableId == currentApp.clientData.SelectedTimeTableId)); // TODO audit
                    currentClient.updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged();
                    currentClient.changeSelectedTimeTableAndUpdateTableOfContents(currentApp.clientData.SelectedTimeTableId);
                    isUpdateSuccess = true;
                }
                break;
            case TrackerEvent.ProcessTimeStamps:
                if (response.TimeTables !== undefined &&
                    response.UnboundTimeStamps !== undefined &&
                    response.TargetWorkUnitId !== 0) { // TODO this function is not really safe as data could have changed in the meantime
                    currentApp.clientData.TimeTables = response.TimeTables;
                    currentApp.clientData.TimeTables.map(t => currentClient.convertTableDataForClient(t));
                    currentApp.clientData.UnboundTimeStamps = response.UnboundTimeStamps;
                    currentApp.clientData.SelectedTimeTable = currentApp.clientData.TimeTables.find(t => (t.TimeTableAndWorkUnits !== undefined) && (t.TimeTableAndWorkUnits.TimeTableId == currentApp.clientData.SelectedTimeTableId)); // TODO audit
                    if (response.WorkUnitDurationString !== undefined) {
                        let isDurationStringFound: boolean = false;
                        currentApp.clientData.TimeTables.map(t => {
                            if (!isDurationStringFound && t.TimeTableAndWorkUnits !== undefined && t.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                                for (let i = 0; i < t.TimeTableAndWorkUnits.WorkUnits.length; i++) {
                                    let currentWorkUnit: WorkUnit = t.TimeTableAndWorkUnits.WorkUnits[i];
                                    if (currentWorkUnit.WorkUnitId == response.TargetWorkUnitId) {
                                        currentWorkUnit.DurationString = response.WorkUnitDurationString;
                                        isDurationStringFound = true;
                                        break;
                                    }
                                }
                            }
                        });
                    }
                    currentClient.updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged();
                    currentClient.changeSelectedTimeTableAndUpdateTableOfContents(currentApp.clientData.SelectedTimeTableId);
                    isUpdateSuccess = true;
                }
                break;
            case TrackerEvent.CreateTimeNorm:
                if (response.TimeTables !== undefined &&
                    response.UnboundTimeStamps !== undefined) { // TODO this function is not really safe as data could have changed in the meantime
                    currentApp.clientData.TimeTables = response.TimeTables;
                    currentApp.clientData.TimeTables.map(t => currentClient.convertTableDataForClient(t));
                    currentApp.clientData.UnboundTimeStamps = response.UnboundTimeStamps;
                    currentApp.clientData.TargetWorkUnitId = response.TargetWorkUnitId;
                    currentApp.clientData.SelectedTimeTable = currentApp.clientData.TimeTables.find(t => (t.TimeTableAndWorkUnits !== undefined) && (t.TimeTableAndWorkUnits.TimeTableId == currentApp.clientData.SelectedTimeTableId)); // TODO audit
                    currentClient.updateTimeTablesAndUnboundAndTableOfContentsAndSelectedAfterDataChanged();
                    currentClient.changeSelectedTimeTableAndUpdateTableOfContents(currentApp.clientData.SelectedTimeTableId);
                    isUpdateSuccess = true;
                }
                break;
            default:
                console.log(`incremental update not implemented for ${TrackerEvent[response.TrackerEvent]}`); // TODO
        }
        if (!isUpdateSuccess) {
            console.log("invalid client state"); 
            location.reload(); // TODO audit security...
        }
        // currentClient.viewModel.updateTimeTablesMapping(currentClient.viewModel.timeTables); // TODO maybe call after every update?
        currentClient.viewModel.hoveredProductivityTimeNormId = undefined; // TODO are there other such bugs
        if (!preventRenderChainedCalls) {
            currentApp.projector.scheduleRender();
        }
    };
};
