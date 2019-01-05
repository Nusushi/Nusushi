

import {TimeTable} from "./TimeTable";
 
export class TimeTableViewModel { 
    TableOwnerUserName: string;
    TimeTableAndWorkUnits: TimeTable | undefined;
    TimeTableDateRange: string;
    EarliestDateTimeInData: Date | string
    LatestDateTimeInData: Date | string
}