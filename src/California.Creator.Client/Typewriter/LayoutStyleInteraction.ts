

import {StyleValueInteractionMapping} from "./StyleValueInteractionMapping";
import {LayoutStyleInteractionType} from "./LayoutStyleInteractionType"; 
export class LayoutStyleInteraction { 
    LayoutStyleInteractionId: number;
    LayoutAtomId: number;
    StyleValueInteractions: StyleValueInteractionMapping[];
    LayoutStyleInteractionType: LayoutStyleInteractionType;
}