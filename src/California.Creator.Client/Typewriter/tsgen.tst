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
    const string PreventDeveloperCodePropagationToken = "PreventDeveloperCodePropagation";

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
        string californiaStoreStub = "interface CaliforniaStore { CaliforniaStoreId: string; }\n";
        switch (classOrInterfaceName)
        {
            case "CaliforniaController":
                return "///<reference types=\"jquery\" />\nimport * as helpers from \"./tsgenHelpers\";";
            case "CaliforniaUserDefaults":
                return californiaStoreStub;
            case "CaliforniaProject":
                return "import {StyleQuantum} from \"./StyleQuantum\";\nimport {StyleAtom} from \"./StyleAtom\";\nimport {StyleMolecule} from \"./StyleMolecule\";\n"
                    + "import {ResponsiveDevice} from \"./ResponsiveDevice\";\nimport {ContentAtom} from \"./ContentAtom\";\nimport {PictureContent} from \"./PictureContent\";\n"
                    + "import {CaliforniaView} from \"./CaliforniaView\";\nimport {SharedProjectInfo} from \"./SharedProjectInfo\";\nimport {StyleValue} from \"./StyleValue\";\nimport {LayoutBase} from \"./LayoutBase\";\nimport {LayoutStyleInteraction} from \"./LayoutStyleInteraction\";\nimport {CaliforniaSelectionOverlay} from \"./CaliforniaSelectionOverlay\";";
            case "StyleValue":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleAtom} from \"./StyleAtom\";";
            case "StyleQuantum":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleAtomQuantumMapping} from \"./StyleAtomQuantumMapping\";";
            case "StyleAtom":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleAtomType} from \"./StyleAtomType\";\nimport {StyleAtomQuantumMapping} from \"./StyleAtomQuantumMapping\";\nimport {StyleValue} from \"./StyleValue\";\nimport {StyleMoleculeAtomMapping} from \"./StyleMoleculeAtomMapping\";";
            case "StyleAtomQuantumMapping":
                return "import {StyleAtom} from \"./StyleAtom\";\nimport {StyleQuantum} from \"./StyleQuantum\";";
            case "StyleMolecule":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleMoleculeAtomMapping} from \"./StyleMoleculeAtomMapping\";\nimport {ContentAtom} from \"./ContentAtom\";\nimport {LayoutBase} from \"./LayoutBase\";";
            case "ContentAtom":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleMolecule} from \"./StyleMolecule\";\n"
                    + "import {ContentAtomType} from \"./ContentAtomType\";\nimport {PictureContent} from \"./PictureContent\";\nimport {CaliforniaView} from \"./CaliforniaView\";\n"
                    + "import {LayoutAtom} from \"./LayoutAtom\";";
            case "ResponsiveDevice":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleMoleculeAtomMapping} from \"./StyleMoleculeAtomMapping\";";
            case "StyleMoleculeAtomMapping":
                return "import {StyleMolecule} from \"./StyleMolecule\";\nimport {StyleAtom} from \"./StyleAtom\";\nimport {ResponsiveDevice} from \"./ResponsiveDevice\";";
            case "PictureContent":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {ContentAtom} from \"./ContentAtom\";";
            case "CaliforniaSelectionOverlay":
                return "import {StyleMolecule} from \"./StyleMolecule\";";
            case "CaliforniaView":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {LayoutRow} from \"./LayoutRow\";\nimport {QueryViewLayoutBoxMapping} from \"./QueryViewLayoutBoxMapping\";";
            case "LayoutAtom":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {ContentAtom} from \"./ContentAtom\";\nimport {LayoutBox} from \"./LayoutBox\";\nimport {StyleMolecule} from \"./StyleMolecule\";\nimport {LayoutType} from \"./LayoutType\";\nimport {LayoutStyleInteraction} from \"./LayoutStyleInteraction\";";
            case "LayoutBox":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {LayoutRow} from \"./LayoutRow\";\nimport {LayoutAtom} from \"./LayoutAtom\";\nimport {StyleMolecule} from \"./StyleMolecule\";\nimport {QueryViewLayoutBoxMapping} from \"./QueryViewLayoutBoxMapping\";\nimport {LayoutType} from \"./LayoutType\";\nimport {SpecialLayoutBoxType} from \"./SpecialLayoutBoxType\";";
            case "LayoutRow":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {LayoutBox} from \"./LayoutBox\";\nimport {StyleMolecule} from \"./StyleMolecule\";\nimport {CaliforniaView} from \"./CaliforniaView\";\nimport {LayoutType} from \"./LayoutType\";";
            case "LayoutBase":
                return "import {StyleMolecule} from \"./StyleMolecule\";\nimport {LayoutType} from \"./LayoutType\";";
            case "CaliforniaClientViewModel":
                return "import {CaliforniaProject} from \"./CaliforniaProject\";\nimport {StyleQuantum} from \"./StyleQuantum\";\nimport {CaliforniaClientPartialData} from \"./CaliforniaClientPartialData\";";
            case "CaliforniaClientPartialData":
                return "import {ContentAtom} from \"./ContentAtom\";";
            case "SharedProjectInfo":
                return californiaStoreStub + "import {CaliforniaProject} from \"./CaliforniaProject\";";
            case "QueryViewLayoutBoxMapping":
                return "import {CaliforniaView} from \"./CaliforniaView\";\nimport {LayoutBox} from \"./LayoutBox\";";
            case "LayoutStyleInteraction":
                return "import {StyleValueInteractionMapping} from \"./StyleValueInteractionMapping\";\nimport {LayoutStyleInteractionType} from \"./LayoutStyleInteractionType\";";
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

    string CustomUrlCalifornia(Method m)
    {
        var url = m.Url().Replace("api/California/", "");
        if (url.StartsWith("${id}"))
        {
            url = "?id=" + url;
        }
        url = $"/california/{m.Name}" + url;
		return url;
    }

    string PropertyDefinition(Property p)
    {
        var propertyName = p.Name;
        // TODO 
        //if (!p.Type.IsPrimitive)
        //{
        //    return $"{propertyName}: {TypeOrUndefined(p.Type)};";
        //}
		//else 
        if ((p.Type.FullName == "System.Int32?" || p.Type.FullName == "System.Int64?") && !p.Attributes.Any(attr => attr.Name == "Required"))
		{
            return $"{propertyName}: {TypeOrUndefined(p.Type)};";
		}
        else if (p.Type.IsDate)
        {
            return $"{propertyName}: {p.Type.Name} | string"; // workaround: json serialized date is passed as string
        }
        else if (p.Type.FullName == "System.String" && !p.Attributes.Any(attr => attr.Name == "Required"))
        {
            return $"{propertyName}: {TypeOrUndefined(p.Type)}";
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

    static string[] IgnoreClasses = new string[] {};
}
$Classes(c => c.Name == "CaliforniaController")[
$ImportFor 
import { CaliforniaApp } from "./../Models/CaliforniaApp";
let currentApp: CaliforniaApp;
export class $Name {
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
        };$Methods(m => !m.Attributes.Any(at => at.Name == PreventDeveloperCodePropagationToken))[
        public $HelperMethodName = ($MethodDefinition) : $HelperMethodResult => {
            return helpers.$HelperMethod(`$CustomUrlCalifornia`, "$HttpMethod", $RequestData, this.beforeSendAjax);
        }]
    }]$Classes(c => (c.Namespace.Contains("Nusushi.Models.CaliforniaViewModels") || c.Namespace.Contains("California.Creator.Service.Models.Core")) && !IgnoreClasses.Any(cl => c.Name.Contains(cl)))[
$ImportFor 
export class $Name$TypeParameters { $Properties(p => !p.Attributes.Any(at => at.Name == PreventDeveloperCodePropagationToken || at.Name == "JsonIgnore"))[
    $PropertyDefinition]
}]$Enums(e => e.Namespace.Contains("California.Creator.Service.Models.Core"))[
export enum $Name {$Values[
    $Name = $Value,]
}]