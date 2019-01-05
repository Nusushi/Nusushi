

import {TimeNorm} from "./TimeNorm"; 
export class TimeStamp { 
    TimeStampId: number;
    AbsoluteOfUtcOffset: string;
    IsNegativeUtcOffset: boolean;
    TimeZoneIdAtCreation: string;
    TrackedTimeAtCreation: Date | string
    TrackedTimeForView: Date | string
    UtcOffsetAtCreation: string;
    TimeString: string;
    DateString: string;
    Name: string;
    TrackerStoreId: string;
    BoundNormStart: TimeNorm | undefined;
    BoundNormEnd: TimeNorm | undefined;
    IsBound: boolean;
}