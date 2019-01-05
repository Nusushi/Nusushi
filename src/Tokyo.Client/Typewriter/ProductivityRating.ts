

import {TimeNorm} from "./TimeNorm";
 
export class ProductivityRating { 
    ProductivityRatingId: number;
    TimeNormId: number;
    TimeNorm: TimeNorm | undefined;
    Name: string;
    ProductivityPercentage: number;
}