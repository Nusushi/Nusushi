

import {CaliforniaProject} from "./CaliforniaProject";
import {LayoutBox} from "./LayoutBox";
import {StyleMolecule} from "./StyleMolecule";
import {CaliforniaView} from "./CaliforniaView";
import {LayoutType} from "./LayoutType"; 
export class LayoutRow { 
    LayoutBaseId: number;
    StyleMolecule: StyleMolecule;
    LayoutSortOrderKey: number;
    LayoutType: LayoutType;
    AllBoxesBelowRow: LayoutBox[];
    PlacedOnViewId: number;
    DeepestLevel: number;
}