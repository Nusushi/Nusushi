

import {CaliforniaProject} from "./CaliforniaProject";
import {StyleAtomType} from "./StyleAtomType";
import {StyleAtomQuantumMapping} from "./StyleAtomQuantumMapping";
import {StyleValue} from "./StyleValue";
import {StyleMoleculeAtomMapping} from "./StyleMoleculeAtomMapping"; 
export class StyleAtom { 
    StyleAtomId: number;
    Name: string;
    StyleAtomType: StyleAtomType;
    AppliedValues: StyleValue[];
    MappedQuantums: StyleAtomQuantumMapping[];
    MappedToMoleculeId: number;
    IsDeletable: boolean;
}