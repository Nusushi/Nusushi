

///<reference types="jquery" />
import * as helpers from "./tsgenHelpers";
import {EditTimeStampPostViewModel} from "./EditTimeStampPostViewModel";
import {CreateTimeStampPostViewModel} from "./CreateTimeStampPostViewModel"; 
export class TokyoController {
        constructor() { 
        } 
        public IndexAction = (id: string, token: string) : JQueryXHR => {
            return helpers.Action(`/tokyo/Index${encodeURIComponent(id)}?token=${encodeURIComponent(token)}`, "get", null);
        }
        public LogoutAction = () : JQueryXHR => {
            return helpers.Action(`/tokyo/Logout`, "post", null);
        }
        public InitialClientDataJson = (jsTimeString: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/InitialClientData?jsTimeString=${encodeURIComponent(jsTimeString)}`, "get", null);
        }
        public EditProductivityRatingJson = (timeNormId: number | undefined, ratingPercentage: number) : JQueryXHR => {
            return helpers.Json(`/tokyo/EditProductivityRating?timeNormId=${timeNormId}&ratingPercentage=${ratingPercentage}`, "post", null);
        }
        public CreateTimeTableJson = () : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateTimeTable`, "post", null);
        }
        public MoveWorkUnitBeforeWorkUnitJson = (moveWorkUnitId: number | undefined, moveBeforeWorkUnitId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/MoveWorkUnitBeforeWorkUnit?moveWorkUnitId=${moveWorkUnitId}&moveBeforeWorkUnitId=${moveBeforeWorkUnitId}`, "post", null);
        }
        public SetWorkUnitAsDefaultTargetJson = (workUnitId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/SetWorkUnitAsDefaultTarget?workUnitId=${workUnitId}`, "post", null);
        }
        public SetViewTimeZoneJson = (timeZoneIdDotNet: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/SetViewTimeZone?timeZoneIdDotNet=${encodeURIComponent(timeZoneIdDotNet)}`, "post", null);
        }
        public SetCultureJson = (cultureIdDotNet: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/SetCulture?cultureIdDotNet=${encodeURIComponent(cultureIdDotNet)}`, "post", null);
        }
        public MoveTimeNormToWorkUnitJson = (timeNormId: number | undefined, targetWorkUnitId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/MoveTimeNormToWorkUnit?timeNormId=${timeNormId}&targetWorkUnitId=${targetWorkUnitId}`, "post", null);
        }
        public ProcessTimeStampsJson = () : JQueryXHR => {
            return helpers.Json(`/tokyo/ProcessTimeStamps`, "post", null);
        }
        public EditTimeNormJson = (timeNormId: number | undefined, newTimeNormName: string, newColorR: number | undefined, newColorG: number | undefined, newColorB: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/EditTimeNorm?timeNormId=${timeNormId}&newTimeNormName=${encodeURIComponent(newTimeNormName)}&newColorR=${newColorR}&newColorG=${newColorG}&newColorB=${newColorB}`, "post", null);
        }
        public CreateWorkUnitJson = (timeTableId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateWorkUnit?timeTableId=${timeTableId}`, "post", null);
        }
        public UnbindTimeNormJson = (timeNormId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/UnbindTimeNorm?timeNormId=${timeNormId}`, "post", null);
        }
        public RemoveWorkUnitJson = (workUnitId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/RemoveWorkUnit?workUnitId=${workUnitId}`, "post", null);
        }
        public RemoveTimeTableJson = (timeTableId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/RemoveTimeTable?timeTableId=${timeTableId}`, "post", null);
        }
        public EditTimeStampPostJson = (editTimeStampPostViewModel: EditTimeStampPostViewModel) : JQueryXHR => {
            return helpers.Json(`/tokyo/EditTimeStampPost`, "post", editTimeStampPostViewModel);
        }
        public UndoLastUserActionJson = () : JQueryXHR => {
            return helpers.Json(`/tokyo/UndoLastUserAction`, "post", null);
        }
        public RemoveTimeStampJson = (timeStampId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/RemoveTimeStamp?timeStampId=${timeStampId}`, "post", null);
        }
        public EditTimeTableJson = (timeTableId: number | undefined, newTimeTableName: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/EditTimeTable?timeTableId=${timeTableId}&newTimeTableName=${encodeURIComponent(newTimeTableName)}`, "post", null);
        }
        public EditWorkUnitJson = (workUnitId: number | undefined, newWorkUnitName: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/EditWorkUnit?workUnitId=${workUnitId}&newWorkUnitName=${encodeURIComponent(newWorkUnitName)}`, "post", null);
        }
        public CreateTimeNormTagJson = (name: string, color: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateTimeNormTag?name=${encodeURIComponent(name)}&color=${encodeURIComponent(color)}`, "post", null);
        }
        public EditTimeNormTagJson = (timeNormTagId: number | undefined, name: string, color: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/EditTimeNormTag?timeNormTagId=${timeNormTagId}&name=${encodeURIComponent(name)}&color=${encodeURIComponent(color)}`, "post", null);
        }
        public RemoveTimeNormTagJson = (timeNormTagId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/RemoveTimeNormTag?timeNormTagId=${timeNormTagId}`, "post", null);
        }
        public ExportTimeTableToCSVAction = (timeTableId: number | undefined) : JQueryXHR => {
            return helpers.Action(`/tokyo/ExportTimeTableToCSV?timeTableId=${timeTableId}`, "get", null);
        }
        public TimeTablePlotAction = (timeTableId: number | undefined) : JQueryXHR => {
            return helpers.Action(`/tokyo/TimeTablePlot?timeTableId=${timeTableId}`, "post", null);
        }
        public CreateTimeStampsJson = (timeStampStrings: string[], count: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateTimeStamps?timeStampStrings=${timeStampStrings}&count=${count}`, "post", null);
        }
        public CreateTimeStampJson = (jsTimeString: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateTimeStamp?jsTimeString=${encodeURIComponent(jsTimeString)}`, "get", null);
        }
        public CreateTimeNormJson = (workUnitId: number | undefined, jsTimeString: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateTimeNorm?workUnitId=${workUnitId}&jsTimeString=${encodeURIComponent(jsTimeString)}`, "post", null);
        }
        public DuplicateTimeStampJson = (timeStampId: number | undefined) : JQueryXHR => {
            return helpers.Json(`/tokyo/DuplicateTimeStamp?timeStampId=${timeStampId}`, "post", null);
        }
        public CreateTimeStampManuallyPostJson = (createTimeStampViewModel: CreateTimeStampPostViewModel) : JQueryXHR => {
            return helpers.Json(`/tokyo/CreateTimeStampManuallyPost`, "post", createTimeStampViewModel);
        }
        public SetSortOrderJson = (sortOrderType: number | undefined, isAscending: boolean) : JQueryXHR => {
            return helpers.Json(`/tokyo/SetSortOrder?sortOrderType=${sortOrderType}&isAscending=${isAscending}`, "post", null);
        }
        public ShareTimeTablesWithTokenJson = (timeTableIds: number[]) : JQueryXHR => {
            return helpers.Json(`/tokyo/ShareTimeTablesWithToken?timeTableIds=${timeTableIds}`, "post", null);
        }
        public ShareTimeTablesJson = (timeTableIds: number[], shareWithEmail: string) : JQueryXHR => {
            return helpers.Json(`/tokyo/ShareTimeTables?timeTableIds=${timeTableIds}&shareWithEmail=${encodeURIComponent(shareWithEmail)}`, "post", null);
        }
        public UnShareTimeTablesJson = (timeTableIds: number[]) : JQueryXHR => {
            return helpers.Json(`/tokyo/UnShareTimeTables?timeTableIds=${timeTableIds}`, "post", null);
        }
    }