/// <reference path="../jsx.ts" />
import { VNode, VNodeProperties } from "maquette";
declare var require: any;
var velocity: any = require("velocity-animate");
import * as maquette from "maquette";
const h = maquette.h;
import { TokyoTrackerApp, DEFAULT_EXCEPTION, parseIntFromAttribute } from "./TokyoTrackerApp";
import { TimelineVM } from "./../ViewModels/TimelineVM";
import * as timelineConstants from "./../Models/TimelineConstants";
import { WorkUnit, TimeNorm } from "./TokyoTrackerGenerated"; // TODO packaging as independent module?
import { TIMELINE_COLOR_STRING, TIMELINE_COLOR_B, TIMELINE_HEIGHT_PX_STRING, TIMELINE_WARNING_COLOR_STRING } from "./../Models/TimelineConstants";

// Timeline constants
export const TIMELINE_MIN_WIDTH: number = 480; // TODO dont download timeline when not rendered
const TIMELINE_ELEMENT_MIN_WIDTH = 20;// 22;
const TIMELINE_LITTLE_ELEMENT_MIN_WIDTH = 6;
const MAX_ELEMENTS: number = 20;
const SCROLL_UP_STEP: number = 1;
const SCROLL_DOWN_STEP: number = -1;
const ANIMATION_FRAME_DURATION = 1000 / 60.0; // TODO events have timestamps in javascript => use delta between timestamps probably more accurate
const TIMELINE_INTERACTION_TIMER_MS: number = Math.round(ANIMATION_FRAME_DURATION);
const TIMELINE_ANIMATION_IN_MS = Math.round(13 * ANIMATION_FRAME_DURATION);
const TIMELINE_ANIMATION_OUT_MS = Math.round(13 * ANIMATION_FRAME_DURATION);
const TIMELINE_MARKER_HEIGHT = 3;

export interface TimelineElement {
    index: number,
    subIndex: number,
    description: string,
    isHighlighted: boolean,
    colorR: number,
    colorG: number,
    colorB: number,
    isStartOfWeekMarker: boolean,
    weekNumber: number,
    subdescription: string,
    subColorR: number,
    subColorG: number,
    subColorB: number,
    isLittleElement: boolean,
    isVisible: boolean,
    dayOfMonthString: string
    markers: TimelineElementMarker[]
}

export interface TimelineElementMarker {
    isTimeNormStart: boolean,
    isTimeNormEnd: boolean,
    level: number,
    mappedTimeSpanIndex: number,
    uniqueId: string
}

export interface TimelineTimeSpan {
    startTimeNumber: number,
    endTimeNumber: number,
    colorR: number,
    colorB: number,
    colorG: number,
    usedLevel: number,
    timeNormId: number
}

let currentApp: TokyoTrackerApp;
let currentTimeline: Timeline;

export class Timeline {
    public viewModel: TimelineVM;
    private timelineIndex: number;
    public timelineDivElement: HTMLDivElement;

    constructor(currentAppArg: TokyoTrackerApp, timelineIndex: number) {
        currentTimeline = this;
        this.timelineIndex = timelineIndex;
        currentApp = currentAppArg;
        document.body.addEventListener("mousemove", this.interactionUpdateHandler);
        document.body.addEventListener("touchmove", this.interactionUpdateTouchHandler);
        this.viewModel = new TimelineVM(this);
        // TODO test timeline for time change during usage
    };

