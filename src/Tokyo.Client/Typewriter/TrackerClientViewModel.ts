

import {TimeStamp} from "./TimeStamp";
import {TimeTableViewModel} from "./TimeTableViewModel";
import {WorkUnit} from "./WorkUnit";
import {ProductivityRating} from "./ProductivityRating";
import {TrackerTimeZone} from "./TrackerTimeZone";
import {TimeNormTag} from "./TimeNormTag";
import {TimeNorm} from "./TimeNorm";
import {TrackerCultureInfo} from "./TrackerCultureInfo"; 
export class TrackerClientViewModel { 
    StatusText: string;
    CurrentRevision: number;
    UrlToReadOnly: string;
    UrlToReadAndEdit: string;
    TrackerEvent: number;
    TargetWorkUnitId: number;
    TimeZones: TrackerTimeZone[] | undefined;
    CultureIds: TrackerCultureInfo[] | undefined;
    WeekDayLetters: string[];
    AbbreviatedMonths: string[];
    StartDayOfWeekIndex: number;
    SelectedCultureId: string;
    SelectedTimeZoneIdTimeStamps: string;
    SelectedTimeZoneIdView: string;
    TrackerClientFlags: boolean[];
    TimeTables: TimeTableViewModel[] | undefined;
    SelectedTimeTableId: number;
    SelectedTimeTable: TimeTableViewModel | undefined;
    UnboundTimeStamps: TimeStamp[] | undefined;
    TargetId: number;
    TimeTable: TimeTableViewModel | undefined;
    TimeTableSecondary: TimeTableViewModel | undefined;
    WorkUnitIdSource: number;
    WorkUnitIdTarget: number;
    WorkUnit: WorkUnit | undefined;
    TimeStamp: TimeStamp | undefined;
    TimeNormNoChildren: TimeNorm | undefined;
    UpdatedName: string;
    TimeNormTag: TimeNormTag | undefined;
    UpdatedRating: ProductivityRating | undefined;
    ClientTimelineOffset: string;
    TimeNormDurationString: string;
    WorkUnitDurationString: string;
}