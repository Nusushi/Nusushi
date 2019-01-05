

import {CaliforniaProject} from "./CaliforniaProject";
import {LayoutRow} from "./LayoutRow";
import {QueryViewLayoutBoxMapping} from "./QueryViewLayoutBoxMapping"; 
export class CaliforniaView { 
    CaliforniaViewId: number;
    Name: string;
    QueryUrl: string;
    HostedByLayoutMappings: QueryViewLayoutBoxMapping[];
    PlacedLayoutRows: LayoutRow[];
    ViewSortOrderKey: number;
    IsInternal: boolean;
    UserDefinedCss: string | undefined
    DeepestLevel: number;
    SpecialStyleViewStyleMoleculeId: number;
    SpecialStyleBodyStyleMoleculeId: number;
    SpecialStyleHtmlStyleMoleculeId: number;
    SpecialStyleViewStyleMoleculeIdString: string;
    SpecialStyleBodyStyleMoleculeIdString: string;
    SpecialStyleHtmlStyleMoleculeIdString: string;
    SpecialStyleViewStyleString: string;
    SpecialStyleBodyStyleString: string;
    SpecialStyleHtmlStyleString: string;
}