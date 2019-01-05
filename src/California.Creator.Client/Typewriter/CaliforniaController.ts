

///<reference types="jquery" />
import * as helpers from "./tsgenHelpers"; 
import { CaliforniaApp } from "./../Models/CaliforniaApp";
let currentApp: CaliforniaApp;
export class CaliforniaController {
        constructor(californiaAppArg: CaliforniaApp) {
            currentApp = californiaAppArg;
        }
        public beforeSendAjax = (jqXHR: JQueryXHR, settings: any): false | void => {
            if (currentApp.isAjaxRequestRunning === true) {
                console.log("ignored request (ajax call still in progress)");
                return false;
            }
            currentApp.isAjaxRequestRunning = true;
            jqXHR.done(xhr => {
                currentApp.isAjaxRequestRunning = false;
            }).fail(xhr => {
                currentApp.isAjaxRequestRunning = false;
                currentApp.projector.scheduleRender();
            });
        };
        public LogoutAction = () : JQueryXHR => {
            return helpers.Action(`/california/Logout`, "post", null, this.beforeSendAjax);
        }
        public IndexAction = (id: string, token: string) : JQueryXHR => {
            return helpers.Action(`/california/Index${encodeURIComponent(id)}?token=${encodeURIComponent(token)}`, "get", null, this.beforeSendAjax);
        }
        public DeleteLayoutStyleInteractionJson = (layoutStyleInteractionId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DeleteLayoutStyleInteraction?layoutStyleInteractionId=${layoutStyleInteractionId}`, "post", null, this.beforeSendAjax);
        }
        public DeleteStyleValueInteractionJson = (layoutStyleInteractionId: number | undefined, styleValueId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DeleteStyleValueInteraction?layoutStyleInteractionId=${layoutStyleInteractionId}&styleValueId=${styleValueId}`, "post", null, this.beforeSendAjax);
        }
        public CreateLayoutStyleInteractionForLayoutAtomJson = (layoutAtomId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/CreateLayoutStyleInteractionForLayoutAtom?layoutAtomId=${layoutAtomId}`, "post", null, this.beforeSendAjax);
        }
        public pubAction = (view: string, id: string) : JQueryXHR => {
            return helpers.Action(`/california/pub${encodeURIComponent(id)}?view=${encodeURIComponent(view)}`, "get", null, this.beforeSendAjax);
        }
        public StaticCssAction = (id: string) : JQueryXHR => {
            return helpers.Action(`/california/StaticCss${encodeURIComponent(id)}`, "get", null, this.beforeSendAjax);
        }
        public StaticJsAction = (id: string) : JQueryXHR => {
            return helpers.Action(`/california/StaticJs${encodeURIComponent(id)}`, "get", null, this.beforeSendAjax);
        }
        public PublishAction = (californiaProjectId: number | undefined, view: string) : JQueryXHR => {
            return helpers.Action(`/california/Publish?californiaProjectId=${californiaProjectId}&view=${encodeURIComponent(view)}`, "post", null, this.beforeSendAjax);
        }
        public InitialClientDataJson = (jsTimeString: string) : JQueryXHR => {
            return helpers.Json(`/california/InitialClientData?jsTimeString=${encodeURIComponent(jsTimeString)}`, "post", null, this.beforeSendAjax);
        }
        public SetSpecialLayoutBoxTypeJson = (layoutBoxId: number | undefined, specialLayoutBoxType: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SetSpecialLayoutBoxType?layoutBoxId=${layoutBoxId}&specialLayoutBoxType=${specialLayoutBoxType}`, "post", null, this.beforeSendAjax);
        }
        public DeleteStyleQuantumJson = (styleQuantumId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DeleteStyleQuantum?styleQuantumId=${styleQuantumId}`, "post", null, this.beforeSendAjax);
        }
        public DeleteLayoutJson = (layoutBaseId: number | undefined, isOnlyBelow: boolean) : JQueryXHR => {
            return helpers.Json(`/california/DeleteLayout?layoutBaseId=${layoutBaseId}&isOnlyBelow=${isOnlyBelow}`, "post", null, this.beforeSendAjax);
        }
        public SetStyleMoleculeAsReferenceStyleJson = (styleMoleculeId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SetStyleMoleculeAsReferenceStyle?styleMoleculeId=${styleMoleculeId}`, "post", null, this.beforeSendAjax);
        }
        public SetStyleMoleculeReferenceJson = (styleMoleculeId: number | undefined, referenceStyleMoleculeId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SetStyleMoleculeReference?styleMoleculeId=${styleMoleculeId}&referenceStyleMoleculeId=${referenceStyleMoleculeId}`, "post", null, this.beforeSendAjax);
        }
        public SyncStyleMoleculeToReferenceStyleJson = (styleMoleculeId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SyncStyleMoleculeToReferenceStyle?styleMoleculeId=${styleMoleculeId}`, "post", null, this.beforeSendAjax);
        }
        public SyncStyleMoleculeFromReferenceStyleJson = (styleMoleculeId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SyncStyleMoleculeFromReferenceStyle?styleMoleculeId=${styleMoleculeId}`, "post", null, this.beforeSendAjax);
        }
        public SyncLayoutStylesImitatingReferenceLayoutJson = (targetLayoutMoleculeId: number | undefined, referenceLayoutMoleculeId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SyncLayoutStylesImitatingReferenceLayout?targetLayoutMoleculeId=${targetLayoutMoleculeId}&referenceLayoutMoleculeId=${referenceLayoutMoleculeId}`, "post", null, this.beforeSendAjax);
        }
        public SetLayoutBoxCountForRowOrBoxJson = (layoutRowId: number | undefined, boxStyleMoleculeId: number | undefined, targetBoxCount: number | undefined, isFitWidth: boolean) : JQueryXHR => {
            return helpers.Json(`/california/SetLayoutBoxCountForRowOrBox?layoutRowId=${layoutRowId}&boxStyleMoleculeId=${boxStyleMoleculeId}&targetBoxCount=${targetBoxCount}&isFitWidth=${isFitWidth}`, "post", null, this.beforeSendAjax);
        }
        public CreateStyleValueForAtomJson = (styleAtomId: number | undefined, cssProperty: string) : JQueryXHR => {
            return helpers.Json(`/california/CreateStyleValueForAtom?styleAtomId=${styleAtomId}&cssProperty=${encodeURIComponent(cssProperty)}`, "post", null, this.beforeSendAjax);
        }
        public CreateCaliforniaViewJson = (californiaProjectId: number | undefined, californiaViewName: string) : JQueryXHR => {
            return helpers.Json(`/california/CreateCaliforniaView?californiaProjectId=${californiaProjectId}&californiaViewName=${encodeURIComponent(californiaViewName)}`, "post", null, this.beforeSendAjax);
        }
        public CreateCaliforniaViewFromReferenceViewJson = (californiaProjectId: number | undefined, californiaViewName: string, referenceCaliforniaViewId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/CreateCaliforniaViewFromReferenceView?californiaProjectId=${californiaProjectId}&californiaViewName=${encodeURIComponent(californiaViewName)}&referenceCaliforniaViewId=${referenceCaliforniaViewId}`, "post", null, this.beforeSendAjax);
        }
        public DeleteCaliforniaViewJson = (californiaViewId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DeleteCaliforniaView?californiaViewId=${californiaViewId}`, "post", null, this.beforeSendAjax);
        }
        public CreateStyleValueInteractionJson = (layoutStyleInteractionId: number | undefined, styleValueId: number | undefined, cssValue: string) : JQueryXHR => {
            return helpers.Json(`/california/CreateStyleValueInteraction?layoutStyleInteractionId=${layoutStyleInteractionId}&styleValueId=${styleValueId}&cssValue=${encodeURIComponent(cssValue)}`, "post", null, this.beforeSendAjax);
        }
        public CreateStyleAtomForMoleculeJson = (styleMoleculeId: number | undefined, styleAtomType: number | undefined, responsiveDeviceId: number | undefined, stateModifier: string) : JQueryXHR => {
            return helpers.Json(`/california/CreateStyleAtomForMolecule?styleMoleculeId=${styleMoleculeId}&styleAtomType=${styleAtomType}&responsiveDeviceId=${responsiveDeviceId}&stateModifier=${encodeURIComponent(stateModifier)}`, "post", null, this.beforeSendAjax);
        }
        public DeleteStyleAtomJson = (styleAtomId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DeleteStyleAtom?styleAtomId=${styleAtomId}`, "post", null, this.beforeSendAjax);
        }
        public ApplyStyleQuantumToAtomJson = (styleAtomId: number | undefined, styleQuantumId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/ApplyStyleQuantumToAtom?styleAtomId=${styleAtomId}&styleQuantumId=${styleQuantumId}`, "post", null, this.beforeSendAjax);
        }
        public CreateStyleQuantumJson = (californiaProjectId: number | undefined, quantumName: string, cssProperty: string, cssValue: string) : JQueryXHR => {
            return helpers.Json(`/california/CreateStyleQuantum?californiaProjectId=${californiaProjectId}&quantumName=${encodeURIComponent(quantumName)}&cssProperty=${encodeURIComponent(cssProperty)}&cssValue=${encodeURIComponent(cssValue)}`, "post", null, this.beforeSendAjax);
        }
        public UpdateTextContentAtomJson = (contentAtomId: number | undefined, updatedTextContent: string) : JQueryXHR => {
            return helpers.Json(`/california/UpdateTextContentAtom?contentAtomId=${contentAtomId}&updatedTextContent=${encodeURIComponent(updatedTextContent)}`, "post", null, this.beforeSendAjax);
        }
        public UpdateStyleQuantumJson = (styleQuantumId: number | undefined, cssValue: string) : JQueryXHR => {
            return helpers.Json(`/california/UpdateStyleQuantum?styleQuantumId=${styleQuantumId}&cssValue=${encodeURIComponent(cssValue)}`, "post", null, this.beforeSendAjax);
        }
        public UpdateStyleValueJson = (styleValueId: number | undefined, cssValue: string) : JQueryXHR => {
            return helpers.Json(`/california/UpdateStyleValue?styleValueId=${styleValueId}&cssValue=${encodeURIComponent(cssValue)}`, "post", null, this.beforeSendAjax);
        }
        public UpdateUserDefinedCssForProjectJson = (californiaProjectId: number | undefined, cssValue: string) : JQueryXHR => {
            return helpers.Json(`/california/UpdateUserDefinedCssForProject?californiaProjectId=${californiaProjectId}&cssValue=${encodeURIComponent(cssValue)}`, "post", null, this.beforeSendAjax);
        }
        public UpdateUserDefinedCssForViewJson = (californiaViewId: number | undefined, cssValue: string) : JQueryXHR => {
            return helpers.Json(`/california/UpdateUserDefinedCssForView?californiaViewId=${californiaViewId}&cssValue=${encodeURIComponent(cssValue)}`, "post", null, this.beforeSendAjax);
        }
        public DeleteStyleValueJson = (styleValueId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DeleteStyleValue?styleValueId=${styleValueId}`, "post", null, this.beforeSendAjax);
        }
        public DuplicateStyleQuantumJson = (styleQuantumId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/DuplicateStyleQuantum?styleQuantumId=${styleQuantumId}`, "post", null, this.beforeSendAjax);
        }
        public CreateLayoutAtomForBoxJson = (targetLayoutBoxId: number | undefined, referenceLayoutAtomId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/CreateLayoutAtomForBox?targetLayoutBoxId=${targetLayoutBoxId}&referenceLayoutAtomId=${referenceLayoutAtomId}`, "post", null, this.beforeSendAjax);
        }
        public CreateLayoutBoxForBoxOrRowJson = (targetLayoutBoxOrRowId: number | undefined, referenceLayoutBoxId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/CreateLayoutBoxForBoxOrRow?targetLayoutBoxOrRowId=${targetLayoutBoxOrRowId}&referenceLayoutBoxId=${referenceLayoutBoxId}`, "post", null, this.beforeSendAjax);
        }
        public CreateLayoutBoxForAtomInPlaceJson = (targetLayoutAtomId: number | undefined, referenceLayoutBoxId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/CreateLayoutBoxForAtomInPlace?targetLayoutAtomId=${targetLayoutAtomId}&referenceLayoutBoxId=${referenceLayoutBoxId}`, "post", null, this.beforeSendAjax);
        }
        public CreateLayoutRowForViewJson = (targetCaliforniaViewId: number | undefined, referenceLayoutRowId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/CreateLayoutRowForView?targetCaliforniaViewId=${targetCaliforniaViewId}&referenceLayoutRowId=${referenceLayoutRowId}`, "post", null, this.beforeSendAjax);
        }
        public SetLayoutRowOrBoxAsInstanceableJson = (californiaProjectId: number | undefined, layoutRowOrBoxId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/SetLayoutRowOrBoxAsInstanceable?californiaProjectId=${californiaProjectId}&layoutRowOrBoxId=${layoutRowOrBoxId}`, "post", null, this.beforeSendAjax);
        }
        public MoveStyleAtomToResponsiveDeviceJson = (styleAtomId: number | undefined, targetResponsiveDeviceId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/MoveStyleAtomToResponsiveDevice?styleAtomId=${styleAtomId}&targetResponsiveDeviceId=${targetResponsiveDeviceId}`, "post", null, this.beforeSendAjax);
        }
        public RefreshExternalApisAction = () : JQueryXHR => {
            return helpers.Action(`/california/RefreshExternalApis`, "get", null, this.beforeSendAjax);
        }
        public MoveLayoutMoleculeIntoLayoutMoleculeJson = (movedLayoutMoleculeId: number | undefined, targetContainerLayoutMoleculeId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/california/MoveLayoutMoleculeIntoLayoutMolecule?movedLayoutMoleculeId=${movedLayoutMoleculeId}&targetContainerLayoutMoleculeId=${targetContainerLayoutMoleculeId}`, "post", null, this.beforeSendAjax);
        }
        public MoveLayoutMoleculeNextToLayoutMoleculeJson = (movedLayoutMoleculeId: number | undefined, targetNeighborLayoutMoleculeId: number | undefined, isMoveBefore: boolean) : JQueryXHR => {
            return helpers.Json(`/california/MoveLayoutMoleculeNextToLayoutMolecule?movedLayoutMoleculeId=${movedLayoutMoleculeId}&targetNeighborLayoutMoleculeId=${targetNeighborLayoutMoleculeId}&isMoveBefore=${isMoveBefore}`, "post", null, this.beforeSendAjax);
        }
    }