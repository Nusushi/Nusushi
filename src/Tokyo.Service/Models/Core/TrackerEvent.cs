using System;
using System.Collections.Generic;
using System.Text;

namespace Tokyo.Service.Models.Core
{
    // TODO numbers are set explicitly to match with tests
    public enum TrackerEvent // TODO sort, close caps and lock
    {
        CreateTimeStamp = 0,
        CreateTimeStampManually = 1,
        ShowTimeStamp = 2,
        ShowTimeStamps = 3,
        EditTimeStamp = 4,
        RemoveTimeStamp = 5,
        RemoveTimeStamps = 6,
        CreateTimeNormTag = 7,
        ShowTimeNormTag = 8,
        ShowTimeNormTags = 9,
        EditTimeNormTag = 10,
        RemoveTimeNormTag = 11,
        RemoveTimeNorm = 12,
        ShowTimeTables = 13,
        RemoveTimeTable = 14,
        CreateProfile = 15,
        RemoveProfile = 16,
        ProcessTimeStamps = 17,
        AbortSimilarRequest = 18,
        ShareTimeTables = 19,
        ShowSharedTimeTables = 20,
        UnshareTimeTables = 21,
        QueryTableOwners = 22,
        EditWorkUnit = 23,
        CreateWorkUnit = 24,
        RemoveWorkUnit = 25,
        ShowDefaultTargetWorkUnit = 26,
        MoveWorkUnit = 27,
        SetDefaultTargetWorkUnit = 28,
        MoveTimeNorm = 29,
        ShowInitialClientData = 30,
        EditTimeNorm = 31,
        EditProductivityRating = 32,
        CreateTimeTable = 33,
        ExportTimeTableToCSV = 34,
        ShowPlot = 35,
        EditTimeTable = 36,
        SetDefaultTimeZone = 37,
        SetDeviceTimeZone = 38,
        SetViewTimeZone = 39,
        AIGeneral = 40,
        UnbindTimeNorm = 41,
        SetCulture = 42,
        DuplicateTimeStamp = 43,
        CreateTimeNorm = 44
    }
}
