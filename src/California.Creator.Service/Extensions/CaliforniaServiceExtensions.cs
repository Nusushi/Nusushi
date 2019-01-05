using AngleSharp.Css;
using California.Creator.Service.Builders;
using California.Creator.Service.Data;
using California.Creator.Service.Middlewares;
using California.Creator.Service.Models;
using California.Creator.Service.Models.Core;
using California.Creator.Service.Options;
using California.Creator.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nusushi.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace California.Creator.Service.Extensions
{
    public static class CaliforniaServiceExtensions
    {
        public static IApplicationBuilder UseCaliforniaCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CaliforniaCookieMiddleware>();
        }

        public static CaliforniaServiceBuilder AddCaliforniaServiceSqlite(this IServiceCollection services) => services.AddCaliforniaServiceSqlite(options => { });
        public static CaliforniaServiceBuilder AddCaliforniaService(this IServiceCollection services) => services.AddCaliforniaService(options => { });
        public static CaliforniaServiceBuilder AddCaliforniaServiceSqlite(this IServiceCollection services, Action<CaliforniaServiceOptions> configure)
        {
            // TODO document: database context filters do not work with sqlite => test unique constraints for affected properties (optional foreign key)
            return services.AddCaliforniaService(options =>
            {
                configure(options);
                options.DatabaseTechnology = CaliforniaServiceOptions.DbEngineSelector.Sqlite;
            });
        }
        public static CaliforniaServiceBuilder AddCaliforniaService(this IServiceCollection services, Action<CaliforniaServiceOptions> configure) // TODO configure vs optionsBuilder and expose functions => no validity check required?
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            // service configuration
            services.AddOptions();
            services.Configure<CaliforniaServiceOptions>(options => configure(options));
            services.Configure<CaliforniaServiceOptions>(options => SetStyleAtomDefaultProperties(options));
            services.AddAuthentication(); // TODO is this making db calls redundant to claim/project authorization?
            services.AddAuthorization(options =>
            {
                options.AddCaliforniaServicePolicies();
            });
            // register DI
            services.AddSingleton<IAuthorizationHandler, CaliforniaProjectAuthorizationHandler>();
            services.AddSingleton<CaliforniaDbStateSqlite, CaliforniaDbStateSqlite>(); // TODO only when using sqlite
            services.AddTransient<CaliforniaService, CaliforniaService>();
            // TODO options must be checked somewhere
            // check system compatibility TODO
            // check properly initialized service

            // TODO check events are properly defined (see TrackerLogMessages)

            return new CaliforniaServiceBuilder(services);
        }

        public static AuthorizationOptions AddCaliforniaServicePolicies(this AuthorizationOptions authorizationOptions)
        {
            authorizationOptions.AddPolicy(NusushiPolicy.AdminOnly, policy => policy.RequireClaim(ClaimTypes.Role, NusushiClaim.AdministratorRoleValue)); // TODO check if IssuerInstance must be checked for security
            return authorizationOptions;
        }

        public static string MakeCookieValue(this CaliforniaClientSession currentSession)
            => JsonConvert.SerializeObject(currentSession, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

        public static void UpdateCaliforniaOptionsCookie(this CaliforniaClientSession currentSession, HttpResponse response, string cookieName)
        {
            var updatedCookieValue = MakeCookieValue(currentSession);

            if (!string.IsNullOrEmpty(updatedCookieValue))
            {
                response.Cookies.Append(cookieName, updatedCookieValue,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        SameSite = SameSiteMode.Strict,
                        HttpOnly = false // TODO security documentation
                    });
            }
            else
            {
                response.Cookies.Delete(cookieName);
            }
        }

        public static void SetStyleAtomDefaultProperties(CaliforniaServiceOptions options)
        {
            // TODO check invalid style atom type in create store...
            // TODO document: css property => style atom type association may not be changed after style values are applied to atoms (=> never; or need complex migration routines that create/reapply style values + quantums)
            // initialize list for every style atom type
            foreach (var styleAtomTypeAsObject in Enum.GetValues(typeof(StyleAtomType)))
            {
                StyleAtomType styleAtomType = (StyleAtomType)styleAtomTypeAsObject;
                if (!options.StyleAtomDefaultProperties.ContainsKey(styleAtomType))
                {
                    options.StyleAtomDefaultProperties[styleAtomType] = new List<string>();
                }
            }
            options.StyleAtomDefaultProperties[StyleAtomType.Divider].AddRange(new string[] { PropertyNames.BorderColor });
            options.StyleAtomDefaultProperties[StyleAtomType.Box].AddRange(new string[] 
            {
                PropertyNames.FlexBasis,
                PropertyNames.FlexShrink,
                PropertyNames.FlexGrow,
                PropertyNames.Left,
                PropertyNames.Right,
                PropertyNames.Top,
                PropertyNames.Bottom,
                PropertyNames.Position,
                PropertyNames.JustifyContent,
                PropertyNames.Overflow
            });
            options.StyleAtomDefaultProperties[StyleAtomType.Font].AddRange(new string[] { PropertyNames.FontFamily, PropertyNames.FontWeight, PropertyNames.Color });
            options.StyleAtomDefaultProperties[StyleAtomType.Background].AddRange(new string[] 
            {
                PropertyNames.BackgroundColor,
                PropertyNames.BackgroundImage,
                PropertyNames.BackgroundOrigin,
                PropertyNames.BackgroundPosition,
                PropertyNames.BackgroundSize,
                PropertyNames.BackgroundRepeat,
                PropertyNames.BorderRadius,
                PropertyNames.Border,
                PropertyNames.BoxShadow,
                PropertyNames.BorderTop,
                PropertyNames.BorderRight,
                PropertyNames.BorderBottom,
                PropertyNames.BorderLeft,
                PropertyNames.Outline,
                "outline-offset", // TODO manual
            });
            options.StyleAtomDefaultProperties[StyleAtomType.Typography].AddRange(new string[] 
            {
                PropertyNames.LineHeight,
                PropertyNames.LetterSpacing,
                PropertyNames.FontSize,
                PropertyNames.TextAlign,
                PropertyNames.FontStretch,
                PropertyNames.FontStyle,
                PropertyNames.TextDecoration
            });
            options.StyleAtomDefaultProperties[StyleAtomType.Spacing].AddRange(new string[] 
            {
                PropertyNames.Margin,
                PropertyNames.MarginTop,
                PropertyNames.MarginBottom,
                PropertyNames.MarginLeft,
                PropertyNames.MarginRight,
                PropertyNames.Padding,
                PropertyNames.PaddingTop,
                PropertyNames.PaddingBottom,
                PropertyNames.PaddingLeft,
                PropertyNames.PaddingRight,
                PropertyNames.Display,
                PropertyNames.Flex,
                PropertyNames.FlexFlow,
                PropertyNames.FlexDirection,
                PropertyNames.FlexWrap,
                PropertyNames.Float,
                PropertyNames.Width,
                PropertyNames.MinWidth,
                PropertyNames.MaxWidth,
                PropertyNames.Height,
                PropertyNames.MinHeight,
                PropertyNames.MaxHeight,
                PropertyNames.AlignSelf
            });
            options.StyleAtomDefaultProperties[StyleAtomType.List].AddRange(new string[]
            {
                PropertyNames.ListStylePosition,
                PropertyNames.ListStyleType,
                PropertyNames.ListStyleImage
            });

            var allProps = typeof(PropertyNames).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static); // TEST list for completeness and TEST detect api changes
            var remainingProperties = allProps.Select(p => p.GetValue(null).ToString()).ToList();
            remainingProperties.AddRange(new string[]
            {
                "user-select",
                "-webkit-user-select"
            });
            // filter duplicates for every style atom type
            foreach (var styleAtomTypeAsObject in Enum.GetValues(typeof(StyleAtomType)))
            {
                StyleAtomType styleAtomType = (StyleAtomType)styleAtomTypeAsObject;
                var distinctProperties = options.StyleAtomDefaultProperties[styleAtomType].Distinct().ToList();
                options.StyleAtomDefaultProperties[styleAtomType] = distinctProperties;
                foreach (var prop in distinctProperties)
                {
                    remainingProperties.Remove(prop);
                }
            }
            options.StyleAtomDefaultProperties[StyleAtomType.Generic] = remainingProperties;
        }

        // TODO move to service
        internal static void ApplyStyleQuantum(this StyleAtom styleAtom, StyleQuantum styleQuantum, CaliforniaServiceOptions serviceOptions) // TEST _internal_ cannot be called from service consumer outside solution
        {
            if (!serviceOptions.StyleAtomDefaultProperties[styleAtom.StyleAtomType.Value].Contains(styleQuantum.CssProperty))
            {
                throw new InvalidOperationException("Wrong style atom type for css property.");
            }
            // replace already applied style quantum for the same css property
            var appliedQuantumsSameProperty = styleAtom.MappedQuantums.Where(mapping => mapping.StyleQuantum.CssProperty == styleQuantum.CssProperty);
            var appliedQuantumsCount = appliedQuantumsSameProperty.Count();
            if (appliedQuantumsCount > 1)
            {
                throw new ApplicationException("Multiple style quantums for the same css property are applied to one style atom.");
            }
            else if (appliedQuantumsCount == 1)
            {
                appliedQuantumsSameProperty.First().StyleQuantum = styleQuantum;
            }
            else
            {
                styleAtom.MappedQuantums.Add(new StyleAtomQuantumMapping()
                {
                    StyleQuantum = styleQuantum
                });
            }

            var appliedValuesSameProperty = styleAtom.AppliedValues.Where(val => val.CssProperty == styleQuantum.CssProperty);
            var appliedValuesCount = appliedValuesSameProperty.Count();
            if (appliedValuesCount > 1)
            {
                throw new ApplicationException("Multiple style values for the same css property are applied to one style atom.");
            }
            else if (appliedValuesCount == 1)
            {
                appliedValuesSameProperty.First().CssValue = styleQuantum.CssValue;
            }
            else
            {
                // TODO this should throw if by default a stylevalue must always exist for the css property of an applied quantum
                styleAtom.AppliedValues.Add(new StyleValue()
                {
                    CssProperty = styleQuantum.CssProperty,
                    CssValue = styleQuantum.CssValue
                });
            }
        }
        
        internal static void ApplyStyleAtom(this StyleMolecule styleMolecule, StyleAtom styleAtom, ResponsiveDevice responsiveDevice, string stateModifier)
        {
            styleMolecule.ApplyStyleAtomRange(new StyleAtom[] { styleAtom }, responsiveDevice, stateModifier);
        }
        
        internal static void ApplyStyleAtomRange(this StyleMolecule styleMolecule, IEnumerable<StyleAtom> styleAtoms, ResponsiveDevice responsiveDevice, string stateModifier) // TEST cannot be called from service consumer outside solution or move into internalextensions
        {
            if (stateModifier != null && stateModifier == "")
            {
                throw new InvalidOperationException("Supplied state modifier may not be empty. Use null instead.");
            }
            
            foreach (var styleAtom in styleAtoms)
            {
                var styleMoleculeMapping = new StyleMoleculeAtomMapping()
                {
                    ResponsiveDevice = responsiveDevice,
                    StateModifier = stateModifier
                };
                styleMolecule.MappedStyleAtoms.Add(styleMoleculeMapping);
                styleAtom.MappedToMolecule = styleMoleculeMapping;
            }
        }

        public static string GenerateJs(this CaliforniaProject californiaProject)
        {
            // TODO change class of html element and generate class css in css generator
            Dictionary<int, string> interactionDeclarationSnippets = new Dictionary<int, string>();
            Dictionary<int, string> interactionSnippetsForSwitch = new Dictionary<int, string>();
            foreach (var interaction in californiaProject.LayoutStyleInteractions) // TODO generate code only for rendered interactions // TODO code duplication with client app
            {
                if (interaction.LayoutStyleInteractionType != LayoutStyleInteractionType.StyleToggle)
                {
                    throw new NotImplementedException();
                    // TODO scroll event interaction
                    /*window.addEventListener("scroll", function(evt: UIEvent): void {
                        console.log(((evt.target as HTMLDocument).scrollingElement as HTMLBodyElement).scrollTop + "," + ((evt.target as HTMLDocument).scrollingElement as HTMLBodyElement).scrollHeight);
                    });*/
                }
                else
                {
                    string updatedCss = "";
                    string restoredOrNulledCss = "";
                    foreach (var styleMap in interaction.StyleValueInteractions)
                    {
                        var elementFind = $"var el = document.getElementById(\"t{styleMap.StyleValue.StyleAtom.MappedToMolecule.StyleMolecule.StyleForLayoutId}\");\n";
                        updatedCss += elementFind + $"el.style.setProperty(\"{styleMap.StyleValue.CssProperty}\", \"{styleMap.CssValue}\");";
                        restoredOrNulledCss += elementFind + $"el.style.setProperty(\"{styleMap.StyleValue.CssProperty}\", \"{styleMap.StyleValue.CssValue}\");";
                    }
                    var toggleVarName = $"t{interaction.LayoutAtomId}";
                    if (updatedCss.Length > 0 || restoredOrNulledCss.Length > 0)
                    {
                        interactionSnippetsForSwitch[interaction.LayoutStyleInteractionId] = $"if ({toggleVarName}) {{" +
                            $"{updatedCss}" +
                            $"}}" +
                            $"else {{" +
                            $"{restoredOrNulledCss}" +
                            $"}}\n" +
                            $"{toggleVarName} = !{toggleVarName};\n";
                        interactionDeclarationSnippets[interaction.LayoutStyleInteractionId] = toggleVarName;
                    }
                }
            }
            var publishedReloadIntervalMs = 10000;
            // same toggle routine for each interaction, path depends on element id
            return string.Join("\n", interactionDeclarationSnippets.Select(varKvp => $"var {varKvp.Value}=true;")) +
                        $"var tar = \"\";\nvar ifn = function() {{\n" +
                        $"switch(tar) {{\n" +
                        string.Join("\n", interactionSnippetsForSwitch.Select(kvp => $"case \"{interactionDeclarationSnippets[kvp.Key]}\":\n{kvp.Value}\nbreak;")) +
                        $"default:\nbreak;\n" +
                        $"}};\n" +  // end switch
                        $"}};\n" + // end function
                        "";//$"setTimeout(function(){{location.reload();}}, {publishedReloadIntervalMs})\n"; // TODO reload triggered by backend (signalR or custom js)
        }

        // magic string values and helper methods for bad data model TODO
        public const string CaliforniaViewStylesHolder = "[Internal] Special Styles";
        public const string InstanceableAtomsHolder = "[Internal] Box Instanceable Atoms";
        public static string GetStyleMoleculeNameForViewOfCaliforniaView(string californiaViewName)
        {
            return $"[Internal] {californiaViewName} View Style";
        }
        public static string GetStyleMoleculeNameForHtmlOfCaliforniaView(string californiaViewName)
        {
            return $"[Internal] {californiaViewName} Html Style";
        }
        public static string GetStyleMoleculeNameForBodyOfCaliforniaView(string californiaViewName)
        {
            return $"[Internal] {californiaViewName} Body Style";
        }

        public static string GeneratePedanticCss(this CaliforniaProject californiaProject)
        {
            string cssStyleSheet = californiaProject.UserDefinedCss;
            foreach (var californiaView in californiaProject.CaliforniaViews.Where(v => !v.IsInternal.Value))
            {
                if (californiaView.UserDefinedCss != null)
                {
                    cssStyleSheet += californiaView.UserDefinedCss;
                }
            }
            if (californiaProject.UserDefinedCss != null)
            {
                cssStyleSheet += californiaProject.UserDefinedCss;
            }
            foreach (var responsiveDevice in californiaProject.ResponsiveDevices.OrderBy(r => r.WidthThreshold)) // TODO will switch around devices with same widththreshold (usually multiple with val==0)
            {
                string cssPerDevice = "";
                foreach (var styleMolecule in californiaProject.StyleMolecules) // TODO optimize query speed everywhere: this loop can be parallelized on SMP architecture
                {
                    GenerateAndAppendCss(styleMolecule, responsiveDevice, ref cssPerDevice);
                }
                cssPerDevice.WrapMediaQuery(responsiveDevice);
                if (cssPerDevice.Length > 0)
                {
                    cssStyleSheet += cssPerDevice;
                }
            }
            return cssStyleSheet;
        }

        public static string WrapMediaQuery(this string cssRule, ResponsiveDevice responsiveDevice)
        {
            if (cssRule.Length > 0)
            {
                if (responsiveDevice.WidthThreshold > 0)
                {
                    return $"@media(min-width:{responsiveDevice.WidthThreshold}px){{{cssRule}}}";
                }
            }
            return cssRule;
        }

        public static void GenerateAndAppendCss(StyleMolecule styleMolecule, ResponsiveDevice responsiveDevice, ref string cssPerDevice)
        {
            string cssClassIdentifier = $".s{styleMolecule.StyleMoleculeId}";
            Dictionary<string, string> css = new Dictionary<string, string>(); // val => prop
            Dictionary<string, Dictionary<string, string>> stateModifiedCss = new Dictionary<string, Dictionary<string, string>>(); // pseudo => val => prop
            foreach (var map in styleMolecule.MappedStyleAtoms.Where(m => m.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId))
            {
                Dictionary<string, string> targetDict;
                if (map.StateModifier == null)
                {
                    targetDict = css;
                }
                else
                {
                    if (!stateModifiedCss.ContainsKey(map.StateModifier))
                    {
                        targetDict = new Dictionary<string, string>();
                        stateModifiedCss[map.StateModifier] = targetDict;
                    }
                    else
                    {
                        targetDict = stateModifiedCss[map.StateModifier];
                    }
                }
                foreach (var val in map.StyleAtom.AppliedValues)
                {
                    if (!string.IsNullOrEmpty(val.CssValue))
                    {
                        targetDict[val.CssProperty] = val.CssValue;
                    }
                }
            }
            string cssString = string.Join(";", css.Select(kvp => $"{kvp.Key}:{kvp.Value}").Cast<string>());
            if (cssString.Length > 0)
            {
                cssPerDevice += $"{cssClassIdentifier}{{{cssString}}}";
            }
            foreach (var stateModifierKvp in stateModifiedCss) // TODO handle null / empty css value => auto set to default or something
            {
                var pseudoDict = stateModifierKvp.Value;
                string pseudoCssString = string.Join(";", pseudoDict.Select(kvp => $"{kvp.Key}:{kvp.Value}").Cast<string>());
                cssPerDevice += $"{cssClassIdentifier}{stateModifierKvp.Key}{{{pseudoCssString}}}";
            }
        }

        public static string GenerateOptimizedCss(this CaliforniaSelectionOverlay californiaSelectionOverlay)
        {
            /*foreach (var styleMolecule in UserStyleMolecules.Dict.Values) TODO unused?
            {
                styleMolecule.StyleCssClasses.Clear();
                styleMolecule.StyleCssClasses.Add($"s{styleMolecule.Id}");
            }*/
            var styleCss = "";
            var combinedStyleCss = "";
            foreach (var californiaView in californiaSelectionOverlay.CaliforniaProject.CaliforniaViews.Where(v => !v.IsInternal.Value))
            {
                if (californiaView.UserDefinedCss != null)
                {
                    combinedStyleCss += californiaView.UserDefinedCss;
                }
            }
            if (californiaSelectionOverlay.CaliforniaProject.UserDefinedCss != null)
            {
                combinedStyleCss += californiaSelectionOverlay.CaliforniaProject.UserDefinedCss;
            }
            int maxIterations = 100000;
            //var styleIdsToIncludeRaw = new List<int>(); TODO
            var generatedCssClassNames = new List<string>();
            var newCssClassName = "a";
            // TODO only instanced layout molecules
            //styleIdsToIncludeRaw.Add(UserNavigations.Dict[0].StyleMoleculeId); TODO inline style handling
            foreach (var responsiveDevice in californiaSelectionOverlay.CaliforniaProject.ResponsiveDevices.OrderBy(r => r.WidthThreshold)) // TODO will switch around devices with same widththreshold (usually multiple with val==0)
            {
                bool isPseudo = false;
                for (int pseudoLoopIt = 0; pseudoLoopIt <= 1; pseudoLoopIt++)
                {
                    isPseudo = (pseudoLoopIt == 1);
                    /*if (styleKvp.Key == -1) TODO manual atom definitions
                    {
                        // atom level definitions
                        continue;
                    }*/
                    var styleMoleculesToProcess = californiaSelectionOverlay.CaliforniaProject.StyleMolecules.ToList();
                    var currentIteration = 0;
                    var settedCssPropertiesPerStyleMolecule = new Dictionary<int, List<KeyValuePair<string, string>>>();
                    var settedPseudoCssPropertiesPerStyleMolecule = new Dictionary<int, List<KeyValuePair<string, List<KeyValuePair<string, string>>>>>();
                    List<KeyValuePair<string, string>> settedCssProperties = null;
                    // prefill
                    foreach (var styleMolecule in californiaSelectionOverlay.CaliforniaProject.StyleMolecules)
                    {
                        if (!isPseudo)
                        {
                            settedCssProperties = styleMolecule.MappedStyleAtoms.Where(m => m.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && m.StateModifier == null).SelectMany(m => m.StyleAtom.AppliedValues).Where(va => !string.IsNullOrEmpty(va.CssValue)).Select(va => new KeyValuePair<string, string>(va.CssProperty, va.CssValue)).OrderBy(kvp => kvp.Key).ToList();
                            settedCssPropertiesPerStyleMolecule[styleMolecule.StyleMoleculeId] = settedCssProperties;
                        }
                        if (isPseudo)
                        {
                            var pseudoModifiersOfStyleMolecule = styleMolecule.MappedStyleAtoms.Where(m => m.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && m.StateModifier != null).Select(m => m.StateModifier).Distinct().OrderBy(mo => mo);
                            var settedPropsPerPseudo = new List<KeyValuePair<string, List<KeyValuePair<string, string>>>>();
                            foreach (var pseudoName in pseudoModifiersOfStyleMolecule)
                            {
                                settedCssProperties = styleMolecule.MappedStyleAtoms.Where(m => m.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && m.StateModifier == pseudoName).SelectMany(m => m.StyleAtom.AppliedValues).Where(va => !string.IsNullOrEmpty(va.CssValue)).Select(va => new KeyValuePair<string, string>(va.CssProperty, va.CssValue)).OrderBy(kvp => kvp.Key).ToList();
                                settedPropsPerPseudo.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(pseudoName, settedCssProperties));
                            }
                            settedPseudoCssPropertiesPerStyleMolecule[styleMolecule.StyleMoleculeId] = settedPropsPerPseudo;
                        }
                    }

                    while (styleMoleculesToProcess.Count > 0)
                    {
                        var currentStyle = styleMoleculesToProcess.First();
                        var pseudoModifiersOfStyleMolecule = currentStyle.MappedStyleAtoms.Where(m => m.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && m.StateModifier != null).Select(m => m.StateModifier).Distinct().OrderBy(mo => mo);
                        var innerPseudoLoopRuns = 1;
                        if (isPseudo)
                        {
                            innerPseudoLoopRuns = pseudoModifiersOfStyleMolecule.Count();
                        }
                        // dynamically create css classes for settedCssProperties
                        for (int innerPseudoLoopIt = 0; innerPseudoLoopIt < innerPseudoLoopRuns; innerPseudoLoopIt++)
                        {
                            var pseudoSelector = "";
                            if (isPseudo)
                            {
                                pseudoSelector = pseudoModifiersOfStyleMolecule.ElementAt(innerPseudoLoopIt);
                            }

                            List<KeyValuePair<string, string>> currentSettetCssProperties = new List<KeyValuePair<string, string>>();
                            if (!isPseudo)
                            {
                                currentSettetCssProperties = settedCssPropertiesPerStyleMolecule[currentStyle.StyleMoleculeId];
                            }
                            else
                            {
                                var pseudoProps = settedPseudoCssPropertiesPerStyleMolecule[currentStyle.StyleMoleculeId].FirstOrDefault(kvp => kvp.Key == pseudoSelector);
                                if (!string.IsNullOrEmpty(pseudoProps.Key))
                                {
                                    currentSettetCssProperties = pseudoProps.Value;
                                }
                            }
                            if (currentIteration > maxIterations)
                            {
                                throw new ArgumentException("Could not assemble CSS");
                            }
                            currentIteration++;

                            // process all css properties of style molecule => find common properties package
                            while (currentSettetCssProperties.Count > 0)
                            {
                                if (currentIteration > maxIterations)
                                {
                                    throw new ArgumentException("Could not assemble CSS");
                                }
                                currentIteration++;
                                // stage 1 => find property shared across most styles
                                var sharingStyleIds = new Dictionary<int, List<int>>();
                                for (int i = 0; i < currentSettetCssProperties.Count; i++)
                                {
                                    var currentSharedStyles = new List<int>();
                                    var currentPropertyKvp = currentSettetCssProperties[i];
                                    foreach (var styleMolecule in styleMoleculesToProcess)
                                    {
                                        var correspondingPropertyKvp = new KeyValuePair<string, string>();
                                        if (!isPseudo)
                                        {
                                            correspondingPropertyKvp = settedCssPropertiesPerStyleMolecule[styleMolecule.StyleMoleculeId].FirstOrDefault(innerKvp => innerKvp.Key == currentPropertyKvp.Key);
                                        }
                                        else
                                        {
                                            var pseudoCorresponding = settedPseudoCssPropertiesPerStyleMolecule[styleMolecule.StyleMoleculeId].FirstOrDefault(kvp => kvp.Key == pseudoSelector);
                                            if (!string.IsNullOrEmpty(pseudoCorresponding.Key))
                                            {
                                                correspondingPropertyKvp = pseudoCorresponding.Value.FirstOrDefault(innerKvp => innerKvp.Key == currentPropertyKvp.Key);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(correspondingPropertyKvp.Key))
                                        {
                                            // property matches, value is not null
                                            if (correspondingPropertyKvp.Value == currentPropertyKvp.Value)
                                            {
                                                currentSharedStyles.Add(styleMolecule.StyleMoleculeId);
                                            }
                                        }
                                    }
                                    sharingStyleIds[i] = currentSharedStyles;
                                }
                                var mostSharedStyleIdsKvp = sharingStyleIds.OrderBy(kvp => kvp.Value.Count);
                                var mostSharedStylesPropertyKvp = mostSharedStyleIdsKvp.ElementAt(0);
                                // stage 2 => find all properties shared by selected styles
                                var sharedCssPropertyPackage = new Dictionary<string, string>();
                                for (int i = 0; i < currentSettetCssProperties.Count; i++)
                                {

                                    var currentPropertyKvp = currentSettetCssProperties[i];
                                    var isCommonForSharingStyles = true;
                                    if (!isPseudo)
                                    {
                                        isCommonForSharingStyles = mostSharedStylesPropertyKvp.Value.All(sharingStyleId => settedCssPropertiesPerStyleMolecule[sharingStyleId].Contains(currentPropertyKvp));
                                    }
                                    else
                                    {
                                        isCommonForSharingStyles = mostSharedStylesPropertyKvp.Value.All(sharingStyleId => {
                                            var pseudoPropsForSharing = settedPseudoCssPropertiesPerStyleMolecule[sharingStyleId].FirstOrDefault(kvp => kvp.Key == pseudoSelector);
                                            if (pseudoPropsForSharing.Key != null)
                                            {
                                                return pseudoPropsForSharing.Value.Contains(currentPropertyKvp);
                                            }
                                            return false;
                                        });
                                    }
                                    if (isCommonForSharingStyles)
                                    {
                                        sharedCssPropertyPackage[currentPropertyKvp.Key] = currentPropertyKvp.Value;
                                    }
                                }
                                foreach (var sharedStyleId in mostSharedStylesPropertyKvp.Value)
                                {
                                    foreach (var extractedCssPropKvp in sharedCssPropertyPackage)
                                    {
                                        if (!isPseudo)
                                        {
                                            settedCssPropertiesPerStyleMolecule[sharedStyleId].Remove(extractedCssPropKvp);
                                        }
                                        else
                                        {
                                            settedPseudoCssPropertiesPerStyleMolecule[sharedStyleId].First(kvp => kvp.Key == pseudoSelector).Value.Remove(extractedCssPropKvp);
                                        }
                                    }
                                }
                                // modify css classes
                                newCssClassName = GetUniqueName("a", generatedCssClassNames, null);
                                generatedCssClassNames.Add(newCssClassName);
                                foreach (var sharingStyleId in mostSharedStylesPropertyKvp.Value)
                                {
                                    if (californiaSelectionOverlay.StyleMoleculeCssClassMapping.ContainsKey(sharingStyleId))
                                    {
                                        californiaSelectionOverlay.StyleMoleculeCssClassMapping[sharingStyleId].Add(newCssClassName);
                                    }
                                    else
                                    {
                                        californiaSelectionOverlay.StyleMoleculeCssClassMapping[sharingStyleId] = new List<string>() { newCssClassName };
                                    }
                                }
                                var styleCssTemp = string.Join("", sharedCssPropertyPackage.Where(property => !string.IsNullOrEmpty(property.Value)).Select(prop => $"{prop.Key}:{prop.Value};"));
                                if (!string.IsNullOrEmpty(styleCssTemp))
                                {
                                    if (!isPseudo)
                                    {
                                        styleCss += $".{newCssClassName}{{{styleCssTemp}}}\n";
                                    }
                                    else
                                    {
                                        styleCss += $".{newCssClassName}{pseudoSelector}{{{styleCssTemp}}}\n";
                                    }
                                }
                                sharingStyleIds.Clear();
                                sharedCssPropertyPackage.Clear();
                            }
                        }
                        styleMoleculesToProcess.RemoveAt(0);
                    }
                    /*foreach (var rawStyleId in styleIdsToIncludeRaw) TODO
                    {
                        var rawStyle = UserStyleMolecules.Dict[rawStyleId];
                        styleCss += GenerateCss(rawStyle, styleKvp.Key, false);
                    }*/
                    combinedStyleCss += styleCss.WrapMediaQuery(responsiveDevice);
                    styleCss = "";
                }
            }
            return combinedStyleCss;
        }

        public static string GetUniqueName(string targetName, IEnumerable<string> existingNames, char? splitMarker = '-')
        {
            // TODO add different naming schemes
            if (!existingNames.Any(existingName => existingName.Equals(targetName, StringComparison.OrdinalIgnoreCase)))
            {
                return targetName;
            }
            int counter = 0;
            if (splitMarker != null)
            {
                var lastSplitIndex = targetName.LastIndexOf(splitMarker.Value);
                if (lastSplitIndex > 0)
                {
                    if (int.TryParse(targetName.Substring(lastSplitIndex), out counter))
                    {
                        targetName = targetName.Substring(0, lastSplitIndex);
                    }
                }
            }
            var uniqueAtomName = targetName;
            do
            {
                var splitValue = "";
                if (splitMarker.HasValue)
                {
                    splitValue = splitMarker.Value.ToString();
                }
                uniqueAtomName = targetName + $"{splitValue}{counter}";
                counter++;
                if (counter > CaliforniaServiceOptions.AutoGeneratedCssClassesMaxLength)
                {
                    throw new ArgumentOutOfRangeException($"Cannot find unique name for ({targetName})"); // TODO naming scheme
                }
            }
            while (existingNames.Any(existingName => existingName.Equals(uniqueAtomName, StringComparison.OrdinalIgnoreCase)));
            return uniqueAtomName;
        }
    }
}
