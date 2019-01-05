

import {CaliforniaProject} from "./CaliforniaProject";
import {StyleMoleculeAtomMapping} from "./StyleMoleculeAtomMapping"; 
export class ResponsiveDevice { 
    ResponsiveDeviceId: number;
    Name: string;
    NameShort: string;
    WidthThreshold: number;
    AppliedToMappings: StyleMoleculeAtomMapping[];
}