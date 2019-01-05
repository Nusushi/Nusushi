

interface TrackerStore { TrackerStoreId: string; }
import {WorkUnit} from "./WorkUnit";
 
export class TrackerUserDefaults { 
    TrackerUserDefaultsId: number;
    TrackerStoreId: string;
    TrackerStore: TrackerStore | undefined;
    TargetWorkUnitId: number | undefined;
    TargetWorkUnit: WorkUnit | undefined;
    TimeZoneId: string;
}