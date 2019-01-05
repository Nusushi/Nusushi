

interface TrackerStore { TrackerStoreId: string; }
import {WorkUnit} from "./WorkUnit";
import {SharedTimeTableInfo} from "./SharedTimeTableInfo";
import {TimeRange} from "./TimeRange";
import {Plot} from "./Plot"; 
export class TimeTable { 
    TimeTableId: number;
    WorkUnits: WorkUnit[] | undefined;
    Name: string;
    TrackerStoreId: string;
    TrackerStore: TrackerStore | undefined;
    SharedTimeTableInfos: SharedTimeTableInfo[] | undefined;
    IsFrozen: boolean;
    TargetWeeklyTimeId: number | undefined;
    TargetWeeklyTime: TimeRange | undefined;
    Plot: Plot | undefined;
}