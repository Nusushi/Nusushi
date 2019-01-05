

import {CaliforniaProject} from "./CaliforniaProject";
import {StyleMoleculeAtomMapping} from "./StyleMoleculeAtomMapping";
import {ContentAtom} from "./ContentAtom";
import {LayoutBase} from "./LayoutBase"; 
export class StyleMolecule { 
    StyleMoleculeId: number;
    Name: string;
    NameShort: string;
    HtmlTag: string | undefined
    ClonedFromStyleId: number | undefined;
    ClonedFromStyle: StyleMolecule;
    CloneOfStyles: StyleMolecule[];
    StyleForLayoutId: number;
    StyleForLayout: LayoutBase;
    MappedStyleAtoms: StyleMoleculeAtomMapping[];
    IsPositionFixed: boolean;
    TopCssValuePx: string | undefined
    LeftCssValuePx: string | undefined
}