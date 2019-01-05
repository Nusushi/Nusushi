/// <reference path="../jsx.ts" />
import { VNode, VNodeProperties } from "maquette";
declare var require: any;
import * as maquette from "maquette";
const h = maquette.h;
import * as timelineConstants from "./TimelineConstants";
import { TableOfContentsVM } from "./../ViewModels/TableOfContentsVM";
import { TokyoTrackerApp } from "./TokyoTrackerApp";

export interface ClientSectionViewModel {
    Numbering: string;
    Text: string;
    Level: number;
    IsActive: boolean;
    SubSectionVMs: ClientSectionViewModel[];
    IsVisible: boolean;
}

let currentApp: TokyoTrackerApp;
let currentTableOfContents: TableOfContents;

export class TableOfContents {
    public viewModel: TableOfContentsVM;

    constructor(currentAppArg: TokyoTrackerApp) {
        currentTableOfContents = this;
        currentApp = currentAppArg;
        this.viewModel = new TableOfContentsVM(this);
    };
    
    public renderMaquette = (): VNode => {
        return <ul class="nav nav-pills nav-stacked">
            {currentTableOfContents.viewModel.sectionsMapping.results.map(c => c.renderMaquette())}
        </ul> as VNode;
    };

    public createRecursiveSectionsMapping = (): maquette.Mapping<ClientSectionViewModel, { renderMaquette: () => maquette.VNode }> => {
        return maquette.createMapping<ClientSectionViewModel, any>(
            function getSectionSourceKey(source: ClientSectionViewModel) {
                // function that returns a key to uniquely identify each item in the data
                return source.Numbering;
            },
            function createSectionTarget(source: ClientSectionViewModel) {
                // function to create the target based on the source 
                // (the same function that you use in Array.map)
                let isActive = source.IsActive;
                let isVisible = source.IsVisible;
                let subComponents = currentTableOfContents.createRecursiveSectionsMapping(); // Recursive
                subComponents.map(source.SubSectionVMs);
                let indentationPadding = 5.0 * source.Level;
                let text = source.Text;
                return {
                    renderMaquette: function () {
                        let linkElement: VNode;
                        if (source.Level == 0) {
                            linkElement = <a href={`#a_${source.Numbering}`} tid={source.Numbering.toString()} onclick={currentApp.client.jumpToTimeTableClickHandler} 
                            styles={{"color": isActive ? timelineConstants.TIMELINE_COLOR_STRING : undefined}}>{`${text}`}</a> as VNode;
                        }
                        else {
                            linkElement = <a href={`#a_${source.Numbering}`}
                            styles={{"color": isActive ? timelineConstants.TIMELINE_COLOR_STRING : undefined}}>{`${text}`}</a> as VNode;
                        }
                        let resultSections: maquette.VNode[] = [];
                        resultSections.push(<li key={source.Numbering} classes={{"active": isActive}} role="presentation" 
                            styles={{"display": isVisible ? "list-item" : "none", "padding-left": indentationPadding + "px", "width": "100%", "text-decoration": isActive ? "underline" : undefined, "font-weight": source.Level == 0 ? "bold" : undefined}}>
                            {linkElement}
                        </li> as VNode);
                        subComponents.results.map(function (component: any) {
                            resultSections.push(component.renderMaquette());
                        });
                        return resultSections;
                    },
                    update: function (updatedSource: ClientSectionViewModel) {
                        isActive = updatedSource.IsActive;
                        isVisible = updatedSource.IsVisible;
                        subComponents.map(updatedSource.SubSectionVMs);
                        text = updatedSource.Text;
                    }
                };
            },
            function updateSectionTarget(updatedSource: ClientSectionViewModel, target: { renderMaquette(): any, update(updatedSource: ClientSectionViewModel): void }) {
                // This function can be used to update the component with the updated item
                target.update(updatedSource);
            });
    };

