

interface TrackerStore { TrackerStoreId: string; }
import {TimeTable} from "./TimeTable"; 
export class SharedTimeTableInfo { 
    SharedTimeTableInfoId: number;
    SharedWithTrackerStoreId: string;
    SharedWithTrackerStore: TrackerStore | undefined;
    OwnerTrackerStoreId: string;
    OwnerTrackerStore: TrackerStore | undefined;
    TimeTableId: number;
    TimeTable: TimeTable | undefined;
    Name: string;
    ShareEnabledTime: Date | string
    IsReshareAllowed: boolean;
    IsEditAllowed: boolean;
}