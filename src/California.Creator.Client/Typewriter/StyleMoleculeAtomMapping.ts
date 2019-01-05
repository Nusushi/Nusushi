

import {StyleMolecule} from "./StyleMolecule";
import {StyleAtom} from "./StyleAtom";
import {ResponsiveDevice} from "./ResponsiveDevice"; 
export class StyleMoleculeAtomMapping { 
    StyleMoleculeAtomMappingId: number;
    StyleMoleculeId: number;
    StyleMolecule: StyleMolecule;
    ResponsiveDeviceId: number;
    ResponsiveDevice: ResponsiveDevice;
    StateModifier: string | undefined
}