${
    // exports controller API methods and data models to typescript
    // expose as package using multiple export * from "./../Typewriter/ExportedClass"; in a centralized file PackageGenerated.ts
    // consume package with import { ExportedClass, ExportedClass2 } from "./PackageGenerated";
    // imports must be manually adjusted per class
    // data types are further refined/adjusted for javascript compatibility: extended nullable and date export
    // properties with [JsonIgnore] attribute are ignored
    // formatting for jQuery.ajax GET/POST requests, made accessible by helper methods (tsgenHelpers)
    // make sure C# project compiles or strange null reference errors appear during template rendering
    // TODO: generate specialized edit functions for every combination of commonly changed settings (e.g. colorR+G+B vs. name vs. ...)
    using Typewriter.Extensions.Types;
    using Typewriter.Extensions.WebApi;

    string ReturnType(Method m) => m.Type.Name == "IHttpActionResult" ? "void" : m.Type.Name;
    string ServiceName(Class c) => c.Name.Replace("Controller", "Service");

    string ImportFor(Class c) 
    {
        return ImportTokens(c.Name);
    }

    string ImportFor(Interface i) 
    {
        return ImportTokens(i.Name);
    }

    string ImportTokens(string classOrInterfaceName)
    {
        string trackerStoreStub = "interface TrackerStore { TrackerStoreId: string; }\n";
        switch (classOrInterfaceName)
        {
            case "TokyoController":
                return "///<reference types=\"jquery\" />\nimport * as helpers from \"./tsgenHelpers\";\nimport {EditTimeStampPostViewModel} from \"./EditTimeStampPostViewModel\";\nimport {CreateTimeStampPostViewModel} from \"./CreateTimeStampPostViewModel\";";
            case "TimeTablesViewModel":
                return "interface SelectListItem {\nText: string;\nValue: any;\n};\nimport {TimeTableViewModel} from \"./TimeTableViewModel\";\n";
            case "TimeTableViewModel":
                return "import {TimeTable} from \"./TimeTable\";\n";
            case "IndexViewModel":
                return "import {TimeTablesViewModel} from \"./TimeTablesViewModel\";\n";
            case "SharedTimeTableInfo":
                return trackerStoreStub + "import {TimeTable} from \"./TimeTable\";";
            case "TimeTable":
                return trackerStoreStub + "import {WorkUnit} from \"./WorkUnit\";\nimport {SharedTimeTableInfo} from \"./SharedTimeTableInfo\";\nimport {TimeRange} from \"./TimeRange\";\nimport {Plot} from \"./Plot\";";
            case "WorkUnit":
                return "import {TimeNorm} from \"./TimeNorm\";\nimport {TimeTable} from \"./TimeTable\";\nimport {TrackerUserDefaults} from \"./TrackerUserDefaults\";";
            case "TimeNormTag":
                return trackerStoreStub + "import {TimeNormTagMapping} from \"./TimeNormTagMapping\";";
            case "TimeStamp":
                return "import {TimeNorm} from \"./TimeNorm\";";
            case "Plot":
                return "import {TimeTable} from \"./TimeTable\";";
            case "TimeNorm":
                return "import {TimeNormTagMapping} from \"./TimeNormTagMapping\";\nimport {TimeStamp} from \"./TimeStamp\";\nimport {ProductivityRating} from \"./ProductivityRating\";\nimport {WorkUnit} from \"./WorkUnit\";";
            case "TimeNormTagMapping":
                return "import {TimeNorm} from \"./TimeNorm\";\nimport {TimeNormTag} from \"./TimeNormTag\";";
            case "TimeRange":
                return "import {TimeTable} from \"./TimeTable\";\n";
            case "ProductivityRating":
                return "import {TimeNorm} from \"./TimeNorm\";\n";
            case "TrackerClientViewModel":
                return "import {TimeStamp} from \"./TimeStamp\";\nimport {TimeTableViewModel} from \"./TimeTableViewModel\";\nimport {WorkUnit} from \"./WorkUnit\";\nimport {ProductivityRating} from \"./ProductivityRating\";\nimport {TrackerTimeZone} from \"./TrackerTimeZone\";\nimport {TimeNormTag} from \"./TimeNormTag\";\nimport {TimeNorm} from \"./TimeNorm\";\nimport {TrackerCultureInfo} from \"./TrackerCultureInfo\";";
            case "TrackerUserDefaults":
                return trackerStoreStub + "import {WorkUnit} from \"./WorkUnit\";\n";
            default:
                return "";
        }
    }

    string HelperMethodName(Method m)
    {
        return m.Name + HelperMethod(m);
    }

    string HelperMethod(Method m)
    {
        switch(m.Type.Name)
        {
            case "JsonResult":
                return "Json";
            case "IActionResult":
                return "Action";
            default:
                return "Action";
        }
    }

    string HelperMethodResult(Method m)
    {
        switch(m.Type.Name)
        {
            case "JsonResult":
                return "JQueryXHR";
            case "IActionResult":
                return "JQueryXHR"; // TODO differentiate redirect actions
            default:
                return "JQueryXHR";
        }
    }

    string CustomUrlTokyo(Method m)
    {
        var url = m.Url().Replace("api/Tokyo/", "");
        if (url.StartsWith("${id}"))
        {
            url = "?id=" + url;
        }
        url = $"/tokyo/{m.Name}" + url;
		return url;
    }

    string PropertyDefinition(Property p)
    {
        var propertyName = p.Name;
        if (!p.Type.IsPrimitive)
        {
            return $"{propertyName}: {TypeOrUndefined(p.Type)};";
        }
		else if (p.Type.FullName == "System.Int32?" || p.Type.FullName == "System.Int64?")
		{
			return $"{propertyName}: {TypeOrUndefined(p.Type)};";
		}
        else if (p.Type.IsDate)
        {
            return $"{propertyName}: {p.Type.Name} | string"; // workaround: json serialized date is passed as string
        }
        else
        {
            return $"{propertyName}: {p.Type.Name};";
        }
    }

    string TypeOrUndefined(Type t)
    {
        return $"{t.Name} | undefined";
    }

    string MethodDefinition(Method m)
    {
        var parameterDefinitions = new List<string>();
        foreach (var p in m.Parameters)
        {
            var parameterTypeName = p.Type.Name;
            if (p.Type.FullName == "System.Int32?" || p.Type.FullName == "System.Int64?")
            {
                parameterTypeName = TypeOrUndefined(p.Type);
            }
            parameterDefinitions.Add($"{p.name}: {parameterTypeName}");
        }
        return string.Join(", ", parameterDefinitions);
    }

    static string[] IgnoreClasses = new string[] {"TrackerInsights"};
}
$Classes(c => c.Name == "TokyoController")[
$ImportFor 
export class $Name {
        constructor() { 
        } $Methods[
        public $HelperMethodName = ($MethodDefinition) : $HelperMethodResult => {
            return helpers.$HelperMethod(`$CustomUrlTokyo`, "$HttpMethod", $RequestData);
        }]
    }]$Classes(c => (c.Namespace.Contains("Nusushi.Models.TokyoViewModels") || c.Namespace.Contains("Tokyo.Service.Models.Core")) && !IgnoreClasses.Any(cl => c.Name.Contains(cl)))[
$ImportFor 
export class $Name$TypeParameters { $Properties(p => !p.Attributes.Any(at => at.Name == "JsonIgnore"))[
    $PropertyDefinition]
}]$Enums(e => e.Namespace.Contains("Tokyo.Service.Models.Core"))[
export enum $Name {$Values[
    $Name = $Value,]
}]