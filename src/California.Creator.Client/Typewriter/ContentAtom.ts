

import {CaliforniaProject} from "./CaliforniaProject";
import {StyleMolecule} from "./StyleMolecule";
import {ContentAtomType} from "./ContentAtomType";
import {PictureContent} from "./PictureContent";
import {CaliforniaView} from "./CaliforniaView";
import {LayoutAtom} from "./LayoutAtom"; 
export class ContentAtom { 
    ContentAtomId: number;
    ContentAtomType: ContentAtomType;
    TextContent: string | undefined
    Url: string | undefined
    PictureContentId: number | undefined;
    PictureContent: PictureContent;
    IsDeleted: boolean;
    DeletedDate: Date | string
    InstancedOnLayoutId: number | undefined;
}