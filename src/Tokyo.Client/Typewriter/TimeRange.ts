

import {TimeTable} from "./TimeTable";
 
export class TimeRange { 
    TimeRangeId: number;
    Days: number;
    WithinDayTimeSpan: string;
    TimeTableId: number;
    TimeTable: TimeTable | undefined;
}