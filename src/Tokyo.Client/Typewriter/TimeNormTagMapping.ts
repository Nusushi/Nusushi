

import {TimeNorm} from "./TimeNorm";
import {TimeNormTag} from "./TimeNormTag"; 
export class TimeNormTagMapping { 
    TimeNormTagMappingId: number;
    TimeNormId: number;
    TimeNorm: TimeNorm | undefined;
    TimeNormTagId: number;
    TimeNormTag: TimeNormTag | undefined;
}