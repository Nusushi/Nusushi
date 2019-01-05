

import {CaliforniaProject} from "./CaliforniaProject";
import {LayoutRow} from "./LayoutRow";
import {LayoutAtom} from "./LayoutAtom";
import {StyleMolecule} from "./StyleMolecule";
import {QueryViewLayoutBoxMapping} from "./QueryViewLayoutBoxMapping";
import {LayoutType} from "./LayoutType";
import {SpecialLayoutBoxType} from "./SpecialLayoutBoxType"; 
export class LayoutBox { 
    LayoutBaseId: number;
    StyleMolecule: StyleMolecule;
    LayoutSortOrderKey: number;
    LayoutType: LayoutType;
    PlacedInBoxAtoms: LayoutAtom[];
    PlacedInBoxBoxes: LayoutBox[];
    PlacedBoxInBoxId: number | undefined;
    PlacedBoxInBox: LayoutBox;
    BoxOwnerRowId: number;
    BoxOwnerRow: LayoutRow;
    HostedViewMappings: QueryViewLayoutBoxMapping[];
    SpecialLayoutBoxType: SpecialLayoutBoxType;
    Level: number;
}