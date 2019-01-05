

import {CaliforniaProject} from "./CaliforniaProject";
import {StyleQuantum} from "./StyleQuantum";
import {CaliforniaClientPartialData} from "./CaliforniaClientPartialData"; 
export class CaliforniaClientViewModel { 
    StatusText: string | undefined
    CurrentRevision: number;
    CaliforniaEvent: number;
    CaliforniaProject: CaliforniaProject;
    StyleAtomCssPropertyMapping: { [key: string]: string[]; };
    AllCssProperties: string[];
    ThirdPartyFonts: string[];
    UrlToReadOnly: string | undefined
    UrlToReadAndEdit: string | undefined
    PartialUpdate: CaliforniaClientPartialData;
}