    public renderMaquette = () => { // TODO mouse wheel event controls zoom // TODO multi-touch // TODO bug when 1st day of month is at edge of div // TODO fast refresh => too many elements rendered
        // TODO bad animation: 1st of every month stays light blue!
        let panWarningStyles = {
            "position": "absolute",
            "top": "0",
            "left": "0",
            "margin-left": "50%",
            "color": TIMELINE_WARNING_COLOR_STRING,
            "font-size": "16px",
            "line-height": "1",
            "font-weight": "bold",
            "z-index": "3"
        };
        let isZoomUpActive = currentTimeline.viewModel.timelineZoomLevel != currentTimeline.viewModel.timelineMinZoomLevel;
        let isZoomDownActive = currentTimeline.viewModel.timelineZoomLevel != currentTimeline.viewModel.timelineMaxZoomLevel;
        let buttonGroupStylesSide = { "flex": "1 1 20px", "border-color": "white", "align-items": "center", "opacity": "0.2" }; // column flex
        return <div key={`t${currentTimeline.timelineIndex}`}
            id={`tracker-timeline${currentTimeline.timelineIndex}`}
            class="tracker-timeline"
            styles={{
                "width": "100%",
                "max-width": "100%",
                "flex": "0 0 80px", // set to fixed height to contain elements that are not rendered sometimes 
                "height": TIMELINE_HEIGHT_PX_STRING,
                "display": "flex",
                "flex-flow": "row nowrap",
                "position": "relative",
                "justify-content": "space-between",
                "overflow": "hidden", // TODO should not overflow during animations
                //"-ms-overflow-style": "none"//"-ms-autohiding-scrollbar"
                "cursor": "move",
                "min-width": "200px"
            }}
            //onscroll={currentClient.timelineScrollHandler}
            onmousedown={currentTimeline.interactionStartHandler}
            ontouchstart={currentTimeline.interactionStartTouchHandler}
            ondblclick={currentTimeline.timelineResetHandler}
            afterCreate={currentTimeline.timelineAfterCreateHandler}
            /*onmousedown={currentClient.timelinePanStartHandler}
            onmousemove={currentClient.timelinePanHandler}
            ontouchstart={currentClient.timelinePanStartTouchHandler}
            ontouchmove={currentClient.timelinePanTouchHandler}*/>
            {<div key="-1" styles={{ "position": "absolute", "left": "50%", "top": "0px", "opacity": currentTimeline.viewModel.isEnoughSpace ? "0.2" : "1.0", "background-color": currentTimeline.viewModel.isEnoughSpace ? "black" : "rgb(200,0,0)", "height": "10px", "width": "1px" }}></div>}
                {currentTimeline.viewModel.timelineElementMapping.results.map(r => r.renderMaquette())}
                {currentTimeline.viewModel.timelineFixedMonthString !== undefined ?
                    <p key="x0" 
                        lid="0"
                        vid="1"
                        hid="0"
                        enterAnimation={currentTimeline.timelineElementEnterAnimation} 
                        exitAnimation={currentTimeline.timelineElementExitAnimation}
                        styles={{"position": "absolute", "top": "63px", "left": "0px", "padding": "0", "margin-top": "-20px", "font-size": "22px", "font-weight": "400", "color": timelineConstants.TIMELINE_COLOR_STRING}}>{currentTimeline.viewModel.timelineFixedMonthString}
                </p> : undefined}
            <div key="0zp" styles={{ "position": "absolute", "right": "0", "top": "0", "height": "75px", "flex": "0 0 30px", "width": "30px", "display": "flex", "flex-flow": "column nowrap", "padding-bottom": "1px" }}>
                {isZoomUpActive ? <button key="a" type="button" class="btn btn-sm" spu="0" onclick={currentTimeline.zoomUpClickHandler} styles={buttonGroupStylesSide}>/\</button> : <button disabled key="aa" type="button" class="btn btn-sm" onclick={currentTimeline.zoomUpClickHandler} styles={buttonGroupStylesSide}>/\</button>}
                {isZoomDownActive ? <button key="b" type="button" class="btn btn-sm" spu="0" onclick={currentTimeline.zoomDownClickHandler} styles={buttonGroupStylesSide}>\/</button> : <button disabled key="bb" type="button" class="btn btn-sm" onclick={currentTimeline.zoomDownClickHandler} styles={buttonGroupStylesSide}>\/</button>}
            </div>
            {currentTimeline.viewModel.isSensorEnabled ? <p key="0sp" styles={panWarningStyles}>#{currentTimeline.viewModel.sensorDataCounter.toString()}: {currentTimeline.viewModel.sensorStatus} x:{currentTimeline.viewModel.sensorXCurrent}, y:{currentTimeline.viewModel.sensorYCurrent}, z:{currentTimeline.viewModel.sensorZCurrent}</p> : undefined /*TODO animate &gt;&gt;&gt; arrows*/}
        </div> as VNode;
    }

    public zoomUpClickHandler = (evt: MouseEvent) => {
        currentTimeline.adjustZoom(-1);
    }

    public zoomDownClickHandler = (evt: MouseEvent) => {
        currentTimeline.adjustZoom(+1);
    }

    public createTimelineElementMapping = (): maquette.Mapping<TimelineElement, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<TimelineElement, any>(
            function getSectionSourceKey(source: TimelineElement) {
                // function that returns a key to uniquely identify each item in the data
                return `${source.index},${source.subIndex}`;
            },
            function createSectionTarget(source: TimelineElement) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let isVisible = source.isVisible; // TODO do not delete items from array
                let isLittleElement = source.isLittleElement;
                let index = source.index;
                let subIndex = source.subIndex;
                let description = source.description;
                let subdescription = source.subdescription;
                let markers = source.markers;
                let isHighlighted = source.isHighlighted;
                let markerMapping: maquette.Mapping<TimelineElementMarker, { renderMaquette: () => maquette.VNode }> = currentTimeline.createTimelineMarkerMapping();
                markerMapping.map(markers);
                let dayOfMonthString: string = source.dayOfMonthString;
                let subColorString: string = `rgb(${source.subColorR},${source.subColorG},${source.subColorB}`;
                let colorString: string = `rgb(${source.colorR},${source.colorG},${source.colorB}`;
                let isStartOfWeekMarker: boolean = source.isStartOfWeekMarker;
                let weekNumber: number = source.weekNumber;
                let weekNumberOffsetString: string = currentTimeline.viewModel.timelineLittleElementVisibleCount > 0 ? "calc(50% - " + (`${source.weekNumber}`.length * -2) + "px)" : (`${source.weekNumber}`.length * -2) + "px"; /*depends on font sizing rem(horizontal, number + spacing if length > 2)*/
                return {
                    renderMaquette: function () {
                        let isMonthElement: boolean = subdescription !== "";
                        let targetWidth: number;
                        if (isVisible) {
                            if (isLittleElement) {
                                targetWidth = TIMELINE_LITTLE_ELEMENT_MIN_WIDTH;
                            }
                            else {
                                targetWidth = TIMELINE_ELEMENT_MIN_WIDTH;
                            }
                        }
                        else {
                            targetWidth = 0;
                        }
                        // TODO underline "today" user time
                        let startDayOfWeekMarkerStyles = {
                            "position": "absolute",
                            "top": "0",
                            "left": "0",
                            "margin-left": weekNumberOffsetString,
                            "height": "0.6rem",
                            "opacity": "0.4",
                            "margin-top": "0",
                            "margin-bottom": "0",
                            "font-size": "0.5rem",
                            "line-height": "0.5rem",
                            "padding-left": "3px",
                            "letter-spacing": "0.2px",
                            "background-color": "white",
                            "z-index": "1",
                            "font-color": TIMELINE_COLOR_STRING
                        };
                        let targetFlex = isVisible ? 1 : 0;
                        return <div key={`${index},${subIndex}`} styles={{
                            "position": "relative", // child element absolute
                            "flex": `${targetFlex} ${targetFlex} ${targetWidth}px`,
                            "width": targetWidth + "px",
                            "min-width": targetWidth + "px",
                            "height": "80px",
                            "text-align": "center",
                            "font-weight": "bold",
                            "line-height": "1.5",
                            "padding-top": isLittleElement ? "10px" : undefined,
                            "font-size": isLittleElement ? "10px" : currentTimeline.viewModel.timelineFontSizeString,
                            "-webkit-user-select": "none",
                            "overflow-x": "visible",
                            "overflow-y": "visible",
                            "color": colorString
                            /*TODO display as flex column nowrap*/
                        }} 
                            lid={isLittleElement ? "1" : "0"} 
                            hid={isHighlighted ? "1" : "0"} 
                            vid={isVisible ? "1" : "0"}
                            enterAnimation={currentTimeline.timelineElementEnterAnimation} 
                            exitAnimation={currentTimeline.timelineElementExitAnimation}>
                            <p key="1" styles={{"padding": "0"}}>{description/*TODO add current culture to element key and animate texts on language change*/}</p>
                            { isLittleElement ? undefined : 
                                <p key="2" styles={{ "padding": "0", "-webkit-user-select": "none", "width": "auto", "margin-top": "-20px", "font-size": "14px", "font-weight": "lighter", "color": TIMELINE_COLOR_STRING}}>{dayOfMonthString}</p>
                            }
                            { /*TODO style duplication with fixed month marker*/ isLittleElement || !isMonthElement ? undefined :
                                <p key="3" styles={{ "padding": "0", "margin-top": "-20px", "width": "auto", "font-size": "22px", "font-weight": "lighter", "color": TIMELINE_COLOR_STRING, "border-left": `solid ${TIMELINE_COLOR_STRING} 1px` }}>{subdescription}</p>
                            }
                            {markerMapping.results.map(m => m.renderMaquette())}
                            {isStartOfWeekMarker ? <p key="4" styles={startDayOfWeekMarkerStyles}>{weekNumber}</p> : undefined}
                        </div>;
                    },
                    update: function (updatedSource: TimelineElement, newIndex: number) {
                        // do nothing
                        isVisible = updatedSource.isVisible;
                        isLittleElement = updatedSource.isLittleElement;
                        index = updatedSource.index;
                        subIndex = updatedSource.subIndex;
                        description = updatedSource.description;
                        subdescription = updatedSource.subdescription;
                        isHighlighted = updatedSource.isHighlighted;
                        markers = updatedSource.markers;
                        markerMapping.map(markers);
                        dayOfMonthString = updatedSource.dayOfMonthString;
                        colorString = `rgb(${updatedSource.colorR},${updatedSource.colorG},${updatedSource.colorB}`;
                        subColorString = `rgb(${updatedSource.subColorR},${updatedSource.subColorG},${updatedSource.subColorB}`;
                        isStartOfWeekMarker = updatedSource.isStartOfWeekMarker;
                        weekNumber = updatedSource.weekNumber;
                        weekNumberOffsetString = currentTimeline.viewModel.timelineLittleElementVisibleCount > 0 ? "calc(50% - " + (`${source.weekNumber}`.length * -2) + "px)" : (`${source.weekNumber}`.length * -2) + "px"; /*depends on font sizing rem(horizontal, number + spacing if length > 2)*/
                    }
                };
            },
            function updateSectionTarget(updatedSource: TimelineElement, target: { renderMaquette(): any, update(updatedSource: TimelineElement): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    }

    public timelineAfterCreateHandler = (element: Element, projectionOptions: maquette.ProjectionOptions, vnodeSelector: string, properties: maquette.VNodeProperties, children: VNode[]) => {
        currentTimeline.timelineDivElement = element as HTMLDivElement;
    }

    public updateActiveTimeNorm = (timeNormId: number) => {
        // TODO unused
        let targetTimeNorm: TimeNorm | undefined = undefined;
        let parentWorkUnit: WorkUnit | undefined = currentTimeline.viewModel.workUnits.find(workUnit => {
            if (workUnit.TimeNorms) {
                let targetNormIndex = workUnit.TimeNorms.findIndex(t => t.TimeNormId == timeNormId);
                if (targetNormIndex > -1) {
                    targetTimeNorm = workUnit.TimeNorms[targetNormIndex];
                    return true;
                }
            }
            return false;
        });
    }

    public adjustZoom = (zoomChange: number) => {
        let newZoomLevel: number = currentTimeline.viewModel.timelineZoomLevel + zoomChange;
        if (newZoomLevel < currentTimeline.viewModel.timelineMinZoomLevel) { // TODO code duplication
            newZoomLevel = currentTimeline.viewModel.timelineMinZoomLevel;
        }
        else if (newZoomLevel > currentTimeline.viewModel.timelineMaxZoomLevel) {
            newZoomLevel = currentTimeline.viewModel.timelineMaxZoomLevel;
        }
        currentTimeline.viewModel.timelineZoomLevel = newZoomLevel;
        currentTimeline.viewModel.timelineElements = currentTimeline.createTimelineElements();
        currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
    }

    public updateWorkUnitMapping = (newWorkUnits: WorkUnit[]) => {
        currentTimeline.viewModel.workUnits = [];
        currentTimeline.viewModel.workUnits.push(...newWorkUnits.sort((a, b) => { return a.ManualSortOrderKey - b.ManualSortOrderKey; })); //TODO still required?
        currentTimeline.viewModel.activeTimeNorm = undefined;
        /*currentTimeline.viewModel.workUnits.map(workUnit => {
            if (workUnit.TimeNorms) {
                workUnit.TimeNorms.map(timeNorm => {
                    if (timeNorm.StartTime) {
                        // if (.) => min / max date from server TODO, and check for code duplication
                    }
                });
            }
        });*/
        currentTimeline.viewModel.timeNormTimeSpans = [];
        let timeSpans: TimelineTimeSpan[] = [];
        currentTimeline.viewModel.workUnits.map(w => {
            if (w.TimeNorms) {
                w.TimeNorms.map(tn => {
                    if (tn.StartTime && tn.EndTime) {
                        timeSpans.push({
                            startTimeNumber: (tn.StartTime.TrackedTimeForView as Date).getTime(),
                            endTimeNumber: (tn.EndTime.TrackedTimeForView as Date).getTime(),
                            colorR: tn.ColorR,
                            colorG: tn.ColorG,
                            colorB: tn.ColorB,
                            usedLevel: -1,
                            timeNormId: tn.TimeNormId
                        });
                    }
                })
            }
        });
        currentTimeline.viewModel.timeNormTimeSpans = timeSpans;
        currentTimeline.viewModel.timelineElements = currentTimeline.createTimelineElements();
        currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
    };

    public createTimelineMarkerMapping = (): maquette.Mapping<TimelineElementMarker, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<TimelineElementMarker, any>(
            function getSectionSourceKey(source: TimelineElementMarker) {
                // function that returns a key to uniquely identify each item in the data
                return source.uniqueId;
            },
            function createSectionTarget(source: TimelineElementMarker) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let uniqueId: string = source.uniqueId;
                let isStartMarker: boolean = source.isTimeNormStart;
                let isEndMarker: boolean = source.isTimeNormEnd;
                let level: number = source.level;
                let mappedTimeSpanIndex: number = source.mappedTimeSpanIndex;
                let mappedTimeSpan: TimelineTimeSpan = currentTimeline.viewModel.timeNormTimeSpans[source.mappedTimeSpanIndex];
                return {
                    renderMaquette: function () {
                        // TODO start/end marker
                        let targetLeftPercentage: number = 0;
                        let targetWidthPercentage: number = 100;
                        if (isStartMarker) { // TODO better marker positioning
                            if (isEndMarker) {
                                targetLeftPercentage = 25;
                                targetWidthPercentage = 50;
                            }
                            else {
                                targetLeftPercentage = 75;
                                targetWidthPercentage = 25;
                            }
                        }
                        else if (isEndMarker) {  // isEndMarker && !isStartMarker
                            targetLeftPercentage = 0;
                            targetWidthPercentage = 25;
                        }
                        let markerStyles = {
                            "position": "absolute",
                            "top": `${level * TIMELINE_MARKER_HEIGHT}px`, 
                            "left": "0px", 
                            "height": `${TIMELINE_MARKER_HEIGHT}px`, 
                            "width": "100%"
                        };
                        return <div key={`${uniqueId},${isStartMarker},${isEndMarker}`} mid={`${source.mappedTimeSpanIndex}`}
                            styles={markerStyles}>
                            {<div key="1" styles={{"position": "absolute", "left": `${targetLeftPercentage}%`, "width": `${targetWidthPercentage}%`, "height":"100%", "background-color": `rgb(${mappedTimeSpan.colorR},${mappedTimeSpan.colorG},${mappedTimeSpan.colorB}`}}></div>}
                        </div>;
                    },
                    update: function (updatedSource: TimelineElementMarker) {
                        // do nothing
                        uniqueId = updatedSource.uniqueId;
                        isStartMarker = updatedSource.isTimeNormStart;
                        isEndMarker = updatedSource.isTimeNormEnd;
                        level = updatedSource.level;
                        mappedTimeSpanIndex = updatedSource.mappedTimeSpanIndex;                        
                        mappedTimeSpan = currentTimeline.viewModel.timeNormTimeSpans[updatedSource.mappedTimeSpanIndex];
                    }
                };
            },
            function updateSectionTarget(updatedSource: TimelineElementMarker, target: { renderMaquette(): any, update(updatedSource: TimelineElementMarker): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    }

    /*public timelineScrollHandler = (evt: MouseEvent) => {
        // TODO BUGS ON IPAD: slight bump => use mousemove on touch devices or changes scrollable area composition
        let targetDiv: HTMLDivElement = evt.target as HTMLDivElement;
        if (targetDiv.scrollTop != 1) {
            let scrollDiff = Math.abs(targetDiv.scrollTop - 1);
            if (targetDiv.scrollTop < 1) {
                currentClient.viewModel.timelineScrollPosition = Math.min(currentClient.viewModel.timelineScrollPosition + scrollDiff * SCROLL_UP_STEP, MAX_ELEMENTS); // total ??? TODO
            }
            else {
                currentClient.viewModel.timelineScrollPosition = Math.max(currentClient.viewModel.timelineScrollPosition + scrollDiff * SCROLL_DOWN_STEP, -1);
                // bias calculation:
                if (currentClient.viewModel.timelineScrollPosition > 0 && currentClient.viewModel.timelineCursorElementIndex > -1) {
                    let dayElementCount: number = (currentClient.viewModel.timelineElements.length - currentClient.viewModel.timelineLittleElementCount);
                    let biasSmoothingFactor: number = Math.round(dayElementCount / 7.0);
                    let biasOffset: number = currentClient.viewModel.timelineCursorElementIndex - Math.floor(dayElementCount / 2.0);
                    let biasChange: number = Math.round(biasOffset / biasSmoothingFactor);
                    currentClient.viewModel.timelineCenterBias += biasChange;
                    currentClient.viewModel.timelineCursorElementIndex = -1; //Math.round(dayElementCount / 2.0);
                    *//*if (biasOffset > 0) {
                        currentClient.viewModel.timelineCursorElementIndex += 1;
                    }
                    else if (biasOffset < 0) {
                        currentClient.viewModel.timelineCursorElementIndex -= 1;
                    }*//*
                }
            }
        }
        targetDiv.scrollTop = 1;
        currentClient.viewModel.timelineElements = currentClient.viewModel.createTimelineElements();
        currentClient.viewModel.timelineElementMapping.map(currentClient.viewModel.timelineElements);
    };*/

    public interactionStartHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        let targetElement: HTMLElement = evt.target as HTMLElement;
        let attrOrNull = targetElement.attributes.getNamedItem("spu");
        if (attrOrNull !== null) {
            if (parseInt(attrOrNull.value) == 0) {
                // ignore when transparent zoom up or down button is pressed
                return;
            }
        }
        if (evt.buttons == 1) {
            currentTimeline.startTimelineInteraction(evt.screenX, evt.screenY);
        }
    }

    public interactionStartTouchHandler = (evt: TouchEvent) => {
        if (currentTimeline.viewModel.isTimelineInteracting) {
            evt.preventDefault();
        } 
        let touchButton = evt.touches.item(0) as Touch;
        currentTimeline.endTimelineInteraction();
        currentTimeline.startTimelineInteraction(touchButton.screenX, touchButton.screenY);
    }

    public startTimelineInteraction = (xStart: number, yStart: number) => {
        if (!currentTimeline.viewModel.isTimelineInteracting) {
            currentTimeline.viewModel.isTimelineInteracting = true;
            currentTimeline.viewModel.timelinePanStartMousePositionX = xStart;
            currentTimeline.viewModel.timelinePanStartMousePositionY = yStart;
            currentTimeline.viewModel.timelineStartZoomLevel = currentTimeline.viewModel.timelineZoomLevel;
            currentTimeline.viewModel.timelineStartCenterBias = currentTimeline.viewModel.timelineCenterBias;
            currentTimeline.viewModel.timelineTimer = setTimeout(currentTimeline.timerTimelineRenderSetupTimed, TIMELINE_INTERACTION_TIMER_MS);
            currentTimeline.viewModel.timelineAcceleration = 0.0;
            currentTimeline.viewModel.timelineSpeed = 0.0;
            currentTimeline.viewModel.timelineElapsedTimeMs = 0.0;
            currentTimeline.viewModel.timelineChangeInPositionDoubleX = 0.0;
            currentTimeline.viewModel.timelineChangeInPositionDoubleY = 0.0;
        }
        else {
            // double click
            currentTimeline.endTimelineInteraction();
        }
    };

    public interactionUpdateHandler = (evt: MouseEvent) => {
        // timeline interaction for mouse devices
        //evt.preventDefault(); //TODO test for other effects // TODO on iphone the page scrolls up and down while panning timeline left-right => disabling this event here doesn't help
        if (evt.buttons == 1 && currentTimeline.viewModel.isTimelineInteracting) {
            let xOffset: number = evt.screenX - currentTimeline.viewModel.timelinePanStartMousePositionX;
            let yOffset: number = evt.screenY - currentTimeline.viewModel.timelinePanStartMousePositionY;

            // pixel difference of interaction is mapped to zoom/pan setup in non linear way
            let elementFactor: number;
            if (currentTimeline.viewModel.timelineLittleElementVisibleCount > 0) {
                elementFactor = 3.0 / ((currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementVisibleCount) / 46); // TODO code duplication
            }
            else {
                elementFactor = 3.0 / (currentTimeline.viewModel.timelineElements.length / 46.0);
            }
            if (elementFactor != currentTimeline.viewModel.timelineLastElementFactor) {
                // 
                currentTimeline.viewModel.timelineStartCenterBias = currentTimeline.viewModel.timelineCenterBias;
                currentTimeline.viewModel.timelinePanStartMousePositionX = currentTimeline.viewModel.timelinePanStartMousePositionX + xOffset;
                xOffset = 0.0;
                currentTimeline.viewModel.timelineChangeInPositionDoubleX = 0.0;
                currentTimeline.viewModel.timelineLastElementFactor = elementFactor;
            }
            
            if (Math.abs(xOffset) > 10 && Math.abs(yOffset) > 10) {
                currentTimeline.transformPixelsToTimelineSetup(xOffset / elementFactor, yOffset);
            }
            if (Math.abs(xOffset) > 10) {
                currentTimeline.transformPixelsToTimelineSetup(xOffset / elementFactor, currentTimeline.minSignedElement(yOffset, 20));
            }
            else if (Math.abs(yOffset) > 10) {
                currentTimeline.transformPixelsToTimelineSetup(currentTimeline.minSignedElement(xOffset / elementFactor, 10), yOffset);
            }
            else {
                currentTimeline.transformPixelsToTimelineSetup(xOffset / elementFactor, yOffset);
            }
        }
        else if (evt.buttons != 1 && currentTimeline.viewModel.isTimelineInteracting) {
            currentTimeline.endTimelineInteraction(); // TODO check on touch if time is counting up / timer still running
        }
    };

    public interactionUpdateTouchHandler = (evt: TouchEvent) => {
        // timeline interaction for touch devices
        if (currentTimeline.viewModel.isTimelineInteracting) {
            evt.preventDefault();
            let touchButton = evt.touches.item(0) as Touch;
            let xOffset: number = touchButton.screenX - currentTimeline.viewModel.timelinePanStartMousePositionX;
            let yOffset: number = touchButton.screenY - currentTimeline.viewModel.timelinePanStartMousePositionY;

            // pixel difference of interaction is mapped to zoom/pan setup in non linear way
            let elementFactor: number;
            if (currentTimeline.viewModel.timelineLittleElementVisibleCount > 0) {
                elementFactor = 3.0 / ((currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementVisibleCount) / 46);
            }
            else {
                elementFactor = 3.0 / (currentTimeline.viewModel.timelineElements.length / 46.0);
            }
            if (elementFactor != currentTimeline.viewModel.timelineLastElementFactor) {
                currentTimeline.viewModel.timelineStartCenterBias = currentTimeline.viewModel.timelineCenterBias;
                currentTimeline.viewModel.timelinePanStartMousePositionX = currentTimeline.viewModel.timelinePanStartMousePositionX + xOffset;
                xOffset = 0.0;
                currentTimeline.viewModel.timelineChangeInPositionDoubleX = 0.0;
                currentTimeline.viewModel.timelineLastElementFactor = elementFactor;
            }
        
            if (Math.abs(xOffset) > 10 && Math.abs(yOffset) > 10) {
                currentTimeline.transformPixelsToTimelineSetup(currentTimeline.minSignedElement(xOffset / elementFactor, 3.0), currentTimeline.minSignedElement(yOffset / 2.0, 5));
            }
            if (Math.abs(xOffset) > 10) {
                currentTimeline.transformPixelsToTimelineSetup(xOffset / elementFactor, currentTimeline.minSignedElement(yOffset / 2.0, 5));
            }
            else if (Math.abs(yOffset) > 10) {
                currentTimeline.transformPixelsToTimelineSetup(currentTimeline.minSignedElement(xOffset / elementFactor, 3.0), yOffset / 2.0);
            }
            else {
                currentTimeline.transformPixelsToTimelineSetup(currentTimeline.minSignedElement(xOffset / elementFactor, 3.0), currentTimeline.minSignedElement(yOffset / 2.0, 5));
            }
        }
    };

    public transformPixelsToTimelineSetup = (xOffset: number, yOffset: number) => {
        currentTimeline.viewModel.timelineChangeInPositionDoubleX = xOffset / 5.0;

        /*
        if (Math.min(Math.abs(xOffset), 30) > Math.abs(yOffset)) {
            currentTimeline.viewModel.timelineChangeInPositionDoubleY = yOffset / 1000.0;
        }
        else {
            currentTimeline.viewModel.timelineChangeInPositionDoubleY = yOffset / 30.0;
        }*/

        
        return;
        /* TODO
        let accelerationDiff: number = Math.floor(yOffset / 15.0);
        if ((currentTimeline.viewModel.timelineLastAccelerationDiff != 0.0) && (accelerationDiff > 0.0) && (accelerationDiff < currentTimeline.viewModel.timelineLastAccelerationDiff)) {
            currentTimeline.viewModel.timelinePanStartMousePositionY += yOffset;
            currentTimeline.viewModel.timelineSpeed = 0.0;
            currentTimeline.viewModel.timelineAcceleration = 0.0;
            currentTimeline.viewModel.timelineLastAccelerationDiff = 0.0;
        } 
        else if ((currentTimeline.viewModel.timelineLastAccelerationDiff != 0.0) && (accelerationDiff < 0.0) && (accelerationDiff > currentTimeline.viewModel.timelineLastAccelerationDiff)) {
            currentTimeline.viewModel.timelinePanStartMousePositionY -= yOffset;
            currentTimeline.viewModel.timelineSpeed = 0.0;
            currentTimeline.viewModel.timelineAcceleration = 0.0;
            currentTimeline.viewModel.timelineLastAccelerationDiff = 0.0;
        }
        else {
            currentTimeline.viewModel.timelineAcceleration = 3 * accelerationDiff;
            currentTimeline.viewModel.timelineLastAccelerationDiff = accelerationDiff;
        }
        console.log("accel:" + currentTimeline.viewModel.timelineAcceleration);*/
    };

    public timerTimelineRenderSetupTimed = () => {
        // compute current zoom and pan state
        if (!currentTimeline.viewModel.isTimelineInteracting) {
            if (currentTimeline.viewModel.timelineTimer !== undefined) {
                // stop timer
                clearTimeout(currentTimeline.viewModel.timelineTimer);
                currentTimeline.viewModel.timelineTimer = undefined;
            }
            return;
        }
        // panning
        let newTimelineBiasInt: number;
        if (currentTimeline.viewModel.timelineChangeInPositionDoubleX >= 0.0)
        {
            newTimelineBiasInt = Math.floor(currentTimeline.viewModel.timelineChangeInPositionDoubleX);
        }
        else 
        {
            newTimelineBiasInt = Math.round(currentTimeline.viewModel.timelineChangeInPositionDoubleX);
        }
        let targetCenterBiasLevelInt: number = currentTimeline.viewModel.timelineStartCenterBias - newTimelineBiasInt;

        let newTimelinePositionDoubleY: number = currentTimeline.viewModel.timelineChangeInPositionDoubleY;// TODO + (currentTimeline.viewModel.timelineSpeed * currentTimeline.TIMELINE_INTERACTION_TIMER_MS / 1000000.0);
        //console.log("speed:" + currentTimeline.viewModel.timelineSpeed);
        let newTimelinePositionInt: number;
        newTimelinePositionInt = Math.floor(newTimelinePositionDoubleY);
        /*if (newTimelinePositionDoubleY <= 0.0) {
            newTimelinePositionInt = Math.floor(newTimelinePositionDoubleY);
            
        }
        else{
            newTimelinePositionInt = Math.round(newTimelinePositionDoubleY);
        }*/

        let targetZoomLevelInt: number = currentTimeline.viewModel.timelineStartZoomLevel + newTimelinePositionInt;
        // number capped such that -1 <= x* <= 9, TODO
        if (targetZoomLevelInt < currentTimeline.viewModel.timelineMinZoomLevel) { // TODO code duplication
            targetZoomLevelInt = currentTimeline.viewModel.timelineMinZoomLevel;
            currentTimeline.viewModel.timelineSpeed = 0;
            currentTimeline.viewModel.timelineAcceleration = 0.0;
        }
        else if (targetZoomLevelInt > currentTimeline.viewModel.timelineMaxZoomLevel) {
            targetZoomLevelInt = currentTimeline.viewModel.timelineMaxZoomLevel;
            currentTimeline.viewModel.timelineSpeed = 0;
            currentTimeline.viewModel.timelineAcceleration = 0.0;
        }
        else {
            currentTimeline.viewModel.timelineChangeInPositionDoubleY = newTimelinePositionDoubleY;
            currentTimeline.viewModel.timelineSpeed = currentTimeline.viewModel.timelineSpeed + (currentTimeline.viewModel.timelineAcceleration * TIMELINE_INTERACTION_TIMER_MS);
        }
        //console.log("pos:" + newTimelinePositionDouble);
        //console.log("pos:" + newTimelinePositionInt);
        //console.log("zoom:" + targetZoomLevelInt);
        if ((targetZoomLevelInt != currentTimeline.viewModel.timelineZoomLevel) || (targetCenterBiasLevelInt != currentTimeline.viewModel.timelineCenterBias)) {
            
            //let timeA = performance.now();

            if ((performance.now() - currentTimeline.lastRenderTime < ANIMATION_FRAME_DURATION)) // if this is below 1 the animation queue gets messed up TODO
            {
                //console.log("GG"); TODO fix performance issue or calculation
            }
            else
            {
                if (targetZoomLevelInt != currentTimeline.viewModel.timelineZoomLevel) {
                    targetCenterBiasLevelInt = currentTimeline.viewModel.timelineCenterBias;
                }
                else if (targetCenterBiasLevelInt != currentTimeline.viewModel.timelineCenterBias) {
                    targetZoomLevelInt = currentTimeline.viewModel.timelineZoomLevel;
                    currentTimeline.viewModel.isPanningRight = targetCenterBiasLevelInt > currentTimeline.viewModel.timelineCenterBias;
                }
                currentTimeline.viewModel.timelineZoomLevel = targetZoomLevelInt;
                currentTimeline.viewModel.timelineCenterBias = targetCenterBiasLevelInt;
                currentTimeline.viewModel.timelineElements = currentTimeline.createTimelineElements();
                currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
                let timeB = performance.now();
                currentTimeline.lastRenderTime = timeB;
                currentApp.projector.scheduleRender();
            }

            //currentApp.projector.renderNow(); // TODO Forced reflow while executing JavaScript took 36ms, setTimeout' handler took 118ms
            currentTimeline.viewModel.timelineTimer = setTimeout(currentTimeline.timerTimelineRenderSetupTimed, TIMELINE_INTERACTION_TIMER_MS);
            currentTimeline.viewModel.timelineElapsedTimeMs += TIMELINE_INTERACTION_TIMER_MS;
        }
        else {
            currentTimeline.viewModel.timelineElapsedTimeMs += TIMELINE_INTERACTION_TIMER_MS;
            currentTimeline.viewModel.timelineTimer = setTimeout(currentTimeline.timerTimelineRenderSetupTimed, TIMELINE_INTERACTION_TIMER_MS);
        }
        
        //console.log("time:" + currentTimeline.viewModel.timelineElapsedTimeMs);
        //currentApp.projector.scheduleRender();
    };

    public lastRenderTime: number = performance.now();

    public minSignedElement = (currentValue: number, offset: number): number => {
        let result: number;
        if (currentValue >= 0.0)
        {
            result = (currentValue - offset);
            if (result < 0.0) {
                result = 0.0;
            }
        }
        else {
            result = (currentValue + offset);
            if (result > 0.0) {
                result = 0.0;
            }
        }
        return result;
    }

    public endTimelineInteraction = () => {
        currentTimeline.viewModel.isTimelinePanning = false;
        currentTimeline.viewModel.isTimelineInteracting = false;
        currentTimeline.viewModel.timelineLastAccelerationDiff = 0.0;
    }

    /* TODO unused
    public timelinePanStartHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        if (evt.buttons == 1) { // LMB pressed
            currentTimeline.viewModel.isTimelinePanning = true;
            let scrollElement = document.getElementById("tracker-timeline1") as HTMLDivElement;
            //currentTimeline.viewModel.timelinePanStartPanPosition = scrollElement.scrollLeft;
        }
    }

    public timelinePanHandler = (evt: MouseEvent) => {
        evt.preventDefault();
        let scrollElement = document.getElementById("tracker-timeline1") as HTMLDivElement;
        if (evt.buttons == 1 && currentTimeline.viewModel.isTimelinePanning) { // LMB pressed
            // TODO sometime parent Element is directly event target
            let mouseOffsetX: number = evt.screenX - currentTimeline.viewModel.timelinePanStartMousePositionX;
            let scrubFactor: number = Math.max(1.0, currentTimeline.viewModel.timelineElements.length / 40.0); // TODO display current value in UI
            //et targetPanPosition: number = currentTimeline.viewModel.timelinePanStartPanPosition + Math.floor(mouseOffsetX * scrubFactor);
            let elementFactor: number = (currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementVisibleCount) / 46;
            if (mouseOffsetX < (-50 + 50 * elementFactor)) {
                currentTimeline.viewModel.timelineCenterBias += 1;
                currentTimeline.viewModel.timelinePanStartMousePositionX = evt.screenX;
                currentTimeline.viewModel.timelineElements = currentTimeline.viewModel.createTimelineElements();
                currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
            }
            else if (mouseOffsetX > (50 - 50 * elementFactor)) {
                currentTimeline.viewModel.timelineCenterBias -= 1;
                currentTimeline.viewModel.timelinePanStartMousePositionX = evt.screenX;
                currentTimeline.viewModel.timelineElements = currentTimeline.viewModel.createTimelineElements();
                currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
            }
            //scrollElement.scrollLeft = targetPanPosition;
        }
        else if (!currentTimeline.viewModel.isTimelinePanning) {
            currentTimeline.viewModel.timelineCursorElementIndex = Math.floor((evt.clientX - scrollElement.offsetLeft) / scrollElement.clientWidth * (currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementVisibleCount));    
        }
    };

    public updateZoomPosition = (yOffset: number) => {
        let isUpdate: boolean = false;

        
        if (isUpdate) {
            currentTimeline.viewModel.timelineElements = currentTimeline.viewModel.createTimelineElements();
            currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
        }
    };

    public timelinePanStartTouchHandler = (evt: TouchEvent) => {
        evt.preventDefault();
        currentTimeline.viewModel.isTimelinePanning = true;
        let scrollElement = document.getElementById("tracker-timeline1") as HTMLDivElement;
        //currentTimeline.viewModel.timelinePanStartPanPosition = scrollElement.scrollLeft;
        currentTimeline.viewModel.timelinePanStartMousePositionX = (evt.touches.item(0) as Touch).screenX;
    }

    public timelinePanTouchHandler = (evt: TouchEvent) => {
        evt.preventDefault();
        let scrollElement = document.getElementById("tracker-timeline1") as HTMLDivElement;
        if (currentTimeline.viewModel.isTimelinePanning) { // LMB pressed
            // TODO sometime parent Element is directly event target
            let mouseOffsetX: number = (evt.touches.item(0) as Touch).screenX; - currentTimeline.viewModel.timelinePanStartMousePositionX;
            let scrubFactor: number = Math.max(1.0, currentTimeline.viewModel.timelineElements.length / 40.0); // TODO display current value in UI
            //let targetPanPosition: number = currentTimeline.viewModel.timelinePanStartPanPosition + Math.floor(mouseOffsetX * scrubFactor);
            let elementFactor: number = (currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementCount) / 46;
            if (mouseOffsetX < (-50 + 50 * elementFactor)) {
                currentTimeline.viewModel.timelineCenterBias += 1;
                currentTimeline.viewModel.timelinePanStartMousePositionX = (evt.touches.item(0) as Touch).screenX;;
                currentTimeline.viewModel.timelineElements = currentTimeline.viewModel.createTimelineElements();
                currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
            }
            else if (mouseOffsetX > (50 - 50 * elementFactor)) {
                currentTimeline.viewModel.timelineCenterBias -= 1;
                currentTimeline.viewModel.timelinePanStartMousePositionX = (evt.touches.item(0) as Touch).screenX;;
                currentTimeline.viewModel.timelineElements = currentTimeline.viewModel.createTimelineElements();
                currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements);
            }
            //scrollElement.scrollLeft = targetPanPosition;
        }
        else if (!currentTimeline.viewModel.isTimelinePanning) {
            currentTimeline.viewModel.timelineCursorElementIndex = Math.floor(((evt.touches.item(0) as Touch).clientX - scrollElement.offsetLeft) / scrollElement.clientWidth * (currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementCount));    
            
            //currentTimeline.viewModel.timelineCursorElementIndex = Math.floor((evt.clientX - scrollElement.offsetLeft) / scrollElement.scrollWidth * (currentTimeline.viewModel.timelineElements.length - currentTimeline.viewModel.timelineLittleElementCount));
        }
    }*/

    public timelineResetHandler  = (evt: MouseEvent) => { // TODO document? in UI
        /*let scrollElement = ((evt.target as HTMLParagraphElement).parentElement as HTMLDivElement).parentElement as HTMLDivElement;
        scrollElement.scrollLeft = Math.floor(scrollElement.scrollWidth / 2.0 - scrollElement.clientWidth / 2.0);*/
        let targetElement: HTMLElement = evt.target as HTMLElement;
        let attrOrNull = targetElement.attributes.getNamedItem("spu");
        if (attrOrNull !== null) {
            if (parseInt(attrOrNull.value) == 0) {
                // ignore when transparent zoom up or down button is pressed
                return;
            }
        }
        currentTimeline.viewModel.timelineCenterBias = 0;
        currentTimeline.viewModel.timelineElements = currentTimeline.createTimelineElements();
        currentTimeline.viewModel.timelineElementMapping.map(currentTimeline.viewModel.timelineElements); // TODO check everywhere if called unnecessary
    }

    public startMarkerEditAnimation = (timeNormId: number) => {
        let mappedTimeSpanIndex: number = currentTimeline.viewModel.timeNormTimeSpans.findIndex(w => w.timeNormId == timeNormId);
        if (mappedTimeSpanIndex == -1) {
            console.log("animation element undefined"); // TODO
            return;
        }
        let markerElements = $(currentTimeline.timelineDivElement).find(`[mid='${mappedTimeSpanIndex}']`);
        velocity.animate(markerElements, {
            opacity: 0.0,
        }, {
            duration: 500,
            easing: "ease-in-out",
            loop: true
        });
    }

    public stopMarkerEditAnimation = (timeNormId: number) => {
        let mappedTimeSpanIndex: number = currentTimeline.viewModel.timeNormTimeSpans.findIndex(w => w.timeNormId == timeNormId);
        let markerElements = $(currentTimeline.timelineDivElement).find(`[mid='${mappedTimeSpanIndex}']`);
        if (mappedTimeSpanIndex == -1) {
            console.log("animation element undefined"); // TODO
            return;
        }
        velocity.animate(markerElements, "stop", true);
        velocity.animate(markerElements, {
            opacity: 1,
        }, {
            duration: 50,
            easing: "ease-in"
        });
    }

    public timelineElementEnterAnimation = (domNode: HTMLElement, properties?: VNodeProperties) => {
        let isLittleElement: boolean = (domNode.attributes.getNamedItem("lid") as Attr).value === "1"; // TODO unsafe cast
        let isVisible: boolean = (domNode.attributes.getNamedItem("vid") as Attr).value === "1"; // TODO unsafe cast
        let isHighlightedElement: boolean = (domNode.attributes.getNamedItem("hid") as Attr).value === "1"; // TODO unsafe cast
        //velocity.animate(domNode, "stop");
        //console.log("start");
        domNode.style.flexBasis = "0px";
        domNode.style.width = "0px";
        domNode.style.minWidth = "0px";
        domNode.style.flexGrow = "0";
        domNode.style.flexShrink = "0";
        //domNode.style.color = `rgb(${timelineConstants.TIMELINE_INTERMEDIATE_COLOR_R},${timelineConstants.TIMELINE_INTERMEDIATE_COLOR_G},${timelineConstants.TIMELINE_INTERMEDIATE_COLOR_B})`;
        let targetWidth: string = "0";
        if (isVisible) {
            targetWidth = isLittleElement ? TIMELINE_LITTLE_ELEMENT_MIN_WIDTH.toString() : "auto";
        }
        velocity.animate(domNode, {
            flexGrow: isVisible ? 1 : 0,
            flexShrink: isVisible ? 1 : 0,
            flexBasis: targetWidth,
            width: targetWidth,
            minWidth: targetWidth
            },
            TIMELINE_ANIMATION_IN_MS, 
            "ease-in"/*[.71,.34,.34,.71]*//*[.89,.67,.64,.41]*//*[.58,.35,.54,.28]*//*[.45,.46,.53,.53]*/ /*[.46,.46,.03,.98]*/, function (elements: HTMLElement[]) {
                /*elements.map(el => {
                        el.style.flexGrow = "1";
                        el.style.flexShrink = "1";

                        el.style.flexBasis = "1";
                        el.style.width = "1";
                        el.style.minWidth = "1";
                });
                elements.map(el => el.style.flexShrink = "1");
                flexBasis: isLittleElement ? TIMELINE_LITTLE_ELEMENT_MIN_WIDTH : TIMELINE_ELEMENT_MIN_WIDTH; 
                width: isLittleElement ? TIMELINE_LITTLE_ELEMENT_MIN_WIDTH : TIMELINE_ELEMENT_MIN_WIDTH, 
                minWidth: isLittleElement ? TIMELINE_LITTLE_ELEMENT_MIN_WIDTH : TIMELINE_ELEMENT_MIN_WIDTH},*/
            });
        
        if (!isLittleElement) {
            domNode.style.opacity = "0";
            velocity.animate(domNode, {
                colorRed: isHighlightedElement ? timelineConstants.TIMELINE_SHADED_COLOR_R : timelineConstants.TIMELINE_COLOR_R,
                colorGreen: isHighlightedElement ? timelineConstants.TIMELINE_SHADED_COLOR_G : timelineConstants.TIMELINE_COLOR_G,
                colorBlue: isHighlightedElement ? timelineConstants.TIMELINE_SHADED_COLOR_B : timelineConstants.TIMELINE_COLOR_B
            }, { queue: false, duration: TIMELINE_ANIMATION_IN_MS, easing: "ease-in"/*[.39,.06,.41,.06]*/ /*[.58,.35,.38,.99]*//*[.1,.03,.96,1]*//*[0.94, 0.54, 0.96, 0.75]*/, complete: function (elements: Element[]) {elements.map(el => {
                // do nothing
            })} });
            
            if (isVisible) {
                velocity.animate(domNode, {
                    opacity: 1
                }, { queue: false, duration: TIMELINE_ANIMATION_IN_MS, easing: "ease-in"/*[.71,.34,.34,.71]*//*"ease-in"*//*[0.94, 0.54, 0.96, 0.75]*/ });
            }
        }

        if (domNode.children.length > 1) {
            // TODO following line was wrong for fixed month string
            //(domNode.children[1] as HTMLElement).style.color = `rgb(${timelineConstants.TIMELINE_INTERMEDIATE_COLOR_R},${timelineConstants.TIMELINE_INTERMEDIATE_COLOR_G},${timelineConstants.TIMELINE_INTERMEDIATE_COLOR_B})`;
            velocity.animate(domNode.lastElementChild, {
                colorRed: timelineConstants.TIMELINE_COLOR_R,
                colorGreen: timelineConstants.TIMELINE_COLOR_G,
                colorBlue: timelineConstants.TIMELINE_COLOR_B,
            }, { duration: TIMELINE_ANIMATION_IN_MS, easing: "ease-in"/*[.39,.06,.41,.06]*/});
        }
    }

    public timelineElementExitAnimation = (domNode: HTMLElement, removeElement: () => void, properties?: maquette.VNodeProperties) => {
        //velocity.animate(domNode, "stop", true);
        /*console.log("end");
        if (domNode.children.length > 1) {
            velocity.animate(domNode.children[1] as HTMLElement, "stop");
        }
        
        currentApp.projector.renderNow();*/
        let isLittleElement: boolean = (domNode.attributes.getNamedItem("lid") as Attr).value === "1"; // TODO unsafe cast
        velocity.animate(domNode, {opacity:  0, flexBasis: 0, width: 0, minWidth: 0, flexGrow: 0, flexShrink: 0}, {duration : TIMELINE_ANIMATION_OUT_MS, easing: "ease-out", complete: removeElement, queue: false});
        /* TODO animate month item differently 
        if (currentTimeline.viewModel.isPanningRight) {
            
        }*/        
    }

    public createTimelineElements = (): TimelineElement[] => { //TODO cache elements
        // TODO console.log("computing timeline...");
        if (currentTimeline.viewModel.workUnitGraphEndTime.getTime() < currentTimeline.viewModel.workUnitGraphStartTime.getTime()) {
            console.log("timeline end date must be later than start date");
        }
        let elements: TimelineElement[] = []; // TODO dont clear, just add/remove entries at edge
        let fullDaysToDisplay: number = currentTimeline.viewModel.fullDaysToDisplay;
        let elementCountAwayFromCenter = currentTimeline.viewModel.elementCountAwayFromCenter;
        let addLittleElements = fullDaysToDisplay < currentTimeline.viewModel.timelineMaxDayElements;
        // TODO document: timeline timezone always local device time
        let thisMoment: Date = new Date(Date.now());
        let thisMomentYear: number = thisMoment.getFullYear();
        let referenceDateTimeline = new Date(thisMomentYear, thisMoment.getMonth(), thisMoment.getDate(), 0, 0, 0, 0);
        let dayOfWeekStartOfYear = new Date(thisMomentYear, 0, 1, 0, 0, 0, 0).getDay(); // TODO easy cache
        let dateCenter = currentTimeline.viewModel.addFullDaysSameTime(referenceDateTimeline, currentTimeline.viewModel.timelineCenterBias); // TODO cache intermediate results
        currentTimeline.viewModel.timelineDateCenter = dateCenter;
        let monthCenter: number = dateCenter.getMonth();
        let dayOfWeekCenter: number = dateCenter.getDay();
        let dateCenterDaysToStartOfYear = dateCenter.getDate();
        for (let i = 0; i < monthCenter; i++) {
            let lastDayOfPreviousMonth = new Date(dateCenter.getFullYear(), i, 0, 0, 0, 0, 0);
            dateCenterDaysToStartOfYear += lastDayOfPreviousMonth.getDate(); // TODO maybe calculating with a different method is faster; cache for each year that is calculated, +-2 years at startup or background worker...
        }
        // start of year == start of week? plain difference. start of year != start of week? padding. TODO cache
        //let weekNumberCenter: number = (dateCenterDaysToStartOfYear + constantOffsetDependingOnFirstDayOfYear);// TODO check near year boundaries + dayOfWeekStartOfYear + (6 - ((dayOfWeekCenter + currentApp.clientData.StartDayOfWeekIndex) % 6)) /*"first week at start of year"*/) / 7;
        // TODO let constantOffsetDependingOnFirstDayOfYear: number = 7 + (dayOfWeekStartOfYear > 0 ? (dayOfWeekStartOfYear - currentApp.clientData.StartDayOfWeekIndex) : 6); // "what week day is it" (it's like the day of year (1...365), but the year is translated depending on what day the first day of the year is i.e.(5...370) + static 7day offset (1.1. starts with week 1)
        let constantOffsetDependingOnFirstDayOfYear: number = 7 - (dayOfWeekStartOfYear >= currentApp.clientData.StartDayOfWeekIndex ? (dayOfWeekStartOfYear - currentApp.clientData.StartDayOfWeekIndex) : ((dayOfWeekStartOfYear) - currentApp.clientData.StartDayOfWeekIndex));
        let weekNumberCenter = (dateCenterDaysToStartOfYear) + constantOffsetDependingOnFirstDayOfYear;
        console.log(weekNumberCenter);
        let splitCenter: number = currentTimeline.viewModel.timelineCenterBias;
        currentTimeline.viewModel.timelineLittleElementVisibleCount = 0;
        // TODO scroll up/down => splitMultiplier
        let activeTimeSpanIndexes: number[] = [];
        let remainingTimeSpanIndexes: number[] = [];
        let iStart: number = splitCenter - elementCountAwayFromCenter;
        let iLoopLimitOpen: number = splitCenter + elementCountAwayFromCenter + 1;
        for (let everyTimeSpanIndex = 0; everyTimeSpanIndex < currentTimeline.viewModel.timeNormTimeSpans.length; everyTimeSpanIndex++) {
            // TODO ignore reverted time spans (end <= start)
            remainingTimeSpanIndexes.push(everyTimeSpanIndex);
        }
        let preComputedTimesAfter: Date[] = [];
        let preComputedTime: Date = new Date(dateCenter.getTime());
        preComputedTimesAfter.push(preComputedTime);
        for (let i = 0; i < elementCountAwayFromCenter; i++) {
            preComputedTime = currentTimeline.viewModel.addSingleDaySameTime(preComputedTime, true);
            preComputedTimesAfter.push(preComputedTime);
        }
        preComputedTime = preComputedTimesAfter[0];
        let preComputedTimesBefore: Date[] = [];
        for (let i = 0; i < elementCountAwayFromCenter; i++) {
            preComputedTime = currentTimeline.viewModel.addSingleDaySameTime(preComputedTime, false);
            preComputedTimesBefore.push(preComputedTime);
        }
        let preComputedTimesOfElements: Date[] = [];
        let preComputedTimeNumbersOfElements: number[] = [];
        for (let i = 0; i < elementCountAwayFromCenter; i++) {
            preComputedTimesOfElements.push(preComputedTimesBefore.pop() as Date);
        }
        preComputedTimesOfElements = preComputedTimesOfElements.concat(preComputedTimesAfter);
        preComputedTimesOfElements.map(p => preComputedTimeNumbersOfElements.push(p.getTime()));

        currentTimeline.viewModel.timelineFixedMonthString = undefined;
        for (let i = iStart; i < iLoopLimitOpen; i++) {
            let weekNumberElement: number = (dateCenterDaysToStartOfYear + constantOffsetDependingOnFirstDayOfYear - (splitCenter - i));// + dayOfWeekStartOfYear + (6 - ((dayOfWeekCenter + currentApp.clientData.StartDayOfWeekIndex) % 6)) /*"first week at start of year"*/) / 7;
            let progressionDay: number = i - iStart;
            let dateElement: Date = preComputedTimesOfElements[progressionDay];
            let dateElementNumber: number = preComputedTimeNumbersOfElements[progressionDay];
            let dayOfWeek: number = dateElement.getDay(); // TODO dependent on device timezone
            let dayString: string = currentApp.clientData.WeekDayLetters[dayOfWeek];
            let isHighlightColor: boolean = (dayOfWeek == 6) || (dayOfWeek == 0);
            let dateOfElementDayOfMonth: number = dateElement.getDate();
            let dateOfElementMonth: number = dateElement.getMonth();
            let isStartOfWeekMarker: boolean = true; //TODO dayOfWeek == currentApp.clientData.StartDayOfWeekIndex || (dateOfElementMonth == 0 && dateOfElementDayOfMonth == 1);
            let isDisplayMonthOnElement: boolean = (dateOfElementDayOfMonth == 1);
            // display fixed month and/or month on element 
            let monthString: string = "";
            if (isDisplayMonthOnElement || (i == iStart)) {
                let monthNumber: number = dateOfElementMonth;
                monthString = currentApp.clientData.AbbreviatedMonths[monthNumber].toUpperCase() + "'" + dateElement.getFullYear().toString(); // TODO check browser support for toUpperCase
            }
            if (i == iStart && !isDisplayMonthOnElement) {
                // max (3 days, distance to rightmost) to next month marker
                let distanceDays: number = Math.min(3, (iLoopLimitOpen - 1) - iStart - 1);
                let datePlusDistance: Date = currentTimeline.viewModel.addFullDaysSameTime(preComputedTimesOfElements[progressionDay], distanceDays);
                let dayOfMonthPlusMaxDistance: number = datePlusDistance.getDate();
                if (dateOfElementDayOfMonth < dayOfMonthPlusMaxDistance) {
                    // no month change in range
                    currentTimeline.viewModel.timelineFixedMonthString = monthString;
                    isDisplayMonthOnElement = false;
                }
            }

            let markers: TimelineElementMarker[] = [];
            let hourStep: number;
            if (fullDaysToDisplay > 7) {
                hourStep = 4;
            }
            else if (fullDaysToDisplay > 5) {
                hourStep = 2;
            }
            else {
                hourStep = 1;
            }
            let fullHourOffset: number = 0;
            let littleElementMinimalNumber: number = dateElementNumber;

            let nextDayElementTimeNumber: number;
            if (i < (iLoopLimitOpen - 1)) {
                nextDayElementTimeNumber = preComputedTimeNumbersOfElements[progressionDay + 1] - 1; //MS
            }
            else {
                // TODO check overlaps
                nextDayElementTimeNumber = currentTimeline.viewModel.addSingleDaySameTime(preComputedTimesOfElements[progressionDay], true).getTime() + 1; //MS
            }
            let littleElementTimeWidthMs: number = hourStep * 1000 * 60 * 60;
            let littleElementMaximalNumber: number;
            if (addLittleElements) {
                littleElementMaximalNumber = dateElementNumber + littleElementTimeWidthMs;
            }
            else {
                littleElementMaximalNumber = nextDayElementTimeNumber - 1;
            }
            // first loop run targets day elements, subsequent runs target hour elements
            while (littleElementMinimalNumber < nextDayElementTimeNumber) { // TODO hightlight current hour and display time
                //TODO level handling for invisible elements
                if (fullHourOffset > 25) {
                    console.log("day has too many hours");
                    break;
                }
                markers = [];
                let removeFromActiveIndexes: number[] = [];
                let removeFromRemainingIndexes: number[] = [];
                let usedLevels: number[] = [];
                for (let activeIterator = 0; activeIterator < activeTimeSpanIndexes.length; activeIterator++) {
                    let activeTimeSpanIndex: number = activeTimeSpanIndexes[activeIterator];
                    let activeTimeSpan: TimelineTimeSpan = currentTimeline.viewModel.timeNormTimeSpans[activeTimeSpanIndex];
                    let isTimeNormEnd: boolean = littleElementMaximalNumber > activeTimeSpan.endTimeNumber;
                    markers.push({
                        isTimeNormStart: false,
                        isTimeNormEnd: isTimeNormEnd,
                        level: activeTimeSpan.usedLevel,
                        mappedTimeSpanIndex: activeTimeSpanIndex,
                        uniqueId: `${currentApp.clientData.SelectedTimeTableId.toString()},${i},${fullHourOffset},${activeTimeSpan.usedLevel},${fullDaysToDisplay},${currentTimeline.viewModel.timelineCenterBias}`
                    });
                    usedLevels.push(activeTimeSpan.usedLevel);
                    if (isTimeNormEnd) {
                        removeFromActiveIndexes.push(activeTimeSpanIndex);
                        usedLevels.splice(usedLevels.indexOf(activeTimeSpanIndex), 1);
                    }
                }
                for (let remainingIndexIterator = 0; remainingIndexIterator < remainingTimeSpanIndexes.length; remainingIndexIterator++) {
                    let currentTimeSpanIndex = remainingTimeSpanIndexes[remainingIndexIterator];
                    let timeSpan = currentTimeline.viewModel.timeNormTimeSpans[currentTimeSpanIndex];
                    let isOverlap: boolean = false;
                    let isTimeNormEnd: boolean = false;
                    let isTimeNormStart: boolean = true;
                    if ( /*case1 beginning on timeElement*/littleElementMinimalNumber <= timeSpan.startTimeNumber && littleElementMaximalNumber > timeSpan.startTimeNumber) { // TODO check <= vs <
                        isOverlap = true;
                        if (littleElementMaximalNumber > timeSpan.endTimeNumber) {
                            isTimeNormEnd = true;
                        }
                    }
                    else if (/*case2 already begun before timeline range*/i == iStart && fullHourOffset == 0 && littleElementMinimalNumber >= timeSpan.startTimeNumber && littleElementMinimalNumber < timeSpan.endTimeNumber) {
                        isOverlap = true;
                        isTimeNormStart = false;
                        // TODO compile such things automatically with all variants and select visually which one is correct (for test case that covers all corner cases)
                        if (littleElementMaximalNumber >= timeSpan.endTimeNumber) { // TODO check <= vs <
                            isTimeNormEnd = true;
                        }
                    }
                    if (isOverlap) {
                        let targetLevel: number = 0;
                        const MAX_MARKERS: number = 10000;
                        while (targetLevel < MAX_MARKERS) {
                            if (usedLevels.indexOf(targetLevel) == -1) {
                                break;
                            }
                            targetLevel++;
                        }
                        if (targetLevel == MAX_MARKERS) {
                            console.log("too many overlapping time norms");
                        }
                        timeSpan.usedLevel = targetLevel;
                        usedLevels.push(targetLevel);
                        markers.push({
                            isTimeNormStart: isTimeNormStart,
                            isTimeNormEnd: isTimeNormEnd,
                            level: targetLevel,
                            mappedTimeSpanIndex: currentTimeSpanIndex,
                            uniqueId: `${currentApp.clientData.SelectedTimeTableId.toString()},${i},${fullHourOffset},${timeSpan.usedLevel},${fullDaysToDisplay},${currentTimeline.viewModel.timelineCenterBias}`
                        });
                        if (!isTimeNormEnd) {
                            activeTimeSpanIndexes.push(currentTimeSpanIndex);
                        } // else start and end on element at the same time
                        removeFromRemainingIndexes.push(currentTimeSpanIndex);
                    }
                }
                if (fullHourOffset == 0) {
                    elements.push({
                        description: dayString/*"|"*/,
                        subdescription: isDisplayMonthOnElement ? monthString : "",
                        index: i,
                        subIndex: 0,
                        isHighlighted: isHighlightColor,
                        colorR: isHighlightColor ? timelineConstants.TIMELINE_SHADED_COLOR_R : timelineConstants.TIMELINE_COLOR_R,
                        colorG: isHighlightColor ? timelineConstants.TIMELINE_SHADED_COLOR_G : timelineConstants.TIMELINE_COLOR_G,
                        colorB: isHighlightColor ? timelineConstants.TIMELINE_SHADED_COLOR_B : timelineConstants.TIMELINE_COLOR_B,
                        isStartOfWeekMarker: isStartOfWeekMarker,
                        weekNumber: weekNumberElement,
                        subColorR: timelineConstants.TIMELINE_COLOR_R,
                        subColorG: timelineConstants.TIMELINE_COLOR_G,
                        subColorB: timelineConstants.TIMELINE_COLOR_B,
                        isLittleElement: false,
                        isVisible: true,
                        dayOfMonthString: dateOfElementDayOfMonth.toString(),
                        markers: markers
                    });
                }
                if (fullHourOffset >= hourStep && i < (iLoopLimitOpen - 1)) {
                    elements.push({
                        description: ".",
                        subdescription: "",
                        isHighlighted: false,
                        index: i, // TODO not bijective over day change
                        colorR: timelineConstants.TIMELINE_COLOR_R,
                        colorG: timelineConstants.TIMELINE_COLOR_G,
                        colorB: timelineConstants.TIMELINE_COLOR_B,
                        isStartOfWeekMarker: false,
                        weekNumber: 0,
                        subColorR: timelineConstants.TIMELINE_COLOR_R,
                        subColorG: timelineConstants.TIMELINE_COLOR_G,
                        subColorB: timelineConstants.TIMELINE_COLOR_B,
                        isLittleElement: true,
                        isVisible: addLittleElements && fullHourOffset > 0 && ((fullHourOffset % hourStep) == 0),
                        subIndex: i * 100 + fullHourOffset, // TODO max little elements
                        dayOfMonthString: "",
                        markers: markers
                    });
                    if (fullHourOffset > 0) {
                        currentTimeline.viewModel.timelineLittleElementVisibleCount += 1;
                    }
                }
                for (let removeIterator = 0; removeIterator < removeFromRemainingIndexes.length; removeIterator++) {
                    remainingTimeSpanIndexes.splice(remainingTimeSpanIndexes.indexOf(removeFromRemainingIndexes[removeIterator]), 1);
                }
                for (let removeIterator = 0; removeIterator < removeFromActiveIndexes.length; removeIterator++) {
                    activeTimeSpanIndexes.splice(activeTimeSpanIndexes.indexOf(removeFromActiveIndexes[removeIterator]), 1);
                }
                littleElementMinimalNumber += littleElementTimeWidthMs;
                littleElementMaximalNumber += littleElementTimeWidthMs;
                fullHourOffset += hourStep;
                if (!addLittleElements) {
                    break;
                }
            }
        }

        currentTimeline.viewModel.timelineFontSizeString = "20px";
        /*if (elements.length > 42) {
            currentVMtimelineFontSizeString = "22px";
        }
        else {
            currentVMtimelineFontSizeString = "33px";
        }*/
        return elements;
    };
}