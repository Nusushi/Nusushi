

interface CaliforniaStore { CaliforniaStoreId: string; }
import {CaliforniaProject} from "./CaliforniaProject"; 
export class SharedProjectInfo { 
    SharedProjectInfoId: number;
    SharedWithCaliforniaStoreId: string;
    SharedWithCaliforniaStore: CaliforniaStore;
    OwnerCaliforniaStoreId: string;
    OwnerCaliforniaStore: CaliforniaStore;
    CaliforniaProjectId: number;
    CaliforniaProject: CaliforniaProject;
    Name: string;
    ShareEnabledTime: Date | string
    IsReshareAllowed: boolean;
    IsEditAllowed: boolean;
}