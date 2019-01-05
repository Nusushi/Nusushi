import * as maquette from "maquette";
const h = maquette.h;
import { Timeline, TimelineTimeSpan, TimelineElement, TimelineElementMarker } from "./../Models/Timeline";
import { WorkUnit, TimeNorm } from "../Models/TokyoTrackerGenerated";
import * as timelineConstants from "./../Models/TimelineConstants";

let currentVM: TimelineVM;
export class TimelineVM {
    // properties
    public workUnits: WorkUnit[] = [];
    public activeTimeNorm: TimeNorm | undefined = undefined; // TODO unused
    public timeNormTimeSpans: TimelineTimeSpan[] = [];
    public timelineDateCenter: Date | undefined = undefined;
    public workUnitGraphDaysDifference: number;
    public workUnitGraphStartTime: Date;
    public workUnitGraphEndTime: Date;
    public timelineElements: TimelineElement[] = [];
    public timelineElementTimeSpanIndexMapping: {[key: string]: number[]} = {};
    public timelineFontSizeString: string;
    public timelineLittleElementVisibleCount: number = 0;
    public timelineMaxDayElements: number = 14; // TODO
    public timelineFixedMonthString: string | undefined = undefined;
    public isPanningRight: boolean = false;
    public isEnoughSpace: boolean = false;
    public actualSpacePx: number = 1080;
    // panning
    public timelineCursorElementIndex: number = 0;
    public timelineLastElementFactor: number = 0;
    public timelineZoomLevel: number = 6; // number expected to be -1 <= x <= MAX_ELEMENTS
    public timelineMaxZoomLevel: number = 20;
    public timelineMinZoomLevel: number = -1;
    public timelineStartZoomLevel: number = 0;
    public timelineCenterBias: number = 0;
    public timelineStartCenterBias: number = 0;
    public timelineChangeInPositionDoubleX: number = 0.0; // value is transformed to timeline center bias linearly (factor 1)
    public timelineChangeInPositionDoubleY: number = 0.0; // value is transformed to timeline center bias linearly (factor 1)
    public timelineAcceleration: number = 0.0;
    public timelineLastAccelerationDiff: number = 0.0;
    public timelineSpeed: number = 0.0;
    public timelineElapsedTimeMs: number = 0.0;
    public isTimelineInteracting: boolean = false;
    public timelineTimer: number | undefined = undefined;
    public timelinePanStartMousePositionX: number = 0;
    public timelinePanStartMousePositionY: number = 0;
    public isTimelinePanning: boolean = false;
    public displayedWeekNumber: number = 0;
    public isSensorEnabled: boolean = false;    
    // sensor panning / zooming
    public isSensorPanning: boolean = false;
    public isSensorCalibrated: boolean = false;
    public sensorDataCounter: number = 0;
    public sensorDataCounterLastZoomChange: number = 0;
    public sensorDataCounterLastBiasChange: number = 0;
    public sensorXStart: number = 0;
    public sensorYStart: number = 0;
    public sensorZStart: number = 0;
    public sensorStatus: string = "";
    public sensorXCurrent: number = 0;
    public sensorYCurrent: number = 0;
    public sensorZCurrent: number = 0;
    public sensorXPrevious: number = 0;
    public sensorYPrevious: number = 0;
    public sensorZPrevious: number = 0;
    // components
    public timelineElementMapping: maquette.Mapping<TimelineElement, { renderMaquette: () => maquette.VNode }>;
    
    constructor(timeLineArg: Timeline) {
        currentVM = this;
        let currentTime: Date = new Date(Date.now());
        this.workUnitGraphEndTime = this.addFullDaysSameTime(currentTime, +3);
        this.workUnitGraphStartTime = this.addFullDaysSameTime(currentTime, -3);
        this.workUnitGraphDaysDifference = 6;
        this.timelineElementMapping = timeLineArg.createTimelineElementMapping();
        this.timelineElementMapping.map(this.timelineElements);
    }

    get fullDaysToDisplay(): number {
        return 5 + (2 * currentVM.timelineZoomLevel);
    };

    get elementCountAwayFromCenter(): number {
        return Math.floor(this.fullDaysToDisplay / 2.0);
    };
    
    public addFullDaysSameTime = (dateTime: Date, difference: number): Date => {
        let resultDate: Date = new Date(dateTime.getTime());
        for (let i = 0; i < Math.abs(difference); i++) {
            resultDate = currentVM.addSingleDaySameTime(resultDate, difference > 0);
        }
        return resultDate;
    }

    public addFullHoursSameSubTime = (dateTime: Date, difference: number): Date => {
        let resultDate: Date = new Date(dateTime.getTime());
        for (let i = 0; i < Math.abs(difference); i++) {
            resultDate = currentVM.addSingleHourSameSubTime(resultDate, difference > 0);
        }
        return resultDate;
    }

    public addSingleHourSameSubTime = (dateTime: Date, isPositive: boolean): Date => {
        let newDateTimeNumber: number = dateTime.getTime() + (isPositive ? 1000 : -1000); // ms
        return new Date(newDateTimeNumber);
    }

    public addSingleDaySameTime = (dateTime: Date, isPositive: boolean): Date => {
        let year = dateTime.getFullYear();
        let month = dateTime.getMonth();
        let dayOfMonth = dateTime.getDate(); // TODO check every getDate call if it should not be getTime
        let hours = dateTime.getHours();
        let minutes = dateTime.getMinutes();
        let seconds = dateTime.getSeconds();
        let milliseconds = dateTime.getMilliseconds();

        let newDayOfMonth: number = dayOfMonth + (isPositive ? 1 : -1);
        let newMonth: number = month;
        let newYear: number = year;
        const MAX_DAYS_DECEMBER: number = 31;
        const MAX_MONTHS_YEAR: number = 12;
        const STARTDAY_OF_MONTH: number = 1;
        const STARTMONTH_OF_YEAR: number = 1;
        if (isPositive) {
            if (newMonth == MAX_MONTHS_YEAR) {
                if (newDayOfMonth > MAX_DAYS_DECEMBER) {
                    newDayOfMonth = STARTDAY_OF_MONTH;
                    newMonth = 1;
                    newYear = year + 1;
                }
            }
            else {
                let firstMomentOfNextMonth: Date = new Date(year, month + 1, 1, 0, 0, 0, 1);
                let maxDaysInCurrentMonth: number = new Date(firstMomentOfNextMonth.getTime() - 10).getDate();
                if (newDayOfMonth > maxDaysInCurrentMonth) {
                    newDayOfMonth = STARTDAY_OF_MONTH;
                    newMonth = month + 1;
                }
            }
        }
        else {
            if (newDayOfMonth == 0) {
                if (month == STARTMONTH_OF_YEAR) {
                    newYear = year - 1;
                    newMonth = MAX_MONTHS_YEAR;
                    newDayOfMonth = MAX_DAYS_DECEMBER;
                }
                else {
                    let firstMomentOfCurrentMonth: Date = new Date(year, month, 1, 0, 0, 0, 1);
                    let maxDaysInPreviousMonth: number = new Date(firstMomentOfCurrentMonth.getTime() - 10).getDate();
                    newMonth = month - 1;
                    newDayOfMonth = maxDaysInPreviousMonth;
                }
            }
        }
        return new Date(newYear, newMonth, newDayOfMonth, hours, minutes, seconds, milliseconds);
    }
    // TODO javascript console.assert
}