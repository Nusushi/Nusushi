

interface TrackerStore { TrackerStoreId: string; }
import {TimeNormTagMapping} from "./TimeNormTagMapping"; 
export class TimeNormTag { 
    TimeNormTagId: number;
    Name: string;
    TimeNormTagMappings: TimeNormTagMapping[] | undefined;
    Color: string;
    TrackerStoreId: string;
    TrackerStore: TrackerStore | undefined;
}