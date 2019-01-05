

import {CaliforniaProject} from "./CaliforniaProject";
import {ContentAtom} from "./ContentAtom";
import {LayoutBox} from "./LayoutBox";
import {StyleMolecule} from "./StyleMolecule";
import {LayoutType} from "./LayoutType";
import {LayoutStyleInteraction} from "./LayoutStyleInteraction"; 
export class LayoutAtom { 
    LayoutBaseId: number;
    StyleMolecule: StyleMolecule;
    LayoutSortOrderKey: number;
    LayoutType: LayoutType;
    PlacedAtomInBoxId: number;
    PlacedAtomInBox: LayoutBox;
    HostedContentAtom: ContentAtom;
    LayoutStyleInteractions: LayoutStyleInteraction[];
    Level: number;
}