

import {StyleQuantum} from "./StyleQuantum";
import {StyleAtom} from "./StyleAtom";
import {StyleMolecule} from "./StyleMolecule";
import {ResponsiveDevice} from "./ResponsiveDevice";
import {ContentAtom} from "./ContentAtom";
import {PictureContent} from "./PictureContent";
import {CaliforniaView} from "./CaliforniaView";
import {SharedProjectInfo} from "./SharedProjectInfo";
import {StyleValue} from "./StyleValue";
import {LayoutBase} from "./LayoutBase";
import {LayoutStyleInteraction} from "./LayoutStyleInteraction";
import {CaliforniaSelectionOverlay} from "./CaliforniaSelectionOverlay"; 
export class CaliforniaProject { 
    CaliforniaProjectId: number;
    Name: string;
    StyleValues: StyleValue[];
    StyleQuantums: StyleQuantum[];
    StyleAtoms: StyleAtom[];
    StyleMolecules: StyleMolecule[];
    ResponsiveDevices: ResponsiveDevice[];
    LayoutMolecules: LayoutBase[];
    LayoutStyleInteractions: LayoutStyleInteraction[];
    ContentAtoms: ContentAtom[];
    PictureContents: PictureContent[];
    CaliforniaViews: CaliforniaView[];
    SharedProjectInfos: SharedProjectInfo[];
    ProjectDefaultsRevision: number;
    UserDefinedCss: string | undefined
    CurrentSelection: CaliforniaSelectionOverlay;
}