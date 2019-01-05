

import {TimeNorm} from "./TimeNorm";
import {TimeTable} from "./TimeTable";
import {TrackerUserDefaults} from "./TrackerUserDefaults"; 
export class WorkUnit { 
    WorkUnitId: number;
    TimeNorms: TimeNorm[] | undefined;
    TimeTableId: number;
    TimeTable: TimeTable | undefined;
    Name: string;
    ActiveTrackerUserDefaultsId: number | undefined;
    ActiveTrackerUserDefaults: TrackerUserDefaults | undefined;
    ManualSortOrderKey: number;
    IsDisplayAgendaTimeZone: boolean;
    UtcOffsetAgenda: string;
    AbsoluteOfUtcOffsetAgenda: string;
    IsNegativeUtcOffsetAgenda: boolean;
    TimeZoneIdAgenda: string;
    DurationString: string;
    Norm: string;
}