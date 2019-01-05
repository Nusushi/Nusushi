import * as maquette from "maquette";
const h = maquette.h;
import { TableOfContents, ClientSectionViewModel } from "./../Models/TableOfContents";
import { TrackerClientViewModel, TimeTableViewModel } from "../Models/TokyoTrackerGenerated";

let currentVM: TableOfContentsVM;

export class TableOfContentsVM {
    // data
    public sectionsTree: ClientSectionViewModel[] = [];
    public mappedSectionsDictionary: { [key: string]: ClientSectionViewModel } = {};
    public initialIndex: number = -1;
    public currentIndex: number = -1;
    public activeSectionName: string | undefined = undefined;
    // components
    public sectionsMapping: maquette.Mapping<ClientSectionViewModel, { renderMaquette: () => maquette.VNode }>;
    // client settings
    
    constructor(tableOfContentsArg: TableOfContents) {
        currentVM = this;
        this.sectionsMapping = tableOfContentsArg.createRecursiveSectionsMapping();
        this.sectionsMapping.map(this.sectionsTree);
    }

    public updateSectionDictionary = (): void => { // TODO check if functions are declared in this format everywhere
        currentVM.mappedSectionsDictionary = {};
        // initialize
        for (let i = 0; i < currentVM.sectionsTree.length; i++) {
            let topSection = currentVM.sectionsTree[i];
            topSection.IsVisible = true;
            currentVM.mappedSectionsDictionary[topSection.Numbering] = topSection;
            for (let j = 0; j < topSection.SubSectionVMs.length; j++) {
                let subSection1 = topSection.SubSectionVMs[j];
                subSection1.IsVisible = false;
                currentVM.mappedSectionsDictionary[subSection1.Numbering] = subSection1;
                for (let k = 0; k < subSection1.SubSectionVMs.length; k++) {
                    let subSection2 = subSection1.SubSectionVMs[k];
                    subSection2.IsVisible = false;
                    currentVM.mappedSectionsDictionary[subSection2.Numbering] = subSection2;
                    for (let l = 0; l < subSection2.SubSectionVMs.length; l++) {
                        let subSection3 = subSection2.SubSectionVMs[l];
                        subSection3.IsVisible = false;
                        currentVM.mappedSectionsDictionary[subSection3.Numbering] = subSection3;
                    }
                }
            }
        }
    }

    public updateTableOfContents = (timeTables: TimeTableViewModel[] | undefined, selectedTimeTableId: number): void => {
        let sectionsTree: ClientSectionViewModel[] = [];
        if (timeTables !== undefined) {
            for (let i = 0; i < timeTables.length; i++) {
                let currentTable = timeTables[i];
                if (currentTable.TimeTableAndWorkUnits !== undefined && currentTable.TimeTableAndWorkUnits.WorkUnits !== undefined) {
                    let isActiveTable = currentTable.TimeTableAndWorkUnits.TimeTableId == selectedTimeTableId;
                    let subSectionVMs: ClientSectionViewModel[] = [];
                    for (let j = 0; j < currentTable.TimeTableAndWorkUnits.WorkUnits.length; j++) {
                        let currentWorkUnit = currentTable.TimeTableAndWorkUnits.WorkUnits[j];
                        let timeNormSectionVMs: ClientSectionViewModel[] = [];
                        if (currentWorkUnit.TimeNorms !== undefined) {
                            for (let k = 0; k < currentWorkUnit.TimeNorms.length; k++) {
                                let currentTimeNorm = currentWorkUnit.TimeNorms[k];
                                timeNormSectionVMs.push({
                                    IsActive: false, 
                                    IsVisible: false, // TODO should be what is currently visible in scrolled area
                                    Level: 2, 
                                    Numbering: currentVM.createSectionNumberingTimeNorm(currentTable.TimeTableAndWorkUnits.TimeTableId, currentWorkUnit.WorkUnitId, currentTimeNorm.TimeNormId), 
                                    SubSectionVMs: [], 
                                    Text: currentTimeNorm.Name} as ClientSectionViewModel);
                            }
                        }
                        subSectionVMs.push({
                            IsActive: false, 
                            IsVisible: isActiveTable, 
                            Level: 1, 
                            Numbering: currentVM.createSectionNumberingWorkUnit(currentTable.TimeTableAndWorkUnits.TimeTableId, currentWorkUnit.WorkUnitId), 
                            SubSectionVMs: timeNormSectionVMs, 
                            Text: currentWorkUnit.Name} as ClientSectionViewModel);
                    }
                    sectionsTree.push({
                        IsActive: isActiveTable, 
                        IsVisible: true, 
                        Level: 0, 
                        Numbering: currentTable.TimeTableAndWorkUnits.TimeTableId.toString(), 
                        SubSectionVMs: subSectionVMs, 
                        Text: currentTable.TimeTableAndWorkUnits.Name} as ClientSectionViewModel);
                }
            }
        }
        currentVM.updateSectionDictionary();
        currentVM.sectionsTree = sectionsTree;
        currentVM.updateTreeForActiveSection(currentVM.activeSectionName);
    }