    public onContentPositionScrolledHandler = (evt: Event) => {
        // TODO should not be created on small devices
        let allAnchors = document.getElementsByClassName("anchor"); // TODO expensive
        let initialActiveAnchor = document.getElementsByName(`a_${currentTableOfContents.viewModel.activeSectionName}`); // TODO expensive
        // update navigation tree during scroll, when isShowAll
        // set index when initial jump to fragment TODO
        /*if (currentTableOfContents.viewModel.initialIndex < 0) {
            for (let i = 0; i < allAnchors.length; i++) {
                if (allAnchors[i] === initialActiveAnchor[0]) {
                    currentTableOfContents.viewModel.currentIndex = i;
                    currentTableOfContents.viewModel.initialIndex = i;
                    break;
                }
            }
        }*/
        if (currentTableOfContents.viewModel.currentIndex == -1) {
            currentTableOfContents.viewModel.currentIndex = 0;
        }
        // TODO add a tolerance of 5px or something around scroll===0...+5 and TODO add delay of 5seconds or something before triggering animation
        let currentScroll = (evt.target as HTMLDivElement).scrollTop;
        // hide timelime if scroll from top and not explicitly enabled
        if (currentApp.client.viewModel.lastScroll == 0 && !currentApp.client.viewModel.isExplicitlyToggledTimeline) {
            currentApp.client.timelineHideAnimation();
        }
        else if (currentScroll == 0 && currentApp.client.viewModel.lastScroll != 0 && !currentApp.client.viewModel.isExplicitlyToggledTimeline) {
            currentApp.client.timelineShowAnimation();
        }
        currentApp.client.viewModel.lastScroll = currentScroll;
        // decide if navigation tree needs to be updated
        let anchorFound = false;
        let newIndex = currentTableOfContents.viewModel.currentIndex;
        for (let i = 0; i < allAnchors.length; i++) {
            if (allAnchors[i] === undefined) {
                continue;
            }
            let anchorPosition = (allAnchors[i] as HTMLDivElement).offsetTop;
            //console.log(`pos anchor ${i}: ${anchorPosition}`); // position_anchor_0 == 128 with unbound timestamps, 134 without => should just be viewed as 
            if (i == 0) {
                anchorPosition = 0;
            }
            let difference = (currentScroll - anchorPosition);
            //console.log(`diff anchor ${i}: ${difference}`); 

        }
        // TODO REWORK and use after release as open source

        
        // first element, whose successor is +60px off
        // start search from active index
        for (let i = currentTableOfContents.viewModel.currentIndex; i >= 0; i--) {
            if (allAnchors[i] === undefined) {  // TODO can be null why?
                continue;
            }
            let anchorPosition = (allAnchors[i] as HTMLDivElement).offsetTop;
            let difference = (currentScroll - anchorPosition);
            /*if (i === currentTableOfContents.viewModel.initialIndex) { // workaround anchor offset css TODO remove initialIndex
                difference = difference - 60.0;
            }*/
            if (difference <= -129.0) { // TODO value must be adjusted after UI height changes: scroll to first element, set breakpoint on loop start 
                /*if (difference >= -277.0/*-133.0*//* || i === 0) // allow small range past anchor change mark (scroll up) TODO not required anymore?
                {
                    newIndex = i;
                }
                else {
                    newIndex = i - 1;
                }*/
                newIndex = i - 1;
                if (newIndex < 0) {
                    newIndex = 0;
                }
                anchorFound = true;
            }
            else {
                break;
            }
        }
        if (!anchorFound) {
            for (let j = currentTableOfContents.viewModel.currentIndex; j < allAnchors.length; j++) {
                if (allAnchors[j] === undefined) {
                    continue;
                }
                let anchorPosition2 = (allAnchors[j] as HTMLDivElement).offsetTop; // TODO can be null why?
                let difference2 = (currentScroll - anchorPosition2);
                /*if (j === currentTableOfContents.viewModel.initialIndex) { // workaround anchor offset css
                    difference2 = difference2 - 60.0;
                }*/
                if (difference2 >= -90.0) { // TODO value must be adjusted after UI height changes
                    anchorFound = true;
                    newIndex = j;
                    break;
                }
            }
        }
        if (newIndex !== currentTableOfContents.viewModel.currentIndex) {
            if (allAnchors[newIndex] !== undefined) {
                currentTableOfContents.viewModel.currentIndex = newIndex;
                let newAnchorName = (allAnchors[newIndex] as HTMLAnchorElement).name.slice(2); // ignore a_ prefix in anchor name
                currentTableOfContents.viewModel.updateTreeForActiveSection(newAnchorName);
                currentApp.projector.scheduleRender();
            }
        }
    }
}