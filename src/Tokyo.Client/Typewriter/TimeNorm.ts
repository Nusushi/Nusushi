

import {TimeNormTagMapping} from "./TimeNormTagMapping";
import {TimeStamp} from "./TimeStamp";
import {ProductivityRating} from "./ProductivityRating";
import {WorkUnit} from "./WorkUnit"; 
export class TimeNorm { 
    TimeNormId: number;
    Name: string;
    WorkUnitId: number;
    WorkUnit: WorkUnit | undefined;
    StartTimeId: number | undefined;
    StartTime: TimeStamp | undefined;
    EndTimeId: number | undefined;
    EndTime: TimeStamp | undefined;
    TimeNormTagMappings: TimeNormTagMapping[] | undefined;
    ColorR: number;
    ColorG: number;
    ColorB: number;
    ProductivityRatingId: number | undefined;
    ProductivityRating: ProductivityRating | undefined;
    ManualSortOrderKey: number;
    DurationString: string;
    Norm: string;
}