    public createSectionNumberingWorkUnit(timeTableId: number, workUnitId: number) {
        // TODO make sure this is called with safe parameters
        return `${timeTableId}.${workUnitId}`;
    }

    public createSectionNumberingTimeNorm(timeTableId: number, workUnitId: number, timeNormId: number) {
        return `${timeTableId}.${workUnitId}.${timeNormId}`;
    }

    public updateTreeForActiveSection = (newTarget: string | undefined): void => {
        if (newTarget == undefined) {
            currentVM.activeSectionName = undefined;
            return;
        }
        else {
            currentVM.activeSectionName = newTarget;
        }
        
        for (let i = 0; i < currentVM.sectionsTree.length; i++) {
            let topSection = currentVM.sectionsTree[i];
            topSection.IsActive = (topSection.Numbering === currentVM.activeSectionName);
            for (let j = 0; j < topSection.SubSectionVMs.length; j++) {
                let subSection1 = topSection.SubSectionVMs[j];
                subSection1.IsActive = (subSection1.Numbering === currentVM.activeSectionName);
                topSection.IsActive = topSection.IsActive || subSection1.IsActive;
                for (let k = 0; k < subSection1.SubSectionVMs.length; k++) {
                    let subSection2 = subSection1.SubSectionVMs[k];
                    subSection2.IsActive = (subSection2.Numbering === currentVM.activeSectionName);
                    subSection1.IsActive = subSection1.IsActive || subSection2.IsActive;
                    topSection.IsActive = topSection.IsActive || subSection2.IsActive;
                    for (let l = 0; l < subSection2.SubSectionVMs.length; l++) {
                        let subSection3 = subSection2.SubSectionVMs[l];
                        subSection3.IsActive = (subSection3.Numbering === currentVM.activeSectionName);
                        subSection2.IsActive = subSection2.IsActive || subSection3.IsActive;
                        subSection1.IsActive = subSection1.IsActive || subSection3.IsActive;
                        topSection.IsActive = topSection.IsActive || subSection3.IsActive;
                    }
                }
            }
        }
        for (let i = 0; i < currentVM.sectionsTree.length; i++) {
            let topSection = currentVM.sectionsTree[i];
            for (let j = 0; j < topSection.SubSectionVMs.length; j++) {
                let subSection1 = topSection.SubSectionVMs[j];
                subSection1.IsVisible = topSection.IsActive;
                for (let k = 0; k < subSection1.SubSectionVMs.length; k++) {
                    let subSection2 = subSection1.SubSectionVMs[k];
                    subSection2.IsVisible = subSection1.IsActive;
                    for (let l = 0; l < subSection2.SubSectionVMs.length; l++) {
                        let subSection3 = subSection2.SubSectionVMs[l];
                        subSection3.IsVisible = subSection2.IsActive;
                    }
                }
            }
        }

        currentVM.sectionsMapping.map(currentVM.sectionsTree);
    }
}