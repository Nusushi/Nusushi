

import {TimeTable} from "./TimeTable"; 
export class Plot { 
    PlotId: number;
    Name: string;
    ImageData: number[];
    TimeTableId: number;
    TimeTable: TimeTable | undefined;
    MediaType: string;
    Version: number[];
}