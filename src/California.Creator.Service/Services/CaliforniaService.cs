using AngleSharp.Css;
using California.Creator.Service.Data;
using California.Creator.Service.Extensions;
using California.Creator.Service.Models;
using California.Creator.Service.Models.Core;
using California.Creator.Service.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static California.Creator.Service.Extensions.CaliforniaServiceExtensions;

namespace California.Creator.Service.Services
{
    public class CaliforniaService
    {
        private readonly CaliforniaDbContext _data;
        private readonly IAuthorizationService _authorizationService;
        private readonly CaliforniaDbStateSqlite _californiaDbStateSqlite;
        private readonly CaliforniaServiceOptions _serviceOptions;

        public CaliforniaService(CaliforniaDbContext californiaDbContext, IAuthorizationService authorizationService,
            IOptions<CaliforniaServiceOptions> serviceOptions, CaliforniaDbStateSqlite californiaDbStateSqlite)
        {
            _data = californiaDbContext;
            _authorizationService = authorizationService;
            _californiaDbStateSqlite = californiaDbStateSqlite;
            _serviceOptions = serviceOptions.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="californiaContext">User context</param>
        /// <param name="includeCaliforniaProjects">Setup query tree</param>
        /// <param name="shouldTrack">Setup change tracker</param>
        /// <returns>California store for context user id. Null if store does not exist.</returns>
        public async Task<CaliforniaStore> ReadCaliforniaStoreAsync(CaliforniaContext californiaContext, bool includeCaliforniaProjects, bool shouldTrack)
        {
            if (includeCaliforniaProjects)
            {
                if (shouldTrack)
                {
                    return await _data.CaliforniaStores
                        .Include(store => store.CaliforniaProjects)
                        .FirstOrDefaultAsync(store => store.CaliforniaStoreId == californiaContext.UserId);
                }
                else
                {
                    return await _data.CaliforniaStores
                        .Include(store => store.CaliforniaProjects)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(store => store.CaliforniaStoreId == californiaContext.UserId);
                }
            }
            else
            {
                if (shouldTrack)
                {
                    return await _data.CaliforniaStores
                        .FirstOrDefaultAsync(store => store.CaliforniaStoreId == californiaContext.UserId);
                }
                else
                {
                    return await _data.CaliforniaStores
                        .AsNoTracking()
                        .FirstOrDefaultAsync(store => store.CaliforniaStoreId == californiaContext.UserId);
                }
            }
        }

        private StyleAtom CloneStyleAtomDuringInitialization(StyleAtom referenceStyleAtom, string name)
        {
            var clonedAtom = new StyleAtom()
            {
                Name = name,
                StyleAtomType = referenceStyleAtom.StyleAtomType,
                AppliedValues = new List<StyleValue>(referenceStyleAtom.AppliedValues.Select(v => new StyleValue()
                {
                    CssValue = v.CssValue,
                    CssProperty = v.CssProperty
                }))
            };
            foreach (var quantum in referenceStyleAtom.MappedQuantums)
            {
                clonedAtom.ApplyStyleQuantum(quantum.StyleQuantum, _serviceOptions);
            }
            return clonedAtom;
        }

        public async Task CreateUserStoreAndInitializeAsync(CaliforniaContext californiaContext)
        {
            var createdStore = new CaliforniaStore()
            {
                CaliforniaStoreId = californiaContext.UserId
            };

            var defaultProject = new CaliforniaProject() { };
            createdStore.CaliforniaProjects.Add(defaultProject);

            _data.CaliforniaStores.Add(createdStore);

            await _data.SaveChangesAsync();
        }

        public async Task CreateDefaultProjectDataAsync(CaliforniaContext californiaContext) // can be used as TEST for database model (together with remove data) and as initial TEST data and as TEST for average user data size
        {
            var californiaStore = await _data.Set<CaliforniaStore>()
                .Include(s => s.CaliforniaProjects)
                    .ThenInclude(pr => pr.ResponsiveDevices) // revision == 1
                .FirstAsync(s => s.CaliforniaStoreId == californiaContext.UserId);
            var californiaProjectId = californiaStore.CaliforniaProjects.First().CaliforniaProjectId;

            var californiaProject = await _data.Set<CaliforniaProject>().FirstAsync(p => p.CaliforniaProjectId == californiaProjectId);

            var expectedRevision = 0;

            if (californiaProject.ProjectDefaultsRevision == expectedRevision++)
            {
                // --- responsive devices ---
                var responsiveDeviceNone = new ResponsiveDevice() { Name = "None", NameShort = "None", WidthThreshold = -1 }; // TODO const name is fixed
                var responsiveDeviceXs = new ResponsiveDevice() { Name = "Extra small devices", NameShort = "xs", WidthThreshold = 0 };
                var responsiveDeviceSm = new ResponsiveDevice() { Name = "Small devices", NameShort = "sm", WidthThreshold = 544 };
                var responsiveDeviceMd = new ResponsiveDevice() { Name = "Medium devices", NameShort = "md", WidthThreshold = 768 };
                var responsiveDeviceLg = new ResponsiveDevice() { Name = "Large devices", NameShort = "lg", WidthThreshold = 992 };
                var responsiveDeviceXl = new ResponsiveDevice() { Name = "Extra large devices", NameShort = "xl", WidthThreshold = 1200 };

                californiaProject.ResponsiveDevices.AddRange(new ResponsiveDevice[]
                {
                responsiveDeviceNone,
                responsiveDeviceXs,
                responsiveDeviceSm,
                responsiveDeviceMd,
                responsiveDeviceLg,
                responsiveDeviceXl
                });

                // --- style quantums ---
                var defaultStyleQuantums = new List<StyleQuantum>();
                // TODO my playground
                var darkerGrayBgQuantum = new StyleQuantum() { Name = $"Darker-gray-{PropertyNames.BackgroundColor}", CssProperty = PropertyNames.BackgroundColor, CssValue = "rgb(222, 222, 222)" };
                var lighterGrayBgQuantum = new StyleQuantum() { Name = $"Lighter-gray-{PropertyNames.BackgroundColor}", CssProperty = PropertyNames.BackgroundColor, CssValue = "rgb(242, 242, 242)" };
                var whiteBgQuantum = new StyleQuantum() { Name = $"White-{PropertyNames.BackgroundColor}", CssProperty = PropertyNames.BackgroundColor, CssValue = "white" };
                var blackBgQuantum = new StyleQuantum() { Name = $"Black-{PropertyNames.BackgroundColor}", CssProperty = PropertyNames.BackgroundColor, CssValue = "black" };
                defaultStyleQuantums.AddRange(new StyleQuantum[] { darkerGrayBgQuantum, lighterGrayBgQuantum, whiteBgQuantum, blackBgQuantum });
                // TODO tokyotracker playground
                var whiteColorQuantum = new StyleQuantum() { Name = $"White-{PropertyNames.Color}", CssProperty = PropertyNames.Color, CssValue = "white" };
                var blackColorQuantum = new StyleQuantum() { Name = $"Black-{PropertyNames.Color}", CssProperty = PropertyNames.Color, CssValue = "black" };
                var darkBlueColorQuantum = new StyleQuantum() { Name = $"Dark-blue-{PropertyNames.Color}", CssProperty = PropertyNames.Color, CssValue = "rgb(14, 30, 86)" };
                var tokyoBlueColorQuantum = new StyleQuantum() { Name = $"Tokyo-blue-{PropertyNames.Color}", CssProperty = PropertyNames.Color, CssValue = "rgb(42, 178, 191)" };
                var tokyoRedColorQuantum = new StyleQuantum() { Name = $"Tokyo-red-{PropertyNames.Color}", CssProperty = PropertyNames.Color, CssValue = "rgb(200, 0, 0)" };
                defaultStyleQuantums.AddRange(new StyleQuantum[] { darkBlueColorQuantum, tokyoBlueColorQuantum, whiteColorQuantum, blackColorQuantum, tokyoRedColorQuantum });
                // TODO unique name check for new style quantums/atoms? => prevents programming errors and improves user experience (no duplicates), but also user has to define new names
                var gutterNegativeMarginLQuantum = new StyleQuantum() { Name = $"Neg-Gutter-{PropertyNames.MarginLeft}", CssProperty = PropertyNames.MarginLeft, CssValue = "-15px" };
                var gutterNegativeMarginRQuantum = new StyleQuantum() { Name = $"Neg-Gutter-{PropertyNames.MarginRight}", CssProperty = PropertyNames.MarginRight, CssValue = "-15px" };
                var gutterPaddingLQuantum = new StyleQuantum() { Name = $"Gutter-{PropertyNames.PaddingLeft}", CssProperty = PropertyNames.PaddingLeft, CssValue = "15px" };
                var gutterPaddingRQuantum = new StyleQuantum() { Name = $"Gutter-{PropertyNames.PaddingRight}", CssProperty = PropertyNames.PaddingRight, CssValue = "15px" };
                defaultStyleQuantums.AddRange(new StyleQuantum[] { gutterNegativeMarginLQuantum, gutterNegativeMarginRQuantum, gutterPaddingLQuantum, gutterPaddingRQuantum });

                for (int i = 1; i <= 12; i++)
                {
                    var widthPercentage = (100 * ((float)i / 12)).ToString("0.0000", CultureInfo.InvariantCulture) + "%"; // TODO make sure culture code is correct everywhere
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"col-{i}", CssProperty = PropertyNames.Width, CssValue = widthPercentage });
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"col-{i}-max", CssProperty = PropertyNames.MaxWidth, CssValue = widthPercentage });
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"col-{i}-flex", CssProperty = PropertyNames.FlexBasis, CssValue = widthPercentage });

                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"offset-{i}", CssProperty = PropertyNames.MarginLeft, CssValue = widthPercentage });
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"push-{i}", CssProperty = PropertyNames.Left, CssValue = widthPercentage });
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"pull-{i}", CssProperty = PropertyNames.Right, CssValue = widthPercentage });
                }

                defaultStyleQuantums.Add(new StyleQuantum() { Name = $"flex-auto", CssProperty = PropertyNames.FlexBasis, CssValue = "auto" });
                defaultStyleQuantums.Add(new StyleQuantum() { Name = $"width-auto", CssProperty = PropertyNames.Width, CssValue = "auto" });

                var responsiveQuantumSm = new StyleQuantum() { Name = $"{PropertyNames.MaxWidth}-{responsiveDeviceSm.NameShort}", CssProperty = PropertyNames.MaxWidth, CssValue = "576px" };
                var responsiveQuantumMd = new StyleQuantum() { Name = $"{PropertyNames.MaxWidth}-{responsiveDeviceMd.NameShort}", CssProperty = PropertyNames.MaxWidth, CssValue = "720px" };
                var responsiveQuantumLg = new StyleQuantum() { Name = $"{PropertyNames.MaxWidth}-{responsiveDeviceLg.NameShort}", CssProperty = PropertyNames.MaxWidth, CssValue = "940px" };
                var responsiveQuantumXl = new StyleQuantum() { Name = $"{PropertyNames.MaxWidth}-{responsiveDeviceXl.NameShort}", CssProperty = PropertyNames.MaxWidth, CssValue = "1140px" };
                defaultStyleQuantums.AddRange(new StyleQuantum[] { responsiveQuantumSm, responsiveQuantumMd, responsiveQuantumLg, responsiveQuantumXl });

                for (int i = 0; i < 20; i++)
                {
                    double spacerDistance = (double)i * 0.5;
                    var spacerDistanceString = $"{spacerDistance.ToString(CultureInfo.InvariantCulture)}rem"; // TODO round to last significant digit; TODO clean css value should include insignificant digit removal
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"spacer-{i}-top ({spacerDistanceString})", CssProperty = PropertyNames.PaddingTop, CssValue = spacerDistanceString });
                    defaultStyleQuantums.Add(new StyleQuantum() { Name = $"spacer-{i}-bottom ({spacerDistanceString})", CssProperty = PropertyNames.PaddingBottom, CssValue = spacerDistanceString });
                }

                // --- style atoms + molecules ---
                var defaultStyleAtoms = new List<StyleAtom>();

                var spacingMaxWidthSmAtom = new StyleAtom() { Name = "Spacing Max-Width-Sm", StyleAtomType = StyleAtomType.Spacing };
                spacingMaxWidthSmAtom.ApplyStyleQuantum(responsiveQuantumSm, _serviceOptions);
                var spacingMaxWidthMdAtom = new StyleAtom() { Name = "Spacing Max-Width-Md", StyleAtomType = StyleAtomType.Spacing };
                spacingMaxWidthMdAtom.ApplyStyleQuantum(responsiveQuantumMd, _serviceOptions);
                var spacingMaxWidthLgAtom = new StyleAtom() { Name = "Spacing Max-Width-Lg", StyleAtomType = StyleAtomType.Spacing };
                spacingMaxWidthLgAtom.ApplyStyleQuantum(responsiveQuantumLg, _serviceOptions);
                var spacingMaxWidthXlAtom = new StyleAtom() { Name = "Spacing Max-Width-Xl", StyleAtomType = StyleAtomType.Spacing };
                spacingMaxWidthXlAtom.ApplyStyleQuantum(responsiveQuantumXl, _serviceOptions);
                defaultStyleAtoms.AddRange(new StyleAtom[] { spacingMaxWidthSmAtom, spacingMaxWidthMdAtom, spacingMaxWidthLgAtom, spacingMaxWidthXlAtom });

                var rowSpacingAtom = new StyleAtom()
                {
                    Name = "Row Spacing",
                    StyleAtomType = StyleAtomType.Spacing,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.MarginLeft, CssValue = "auto" },
                        new StyleValue() { CssProperty = PropertyNames.MarginRight, CssValue = "auto" }
                    }
                };
                rowSpacingAtom.ApplyStyleQuantum(gutterPaddingLQuantum, _serviceOptions);
                rowSpacingAtom.ApplyStyleQuantum(gutterPaddingRQuantum, _serviceOptions);
                defaultStyleAtoms.Add(rowSpacingAtom);

                var rowSpacingAtomFullWidth = new StyleAtom()
                {
                    Name = "Row Spacing Full Width",
                    StyleAtomType = StyleAtomType.Spacing
                };
                rowSpacingAtomFullWidth.ApplyStyleQuantum(gutterPaddingLQuantum, _serviceOptions);
                rowSpacingAtomFullWidth.ApplyStyleQuantum(gutterPaddingRQuantum, _serviceOptions);
                defaultStyleAtoms.Add(rowSpacingAtomFullWidth);

                var rowStyleMolecule = new StyleMolecule() { Name = "Row (default)", NameShort = "Row", };
                rowStyleMolecule.ApplyStyleAtom(rowSpacingAtom, responsiveDeviceNone, null);
                rowStyleMolecule.ApplyStyleAtom(spacingMaxWidthSmAtom, responsiveDeviceSm, null);
                rowStyleMolecule.ApplyStyleAtom(spacingMaxWidthMdAtom, responsiveDeviceMd, null);
                rowStyleMolecule.ApplyStyleAtom(spacingMaxWidthLgAtom, responsiveDeviceLg, null);
                rowStyleMolecule.ApplyStyleAtom(spacingMaxWidthXlAtom, responsiveDeviceXl, null);

                var rowFullWidthStyleMolecule = new StyleMolecule() { Name = "Row full-width (default)", NameShort = "Row-full", };
                rowFullWidthStyleMolecule.ApplyStyleAtom(rowSpacingAtomFullWidth, responsiveDeviceNone, null);

                var defaultColSpacingPaddedAtom = new StyleAtom()
                {
                    Name = "Col Spacing Padded",
                    StyleAtomType = StyleAtomType.Spacing,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Width, CssValue = "100%" },
                        new StyleValue() { CssProperty = PropertyNames.MaxWidth, CssValue = "100%" }
                    }
                };
                defaultColSpacingPaddedAtom.ApplyStyleQuantum(gutterPaddingLQuantum, _serviceOptions);
                defaultColSpacingPaddedAtom.ApplyStyleQuantum(gutterPaddingRQuantum, _serviceOptions);

                var defaultColBoxAtom = new StyleAtom()
                {
                    Name = "Col Box",
                    StyleAtomType = StyleAtomType.Box,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.FlexBasis, CssValue = "100%" },
                        new StyleValue() { CssProperty = PropertyNames.FlexGrow, CssValue = "0" },
                        new StyleValue() { CssProperty = PropertyNames.FlexShrink, CssValue = "0" },
                        new StyleValue() { CssProperty = PropertyNames.Position, CssValue = "relative" }
                    }
                };

                defaultStyleAtoms.AddRange(new StyleAtom[] { defaultColSpacingPaddedAtom, defaultColBoxAtom });

                var boxStyleMolecule = new StyleMolecule() { Name = "Box (default)", NameShort = "Box" };
                boxStyleMolecule.ApplyStyleAtomRange(new StyleAtom[] { defaultColSpacingPaddedAtom, defaultColBoxAtom }, responsiveDeviceNone, null);

                var defaultFlexContainerSpacingAtom = new StyleAtom()
                {
                    Name = "Flex Container",
                    StyleAtomType = StyleAtomType.Spacing,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Display, CssValue = "flex" },
                        new StyleValue() { CssProperty = PropertyNames.FlexDirection, CssValue = "row" },
                        new StyleValue() { CssProperty = PropertyNames.FlexWrap, CssValue = "wrap" }
                    }
                };
                defaultFlexContainerSpacingAtom.ApplyStyleQuantum(gutterNegativeMarginLQuantum, _serviceOptions);
                defaultFlexContainerSpacingAtom.ApplyStyleQuantum(gutterNegativeMarginRQuantum, _serviceOptions);
                defaultStyleAtoms.Add(defaultFlexContainerSpacingAtom);

                var defaultFlexContainerSpacingAtomFullWidth = CloneStyleAtomDuringInitialization(defaultFlexContainerSpacingAtom, defaultFlexContainerSpacingAtom.Name + " in Full Width");
                defaultStyleAtoms.Add(defaultFlexContainerSpacingAtomFullWidth);

                var gutterGridStyleMolecule = new StyleMolecule() { Name = "[Internal] Gutter/Grid", NameShort = "[Internal] Gutter/Grid" };
                gutterGridStyleMolecule.ApplyStyleAtom(defaultFlexContainerSpacingAtom, responsiveDeviceNone, null);
                var gutterGridStyleMoleculeFullWidth = new StyleMolecule() { Name = "[Internal] Gutter/Grid 2", NameShort = "[Internal] Gutter/Grid 2" };
                gutterGridStyleMoleculeFullWidth.ApplyStyleAtom(defaultFlexContainerSpacingAtomFullWidth, responsiveDeviceNone, null);


                var defaultFlexContainerSpacingAtomCOPY = new StyleAtom()
                {
                    Name = "Flex Container 2",
                    StyleAtomType = StyleAtomType.Spacing,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Display, CssValue = "flex" },
                        new StyleValue() { CssProperty = PropertyNames.FlexDirection, CssValue = "row" },
                        new StyleValue() { CssProperty = PropertyNames.FlexWrap, CssValue = "wrap" }
                    }
                };
                defaultFlexContainerSpacingAtomCOPY.ApplyStyleQuantum(gutterNegativeMarginLQuantum, _serviceOptions);
                defaultFlexContainerSpacingAtomCOPY.ApplyStyleQuantum(gutterNegativeMarginRQuantum, _serviceOptions);
                defaultStyleAtoms.Add(defaultFlexContainerSpacingAtomCOPY);

                var gutterGridStyleMoleculeCOPY = new StyleMolecule() { Name = "[Internal] Gutter/Grid COPY", NameShort = "[Internal] Gutter/Grid COPY" };
                gutterGridStyleMoleculeCOPY.ApplyStyleAtom(defaultFlexContainerSpacingAtomCOPY, responsiveDeviceNone, null);

                var defaultTextContentBoxAtom = new StyleAtom()
                {
                    Name = "Text-Content Box",
                    StyleAtomType = StyleAtomType.Box,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.FlexBasis, CssValue = "100%" },
                        new StyleValue() { CssProperty = PropertyNames.FlexGrow, CssValue = "0" },
                        new StyleValue() { CssProperty = PropertyNames.FlexShrink, CssValue = "0" },
                        new StyleValue() { CssProperty = PropertyNames.Position, CssValue = "relative" }
                    }
                };

                var defaultTextContentSpacingAtom = new StyleAtom()
                {
                    Name = "Text-Content Spacing",
                    StyleAtomType = StyleAtomType.Spacing,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Width, CssValue = "100%" },
                        new StyleValue() { CssProperty = PropertyNames.MaxWidth, CssValue = "100%" },
                        new StyleValue() { CssProperty = PropertyNames.MarginTop, CssValue = "0" },
                        new StyleValue() { CssProperty = PropertyNames.MarginBottom, CssValue = "0" }
                    }
                };
                defaultTextContentSpacingAtom.ApplyStyleQuantum(gutterPaddingLQuantum, _serviceOptions);
                defaultTextContentSpacingAtom.ApplyStyleQuantum(gutterPaddingRQuantum, _serviceOptions);

                var defaultHeadingFontAtom = new StyleAtom()
                {
                    Name = "Heading Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.FontFamily, CssValue = "Source Sans Pro" },
                        new StyleValue() { CssProperty = PropertyNames.FontWeight, CssValue = "bold" },
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "#000000" }
                    }
                };

                var heading1StyleAtoms = new StyleAtom[] { defaultHeadingFontAtom, defaultTextContentBoxAtom, defaultTextContentSpacingAtom };
                defaultStyleAtoms.AddRange(heading1StyleAtoms);
                var heading1StyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
                heading1StyleMolecule.ApplyStyleAtomRange(heading1StyleAtoms, responsiveDeviceNone, null);

                var heading2StyleAtoms = new StyleAtom[] { CloneStyleAtomDuringInitialization(defaultHeadingFontAtom, defaultHeadingFontAtom.Name + " for H2"), CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for H2"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for H2") };
                defaultStyleAtoms.AddRange(heading2StyleAtoms);
                var heading2StyleMolecule = new StyleMolecule() { Name = "H2 (default)", NameShort = "H2", HtmlTag = "h2" };
                heading2StyleMolecule.ApplyStyleAtomRange(heading2StyleAtoms, responsiveDeviceNone, null);

                var heading3StyleAtoms = new StyleAtom[] { CloneStyleAtomDuringInitialization(defaultHeadingFontAtom, defaultHeadingFontAtom.Name + " for H3"), CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for H3"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for H3") };
                defaultStyleAtoms.AddRange(heading3StyleAtoms);
                var heading3StyleMolecule = new StyleMolecule() { Name = "H3 (default)", NameShort = "H3", HtmlTag = "h3" };
                heading3StyleMolecule.ApplyStyleAtomRange(heading3StyleAtoms, responsiveDeviceNone, null);

                var heading4StyleAtoms = new StyleAtom[] { CloneStyleAtomDuringInitialization(defaultHeadingFontAtom, defaultHeadingFontAtom.Name + " for H4"), CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for H4"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for H4") };
                defaultStyleAtoms.AddRange(heading4StyleAtoms);
                var heading4StyleMolecule = new StyleMolecule() { Name = "H4 (default)", NameShort = "H4", HtmlTag = "h4" };
                heading4StyleMolecule.ApplyStyleAtomRange(heading4StyleAtoms, responsiveDeviceNone, null);

                var heading5StyleAtoms = new StyleAtom[] { CloneStyleAtomDuringInitialization(defaultHeadingFontAtom, defaultHeadingFontAtom.Name + " for H5"), CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for H5"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for H5") };
                defaultStyleAtoms.AddRange(heading5StyleAtoms);
                var heading5StyleMolecule = new StyleMolecule() { Name = "H5 (default)", NameShort = "H5", HtmlTag = "h5" };
                heading5StyleMolecule.ApplyStyleAtomRange(heading5StyleAtoms, responsiveDeviceNone, null);

                var heading6StyleAtoms = new StyleAtom[] { CloneStyleAtomDuringInitialization(defaultHeadingFontAtom, defaultHeadingFontAtom.Name + " for H6"), CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for H6"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for H6") };
                defaultStyleAtoms.AddRange(heading6StyleAtoms);
                var heading6StyleMolecule = new StyleMolecule() { Name = "H6 (default)", NameShort = "H6", HtmlTag = "h6" };
                heading6StyleMolecule.ApplyStyleAtomRange(heading6StyleAtoms, responsiveDeviceNone, null);

                var simpleTextFontAtom = new StyleAtom()
                {
                    Name = "Simple Text Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "#000000" },
                        new StyleValue() { CssProperty = PropertyNames.FontFamily, CssValue = "Source Sans Pro" }
                    }
                };

                var simpleTextStyleAtoms = new StyleAtom[] { simpleTextFontAtom, CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for Simple Text"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for Simple Text") };
                defaultStyleAtoms.AddRange(simpleTextStyleAtoms);
                var simpleTextStyleMolecule = new StyleMolecule() { Name = "Simple Text (default)", NameShort = "Text", HtmlTag = "p" };
                simpleTextStyleMolecule.ApplyStyleAtomRange(simpleTextStyleAtoms, responsiveDeviceNone, null);

                var listTextFontAtom = new StyleAtom()
                {
                    Name = "List Text Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "#000000" },
                        new StyleValue() { CssProperty = PropertyNames.FontFamily, CssValue = "Source Sans Pro" }
                    }
                };

                var listTextStyleAtoms = new StyleAtom[] { listTextFontAtom, CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for List Text"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for List Text") };
                defaultStyleAtoms.AddRange(listTextStyleAtoms);
                var listTextStyleMolecule = new StyleMolecule() { Name = "List Text (default)", NameShort = "ListText", HtmlTag = "li" };
                listTextStyleMolecule.ApplyStyleAtomRange(listTextStyleAtoms, responsiveDeviceNone, null);

                var linkFontAtom = new StyleAtom()
                {
                    Name = "Link Default Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "blue" },
                        new StyleValue() { CssProperty = PropertyNames.FontFamily, CssValue = "Source Sans Pro" }
                    }
                };

                var linkHoverFontAtom = new StyleAtom()
                {
                    Name = "Link Hover Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "red" }
                    }
                };

                var linkVisitedTypoAtom = new StyleAtom()
                {
                    Name = "Link Visited Typo",
                    StyleAtomType = StyleAtomType.Typography,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.TextDecoration, CssValue = "underline" }
                    }
                };

                defaultStyleAtoms.Add(linkHoverFontAtom);
                defaultStyleAtoms.Add(linkVisitedTypoAtom);
                var linkStyleAtoms = new StyleAtom[] { linkFontAtom, CloneStyleAtomDuringInitialization(defaultTextContentBoxAtom, defaultTextContentBoxAtom.Name + " for Link"), CloneStyleAtomDuringInitialization(defaultTextContentSpacingAtom, defaultTextContentSpacingAtom.Name + " for Link") };
                defaultStyleAtoms.AddRange(linkStyleAtoms);
                var linkStyleMolecule = new StyleMolecule() { Name = "Link Url (default)", NameShort = "LinkUrl", HtmlTag = "a" };
                linkStyleMolecule.ApplyStyleAtomRange(linkStyleAtoms, responsiveDeviceNone, null);
                linkStyleMolecule.ApplyStyleAtom(linkVisitedTypoAtom, responsiveDeviceNone, ".visited");
                linkStyleMolecule.ApplyStyleAtom(linkHoverFontAtom, responsiveDeviceNone, ":hover");

                var homeViewBackgroundAtom = new StyleAtom()
                {
                    Name = "Home View Background",
                    StyleAtomType = StyleAtomType.Background,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.BackgroundColor, CssValue = "white" }
                    }
                };

                defaultStyleAtoms.Add(homeViewBackgroundAtom);
                var homeViewStyleName = GetStyleMoleculeNameForViewOfCaliforniaView("Home");
                var internalHomeViewStyleMolecule = new StyleMolecule() { Name = homeViewStyleName, NameShort = homeViewStyleName };
                internalHomeViewStyleMolecule.ApplyStyleAtom(homeViewBackgroundAtom, responsiveDeviceNone, null);

                var homeViewBodyStyleName = GetStyleMoleculeNameForBodyOfCaliforniaView("Home");
                var internalHomeViewBodyMolecule = new StyleMolecule() { Name = homeViewBodyStyleName, NameShort = homeViewBodyStyleName }; // TODO code duplication with create view
                // TODO default body styles

                var homeViewHtmlStyleName = GetStyleMoleculeNameForHtmlOfCaliforniaView("Home");
                var internalHomeViewHtmlMolecule = new StyleMolecule() { Name = homeViewHtmlStyleName, NameShort = homeViewHtmlStyleName }; // TODO code duplication with create view
                // TODO default html styles

                // --- layouts + views + content ---
                var defaultCaliforniaViews = new List<CaliforniaView>();

                var boxForDefaultBoxEmbeddedDiv = new LayoutBox() { StyleMolecule = gutterGridStyleMoleculeCOPY, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var defaultBox = new LayoutBox() { StyleMolecule = boxStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                defaultBox.PlacedInBoxBoxes.Add(boxForDefaultBoxEmbeddedDiv);
                var layoutRowMaxWidth = new LayoutRow() { StyleMolecule = rowStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var layoutRowFullWidth = new LayoutRow() { StyleMolecule = rowFullWidthStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var boxForLayoutRowEmbeddedDiv = new LayoutBox() { StyleMolecule = gutterGridStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var boxForFullWidthLayoutRowEmbeddedDiv = new LayoutBox() { StyleMolecule = gutterGridStyleMoleculeFullWidth, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                layoutRowMaxWidth.AllBoxesBelowRow.Add(boxForLayoutRowEmbeddedDiv);
                layoutRowFullWidth.AllBoxesBelowRow.Add(boxForFullWidthLayoutRowEmbeddedDiv);
                var instanceableLayoutRowView = new CaliforniaView() { Name = "[Internal] Instanceable Layout Rows", IsInternal = true, QueryUrl = "", ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey() };
                instanceableLayoutRowView.PlacedLayoutRows.Add(layoutRowMaxWidth);
                instanceableLayoutRowView.PlacedLayoutRows.Add(layoutRowFullWidth);
                defaultCaliforniaViews.Add(instanceableLayoutRowView);

                var heading1ContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Heading H1" };
                var heading1LayoutAtom = new LayoutAtom() { HostedContentAtom = heading1ContentAtom, StyleMolecule = heading1StyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var heading2ContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Heading H2" };
                var heading2LayoutAtom = new LayoutAtom() { HostedContentAtom = heading2ContentAtom, StyleMolecule = heading2StyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var heading3ContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Heading H3" };
                var heading3LayoutAtom = new LayoutAtom() { HostedContentAtom = heading3ContentAtom, StyleMolecule = heading3StyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var heading4ContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Heading H4" };
                var heading4LayoutAtom = new LayoutAtom() { HostedContentAtom = heading4ContentAtom, StyleMolecule = heading4StyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var heading5ContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Heading H5" };
                var heading5LayoutAtom = new LayoutAtom() { HostedContentAtom = heading5ContentAtom, StyleMolecule = heading5StyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var heading6ContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Heading H6" };
                var heading6LayoutAtom = new LayoutAtom() { HostedContentAtom = heading6ContentAtom, StyleMolecule = heading6StyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var simpleTextContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Text" };
                var simpleTextLayoutAtom = new LayoutAtom() { HostedContentAtom = simpleTextContentAtom, StyleMolecule = simpleTextStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var listTextContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "List" };
                var listTextLayoutAtom = new LayoutAtom() { HostedContentAtom = listTextContentAtom, StyleMolecule = listTextStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var linkContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Link, Url = "some@example.com" }; // TODO default email/url...
                var linkLayoutAtom = new LayoutAtom() { HostedContentAtom = linkContentAtom, StyleMolecule = linkStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };

                var layoutBoxForInstanceableAtoms = new LayoutBox() { StyleMolecule = new StyleMolecule() { Name = InstanceableAtomsHolder, NameShort = InstanceableAtomsHolder }, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() }; // TODO unique names => const string for client and service; TODO user cannot use that string
                layoutBoxForInstanceableAtoms.PlacedInBoxAtoms.AddRange(new LayoutAtom[] { heading1LayoutAtom, heading2LayoutAtom, heading3LayoutAtom, heading4LayoutAtom, heading5LayoutAtom, heading6LayoutAtom, simpleTextLayoutAtom, listTextLayoutAtom, linkLayoutAtom });
                var layoutRowForInstanceableAtoms = new LayoutRow() { StyleMolecule = new StyleMolecule() { Name = "[Internal] Row Instanceable Atoms", NameShort = "[Internal] Row Instanceable Atoms"}, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                layoutRowForInstanceableAtoms.AllBoxesBelowRow.Add(layoutBoxForInstanceableAtoms);
                var instanceableLayoutAtomView = new CaliforniaView() { Name = "[Internal] Instanceable Layout Atoms", IsInternal = true, QueryUrl = "", ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey() };
                instanceableLayoutAtomView.PlacedLayoutRows.Add(layoutRowForInstanceableAtoms);
                defaultCaliforniaViews.Add(instanceableLayoutAtomView);

                var layoutRowForUserBoxes = new LayoutRow() { StyleMolecule = new StyleMolecule() { Name = "[Internal] Row User Layout Boxes", NameShort = "[Internal] Row User Layout Boxes" }, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var userInstanceableRows = new CaliforniaView() { Name = "[Internal] User Layout Molecules", IsInternal = true, QueryUrl = "", ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey() };
                layoutRowForUserBoxes.AllBoxesBelowRow.Add(defaultBox);
                userInstanceableRows.PlacedLayoutRows.Add(layoutRowForUserBoxes);
                defaultCaliforniaViews.Add(userInstanceableRows);

                var homeViewStyleHolderRow = new LayoutRow() { StyleMolecule = internalHomeViewStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var homeViewStyleHtmlHolderRow = new LayoutRow() { StyleMolecule = internalHomeViewHtmlMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var homeViewStyleBodyHolderRow = new LayoutRow() { StyleMolecule = internalHomeViewBodyMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                var specialStylesHolderView = new CaliforniaView() { Name = CaliforniaViewStylesHolder, IsInternal = true, QueryUrl = "", ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey() };
                specialStylesHolderView.PlacedLayoutRows.Add(homeViewStyleHolderRow);
                specialStylesHolderView.PlacedLayoutRows.Add(homeViewStyleHtmlHolderRow);
                specialStylesHolderView.PlacedLayoutRows.Add(homeViewStyleBodyHolderRow);
                defaultCaliforniaViews.Add(specialStylesHolderView);

                // TODO update existing projects, delete internal views and replace with new data etc...

                var mainView = new CaliforniaView() { Name = "Home", IsInternal = false, QueryUrl = "Home", ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey() };
                defaultCaliforniaViews.Add(mainView);

                // --- database ---            
                californiaProject.StyleQuantums.AddRange(defaultStyleQuantums);
                californiaProject.StyleAtoms.AddRange(defaultStyleAtoms);
                californiaProject.StyleValues.AddRange(defaultStyleAtoms.SelectMany(a => a.AppliedValues));

                foreach (var view in defaultCaliforniaViews)
                {
                    foreach (var row in view.PlacedLayoutRows)
                    {
                        foreach (var box in row.AllBoxesBelowRow)
                        {
                            AddBoxAndContentsToProjectRecursiveTODO(californiaProject, box, row);
                        }
                        californiaProject.LayoutMolecules.AddRange(row.AllBoxesBelowRow);
                        californiaProject.StyleMolecules.Add(row.StyleMolecule);
                    }
                    californiaProject.LayoutMolecules.AddRange(view.PlacedLayoutRows);
                }

                californiaProject.CaliforniaViews.AddRange(defaultCaliforniaViews);

                californiaProject.ProjectDefaultsRevision = expectedRevision;
                californiaProject.UserDefinedCss = "@charset \"utf-8\";"; // TODO move rest to user defined css defined on CaliforniaView(?)
                californiaProject.UserDefinedCss += $"*,*::before,*::after {{box-sizing: inherit}}";
                californiaProject.UserDefinedCss += $"html{{box-sizing:border-box;-ms-overflow-style: scrollbar}}";
                californiaProject.UserDefinedCss += $"html{{text-shadow:0px 0px 0px white;-webkit-text-size-adjust: 100%;font-family:serif}}"; // TODO bootstrap reboot.css

                await _data.SaveChangesAsync();
            }

            if (californiaProject.ProjectDefaultsRevision == expectedRevision++)
            {
                var responsiveDeviceNone = californiaProject.ResponsiveDevices.First(d => d.NameShort == "None");

                var defaultStyleAtoms = new List<StyleAtom>();
                var baseFontStyleAtom = new StyleAtom()
                {
                    CaliforniaProjectId = californiaProjectId,
                    Name = "Base Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.FontFamily, CssValue = "Merriweather" },
                        new StyleValue() { CssProperty = PropertyNames.FontWeight, CssValue = "normal" },
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "#000000" }
                    }
                };
                defaultStyleAtoms.Add(baseFontStyleAtom);
                var hoverFontStyleAtom = new StyleAtom()
                {
                    CaliforniaProjectId = californiaProjectId,
                    Name = "Hover Font",
                    StyleAtomType = StyleAtomType.Font,
                    AppliedValues = new List<StyleValue>()
                    {
                        new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "red" }
                    }
                };
                defaultStyleAtoms.Add(hoverFontStyleAtom);

                var buttonStyleMolecule = new StyleMolecule() { Name = "Button (default)", NameShort = "BTN", HtmlTag = "button", CaliforniaProjectId = californiaProjectId };
                buttonStyleMolecule.ApplyStyleAtomRange(new StyleAtom[] { baseFontStyleAtom }, responsiveDeviceNone, null);
                buttonStyleMolecule.ApplyStyleAtomRange(new StyleAtom[] { hoverFontStyleAtom }, responsiveDeviceNone, ":hover");
                var buttonContentAtom = new ContentAtom() { ContentAtomType = ContentAtomType.Text, TextContent = "Button", CaliforniaProjectId = californiaProjectId };
                var buttonLayoutAtom = new LayoutAtom() { HostedContentAtom = buttonContentAtom, StyleMolecule = buttonStyleMolecule, CaliforniaProjectId = californiaProjectId, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };


                var instanceableAtomView = await _data.Set<CaliforniaView>()
                                                .Include(v => v.PlacedLayoutRows)
                                                    .ThenInclude(ro => ro.AllBoxesBelowRow)
                                                        .ThenInclude(box => box.PlacedInBoxAtoms)
                                                .FirstAsync(v => v.CaliforniaProjectId == californiaProjectId && v.Name == "[Internal] Instanceable Layout Atoms");
                var instanceableAtomsBox = instanceableAtomView.PlacedLayoutRows[0].AllBoxesBelowRow[0]; // TODO depends on creation/sort order

                instanceableAtomsBox.PlacedInBoxAtoms.Add(buttonLayoutAtom);

                californiaProject.StyleAtoms.AddRange(defaultStyleAtoms);
                californiaProject.StyleValues.AddRange(defaultStyleAtoms.SelectMany(a => a.AppliedValues));

                californiaProject.ProjectDefaultsRevision = expectedRevision;
                await _data.SaveChangesAsync();
            }

            /*
            

            var baseFontAtom = new StyleAtom()
            {
                Name = "Base Font",
                StyleAtomType = StyleAtomType.Font,
                AppliedValues = new List<StyleValue>()
                {
                    new StyleValue() { CssProperty = PropertyNames.FontFamily, CssValue = "Merriweather" },
                    new StyleValue() { CssProperty = PropertyNames.FontWeight, CssValue = "normal" },
                    new StyleValue() { CssProperty = PropertyNames.Color, CssValue = "#000000" }
                }
            };

            var dividerAtom = new StyleAtom()
            {
                Name = "Divider",
                StyleAtomType = StyleAtomType.Divider,
                AppliedValues = new List<StyleValue>()
                {
                    new StyleValue() { CssProperty = PropertyNames.BorderColor, CssValue = "#000000" }
                }
            };

            var pictureAtom = new StyleAtom()
            {
                Name = "Picture",
                StyleAtomType = StyleAtomType.Picture,
                AppliedValues = new List<StyleValue>()
                {
                    new StyleValue() { CssProperty = PropertyNames.Overflow, CssValue = "hidden" }
                }
            };

            var backgroundAtom = new StyleAtom()
            {
                Name = "Background",
                StyleAtomType = StyleAtomType.Background,
                AppliedValues = new List<StyleValue>()
                {
                    new StyleValue() { CssProperty = PropertyNames.BackgroundColor, CssValue = "#FFFFFF" }
                }
            };

            var typographyLinkAtom = new StyleAtom()
            {
                Name = "Link Typo",
                StyleAtomType = StyleAtomType.Typography,
                AppliedValues = new List<StyleValue>()
                {
                    new StyleValue() { CssProperty = PropertyNames.TextDecoration, CssValue = "underline" }
                }
            };

            var typographyAtom = new StyleAtom() { Name = "Typo", StyleAtomType = StyleAtomType.Typography };
            var spacingAtom = new StyleAtom() { Name = "Spacing", StyleAtomType = StyleAtomType.Spacing };
            var backgroundNavigationAtom = new StyleAtom() { Name = "BackgroundNavigation", StyleAtomType = StyleAtomType.Background };
            var gridAtom = new StyleAtom() { Name = "Grid", StyleAtomType = StyleAtomType.Grid };
            var navbarAtom = new StyleAtom() { Name = "Navbar", StyleAtomType = StyleAtomType.Navbar };
            var listAtom = new StyleAtom() { Name = "List", StyleAtomType = StyleAtomType.List };

            

            

            var defaultTextSpacingAtom = new StyleAtom()
            {
                Name = "Text Spacing",
                StyleAtomType = StyleAtomType.Spacing,
                AppliedValues = new List<StyleValue>()
                {
                    new StyleValue() { CssProperty = PropertyNames.Width, CssValue = "100%" },
                    new StyleValue() { CssProperty = PropertyNames.MaxWidth, CssValue = "100%" },
                    new StyleValue() { CssProperty = PropertyNames.MarginTop, CssValue = "0" }, // undo browser default margin for <p>, <h1>, ..., <h6>
                    new StyleValue() { CssProperty = PropertyNames.MarginBottom, CssValue = "0" }
                }
            };
            defaultTextSpacingAtom.ApplyStyleQuantum(gutterPaddingLQuantum, _serviceOptions);
            defaultTextSpacingAtom.ApplyStyleQuantum(gutterPaddingRQuantum, _serviceOptions);
            
            defaultStyleAtoms.AddRange(new StyleAtom[] {
                headingFontAtom,
                baseFontAtom,
                dividerAtom,
                pictureAtom,
                backgroundAtom,
                typographyLinkAtom,
                rowSpacingAtom,
                typographyAtom,
                spacingAtom,
                background2Atom,
                backgroundNavigationAtom,
                gridAtom,
                rowAtom,
                navbarAtom,
                listAtom,
                defaultColSpacingAtom,
                defaultColSpacingPaddedAtom,
                defaultTextSpacingAtom,
                defaultColBoxAtom,
                defaultFlexContainerSpacingAtom
            });

            // --- content atoms ---
            var defaultContentAtoms = new List<ContentAtom>();
            
            defaultContentAtoms.AddRange(new ContentAtom[] { heading1ContentAtom });

            // --- style molecules ---
            
            var defaultStyleMolecules = new List<StyleMolecule>();

            
            californiaProject.StyleAtoms.Add(defaultFlexContainerSpacingAtom);
            californiaProject.StyleMolecules.Add(gutterGridStyleMolecule);

            var heading1StyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };

            //heading1StyleMolecule.ApplyStyleAtoms(new StyleAtom[] { defaultColBoxAtom, defaultTextSpacingAtom, headingFontAtom, typographyAtom }, responsiveDeviceNone, null);

            // TODO
            
            var rowStyleFullMolecule = new StyleMolecule() { Name = "Row-full (default)", NameShort = "Row-full" };
            
            //rowStyleFullMolecule.ApplyStyleAtomRange(new StyleAtom[] { rowAtom, rowSpacingAtom, background2Atom }, responsiveDeviceNone, null);

            defaultStyleMolecules.AddRange(new StyleMolecule[] { rowStyleMolecule, boxStyleMolecule });

            // --- layout rows ---
            
            
            //heading1CopyStyleMolecule.ApplyStyleAtomRange(new StyleAtom[] { rowAtom, rowSpacingAtom, background2Atom }, responsiveDeviceNone, null);
            var heading2CopyStyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
            var heading3CopyStyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
            var heading4CopyStyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
            var heading5CopyStyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
            var heading6CopyStyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
            var heading7CopyStyleMolecule = new StyleMolecule() { Name = "H55 (default) hihi", NameShort = "H1", HtmlTag = "h1" };
            var heading8CopyStyleMolecule = new StyleMolecule() { Name = "H1 (default)", NameShort = "H1", HtmlTag = "h1" };
            
            heading7CopyStyleMolecule.ClonedFromStyle = heading6CopyStyleMolecule;

            _data.Entry(heading6CopyStyleMolecule).CurrentValues.SetValues(heading7CopyStyleMolecule);
            _data.Entry(heading8CopyStyleMolecule).CurrentValues.SetValues(heading6CopyStyleMolecule);
            heading8CopyStyleMolecule.ClonedFromStyle = heading7CopyStyleMolecule.ClonedFromStyle;
            heading8CopyStyleMolecule.Name = heading7CopyStyleMolecule.Name;
            heading8CopyStyleMolecule.NameShort = heading7CopyStyleMolecule.NameShort;
            heading8CopyStyleMolecule.HtmlTag = heading7CopyStyleMolecule.HtmlTag;

            defaultStyleMolecules.AddRange(new StyleMolecule[] { heading1CopyStyleMolecule,  heading4CopyStyleMolecule, heading5CopyStyleMolecule, heading6CopyStyleMolecule, heading7CopyStyleMolecule, heading8CopyStyleMolecule });
            
            californiaProject.StyleValues.AddRange(gutterGridStyleMolecule.MappedStyleAtoms.Select(map => map.StyleAtom).SelectMany(a => a.AppliedValues));
            
            californiaProject.CaliforniaViews.Add(invisibleViewUniqueStyleHolder);

            var dollarsLayoutAtom = new LayoutAtom() { HostedContentAtom = dollarContentAtom, StyleMolecule = heading7CopyStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            var defaultLayoutRows = new List<LayoutRow>();
            
            

            //var headingSingleCopyStyleMolecule = new StyleMolecule() { Name = "H55 (default) wtf", NameShort = "H1", HtmlTag = "h1", IsListed = true };
            //var headingSingleLayoutRow = new LayoutRow() { IsInstanceable = true, StyleMolecule = headingSingleCopyStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };

            var sub3Box = new LayoutBox() { StyleMolecule = heading8CopyStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            var subSubBox = new LayoutBox() { StyleMolecule = heading6CopyStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            subSubBox.PlacedInBoxAtoms.Add(dollarsLayoutAtom);
            subSubBox.PlacedInBoxBoxes.Add(sub3Box);
            var textLayoutAtom = new LayoutAtom() { HostedContentAtom = textContentAtom, StyleMolecule = heading4CopyStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            var textBoxInBox = new LayoutBox() { StyleMolecule = heading5CopyStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            textBoxInBox.PlacedInBoxAtoms.Add(textLayoutAtom);
            textBoxInBox.PlacedInBoxBoxes.Add(subSubBox);
            heading1LayoutBox.PlacedInBoxBoxes.Add(textBoxInBox);
            heading1LayoutRow.InsideRowBoxes.Add(textBoxInBox);
            heading1LayoutRow.InsideRowBoxes.Add(subSubBox);
            heading1LayoutRow.InsideRowBoxes.Add(sub3Box);

            defaultLayoutRows.Add(heading1LayoutRow);
            */
        }

        public List<string> ReadAvailableFonts()
        {
            return _data.Set<Webfont>().Select(f => f.Family).ToList();
        }

        public async Task UpdateUserDefinedCssAsync(CaliforniaView californiaView, string cssValue)
        {
            if (californiaView.UserDefinedCss == cssValue)
            {
                // no change
                return;
            }
            californiaView.UserDefinedCss = cssValue;
            await _data.SaveChangesAsync();
        }        

        public async Task UpdateUserDefinedCssAsync(CaliforniaProject californiaProject, string cssValue)
        {

            if (californiaProject.UserDefinedCss == cssValue)
            {
                // no change
                return;
            }
            californiaProject.UserDefinedCss = cssValue;
            await _data.SaveChangesAsync();
        }

        public async Task LogCaliforniaContextAsync(CaliforniaContext californiaContext)
        {
            // TODO per user/project count?
            // TODO auto generate entities and store in cache
            // TODO parallelization issues with such type of counter
            // TODO order of calls also interesting and later on relations between targeted data
            // TODO front end calls are not logged
            var californiaEventLog = await _data.CaliforniaEventLogs.FirstOrDefaultAsync(e => e.CaliforniaEvent == californiaContext.CaliforniaEvent);
            if (californiaEventLog == null)
            {
                californiaEventLog = new CaliforniaEventLog() { CaliforniaEvent = californiaContext.CaliforniaEvent };
            }
            californiaEventLog.NTimesCalled++;
            await _data.SaveChangesAsync();
        }

        public async Task DeleteStyleValueFromLayoutStyleInteractionAsync(LayoutStyleInteraction layoutStyleInteraction, StyleValue styleValue)
        {
            var targetMapping = layoutStyleInteraction.StyleValueInteractions.First(map => map.StyleValueId == styleValue.StyleValueId);
            layoutStyleInteraction.StyleValueInteractions.Remove(targetMapping);
            await _data.SaveChangesAsync();
        }

        public async Task CreateStyleValueInteractionAsync(LayoutStyleInteraction layoutStyleInteraction, StyleValue styleValue, string cssValue)
        {
            if (styleValue.CssProperty == PropertyNames.ZIndex) // TODO duplicated code with style update, quantum update and quantum create and interaction
            {
                // TODO document
                if (int.TryParse(cssValue, out var parsedZIndex) && parsedZIndex >= 100)
                {
                    throw new NotImplementedException("Z-Index must be smaller than 100.");
                }
            }
            if (string.IsNullOrEmpty(cssValue))
            {
                throw new ArgumentNullException(nameof(cssValue));
            }
            var styleValueInteraction = new StyleValueInteractionMapping()
            {
                LayoutStyleInteractionId = layoutStyleInteraction.LayoutStyleInteractionId,
                StyleValueId = styleValue.StyleValueId,
                CssValue = cssValue
            };
            _data.Add(styleValueInteraction);
            await _data.SaveChangesAsync();
        }

        public async Task DeleteLayoutStyleInteractionAsync(LayoutStyleInteraction layoutStyleInteraction)
        {
            _data.Remove(layoutStyleInteraction);
            await _data.SaveChangesAsync();
        }

        public async Task CreateLayoutStyleInteractionForLayoutAtomAsync(LayoutAtom layoutAtom)
        {
            var newInteraction = new LayoutStyleInteraction()
            {
                LayoutStyleInteractionType = LayoutStyleInteractionType.StyleToggle,
                CaliforniaProjectId = layoutAtom.CaliforniaProjectId,
                LayoutAtomId = layoutAtom.LayoutBaseId
            };
            layoutAtom.LayoutStyleInteractions.Add(newInteraction);
            await _data.SaveChangesAsync();
        }

        public async Task DeleteCaliforniaViewAsync(CaliforniaView californiaView)
        {
            if (californiaView.Name == "Home" || californiaView.Name.Contains("[Internal]")) // TODO magic token // TODO document: home view protected
            {
                throw new InvalidOperationException("Failed to delete california view. Protected or reserved for internal use.");
            }
            if (californiaView.PlacedLayoutRows.Count > 0)
            {
                throw new InvalidOperationException("Failed to delete california view. Remove placed layout rows first.");
            }
            var specialStylesHolderView = await ReadSpecialStylesHolderViewAsync(californiaView.CaliforniaProjectId);
            var styleMoleculeViewName = GetStyleMoleculeNameForViewOfCaliforniaView(californiaView.Name);
            var styleMoleculeBodyName = GetStyleMoleculeNameForBodyOfCaliforniaView(californiaView.Name);
            var styleMoleculeHtmlName = GetStyleMoleculeNameForHtmlOfCaliforniaView(californiaView.Name);
            LayoutRow viewStyleHolderRow = specialStylesHolderView.PlacedLayoutRows.First(r => r.StyleMolecule.Name == styleMoleculeViewName); // TODO cleanup old projects contain layout rows for special styles of deleted views
            LayoutRow bodyStyleHolderRow = specialStylesHolderView.PlacedLayoutRows.First(r => r.StyleMolecule.Name == styleMoleculeBodyName);
            LayoutRow htmlStyleHolderRow = specialStylesHolderView.PlacedLayoutRows.First(r => r.StyleMolecule.Name == styleMoleculeHtmlName);
            _data.Remove(viewStyleHolderRow);
            _data.Remove(bodyStyleHolderRow);
            _data.Remove(htmlStyleHolderRow);
            _data.Remove(californiaView);
            await _data.SaveChangesAsync();
        }

        public async Task CreateOrUpdateFontsAsync(IEnumerable<Webfont> webfonts)
        {
            var currentFonts = await _data.Set<Webfont>().ToListAsync();
            foreach (var font in webfonts)
            {
                var matchingFont = currentFonts.FirstOrDefault(f => f.Family == font.Family);
                if (matchingFont == null)
                {
                    currentFonts.Add(font);
                }
                else
                {
                    currentFonts.Remove(matchingFont);
                    currentFonts.Add(font);
                }
            }
            var rows = await _data.SaveChangesAsync();
        }

        private async Task<CaliforniaView> ReadSpecialStylesHolderViewAsync(int californiaProjectId)
        {
            // TODO rework special style holder routines...
            // find connected layout row + style
            var specialStylesView = await _data.Set<CaliforniaView>()
                .Include(v => v.PlacedLayoutRows)
                    .ThenInclude(la => la.StyleMolecule)
                .FirstAsync(v => v.IsInternal.Value && v.CaliforniaProjectId == californiaProjectId && v.Name == CaliforniaServiceExtensions.CaliforniaViewStylesHolder);
            return specialStylesView;
        }

        private async Task CheckCaliforniaViewNameIsValidAsync(CaliforniaProject californiaProject, string californiaViewName)
        {
            // TODO this method must be made thread safe (i.e. rowversion on project level)
            if (californiaViewName.Contains("[Internal]"))
            {
                throw new ArgumentOutOfRangeException("California view name contains protected identifiers: \"Internal\"");
            }
            if (string.IsNullOrEmpty(californiaViewName))
            {
                throw new ArgumentNullException($"{nameof(californiaViewName)} may not be empty.");
            }
            var allCaliforniaViews = await _data.Set<CaliforniaView>()
                .Where(v => v.CaliforniaProjectId == californiaProject.CaliforniaProjectId && v.IsInternal == false)
                .ToListAsync();
            if (allCaliforniaViews.Any(v => v.Name == californiaViewName))
            {
                throw new InvalidOperationException("California view name must be unique.");
            }
        }

        public async Task<Dictionary<string, List<int>>> ReadUsedFontsAsync(IEnumerable<StyleAtom> styleAtoms)
        {
            var availableGoogleFonts = await _data.Set<Webfont>().ToListAsync();
            var usedGoogleFonts = new Dictionary<string, List<int>>();
            foreach (var styleAtom in styleAtoms)
            {
                var fontFamilySetting = styleAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == PropertyNames.FontFamily);
                if (fontFamilySetting == null)
                {
                    continue;
                }
                string fontFamily = fontFamilySetting.CssValue;
                if (!availableGoogleFonts.Any(f => f.Family == fontFamily))
                {
                    continue;
                }
                var fontWeightSetting = styleAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == PropertyNames.FontWeight);
                if (fontWeightSetting == null)
                {
                    continue; // TODO highlight style atoms where font weight setting is missing TODO responsive settings are messy
                }
                string weightSetting = fontWeightSetting.CssValue;
                // convert setting to Google Fonts compatible weight
                int parsedFontWeight = 0;
                switch (weightSetting)
                {
                    case null:
                        parsedFontWeight = 400;
                        break;
                    case "":
                        parsedFontWeight = 400;
                        break;
                    case "normal":
                        parsedFontWeight = 400;
                        break;
                    case "bold":
                        parsedFontWeight = 700;
                        break;
                    default:
                        if (!int.TryParse(weightSetting, out parsedFontWeight))
                        {
                            // TODO info
                            parsedFontWeight = 400;
                        }
                        break;
                }

                // check if valid value for GFont weight
                var validValues = new List<int> { 100, 300, 400, 700, 900 };
                if (!validValues.Contains(parsedFontWeight))
                {
                    parsedFontWeight = validValues.Last(validVal => parsedFontWeight > validVal);
                }

                if (usedGoogleFonts.ContainsKey(fontFamily) && !usedGoogleFonts[fontFamily].Contains(parsedFontWeight))
                {
                    usedGoogleFonts[fontFamily].Add(parsedFontWeight);
                }
                else
                {
                    usedGoogleFonts[fontFamily] = new List<int> { parsedFontWeight };
                }
            }
            return usedGoogleFonts;
        }

        private async Task CreateCaliforniaViewStyleMoleculesAsync(CaliforniaProject californiaProject, CaliforniaView californiaView, CaliforniaView referenceCaliforniaViewForStyles)
        {
            var specialStylesHolderView = await ReadSpecialStylesHolderViewAsync(californiaProject.CaliforniaProjectId);
            var styleMoleculeViewName = GetStyleMoleculeNameForViewOfCaliforniaView(californiaView.Name);
            var styleMoleculeBodyName = GetStyleMoleculeNameForBodyOfCaliforniaView(californiaView.Name);
            var styleMoleculeHtmlName = GetStyleMoleculeNameForHtmlOfCaliforniaView(californiaView.Name);
            var viewStyleMolecule = new StyleMolecule() { Name = styleMoleculeViewName, NameShort = styleMoleculeViewName, CaliforniaProjectId = californiaProject.CaliforniaProjectId }; // TODO name short was set wrong for old projects before May2018; cleanup: set to name (which is correct)
            var bodyStyleMolecule = new StyleMolecule() { Name = styleMoleculeBodyName, NameShort = styleMoleculeBodyName, CaliforniaProjectId = californiaProject.CaliforniaProjectId };
            var htmlStyleMolecule = new StyleMolecule() { Name = styleMoleculeHtmlName, NameShort = styleMoleculeHtmlName, CaliforniaProjectId = californiaProject.CaliforniaProjectId };
            if (referenceCaliforniaViewForStyles != null)
            {
                var referenceViewStyleMoleculeName = GetStyleMoleculeNameForViewOfCaliforniaView(referenceCaliforniaViewForStyles.Name);
                var referenceViewStyleMoleculeId = specialStylesHolderView.PlacedLayoutRows.First(r => r.StyleMolecule.Name == referenceViewStyleMoleculeName).StyleMolecule.StyleMoleculeId;
                var referenceViewStyleMolecule = await ReadStyleMoleculeByStyleAsync(referenceViewStyleMoleculeId, false);
                CloneStyleSetup(viewStyleMolecule, referenceViewStyleMolecule);

                var referenceBodyStyleMoleculeName = GetStyleMoleculeNameForBodyOfCaliforniaView(referenceCaliforniaViewForStyles.Name);
                var referenceBodyStyleMoleculeId = specialStylesHolderView.PlacedLayoutRows.First(r => r.StyleMolecule.Name == referenceBodyStyleMoleculeName).StyleMolecule.StyleMoleculeId;
                var referenceBodyStyleMolecule = await ReadStyleMoleculeByStyleAsync(referenceBodyStyleMoleculeId, false);
                CloneStyleSetup(bodyStyleMolecule, referenceBodyStyleMolecule);

                var referenceHtmlStyleMoleculeName = GetStyleMoleculeNameForHtmlOfCaliforniaView(referenceCaliforniaViewForStyles.Name);
                var referenceHtmlStyleMoleculeId = specialStylesHolderView.PlacedLayoutRows.First(r => r.StyleMolecule.Name == referenceHtmlStyleMoleculeName).StyleMolecule.StyleMoleculeId;
                var referenceHtmlStyleMolecule = await ReadStyleMoleculeByStyleAsync(referenceHtmlStyleMoleculeId, false);
                CloneStyleSetup(htmlStyleMolecule, referenceHtmlStyleMolecule);
            }
            specialStylesHolderView.PlacedLayoutRows.Add(new LayoutRow()
            {
                CaliforniaProjectId = californiaProject.CaliforniaProjectId,
                PlacedOnViewId = specialStylesHolderView.CaliforniaViewId,
                StyleMolecule = viewStyleMolecule, // TODO clone from style?
                LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey()
            });
            specialStylesHolderView.PlacedLayoutRows.Add(new LayoutRow()
            {
                CaliforniaProjectId = californiaProject.CaliforniaProjectId,
                PlacedOnViewId = specialStylesHolderView.CaliforniaViewId,
                StyleMolecule = bodyStyleMolecule, // TODO clone from style?
                LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey()
            });
            specialStylesHolderView.PlacedLayoutRows.Add(new LayoutRow()
            {
                CaliforniaProjectId = californiaProject.CaliforniaProjectId,
                PlacedOnViewId = specialStylesHolderView.CaliforniaViewId,
                StyleMolecule = htmlStyleMolecule, // TODO clone from style?
                LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey()
            });
        }

        public async Task CreateCaliforniaViewFromReferenceViewAsync(CaliforniaProject californiaProject, string californiaViewName, CaliforniaView referenceCaliforniaView)
        {
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                await CheckCaliforniaViewNameIsValidAsync(californiaProject, californiaViewName);
                var newCaliforniaView = new CaliforniaView()
                {
                    Name = californiaViewName, // TODO name generator
                    QueryUrl = californiaViewName, // TODO name cleanup for URL (no whitespace, normalize etc.)
                    CaliforniaProjectId = californiaProject.CaliforniaProjectId,
                    UserDefinedCss = referenceCaliforniaView.UserDefinedCss,
                    ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey()
                };
                _data.Add(newCaliforniaView);
                await CreateCaliforniaViewStyleMoleculesAsync(californiaProject, newCaliforniaView, referenceCaliforniaView);
                var sortedRows = referenceCaliforniaView.PlacedLayoutRows.OrderBy(r => r.LayoutSortOrderKey);
                await _data.SaveChangesAsync();
                foreach (var referenceLayoutRow in sortedRows)
                {
                    await CloneAndRegisterLayoutRowRecursiveInTransactionAsync(californiaProject.CaliforniaProjectId, referenceLayoutRow, newCaliforniaView);
                }
                transaction.Commit();
            }            
        }

        public async Task CreateCaliforniaViewAsync(CaliforniaProject californiaProject, string californiaViewName)
        {
            await CheckCaliforniaViewNameIsValidAsync(californiaProject, californiaViewName);
            var newCaliforniaView = new CaliforniaView()
            {
                Name = californiaViewName, // TODO name generator
                QueryUrl = californiaViewName, // TODO name cleanup for URL (no whitespace, normalize etc.)
                CaliforniaProjectId = californiaProject.CaliforniaProjectId,
                ViewSortOrderKey = _californiaDbStateSqlite.GetAndIncrementCaliforniaViewSortKey()
            };
            _data.Add(newCaliforniaView);
            await CreateCaliforniaViewStyleMoleculesAsync(californiaProject, newCaliforniaView, null);
            await _data.SaveChangesAsync();
        }

        public async Task<CaliforniaProject> PublishCaliforniaProjectAsync(CaliforniaContext californiaContext, CaliforniaProject californiaProject)
        {
            var fullProject = await ReadCaliforniaProjectForClientAsync(californiaContext, californiaProject.CaliforniaProjectId, true);
            //OptimizeProject(fullProject);
            return fullProject;
        }

        private void OptimizeProject(CaliforniaProject californiaProject)
        {
            // TODO fix data model / magic string constants // TODO code duplication
            var viewName = "Home";
            var view = californiaProject.CaliforniaViews.First(v => v.Name == viewName);
            var defaultStylesView = californiaProject.CaliforniaViews.First(v => v.Name == "[Internal] Special Styles"); // TODO magic string constant
            var homeViewStyle = californiaProject.StyleMolecules.First(s => s.Name == "[Internal] " + viewName + " View Style"); // TODO magic string constant

            var usedStyleMolecules = new List<StyleMolecule>();
            usedStyleMolecules.Add(homeViewStyle);

            foreach (var row in view.PlacedLayoutRows)
            {
                usedStyleMolecules.Add(row.StyleMolecule);
            }
        }
        /*
            var exportedProject = await _californiaPublisher.ExportAsync(fullProject);            

            // TODO can we calculate which layout molecules must have inline css on backend side? layout sort order does not work for html elements with position absolute etc.
            // generate CSS
            var siteCss = GenerateCss(californiaProject); // TODO store css


            // TODO generate javascript (classic): 1) async style loader 2) environment loaders (fonts, wordpress extensions, ...)
            // TODO generate javascript (California-Browser)
            // TODO parse/include client html, css, js
            // TODO count bytes of final website
            var htmlSize = siteHtml.Length;
            var cssSize = siteCss.Length;
            var totalSizeKB = (cssSize + htmlSize) / 1000; // TODO float calculation
            // TODO save copy (make restorable from?)
            // TODO cache rendered view
            // TODO run export processes automatically
            // TODO run tests: rendered layouts for different sizes (hidden elements, etc.), print preview, ...

            return exportedProject;
        }

        private async Task<string> GenerateHtmlAsync(CaliforniaView californiaView)
        {
            // TODO replace style ids with new numbers starting from 0
            using (StringWriter sw = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(actionContext, "ReactorPage", false);
                viewData["ReactorProject"] = published.ProjectSnapshot;
                viewData["ReactorActiveNavigation"] = targetNavigation;
                viewData["ReactorPrimaryNavigation"] = primaryNavi;
                viewData["ReactorPublishID"] = published.Id.ToString();
                ViewContext viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, sw, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);

                published.Pages[targetNavigation.DisplayName] = sw.GetStringBuilder().ToString();
            }
            return published.Pages[targetNavigation.DisplayName];
        }
        */
        /* TODO enable
        public enum InteractionType { Query, Style, Scroll, Draggable, Dropzone }; // TODO
        public enum ReactorNaviType { Page, Scroll, Tab, Sort } // TODO
        public enum ResponsiveDirection { None, Up, Down, OnlyDown } // TODO
        private enum ExportType // TODO
        {
            HtmlCss,
            Sass,
            React,
            Wordpress,
            CaliforniaPack
        }
        // TODO export for CaliforniaView/CaliforniaProject
        public async Task<string> ExportLayoutRowOrBoxAsync(LayoutBase layoutRowOrBox, ExportType exportType)
        {
            switch(exportType)
            {
                default:
                    throw new NotImplementedException();
                    break;
            }
            if (layoutRowOrBox.LayoutType == LayoutType.Box)
            {

            }
            else if (layoutRowOrBox.LayoutType == LayoutType.Row)
            {

            }
            else
            {
                throw new InvalidOperationException();
            }
            throw new NotImplementedException();
        }

        

        private List<StyleMolecule> ReadStyleMoleculesForCaliforniaViewRecursive(CaliforniaView californiaView)
        {
            // TODO data could be gathered while generating html
            throw new NotImplementedException();
            return new List<StyleMolecule>();
        }

        private enum CssGeneratorMode { None, StyleAtom, Auto };
        private string GenerateCss()
        {
            foreach (var styleMolecule in UserStyleMolecules.Dict.Values)
            {
                styleMolecule.StyleCssClasses.Clear();
                styleMolecule.StyleCssClasses.Add($"s{styleMolecule.Id}");
            }
            var styleCss = "";
            var combinedStyleCss = "";
            switch (cssGeneratorMode)
            {
                case CssGeneratorMode.None:
                    // write css variables (s1, ..., sn(mol))
                    // results in many duplicated properties
                    // ignoring CssStyleSheets[0] would be possible if pseudo atoms were separated
                    foreach (var kvp in CssStyleSheets)
                    {
                        styleCss += $"{kvp.Value}\n";
                    }
                    break;
                case CssGeneratorMode.StyleAtom:
                    foreach (var styleKvp in CssStyleSheets)
                    {
                        if (styleKvp.Key == 0)
                        {
                            // properties declared on atom level => just pseudo styles
                            foreach (var styleMolecule in UserStyleMolecules.Dict.Values)
                            {
                                styleCss += GenerateCss(styleMolecule, styleKvp.Key, true);
                            }
                            continue;
                        }
                        else
                        {
                            styleCss += styleKvp.Value;
                        }
                    }
                    foreach (var styleMolecule in UserStyleMolecules.Dict.Values)
                    {
                        styleMolecule.StyleCssClasses = styleMolecule.GetCssAtomClasses(this);
                    }
                    break;
                case CssGeneratorMode.Auto:
                    int maxIterations = 100000;
                    var styleIdsToIncludeRaw = new List<int>();
                    var generatedCssClassNames = new List<string>();
                    var newCssClassName = "a";
                    // TODO only instanced layout molecules
                    styleIdsToIncludeRaw.Add(UserNavigations.Dict[0].StyleMoleculeId);
                    foreach (var styleKvp in CssStyleSheets)
                    {
                        bool isPseudo = false;
                        for (int pseudoLoopIt = 0; pseudoLoopIt < 2; pseudoLoopIt++)
                        {
                            isPseudo = (pseudoLoopIt == 1);
                            if (styleKvp.Key == -1)
                            {
                                // atom level definitions
                                continue;
                            }
                            var styleMoleculesToProcess = UserStyleMolecules.Dict.Values.ToList();
                            var currentIteration = 0;
                            var settedCssPropertiesPerStyleMolecule = new Dictionary<int, List<KeyValuePair<string, string>>>();
                            var settedPseudoCssPropertiesPerStyleMolecule = new Dictionary<int, List<KeyValuePair<string, List<KeyValuePair<string, string>>>>>();
                            List<KeyValuePair<string, string>> settedCssProperties = null;
                            // prefill
                            foreach (var styleMolecule in UserStyleMolecules.Dict.Values)
                            {
                                if (!isPseudo)
                                {
                                    settedCssProperties = styleMolecule.ReactorStyleAtomIdsPerDevice[styleKvp.Key].SelectMany(atomId => UserStyleAtoms.Dict[atomId].CssProperties).Where(cssKvp => !string.IsNullOrEmpty(cssKvp.Value)).OrderBy(kvp => kvp.Key).ToList();
                                    settedCssPropertiesPerStyleMolecule[styleMolecule.Id] = settedCssProperties;
                                }
                                if (isPseudo)
                                {
                                    var settedPropsPerPseudo = new List<KeyValuePair<string, List<KeyValuePair<string, string>>>>();
                                    foreach (var pseudoName in styleMolecule.PseudoStyleIdsPerDevicePerSelector[styleKvp.Key].Keys)
                                    {
                                        settedCssProperties = styleMolecule.PseudoStyleIdsPerDevicePerSelector[styleKvp.Key][pseudoName].SelectMany(atomId => UserStyleAtoms.Dict[atomId].CssProperties).Where(cssKvp => !string.IsNullOrEmpty(cssKvp.Value)).OrderBy(kvp => kvp.Key).ToList();
                                        settedPropsPerPseudo.Add(new KeyValuePair<string, List<KeyValuePair<string, string>>>(pseudoName, settedCssProperties));
                                    }
                                    settedPseudoCssPropertiesPerStyleMolecule[styleMolecule.Id] = settedPropsPerPseudo;
                                }
                            }

                            while (styleMoleculesToProcess.Count > 0)
                            {
                                var currentStyle = styleMoleculesToProcess.First();
                                var innerPseudoLoopRuns = 1;
                                if (isPseudo)
                                {
                                    innerPseudoLoopRuns = currentStyle.PseudoStyleIdsPerDevicePerSelector[styleKvp.Key].Count;
                                }
                                // dynamically create css classes for settedCssProperties
                                for (int innerPseudoLoopIt = 0; innerPseudoLoopIt < innerPseudoLoopRuns; innerPseudoLoopIt++)
                                {
                                    var pseudoSelector = "";
                                    if (isPseudo)
                                    {
                                        pseudoSelector = currentStyle.PseudoStyleIdsPerDevicePerSelector[styleKvp.Key].Keys.ElementAt(innerPseudoLoopIt);
                                    }

                                    List<KeyValuePair<string, string>> currentSettetCssProperties = new List<KeyValuePair<string, string>>();
                                    if (!isPseudo)
                                    {
                                        currentSettetCssProperties = settedCssPropertiesPerStyleMolecule[currentStyle.Id];
                                    }
                                    else
                                    {
                                        var pseudoProps = settedPseudoCssPropertiesPerStyleMolecule[currentStyle.Id].FirstOrDefault(kvp => kvp.Key == pseudoSelector);
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
                                                    correspondingPropertyKvp = settedCssPropertiesPerStyleMolecule[styleMolecule.Id].FirstOrDefault(innerKvp => innerKvp.Key == currentPropertyKvp.Key);
                                                }
                                                else
                                                {
                                                    var pseudoCorresponding = settedPseudoCssPropertiesPerStyleMolecule[styleMolecule.Id].FirstOrDefault(kvp => kvp.Key == pseudoSelector);
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
                                                        currentSharedStyles.Add(styleMolecule.Id);
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
                                                isCommonForSharingStyles = mostSharedStylesPropertyKvp.Value.All(sharingStyleId =>
                                                {
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
                                            UserStyleMolecules.Dict[sharingStyleId].StyleCssClasses.Add(newCssClassName);
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
                            foreach (var rawStyleId in styleIdsToIncludeRaw)
                            {
                                var rawStyle = UserStyleMolecules.Dict[rawStyleId];
                                styleCss += GenerateCss(rawStyle, styleKvp.Key, false);
                            }
                            combinedStyleCss += styleCss.WrapMediaQuery(UserResponsiveSettings.Dict[styleKvp.Key]);
                            styleCss = "";
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("cssGenerationMode");
            }
            return combinedStyleCss;
        }

        private string GenerateCss(CaliforniaProject californiaProject, int inlineCssLimitLayoutBaseId)
        {
            string styleCss;
            string styleSheetCss = "";
            foreach (var styleMolecule in californiaProject.StyleMolecules) // TODO filter only used styles
            {
                // pseudo styles
                if (!isPseudo)
                {
                    if (styleMolecule.ReactorStyleAtomIdsPerDevice.ContainsKey(responsiveId))
                    {
                        styleCss = string.Join("", GetStyleAtoms(styleMolecule.ReactorStyleAtomIdsPerDevice[responsiveId]).SelectMany(atom => atom.CssProperties.Where(property => property.Value != null && property.Value != "")).Select(prop => $"{prop.Key}:{prop.Value};"));
                        if (!string.IsNullOrEmpty(styleCss))
                        {
                            styleSheetCss += $".s{styleMolecule.Id}{{{styleCss}}}\n";
                        }
                    }
                }
                else //if (isPseudo)
                {
                    if (styleMolecule.PseudoStyleIdsPerDevicePerSelector.ContainsKey(responsiveId))
                    {
                        foreach (var pseudoSelectorKvp in styleMolecule.PseudoStyleIdsPerDevicePerSelector[responsiveId])
                        {
                            styleCss = "";
                            foreach (var pseudoId in pseudoSelectorKvp.Value)
                            {
                                var pseudoStyle = UserStyleAtoms.Dict[pseudoId];
                                styleCss += string.Join("", pseudoStyle.CssProperties.Where(prop => (prop.Value != null) && (prop.Value != "")).Select(prop => $"{prop.Key}:{prop.Value};"));
                                styleCss += "\n";
                            }
                            if (!string.IsNullOrEmpty(styleCss))
                            {
                                styleSheetCss += $".s{styleMolecule.Id}{pseudoSelectorKvp.Key}{{{styleCss}}}\n";
                            }
                        }
                    }
                }
            }
            return styleSheetCss;
        }

        */

        public async Task MoveLayoutMoleculeNextToLayoutMoleculeAsync(LayoutBase movedLayoutMolecule, LayoutBase targetNeighborLayoutMolecule, bool isMoveBefore)
        {
            // TODO compare Serializable to dbContext.UpdateRange()
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                await MoveLayoutMoleculeNextToLayoutMoleculeInSerializableTransactionAsync(movedLayoutMolecule, targetNeighborLayoutMolecule, isMoveBefore);
                transaction.Commit();
            }
        }

        private async Task MoveLayoutMoleculeNextToLayoutMoleculeInSerializableTransactionAsync(LayoutBase movedLayoutMolecule, LayoutBase targetNeighborLayoutMolecule, bool isMoveBefore)
        {
            // TODO expensive because it also mixes sort order of layout molecules from other views etc..., // TODO test if locks much with reorder/molecules created by other users
            if (movedLayoutMolecule.LayoutBaseId == targetNeighborLayoutMolecule.LayoutBaseId)
            {
                throw new InvalidOperationException("Provided layout molecules must be different.");
            }
            if (isMoveBefore == false)
            {
                throw new NotImplementedException("Can only move in front of another item.");
            }
            if (movedLayoutMolecule.LayoutType == LayoutType.Atom)
            {
                var movedLayoutAtom = movedLayoutMolecule as LayoutAtom;
                if (targetNeighborLayoutMolecule.LayoutType == LayoutType.Atom)
                {
                    if (movedLayoutAtom.PlacedAtomInBoxId != (targetNeighborLayoutMolecule as LayoutAtom).PlacedAtomInBoxId)
                    {
                        throw new NotImplementedException("Move layout atom into the same box as target atom.");
                    }
                }
                else if (targetNeighborLayoutMolecule.LayoutType == LayoutType.Box)
                {
                    var targetNeighborLayoutBox = targetNeighborLayoutMolecule as LayoutBox;
                    if (movedLayoutAtom.PlacedAtomInBoxId != targetNeighborLayoutBox.PlacedBoxInBoxId)
                    {
                        throw new NotImplementedException("Move layout atom into the same box as target box.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Incompatible move types.");
                }
            }
            else if (movedLayoutMolecule.LayoutType == LayoutType.Box)
            {
                LayoutBox movedLayoutBox = movedLayoutMolecule as LayoutBox;
                if (targetNeighborLayoutMolecule.LayoutType == LayoutType.Atom)
                {
                    LayoutAtom targetNeighborLayoutAtom = targetNeighborLayoutMolecule as LayoutAtom;
                    if (movedLayoutBox.PlacedBoxInBoxId != targetNeighborLayoutAtom.PlacedAtomInBoxId)
                    {
                        throw new NotImplementedException("Move layout box into the same box as atom.");
                    }
                }
                else if (targetNeighborLayoutMolecule.LayoutType == LayoutType.Box)
                {
                    LayoutBox targetNeighborLayoutBox = targetNeighborLayoutMolecule as LayoutBox;
                    if (movedLayoutBox.PlacedBoxInBoxId == null)
                    {
                        if (targetNeighborLayoutBox.PlacedBoxInBoxId != null || movedLayoutBox.BoxOwnerRowId != targetNeighborLayoutBox.BoxOwnerRowId)
                        {
                            throw new NotImplementedException("Move layout box into the same row/box.");
                        }
                    }
                    else
                    {
                        if (movedLayoutBox.PlacedBoxInBoxId != targetNeighborLayoutBox.PlacedBoxInBoxId) // box is in the same parent row implicitely
                        {
                            throw new NotImplementedException("Move layout box into the same row/box.");
                        }
                    }
                } 
                else
                {
                    throw new InvalidOperationException("Incompatible move types.");
                }
            }
            else if (movedLayoutMolecule.LayoutType == LayoutType.Row) // TODO check all enum in -if else if- usage for final else => exception statement
            {
                if (targetNeighborLayoutMolecule.LayoutType != LayoutType.Row)
                {
                    throw new InvalidOperationException("Incompatible move types.");
                }
                if ((movedLayoutMolecule as LayoutRow).PlacedOnViewId != (targetNeighborLayoutMolecule as LayoutRow).PlacedOnViewId)
                {
                    throw new NotImplementedException(); // TODO
                }
            }
            else
            {
                throw new ApplicationException("Unexpected layout type.");
            }

            
            var moveItem = movedLayoutMolecule; // TODO code duplication
            var targetItem = targetNeighborLayoutMolecule; // TODO code duplication

            // update item position key: example: user (A) moves item 5 in front of item 2; example*: user (A) moves item 2 in front of item 6
            // element => positionKey
            // state1:             |     state2:                |     state2*:
            // (B)item 1 => 1      |     (B)item 1 => 1         |     (B)item 1 => 1
            // (A)item 2 => 2      |     (A)item 2 => -4-       |     (A)item 2 => -5-
            // (B)item 3 => 3      |     (B)item 3 => 3         |     (B)item 3 => 3
            // (A)item 4 => 4      |     (A)item 4 => -5-       |     (A)item 4 => -2-
            // (A)item 5 => 5      |     (A)item 5 => -2-       |     (A)item 5 => -4-
            // (A)item 6 => 6      |     (A)item 6 => 6         |     (A)item 6 => 6
            // (A)item 7 => 7      |     (A)item 7 => 7         |     (A)item 7 => 7
            // (A)item 8 => 8      |     (A)item 8 => 8         |     (A)item 8 => 8
            // (B)item 9 => 9      |     (B)item 9 => 9         |     (B)item 9 => 9
            // (B)item 10 => 10    |     (B)item 10 => 10       |     (B)item 10 => 10

            bool isMoveDirectionUp = moveItem.LayoutSortOrderKey > targetItem.LayoutSortOrderKey;
            var minAffectedSortKey = isMoveDirectionUp ? targetItem.LayoutSortOrderKey : moveItem.LayoutSortOrderKey; // TODO assert with math min
            var maxAffectedSortKey = isMoveDirectionUp ? moveItem.LayoutSortOrderKey : (targetItem.LayoutSortOrderKey - 1);

            // TODO should only affect items in the next higher up container instead of all project elements
            List<LayoutBase> affectedItems;
            if (_serviceOptions.DatabaseTechnology == CaliforniaServiceOptions.DbEngineSelector.Mssql)
            {
                affectedItems = await _data.Set<LayoutBase>().FromSql($"SELECT * FROM dbo.LayoutBase WITH (TABLOCKX, HOLDLOCK) WHERE (CaliforniaProjectId={movedLayoutMolecule.CaliforniaProjectId} AND LayoutSortOrderKey>={minAffectedSortKey} AND LayoutSortOrderKey<={maxAffectedSortKey})").OrderBy(l => l.LayoutSortOrderKey).ToListAsync();
            }
            else if (_serviceOptions.DatabaseTechnology == CaliforniaServiceOptions.DbEngineSelector.Sqlite)
            {
                affectedItems = await _data.Set<LayoutBase>().FromSql($"SELECT * FROM LayoutBase WHERE (CaliforniaProjectId={movedLayoutMolecule.CaliforniaProjectId} AND LayoutSortOrderKey>={minAffectedSortKey} AND LayoutSortOrderKey<={maxAffectedSortKey})").OrderBy(l => l.LayoutSortOrderKey).ToListAsync();
            }
            else
            {
                throw new NotImplementedException(_serviceOptions.DatabaseTechnology.ToString());
            }

            /*Generated SQL Query after two lines:
                info: Microsoft.EntityFrameworkCore.Database.Command[20101]
                    Executed DbCommand (28ms) [Parameters=[@p0='403', @p1='15395', @p2='15400'], CommandType='Text', CommandTimeout='30']
                    SELECT * FROM dbo.LayoutBase WITH (TABLOCKX, HOLDLOCK) WHERE (CaliforniaProjectId=@p0 AND LayoutSortOrderKey>=@p1 AND LayoutSortOrderKey<=@p2) ORDER BY LayoutSortOrderKey;
                info: Microsoft.EntityFrameworkCore.Database.Command[20101]
                    Executed DbCommand (6ms) [Parameters=[@p1='21893', @p0='15395', @p3='21900', @p2='15400'], CommandType='Text', CommandTimeout='30']
                    SET NOCOUNT ON;
                    UPDATE [LayoutBase] SET [LayoutSortOrderKey] = @p0
                    WHERE [LayoutBaseId] = @p1;
                    SELECT @@ROWCOUNT;

                    UPDATE [LayoutBase] SET [LayoutSortOrderKey] = @p2
                    WHERE [LayoutBaseId] = @p3;
                    SELECT @@ROWCOUNT;
             */
            //var affectedItems = await _data.Set<LayoutBase>().FromSql($"SELECT * FROM dbo.LayoutBase WITH (TABLOCKX, HOLDLOCK)").Where(l => l.CaliforniaProjectId==movedLayoutMolecule.CaliforniaProjectId && l.LayoutSortOrderKey>=minAffectedSortKey && l.LayoutSortOrderKey<=maxAffectedSortKey).OrderBy(l => l.LayoutSortOrderKey).ToListAsync();
            /*Generated SQL Query after two lines:
                info: Microsoft.EntityFrameworkCore.Database.Command[20101]
                    Executed DbCommand (2ms) [Parameters=[@__californiaContext_CaliforniaEvent_0='MoveLayoutMoleculeNextToLayoutMolecule'], CommandType='Text', CommandTimeout='30']
                    SELECT TOP(1) [e].[CaliforniaEventLogId], [e].[CaliforniaEvent], [e].[NTimesCalled], [e].[RowVersion]
                    FROM [CaliforniaEventLogs] AS [e]
                    WHERE [e].[CaliforniaEvent] = @__californiaContext_CaliforniaEvent_0
                info: Microsoft.EntityFrameworkCore.Database.Command[20101]
                    Executed DbCommand (22ms) [Parameters=[@__movedLayoutMolecule_CaliforniaProjectId_1='403', @__minAffectedSortKey_2='15401', @__maxAffectedSortKey_3='15402'], CommandType='Text', CommandTimeout='30']
                    SELECT [l].[LayoutBaseId], [l].[CaliforniaProjectId], [l].[Discriminator], [l].[LayoutSortOrderKey], [l].[PlacedAtomInBoxId], [l].[BoxOwnerRowId], [l].[PlacedBoxInBoxId], [l].[SpecialLayoutBoxType], [l].[PlacedOnViewId]
                    FROM (
                        SELECT * FROM dbo.LayoutBase WITH (TABLOCKX, HOLDLOCK)
                    ) AS [l]
                    WHERE [l].[Discriminator] IN (N'LayoutRow', N'LayoutBox', N'LayoutAtom') AND ((([l].[CaliforniaProjectId] = @__movedLayoutMolecule_CaliforniaProjectId_1) AND ([l].[LayoutSortOrderKey] >= @__minAffectedSortKey_2)) AND ([l].[LayoutSortOrderKey] <= @__maxAffectedSortKey_3))
                    ORDER BY [l].[LayoutSortOrderKey]
             */
            //var affectedItems = await _data.Set<LayoutBase>().FromSql($"SELECT * FROM dbo.{nameof(LayoutBase)} WITH (TABLOCKX) " TODO
            //var affectedItems = await _data.Set<LayoutBase>().FromSql($"SELECT * FROM dbo.{nameof(LayoutBase)} WITH (XLOCK, ROWLOCK) "
            //var affectedItems = await _data.Set<LayoutBase>().FromSql($"SELECT * FROM dbo.{nameof(LayoutBase)} WITH (UPDLOCK) " 

            var affectedCount = affectedItems.Count;
            // TODO assert affectedCount > 0
            var previousIndexes = affectedItems.Select(w => w.LayoutSortOrderKey).ToList();

            for (int i = 0; i < affectedCount; i++)
            {
                var takeFromIndex = isMoveDirectionUp ? (i + 1) : (i - 1);
                if (takeFromIndex < 0)
                {
                    takeFromIndex = affectedCount - 1;
                }
                else if (takeFromIndex == affectedCount)
                {
                    takeFromIndex = 0;
                }
                affectedItems[i].LayoutSortOrderKey = previousIndexes[takeFromIndex];
            }
            //await Task.Delay(10000); TODO tests
            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != affectedCount)
            {
                throw new ApplicationException($"Changing position for layout molecule {movedLayoutMolecule.LayoutBaseId} failed.");
            }
        }

        public async Task MoveLayoutMoleculeIntoLayoutMoleculeAsync(LayoutBase movedLayoutMolecule, LayoutBase targetContainerLayoutMolecule)
        {
            if (movedLayoutMolecule.LayoutBaseId == targetContainerLayoutMolecule.LayoutBaseId)
            {
                throw new InvalidOperationException("Provided layout molecules must be different.");
            }
            if (movedLayoutMolecule.LayoutType == LayoutType.Atom)
            {
                if (targetContainerLayoutMolecule.LayoutType != LayoutType.Box)
                {
                    throw new InvalidOperationException("Layout atom can only be moved into a layout box.");
                }
                else
                {
                    (movedLayoutMolecule as LayoutAtom).PlacedAtomInBoxId = targetContainerLayoutMolecule.LayoutBaseId;
                }
            }
            else if (movedLayoutMolecule.LayoutType == LayoutType.Box)
            {
                LayoutBox movedLayoutBox = movedLayoutMolecule as LayoutBox;
                if (targetContainerLayoutMolecule.LayoutType == LayoutType.Box)
                {
                    movedLayoutBox.PlacedBoxInBoxId = targetContainerLayoutMolecule.LayoutBaseId;
                    UpdateLayoutBoxOwnerRowRecursive(movedLayoutBox, (targetContainerLayoutMolecule as LayoutBox).BoxOwnerRowId);
                }
                else if (targetContainerLayoutMolecule.LayoutType == LayoutType.Row)
                {
                    movedLayoutBox.PlacedBoxInBoxId = null;
                    UpdateLayoutBoxOwnerRowRecursive(movedLayoutBox, targetContainerLayoutMolecule.LayoutBaseId);
                }
                else
                {
                    throw new InvalidOperationException("Layout box can only be moved into a layout row or box.");
                }
            }
            else
            {
                throw new NotImplementedException(); // TODO move row
            }

            var affectedRows = await _data.SaveChangesAsync();
        }

        private void UpdateLayoutBoxOwnerRowRecursive(LayoutBox layoutBox, int boxOwnerRowId)
        {
            layoutBox.BoxOwnerRowId = boxOwnerRowId;
            foreach (var subBox in layoutBox.PlacedInBoxBoxes)
            {
                subBox.BoxOwnerRowId = boxOwnerRowId;
                UpdateLayoutBoxOwnerRowRecursive(subBox, boxOwnerRowId);
            }
        }

        public async Task SetLayoutMoleculeAsInstanceableAsync(CaliforniaProject californiaProject, LayoutBase targetLayoutRowOrBox)
        {
            // TODO also store interactions, if interaction target is contained in layout molecule // TODO otherwise display error
            if (targetLayoutRowOrBox.LayoutType != LayoutType.Box && targetLayoutRowOrBox.LayoutType != LayoutType.Row)
            {
                throw new InvalidOperationException("Layout molecule must have row or box type.");
            }
            var targetCaliforniaView = await _data.Set<CaliforniaView>()
                .Include(v => v.PlacedLayoutRows)
                .FirstAsync(v => v.CaliforniaProjectId == californiaProject.CaliforniaProjectId && v.IsInternal == true && v.Name == "[Internal] User Layout Molecules");
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable))
            {
                // clone layout molecule
                if (targetLayoutRowOrBox.LayoutType == LayoutType.Row)
                {
                    var targetLayoutRow = targetLayoutRowOrBox as LayoutRow;
                    var clonedRow = await CloneAndRegisterLayoutRowRecursiveInTransactionAsync(californiaProject.CaliforniaProjectId, targetLayoutRow, targetCaliforniaView);
                }
                else
                {
                    var targetLayoutBox = targetLayoutRowOrBox as LayoutBox;
                    var userLayoutBoxesHolderRow = targetCaliforniaView.PlacedLayoutRows.First(); // TODO depends on project structure
                    var clonedBox = await CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(californiaProject.CaliforniaProjectId, targetLayoutBox, userLayoutBoxesHolderRow, null);
                }
                await _data.SaveChangesAsync();
                transaction.Commit();
            }
        }

        public async Task SetSpecialLayoutBoxTypeAsync(LayoutBox layoutBox, SpecialLayoutBoxType specialLayoutBoxType)
        {
            layoutBox.SpecialLayoutBoxType = specialLayoutBoxType;
            await _data.SaveChangesAsync();
        }

        public async Task SyncStyleMoleculeToReferenceStyleAsync(StyleMolecule styleMolecule)
        {
            if (styleMolecule.ClonedFromStyleId == null)
            {
                throw new InvalidOperationException("Target style is reference style molecule.");
            }
            // TODO merge load with authorization
            styleMolecule = await ReadStyleMoleculeByStyleAsync(styleMolecule.StyleMoleculeId, false);
            var referenceStyleMolecule = await ReadStyleMoleculeByStyleAsync(styleMolecule.ClonedFromStyleId.Value, true);
            // hard sync to reference style molecule
            referenceStyleMolecule.MappedStyleAtoms.Clear(); // TODO data remove? or cascades?
            throw new NotImplementedException();
            if (styleMolecule.ClonedFromStyleId == null)
            {
                throw new InvalidOperationException("Reference style is not set.");
            }
            if (referenceStyleMolecule.ClonedFromStyleId.Value != styleMolecule.ClonedFromStyleId) // TODO best practise? TODO document comparison int and int? will break if either type changes
            {
                throw new InvalidOperationException("Reference style is not equal.");
            }
            CloneStyleSetup(referenceStyleMolecule, styleMolecule);
            // soft sync to reference style clones (except target style molecule)
            // TODO soft sync: should atoms be deleted with all applied values, when they are set in sync target but are missing in sync source
            foreach (var clonedStyle in referenceStyleMolecule.CloneOfStyles.Except(new StyleMolecule[] { styleMolecule }))
            {
                throw new NotImplementedException();
                // TODO soft sync does not work with current implementation, when an atom is removed with multiple values at once
            }
            throw new NotImplementedException();
            await _data.SaveChangesAsync();
        }

        public async Task SyncStyleMoleculeFromReferenceStyleAsync(StyleMolecule styleMolecule)
        {
            if (styleMolecule.ClonedFromStyleId == null)
            {
                throw new InvalidOperationException("Target style is reference style molecule.");
            }
            // TODO merge load with authorization
            styleMolecule = await ReadStyleMoleculeByStyleAsync(styleMolecule.StyleMoleculeId, false);
            var referenceStyleMolecule = await ReadStyleMoleculeByStyleAsync(styleMolecule.ClonedFromStyleId.Value, false);
            styleMolecule.MappedStyleAtoms.Clear(); // TODO data remove? or cascades?
            throw new NotImplementedException(); // TODO best practise: throw notimplemented better than comment for intellisense (?) for partial code + warning as reminder
            if (styleMolecule.ClonedFromStyleId == null)
            {
                throw new InvalidOperationException("Reference style is not set.");
            }
            if (referenceStyleMolecule.ClonedFromStyleId.Value != styleMolecule.ClonedFromStyleId) // TODO best practise? TODO document comparison int and int? will break if either type changes
            {
                throw new InvalidOperationException("Reference style is not equal.");
            }
            CloneStyleSetup(styleMolecule, referenceStyleMolecule);
            await _data.SaveChangesAsync();
        }

        public async Task SyncLayoutStylesImitatingReferenceLayoutAsync(LayoutBase targetLayoutMolecule, LayoutBase referenceLayoutMolecule)
        {
            await SyncLayoutStylesImitatingReferenceLayoutRecursive(targetLayoutMolecule, referenceLayoutMolecule);
            await _data.SaveChangesAsync();
        }

        private async Task SyncLayoutStylesImitatingReferenceLayoutRecursive(LayoutBase targetLayoutMolecule, LayoutBase referenceLayoutMolecule)
        {
            if (targetLayoutMolecule.LayoutBaseId == referenceLayoutMolecule.LayoutBaseId)
            {
                throw new ApplicationException("Target layout molecule must be different.");
            }
            // TODO document depending on data model and expected structure: optimization.. process request until exception vs. check all requirements ad priori
            // layout+style structure must be identical: same recursive structure row=>boxes=>atoms AND same ordering in every layer AND reference styles must be identical
            if (targetLayoutMolecule.LayoutType != referenceLayoutMolecule.LayoutType)
            {
                throw new InvalidOperationException("Layout types must be equal.");
            }
            var targetStyleMolecule = await ReadStyleMoleculeByLayoutAsync(targetLayoutMolecule.LayoutBaseId, false);
            var referenceStyleMolecule = await ReadStyleMoleculeByLayoutAsync(referenceLayoutMolecule.LayoutBaseId, false);
            if (targetStyleMolecule.ClonedFromStyleId == null)
            {
                throw new InvalidOperationException("Reference style is not set.");
            }
            if (referenceStyleMolecule.ClonedFromStyleId.Value != targetStyleMolecule.ClonedFromStyleId) // TODO best practise? TODO document comparison int and int? will break if either type changes
            {
                throw new InvalidOperationException("Reference style is not equal.");
            }
            targetStyleMolecule.MappedStyleAtoms.Clear();
            CloneStyleSetup(targetStyleMolecule, referenceStyleMolecule);
            if (targetLayoutMolecule.LayoutType == LayoutType.Row)
            {
                LayoutRow targetRow = targetLayoutMolecule as LayoutRow;
                LayoutRow referenceRow = referenceLayoutMolecule as LayoutRow;
                var orderedBoxes = targetRow.AllBoxesBelowRow.Where(b => b.BoxOwnerRowId == targetRow.LayoutBaseId).OrderBy(b => b.LayoutSortOrderKey);
                var orderedBoxesInReference = referenceRow.AllBoxesBelowRow.Where(b => b.BoxOwnerRowId == referenceRow.LayoutBaseId).OrderBy(b => b.LayoutSortOrderKey);
                if (orderedBoxes.Count() != orderedBoxesInReference.Count())
                {
                    throw new InvalidOperationException("Layout box count in row is not equal.");
                }
                for (int i = 0; i < orderedBoxes.Count(); i++)
                {
                    await SyncLayoutStylesImitatingReferenceLayoutRecursive(orderedBoxes.ElementAt(i), orderedBoxesInReference.ElementAt(i));
                }
            }
            else if (targetLayoutMolecule.LayoutType == LayoutType.Box)
            {
                LayoutBox targetBox = targetLayoutMolecule as LayoutBox;
                LayoutBox referenceBox = referenceLayoutMolecule as LayoutBox;
                if (targetBox.SpecialLayoutBoxType != referenceBox.SpecialLayoutBoxType)
                {
                    throw new InvalidOperationException("Special layout box type is not equal.");
                }
                var orderedBoxes = targetBox.PlacedInBoxBoxes.OrderBy(b => b.LayoutSortOrderKey).Cast<LayoutBase>(); // TODO code duplication with some places (rendering, recursive box cloning)
                var orderedAtoms = targetBox.PlacedInBoxAtoms.OrderBy(b => b.LayoutSortOrderKey).Cast<LayoutBase>();
                var orderedBoxesInReference = referenceBox.PlacedInBoxBoxes.OrderBy(b => b.LayoutSortOrderKey).Cast<LayoutBase>();
                var orderedAtomsInReference = referenceBox.PlacedInBoxAtoms.OrderBy(b => b.LayoutSortOrderKey).Cast<LayoutBase>();
                if (orderedBoxes.Count() != orderedBoxesInReference.Count())
                {
                    throw new InvalidOperationException("Layout box count in box is not equal.");
                }
                if (orderedAtoms.Count() != orderedAtomsInReference.Count())
                {
                    throw new InvalidOperationException("Layout atom count in box is not equal.");
                }
                var orderedBoxesAndAtoms = orderedBoxes.Concat(orderedAtoms).OrderBy(x => x.LayoutSortOrderKey);
                var orderedBoxesAndAtomsInReference = orderedBoxesInReference.Concat(orderedAtomsInReference).OrderBy(x => x.LayoutSortOrderKey);
                for (int i = 0; i < orderedBoxesAndAtoms.Count(); i++)
                {
                    await SyncLayoutStylesImitatingReferenceLayoutRecursive(orderedBoxesAndAtoms.ElementAt(i), orderedBoxesAndAtomsInReference.ElementAt(i));
                }
            }
        }

        public async Task SetStyleMoleculeReferenceAsync(StyleMolecule styleMolecule, StyleMolecule referenceStyleMolecule)
        {
            if (styleMolecule.StyleMoleculeId == referenceStyleMolecule.StyleMoleculeId)
            {
                throw new InvalidOperationException("Style cannot be clone of itself.");
            }
            if (referenceStyleMolecule.ClonedFromStyleId != null)
            {
                throw new InvalidOperationException("Selected new reference style is not a valid reference style.");
            }
            // TODO merge load full style setup with authorization
            referenceStyleMolecule = await ReadStyleMoleculeByStyleAsync(referenceStyleMolecule.StyleMoleculeId, false);
            // if clones of target style exist, promote first style to reference style and update reference for clones
            if (styleMolecule.CloneOfStyles.Count > 0)
            {
                var firstClone = styleMolecule.CloneOfStyles.First();
                firstClone.ClonedFromStyleId = null;
                foreach (var clonedStyle in styleMolecule.CloneOfStyles.Skip(1))
                {
                    clonedStyle.ClonedFromStyleId = firstClone.StyleMoleculeId;
                }
            }
            // sync style settings for new reference style
            styleMolecule.MappedStyleAtoms.Clear(); // TODO data remove? or cascades?
            styleMolecule.ClonedFromStyleId = referenceStyleMolecule.StyleMoleculeId;
            CloneStyleSetup(styleMolecule, referenceStyleMolecule);
            await _data.SaveChangesAsync();
        }

        public async Task SetStyleMoleculeAsReferenceStyleAsync(StyleMolecule styleMolecule)
        {
            styleMolecule.ClonedFromStyleId = null;
            await _data.SaveChangesAsync();
        }

        public async Task MoveStyleAtomToResponsiveDevice(StyleAtom styleAtom, ResponsiveDevice responsiveDevice)
        {
            var currentStateModifier = styleAtom.MappedToMolecule.StateModifier;
            var styleMolecule = await _data.Set<StyleMolecule>()
                .Include(s => s.MappedStyleAtoms)
                    .ThenInclude(at => at.StyleAtom)
                .Include(s => s.CloneOfStyles)
                .FirstAsync(s => s.StyleMoleculeId == styleAtom.MappedToMolecule.StyleMoleculeId);
            if (styleMolecule.CloneOfStyles.Count > 0)
            {
                // TODO soft sync / changes to child molecules
                throw new NotImplementedException();
            }
            if (styleMolecule.MappedStyleAtoms.Any(map => map.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && map.StyleAtom.StyleAtomType == styleAtom.StyleAtomType && map.StateModifier == currentStateModifier)) // TODO this "equivalence check" is used at multiple locations, create unique-Index?
            {
                // TODO merge operation
                var stateModifierInfo = currentStateModifier != null ? $"(state: {currentStateModifier})" : "";
                throw new ApplicationException($"Style atom of type {styleAtom.StyleAtomType} already exists for responsive device {responsiveDevice.NameShort}{stateModifierInfo}.");
            }
            styleAtom.MappedToMolecule.ResponsiveDevice = responsiveDevice;
            await _data.SaveChangesAsync();
        }

        private void AddBoxAndContentsToProjectRecursiveTODO(CaliforniaProject californiaProject, LayoutBox box, LayoutRow ownerRow)
        {
            foreach (var subBox in box.PlacedInBoxBoxes)
            {
                AddBoxAndContentsToProjectRecursiveTODO(californiaProject, subBox, ownerRow);
            }
            foreach (var atom in box.PlacedInBoxAtoms)
            {
                californiaProject.ContentAtoms.Add(atom.HostedContentAtom);
                californiaProject.StyleMolecules.Add(atom.StyleMolecule);
            }
            californiaProject.LayoutMolecules.AddRange(box.PlacedInBoxAtoms);
            californiaProject.StyleMolecules.Add(box.StyleMolecule);
            box.BoxOwnerRow = ownerRow;
            californiaProject.LayoutMolecules.Add(box);
        }

        public async Task SetLayoutBoxCountForRowAsync(CaliforniaContext californiaContext, int layoutRowId, int boxStyleMoleculeId, int targetBoxCount, bool isFitWidth)
        {
            if (isFitWidth)
            {
                throw new NotImplementedException();
            }
            var layoutRow = await _data.Set<LayoutRow>()
                    .Include(r => r.AllBoxesBelowRow)
                        .ThenInclude(bo => bo.PlacedInBoxBoxes)
                    .Include(r => r.AllBoxesBelowRow)
                        .ThenInclude(bo => bo.PlacedInBoxAtoms)
                    .FirstAsync(r => r.LayoutBaseId == layoutRowId);
            // TODO must be only applied for directly inside row boxes
            var layoutBaseComparer = new Comparison<LayoutBox>((b1, b2) => b1.LayoutSortOrderKey < b2.LayoutSortOrderKey ? -1 : b1.LayoutSortOrderKey > b2.LayoutSortOrderKey ? 1 : throw new ApplicationException("Sort order keys are not unique."));
            var directlyInsideRowBoxes = layoutRow.AllBoxesBelowRow.Where(boxAnyDeepness => boxAnyDeepness.PlacedBoxInBoxId == null).ToList();
            directlyInsideRowBoxes.Sort(layoutBaseComparer);
            var currentBoxCount = directlyInsideRowBoxes.Count;
            if (currentBoxCount == targetBoxCount)
            {
                // do nothing
                return;
            }
            else if (targetBoxCount > currentBoxCount)
            {
                // create boxes
                for (int i = 0; i < (targetBoxCount - currentBoxCount); i++)
                {
                    // TODO fix created box sizes to add up to 100% (?)
                    var styleMolecule = await CloneAndRegisterStyleMoleculeAsync(layoutRow.CaliforniaProjectId, boxStyleMoleculeId);
                    var createdBox = new LayoutBox() { StyleMolecule = styleMolecule, BoxOwnerRowId = layoutRow.LayoutBaseId, CaliforniaProjectId = layoutRow.CaliforniaProjectId, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                    layoutRow.AllBoxesBelowRow.Add(createdBox);
                }
            }
            else
            {
                // remove boxes
                var deleteCount = currentBoxCount - targetBoxCount;
                int lastRemainingBoxIndex = currentBoxCount - deleteCount - 1;
                var replacementBox = directlyInsideRowBoxes[lastRemainingBoxIndex];
                // TODO replacement box must allow adding of new layout elements
                //if (replacementBox.BoxType == autoElements)
                //{
                //    var styleMolecule = await CloneAndRegisterStyleMoleculeAsync(layoutRow.CaliforniaProjectId, boxStyleMoleculeId);
                //    var createdBox = new LayoutBox() { StyleMolecule = styleMolecule, BoxOwnerRowId = layoutRow.LayoutBaseId, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
                //    _data.Add(createdBox);
                //    layoutRow.InsideRowBoxes.Add(createdBox);
                //    lastRemainingBoxIndex += 1;
                //}
                for (int i = 1; i <= deleteCount; i++)
                {
                    var deleteBox = directlyInsideRowBoxes[lastRemainingBoxIndex + i];
                    //if (deleteBox.BoxType != autoElements)
                    //{
                    // move atoms from deleted box to last remaining box
                    replacementBox.PlacedInBoxBoxes.AddRange(deleteBox.PlacedInBoxBoxes);
                    deleteBox.PlacedInBoxBoxes.Clear(); // TODO is this necessary or are keys already updated with addRange
                    replacementBox.PlacedInBoxAtoms.AddRange(deleteBox.PlacedInBoxAtoms);
                    deleteBox.PlacedInBoxAtoms.Clear(); // TODO is this necessary or are keys already updated with addRange
                    _data.Remove(deleteBox);
                    //}
                }
                for (int i = 0; i < deleteCount; i++)
                {
                    layoutRow.AllBoxesBelowRow.Remove(directlyInsideRowBoxes[lastRemainingBoxIndex + 1 + i]);
                }
            }
            await _data.SaveChangesAsync();
        }

        public async Task DeleteLayoutAtomAsync(CaliforniaContext californiaContext, int layoutBaseId, bool saveChanges)
        {
            var layoutAtom = await _data.Set<LayoutAtom>()
                    .Include(a => a.HostedContentAtom)
                    .FirstAsync(a => a.LayoutBaseId == layoutBaseId);

            if (layoutAtom.HostedContentAtom != null)
            {
                await DeleteContentAtomAsync(layoutAtom.HostedContentAtom.ContentAtomId, false);
            }
            _data.Remove(layoutAtom);
            if (saveChanges)
            {
                await _data.SaveChangesAsync();
            }
        }

        public async Task DeleteContentAtomAsync(int contentAtomId) => await DeleteContentAtomAsync(contentAtomId, true);
        private async Task DeleteContentAtomAsync(int contentAtomId, bool saveChanges)
        {
            // content atom is not deleted from database, but instead put into deleted state (filter on IsDeleted) and gets in retention period
            // TODO auto delete after retention period / TEST legal consequences of retention after customer cancels contract / TEST legal knowledge of customer of retention
            var contentAtom = await _data.FindAsync<ContentAtom>(contentAtomId);

            if (contentAtom.IsDeleted)
            {
                throw new ApplicationException("Content atom has already been scheduled for deletion.");
            }

            contentAtom.IsDeleted = true;
            contentAtom.DeletedDate = DateTimeOffset.UtcNow;
            contentAtom.InstancedOnLayoutId = null;

            if (saveChanges)
            {
                await _data.SaveChangesAsync();
            }
        }

        public async Task DeleteLayoutBoxOrBelowRecursiveAsync(CaliforniaContext californiaContext, int layoutBaseId, bool saveChanges, bool isOnlyBelow)
        {
            var layoutBox = await _data.Set<LayoutBox>()
                    .Include(b => b.HostedViewMappings)
                    .Include(b => b.PlacedInBoxAtoms)
                    .Include(b => b.PlacedInBoxBoxes)
                    .FirstAsync(b => b.LayoutBaseId == layoutBaseId);

            if (layoutBox.HostedViewMappings.Count > 0)
            {
                throw new NotImplementedException();
            }
            foreach (var layoutAtom in layoutBox.PlacedInBoxAtoms)
            {
                await DeleteLayoutAtomAsync(californiaContext, layoutAtom.LayoutBaseId, false);
            }
            foreach (var subBox in layoutBox.PlacedInBoxBoxes)
            {
                await DeleteLayoutBoxOrBelowRecursiveAsync(californiaContext, subBox.LayoutBaseId, false, false);
            }
            if (!isOnlyBelow)
            {
                _data.Remove(layoutBox);
            }            
            if (saveChanges)
            {
                await _data.SaveChangesAsync();
            }
        }

        public async Task DeleteLayoutRowOrBelowRecursiveAsync(CaliforniaContext californiaContext, int layoutBaseId, bool isOnlyBelow)
        {
            // TODO try reducing DB calls by loading all values required for deletion here
            var layoutRow = await _data.Set<LayoutRow>()
                    .Include(r => r.AllBoxesBelowRow)
                    .FirstAsync(r => r.LayoutBaseId == layoutBaseId);

            var topBoxes = layoutRow.AllBoxesBelowRow.Where(b => b.PlacedBoxInBoxId == null);
            foreach (var layoutBox in topBoxes)
            {
                await DeleteLayoutBoxOrBelowRecursiveAsync(californiaContext, layoutBox.LayoutBaseId, false, false);
            }
            if (!isOnlyBelow)
            {
                _data.Remove(layoutRow);
            }
            await _data.SaveChangesAsync();
        }

        public async Task<CaliforniaView> CreateLayoutRowForViewAsync(CaliforniaContext californiaContext, CaliforniaView targetCaliforniaView, LayoutRow referenceLayoutRow)
        {
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)) // TODO make weaker locking, but must assure no boxes/atoms can be read/copied while cloning // TODO test locking // TODO test error in between rolls back all
            {
                var clonedLayoutRow = await CloneAndRegisterLayoutRowRecursiveInTransactionAsync(targetCaliforniaView.CaliforniaProjectId, referenceLayoutRow, targetCaliforniaView);
                transaction.Commit();
            }
            return targetCaliforniaView;
        }

        public async Task CreateLayoutBoxForBoxAsync(CaliforniaContext californiaContext, LayoutBox targetLayoutBox, LayoutBox referenceLayoutBox)
        {
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)) // TODO make weaker locking, but must assure no boxes/atoms can be read/copied while cloning // TODO test locking // TODO test error in between rolls back all
            {
                var clonedLayoutBox = await CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(targetLayoutBox.BoxOwnerRow.CaliforniaProjectId, referenceLayoutBox, targetLayoutBox.BoxOwnerRow, targetLayoutBox);
                await _data.SaveChangesAsync();
                transaction.Commit();
            }
        }

        public async Task CreateLayoutBoxForRowAsync(CaliforniaContext californiaContext, LayoutRow targetLayoutRow, LayoutBox referenceLayoutBox)
        {
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)) // TODO make weaker locking, but must assure no boxes/atoms can be read/copied while cloning // TODO test locking // TODO test error in between rolls back all
            {
                var clonedLayoutBox = await CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(targetLayoutRow.CaliforniaProjectId, referenceLayoutBox, targetLayoutRow, null);
                await _data.SaveChangesAsync();
                transaction.Commit();
            }
        }

        public async Task CreateLayoutBoxForAtomInPlaceAsync(CaliforniaContext californiaContext, LayoutAtom layoutAtom, LayoutBox referenceLayoutBox, LayoutBox atomOwnerBox)
        {
            using (var transaction = await _data.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable)) // TODO make weaker locking for first part and last part of query, but must assure no boxes/atoms can be read/copied while cloning // TODO test locking // TODO test error in between rolls back all
            {
                var clonedLayoutBox = await CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(atomOwnerBox.CaliforniaProjectId, referenceLayoutBox, atomOwnerBox.BoxOwnerRow, atomOwnerBox);
                // TODO document // TODO this will get messy 
                await MoveLayoutMoleculeNextToLayoutMoleculeInSerializableTransactionAsync(clonedLayoutBox, layoutAtom, true);
                var currentIteration = 0;
                var deepestNesting = 100; // TODO document
                var deepestBox = clonedLayoutBox;
                while (currentIteration < deepestNesting)
                {
                    if (deepestBox.PlacedInBoxBoxes.Count > 0)
                    {
                        deepestBox = deepestBox.PlacedInBoxBoxes.OrderBy(b => b.LayoutSortOrderKey).First();
                    }
                    else
                    {
                        break;
                    }
                }
                await MoveLayoutMoleculeIntoLayoutMoleculeAsync(layoutAtom, deepestBox);
                // TODO set atom sizing to auto? // TODO set box flex to nowrap?
                transaction.Commit();
            }
        }

        public async Task<LayoutBox> CreateLayoutAtomForBoxAsync(CaliforniaContext californiaContext, LayoutBox targetLayoutBox, LayoutAtom layoutAtom)
        {
            if (string.IsNullOrEmpty(layoutAtom.StyleMolecule.HtmlTag))
            {
                throw new InvalidOperationException("Reference layout atom must have html tag.");
            }
            var instanceableAtomHolderBoxStyle = await _data.Set<StyleMolecule>()
                .FirstAsync(s => s.CaliforniaProjectId == targetLayoutBox.CaliforniaProjectId && s.Name == InstanceableAtomsHolder); // TODO across projects?
            if (instanceableAtomHolderBoxStyle.StyleForLayoutId != layoutAtom.PlacedAtomInBoxId)
            {
                // use atom from default box
                var instanceableAtomHolderBox = await _data.Set<LayoutBox>()
                    .Include(b => b.PlacedInBoxAtoms)
                        .ThenInclude(at => at.StyleMolecule)
                    .Include(b => b.PlacedInBoxAtoms)
                        .ThenInclude(at => at.HostedContentAtom) // TODO interactions
                    .FirstAsync(b => b.LayoutBaseId == instanceableAtomHolderBoxStyle.StyleForLayoutId);
                layoutAtom = instanceableAtomHolderBox.PlacedInBoxAtoms.First(a => a.StyleMolecule.HtmlTag == layoutAtom.StyleMolecule.HtmlTag);
            }

            var clonedLayoutAtom = await CloneAndRegisterLayoutAtomAsync(targetLayoutBox.CaliforniaProjectId, layoutAtom);
            targetLayoutBox.PlacedInBoxAtoms.Add(clonedLayoutAtom);
            await _data.SaveChangesAsync();
            return targetLayoutBox;
        }

        private async Task<LayoutRow> CloneAndRegisterLayoutRowRecursiveInTransactionAsync(int californiaProjectId, LayoutRow referenceLayoutRow, CaliforniaView rowHolderView) // workaround sort order => force key generation
        {
            var clonedStyleMolecule = await CloneAndRegisterStyleMoleculeAsync(californiaProjectId, referenceLayoutRow.StyleMolecule.StyleMoleculeId);
            var clonedLayoutRow = new LayoutRow() { CaliforniaProjectId = californiaProjectId, StyleMolecule = clonedStyleMolecule, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            rowHolderView.PlacedLayoutRows.Add(clonedLayoutRow);
            _data.Add(clonedLayoutRow);
            await _data.SaveChangesAsync();
            var sortedBoxes = referenceLayoutRow.AllBoxesBelowRow.Where(allRowBoxes => allRowBoxes.PlacedBoxInBoxId == null).OrderBy(box => box.LayoutSortOrderKey);
            foreach (var box in sortedBoxes)
            {
                var clonedLayoutBox = await CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(californiaProjectId, box, clonedLayoutRow, null);
            }
            return clonedLayoutRow;
        }
        
        private async Task<LayoutBox> CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(int californiaProjectId, LayoutBox referenceLayoutBox, LayoutRow ownerRow, LayoutBox ownerBoxOrNull) // workaround sort order => force key generation
        {
            // TODO ordering in change tracker seems to be different/randomized from ordering in list containers; current solution: transaction around layout copy and force serial key generation
            var clonedStyleMolecule = await CloneAndRegisterStyleMoleculeAsync(californiaProjectId, referenceLayoutBox.StyleMolecule.StyleMoleculeId);
            var clonedLayoutBox = new LayoutBox() { CaliforniaProjectId = californiaProjectId, StyleMolecule = clonedStyleMolecule, BoxOwnerRow = ownerRow, SpecialLayoutBoxType = referenceLayoutBox.SpecialLayoutBoxType, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            _data.Add(clonedLayoutBox);
            ownerRow.AllBoxesBelowRow.Add(clonedLayoutBox);
            if (ownerBoxOrNull != null)
            {
                ownerBoxOrNull.PlacedInBoxBoxes.Add(clonedLayoutBox);
            }
            await _data.SaveChangesAsync();
            IOrderedEnumerable<LayoutBase> sortedBoxesAndAtoms = referenceLayoutBox.PlacedInBoxBoxes.Cast<LayoutBase>().Concat(referenceLayoutBox.PlacedInBoxAtoms.Cast<LayoutBase>()).OrderBy(layoutBase => layoutBase.LayoutSortOrderKey);
            foreach (var layoutBase in sortedBoxesAndAtoms)
            {
                if (layoutBase.LayoutType == LayoutType.Box)
                {
                    var subBox = layoutBase as LayoutBox;
                    var clonedSubBox = await CloneAndRegisterLayoutBoxRecursiveInTransactionAsync(californiaProjectId, subBox, ownerRow, clonedLayoutBox);
                    await _data.SaveChangesAsync();
                }
                else if (layoutBase.LayoutType == LayoutType.Atom)
                {
                    var subAtom = layoutBase as LayoutAtom;
                    var clonedAtom = await CloneAndRegisterLayoutAtomAsync(californiaProjectId, subAtom);
                    clonedLayoutBox.PlacedInBoxAtoms.Add(clonedAtom);
                    await _data.SaveChangesAsync();
                }
                else
                {
                    throw new ApplicationException("Unexpected layout type.");
                }
            }
            return clonedLayoutBox;
        }

        // TODO problem: sort order is not retained as expected
        /*
         * 
         * 
            Executed DbCommand (34ms) [Parameters=[@p62='28', @p63='LayoutAtom' (Nullable = false) (Size = 4000), @p64='6213', @p65='28', @p66='LayoutAtom' (Nullable = false) (Size = 4000), @p67='6213', @p68='28', 
            @p69='LayoutAtom' (Nullable = false) (Size = 4000), @p70='6213', @p71='28', @p72='LayoutAtom' (Nullable = false) (Size = 4000), @p73='6213', @p74='28', @p75='LayoutAtom' (Nullable = false) (Size = 4000), 
            @p76='6213', @p77='28', @p78='LayoutAtom' (Nullable = false) (Size = 4000), @p79='6213', @p80='28', @p81='LayoutAtom' (Nullable = false) (Size = 4000), @p82='6213', @p83='28', 
            @p84='LayoutAtom' (Nullable = false) (Size = 4000), @p85='6213', @p86='28', @p87='LayoutAtom' (Nullable = false) (Size = 4000), @p88='6212', @p89='28', @p90='LayoutAtom' (Nullable = false) (Size = 4000), 
            @p91='6213', @p92='28', @p93='LayoutAtom' (Nullable = false) (Size = 4000), @p94='6213', @p95='28', @p96='LayoutAtom' (Nullable = false) (Size = 4000), @p97='6213', @p98='28', 
            @p99='LayoutAtom' (Nullable = false) (Size = 4000), @p100='6213', @p101='28', @p102='LayoutAtom' (Nullable = false) (Size = 4000), @p103='6213', ...], CommandType='Text', CommandTimeout='30']
            SET NOCOUNT ON;
            DECLARE @inserted0 TABLE ([LayoutBaseId] int, [_Position] [int]);
            MERGE [LayoutBase] USING (
            VALUES (@p62, @p63, @p64, 0),
            (@p65, @p66, @p67, 1),
            (@p68, @p69, @p70, 2),
            (@p71, @p72, @p73, 3),
            (@p74, @p75, @p76, 4),
            (@p77, @p78, @p79, 5),
            (@p80, @p81, @p82, 6),
            (@p83, @p84, @p85, 7),
            (@p86, @p87, @p88, 8),
            (@p89, @p90, @p91, 9),
            (@p92, @p93, @p94, 10),
            (@p95, @p96, @p97, 11),
            (@p98, @p99, @p100, 12),
            (@p101, @p102, @p103, 13)) AS i ([CaliforniaProjectId], [Discriminator], [PlacedAtomInBoxId], _Position) ON 1=0
            WHEN NOT MATCHED THEN
            INSERT ([CaliforniaProjectId], [Discriminator], [PlacedAtomInBoxId])
            VALUES (i.[CaliforniaProjectId], i.[Discriminator], i.[PlacedAtomInBoxId])
            OUTPUT INSERTED.[LayoutBaseId], i._Position
            INTO @inserted0;

            SELECT [t].[LayoutBaseId], [t].[LayoutSortOrderKey] FROM [LayoutBase] t
            INNER JOIN @inserted0 i ON ([t].[LayoutBaseId] = [i].[LayoutBaseId])
            ORDER BY [i].[_Position];
         * 
         */

        private async Task<LayoutAtom> CloneAndRegisterLayoutAtomAsync(int californiaProjectId, LayoutAtom referenceLayoutAtom)
        {
            var clonedContentAtom = CloneAndRegisterContentAtom(californiaProjectId, referenceLayoutAtom.HostedContentAtom);
            var clonedStyleMolecule = await CloneAndRegisterStyleMoleculeAsync(californiaProjectId, referenceLayoutAtom.StyleMolecule.StyleMoleculeId);
            var clonedLayoutAtom = new LayoutAtom() { CaliforniaProjectId = californiaProjectId, StyleMolecule = clonedStyleMolecule, HostedContentAtom = clonedContentAtom, LayoutSortOrderKey = _californiaDbStateSqlite.GetAndIncrementLayoutBaseSortKey() };
            _data.Add(clonedLayoutAtom);
            return clonedLayoutAtom;
        }

        private async Task<StyleMolecule> CloneAndRegisterStyleMoleculeAsync(int californiaProjectId, int referenceStyleMoleculeId)
        {
            var referenceStyleMolecule = await ReadStyleMoleculeByStyleAsync(referenceStyleMoleculeId, false);
            StyleMolecule clonedStyleMolecule;
            clonedStyleMolecule = new StyleMolecule()
            {
                CaliforniaProjectId = californiaProjectId,
                HtmlTag = referenceStyleMolecule.HtmlTag,
                Name = referenceStyleMolecule.Name,
                NameShort = referenceStyleMolecule.NameShort,
                ClonedFromStyleId = referenceStyleMolecule.ClonedFromStyleId ?? referenceStyleMolecule.StyleMoleculeId // select clone reference if style molecule is already cloned
            };
            _data.Add(clonedStyleMolecule);
            // clone atoms
            CloneStyleSetup(clonedStyleMolecule, referenceStyleMolecule);
            return clonedStyleMolecule;
        }

        private void CloneStyleSetup(StyleMolecule targetStyleMolecule, StyleMolecule referenceStyleMolecule)
        {
            foreach (var map in referenceStyleMolecule.MappedStyleAtoms)
            {
                var clonedAtom = new StyleAtom()
                {
                    CaliforniaProjectId = targetStyleMolecule.CaliforniaProjectId,
                    Name = map.StyleAtom.Name,
                    StyleAtomType = map.StyleAtom.StyleAtomType
                };
                _data.Add(clonedAtom);
                foreach (var value in map.StyleAtom.AppliedValues)
                {
                    var clonedStyleValue = new StyleValue() { CaliforniaProjectId = targetStyleMolecule.CaliforniaProjectId, CssProperty = value.CssProperty, CssValue = value.CssValue };
                    clonedAtom.AppliedValues.Add(clonedStyleValue);
                    _data.Add(clonedStyleValue);
                }
                foreach (var quantum in map.StyleAtom.MappedQuantums)
                {
                    clonedAtom.ApplyStyleQuantum(quantum.StyleQuantum, _serviceOptions);
                }
                if (map.ResponsiveDevice.CaliforniaProjectId != targetStyleMolecule.CaliforniaProjectId)
                {
                    throw new NotImplementedException("Find equivalent responsive device in target project.");
                }
                targetStyleMolecule.ApplyStyleAtom(clonedAtom, map.ResponsiveDevice, map.StateModifier);
            }
        }

        private async Task<StyleMolecule> ReadStyleMoleculeByStyleAsync(int styleMoleculeId, bool includeCloneOfStyles)
        {
            // --- TODO code duplication
            if (includeCloneOfStyles)
            {
                return await _data.Set<StyleMolecule>()
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.AppliedValues)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.MappedQuantums)
                                .ThenInclude(mapp => mapp.StyleQuantum)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.ResponsiveDevice)
                    // equivalent chain for style cones
                    .Include(s => s.CloneOfStyles)
                        .ThenInclude(s => s.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                                .ThenInclude(ato => ato.AppliedValues)
                    .Include(s => s.CloneOfStyles)
                        .ThenInclude(s => s.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                                .ThenInclude(ato => ato.MappedQuantums)
                                    .ThenInclude(mapp => mapp.StyleQuantum)
                    .Include(s => s.CloneOfStyles)
                        .ThenInclude(s => s.MappedStyleAtoms)
                            .ThenInclude(ma => ma.ResponsiveDevice)
                    .SingleAsync(s => s.StyleMoleculeId == styleMoleculeId); // TODO everywhere: single vs first => does ef core recognise index as unique to prevent unnecessary checks
            }
            else
            {
                return await _data.Set<StyleMolecule>()
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.AppliedValues)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.MappedQuantums)
                                .ThenInclude(mapp => mapp.StyleQuantum)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.ResponsiveDevice)
                    .SingleAsync(s => s.StyleMoleculeId == styleMoleculeId);
            }
            // --- TODO end code duplication
        }

        private async Task<StyleMolecule> ReadStyleMoleculeByLayoutAsync(int layoutBaseId, bool includeCloneOfStyles)
        {
            // --- TODO code duplication
            if (includeCloneOfStyles)
            {
                return await _data.Set<StyleMolecule>()
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.AppliedValues)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.MappedQuantums)
                                .ThenInclude(mapp => mapp.StyleQuantum)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.ResponsiveDevice)
                    // equivalent chain for style cones
                    .Include(s => s.CloneOfStyles)
                        .ThenInclude(s => s.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                                .ThenInclude(ato => ato.AppliedValues)
                    .Include(s => s.CloneOfStyles)
                        .ThenInclude(s => s.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                                .ThenInclude(ato => ato.MappedQuantums)
                                    .ThenInclude(mapp => mapp.StyleQuantum)
                    .Include(s => s.CloneOfStyles)
                        .ThenInclude(s => s.MappedStyleAtoms)
                            .ThenInclude(ma => ma.ResponsiveDevice)
                    .FirstAsync(s => s.StyleForLayoutId == layoutBaseId); // TODO security audit authorization: does style always same accessibility level as layout // TODO document
            }
            else
            {
                return await _data.Set<StyleMolecule>()
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.AppliedValues)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.StyleAtom)
                            .ThenInclude(ato => ato.MappedQuantums)
                                .ThenInclude(mapp => mapp.StyleQuantum)
                    .Include(s => s.MappedStyleAtoms)
                        .ThenInclude(ma => ma.ResponsiveDevice)
                    .FirstAsync(s => s.StyleForLayoutId == layoutBaseId); // TODO security audit authorization: does style always same accessibility level as layout // TODO document
            }
            // --- TODO end code duplication
        }

        private ContentAtom CloneAndRegisterContentAtom(int californiaProjectId, ContentAtom referenceContentAtom)
        {
            if (referenceContentAtom.IsDeleted)
            {
                throw new ApplicationException("Cannot clone deleted atom.");
            }
            ContentAtom clonedContentAtom;
            switch (referenceContentAtom.ContentAtomType)
            {
                case ContentAtomType.Text:
                    clonedContentAtom = new ContentAtom() { CaliforniaProjectId = californiaProjectId, ContentAtomType = referenceContentAtom.ContentAtomType, TextContent = referenceContentAtom.TextContent };
                    _data.Add(clonedContentAtom);
                    break;
                case ContentAtomType.Link:
                    clonedContentAtom = new ContentAtom() { CaliforniaProjectId = californiaProjectId, ContentAtomType = referenceContentAtom.ContentAtomType, Url = referenceContentAtom.Url };
                    _data.Add(clonedContentAtom);
                    break;
                default:
                    throw new NotImplementedException($"Clone not implemented for content atom type {referenceContentAtom.ContentAtomType}."); // TODO
            }
            return clonedContentAtom;
        }

        public async Task<ContentAtom> UpdateContentAtomAsync(CaliforniaContext californiaContext, ContentAtom contentAtom, string updatedStringContent)
        {
            if (contentAtom.ContentAtomType == ContentAtomType.Text)
            {
                contentAtom.TextContent = updatedStringContent;
            }
            else if (contentAtom.ContentAtomType == ContentAtomType.Link)
            {
                // TODO check if url already used in project
                contentAtom.Url = updatedStringContent;
            }
            else
            {
                throw new InvalidOperationException($"Atom type must be {nameof(ContentAtomType.Text)}.");
            }
            
            await _data.SaveChangesAsync();
            return contentAtom;
        }

        public async Task DeleteStyleAtomAsync(CaliforniaContext californiaContext, StyleAtom styleAtom)
        {
            if (!styleAtom.IsDeletable)
            {
                throw new InvalidOperationException("Style atom is not deletable.");
            }
            var styleMolecule = await ReadStyleMoleculeByStyleAsync(styleAtom.MappedToMolecule.StyleMoleculeId, true);
            _data.Remove(styleAtom.MappedToMolecule);
            _data.Remove(styleAtom);

            // soft sync change
            foreach (var clonedStyle in styleMolecule.CloneOfStyles)
            {
                var equivalentAtom = FindEquivalentStyleAtomInMolecule(clonedStyle, styleAtom);
                if (equivalentAtom != null && equivalentAtom.IsDeletable)
                {
                    _data.Remove(equivalentAtom.MappedToMolecule);
                    _data.Remove(equivalentAtom);
                }
            }

            await _data.SaveChangesAsync();
        }

        public async Task<StyleAtom> CreateStyleAtomForMoleculeAsync(StyleMolecule styleMolecule, StyleAtomType targetType, ResponsiveDevice responsiveDevice, string stateModifier, bool saveChanges)
        {
            if (styleMolecule.MappedStyleAtoms.Any(map => map.StyleAtom.StyleAtomType == targetType && map.ResponsiveDeviceId == responsiveDevice.ResponsiveDeviceId && map.StateModifier == stateModifier))
            {
                throw new InvalidOperationException("Style atom already exists.");
            }
            var createdStyleAtom = new StyleAtom() { CaliforniaProjectId = styleMolecule.CaliforniaProjectId, Name = targetType.ToString(), StyleAtomType = targetType };
            _data.Add(createdStyleAtom);
            styleMolecule.ApplyStyleAtomRange(new StyleAtom[] { createdStyleAtom }, responsiveDevice, stateModifier);
            if (saveChanges)
            {
                await _data.SaveChangesAsync();
            }
            return createdStyleAtom;
        }

        public async Task<StyleAtom> SetStyleQuantumToAtomAsync(StyleAtom styleAtom, StyleQuantum styleQuantum)
        {
            var styleMolecule = await ReadStyleMoleculeByStyleAsync(styleAtom.MappedToMolecule.StyleMoleculeId, true);
            var removedExistingQuantumMapping = styleAtom.MappedQuantums.FirstOrDefault(map => map.StyleQuantum.CssProperty == styleQuantum.CssProperty);
            var oldStyleValue = styleAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == styleQuantum.CssProperty)?.CssValue ?? "";
            styleAtom.ApplyStyleQuantum(styleQuantum, _serviceOptions);

            // soft sync sucks, because when you remove quantum from clone to override, it is not clear
            foreach (var clonedStyle in styleMolecule.CloneOfStyles)
            {
                var equivalentAtom = FindEquivalentStyleAtomInMolecule(clonedStyle, styleAtom);
                var equivalentValue = equivalentAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == styleQuantum.CssProperty);
                if (equivalentValue != null)
                {
                    if (equivalentValue.CssValue == oldStyleValue)
                    {
                        var equivalentAppliedQuantumMapping = equivalentAtom.MappedQuantums.FirstOrDefault(map => map.StyleQuantum.CssProperty == styleQuantum.CssProperty);
                        if (equivalentAppliedQuantumMapping == null || (removedExistingQuantumMapping != null && equivalentAppliedQuantumMapping.StyleQuantumId == removedExistingQuantumMapping.StyleQuantumId))
                        {
                            equivalentAtom.ApplyStyleQuantum(styleQuantum, _serviceOptions);
                        }
                    }
                }
            }
            await _data.SaveChangesAsync();
            return styleAtom;
        }

        public async Task<StyleQuantum> CreateStyleQuantumAsync(CaliforniaContext californiaContext, int californiaProjectId, string quantumName, string cssProperty, string cssValue)
        {
            if (cssProperty == PropertyNames.ZIndex) // TODO duplicated code with style update, quantum update and quantum create and interaction
            {
                // TODO document
                if (int.TryParse(cssValue, out var parsedZIndex) && parsedZIndex >= 100)
                {
                    throw new NotImplementedException("Z-Index must be smaller than 100.");
                }
            }
            var styleQuantum = new StyleQuantum()
            {
                CaliforniaProjectId = californiaProjectId,
                CssProperty = cssProperty,
                CssValue = cssValue,
                Name = quantumName
            };
            _data.Add(styleQuantum);
            var changedRows = await _data.SaveChangesAsync();
            if (changedRows != 1)
            {
                throw new ApplicationException("Unexpected changed rows.");
            }
            return styleQuantum;
        }

        public async Task<StyleValue> UpdateStyleValueAsync(StyleValue styleValue, string cssValue)
        {
            if (styleValue.CssProperty == PropertyNames.ZIndex) // TODO duplicated code with style update, quantum update and quantum create and interaction
            {
                // TODO document
                if (int.TryParse(cssValue, out var parsedZIndex) && parsedZIndex >= 100)
                {
                    throw new NotImplementedException("Z-Index must be smaller than 100.");
                }
            }
            // TODO clean css value should include insignificant digit removal
            if (styleValue.CssValue == cssValue)
            {
                return styleValue;
            }
            var styleMolecule = await ReadStyleMoleculeByStyleAsync(styleValue.StyleAtom.MappedToMolecule.StyleMoleculeId, true);
            var originalCssValueForSoftSync = styleValue.CssValue;
            styleValue.CssValue = cssValue;
            var appliedQuantumMapping = styleValue.StyleAtom.MappedQuantums.FirstOrDefault(map => map.StyleQuantum.CssProperty == styleValue.CssProperty);
            if (appliedQuantumMapping != null)
            {
                styleValue.StyleAtom.MappedQuantums.Remove(appliedQuantumMapping);
                _data.Remove(appliedQuantumMapping);
            }
            // TODO this soft sync sucks because one doesnt know after multiple changes if cloned molecule changes are meant to be same or override
            // soft sync changes
            foreach (var clonedStyle in styleMolecule.CloneOfStyles)
            {
                bool updateEquivalentValue = true;
                var equivalentAtom = FindEquivalentStyleAtomInMolecule(clonedStyle, styleValue.StyleAtom);
                if (equivalentAtom != null)
                {
                    var equivalentQuantumMapping = equivalentAtom.MappedQuantums.FirstOrDefault(map => map.StyleQuantum.CssProperty == styleValue.CssProperty);
                    if (equivalentQuantumMapping != null && appliedQuantumMapping != null)
                    {
                        if (equivalentQuantumMapping.StyleQuantumId != appliedQuantumMapping.StyleQuantumId)
                        {
                            updateEquivalentValue = false; // quantum is different => do nothing
                        }
                    }
                    if (updateEquivalentValue)
                    {
                        var equivalentStyleValue = equivalentAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == styleValue.CssProperty);
                        if (equivalentStyleValue != null && equivalentStyleValue.CssValue == originalCssValueForSoftSync)
                        {
                            // same value, update value and remove quantum if was set
                            equivalentStyleValue.CssValue = cssValue;
                            if (equivalentQuantumMapping != null)
                            {
                                equivalentAtom.MappedQuantums.Remove(equivalentQuantumMapping);
                                _data.Remove(equivalentQuantumMapping);
                            }
                        }
                    }
                }
            }

            var changedRows = await _data.SaveChangesAsync();
            return styleValue;
        }

        public async Task<StyleQuantum> UpdateStyleQuantumAsync(StyleQuantum styleQuantum, string cssValue)
        {
            if (styleQuantum.CssProperty == PropertyNames.ZIndex) // TODO duplicated code with style update, quantum update and quantum create and interaction
            {
                // TODO document
                if (int.TryParse(cssValue, out var parsedZIndex) && parsedZIndex >= 100)
                {
                    throw new NotImplementedException("Z-Index must be smaller than 100.");
                }
            }
            if (styleQuantum.CssValue == cssValue)
            {
                return styleQuantum;
            }
            var affectedAtomIds = styleQuantum.MappedToAtoms.Select(map => map.StyleAtomId);
            var affectedAtoms = await _data.Set<StyleAtom>()
                .Include(atom => atom.AppliedValues)
                .Where(atom => affectedAtomIds.Contains(atom.StyleAtomId))
                .ToListAsync();
            styleQuantum.CssValue = cssValue;
            foreach (var styleAtom in affectedAtoms)
            {
                styleAtom.AppliedValues.First(val => val.CssProperty == styleQuantum.CssProperty).CssValue = cssValue;
            }
            var changedRows = await _data.SaveChangesAsync();
            if (changedRows != (1 + affectedAtoms.Count))
            {
                throw new ApplicationException("Unexpected changed rows.");
            }
            return styleQuantum;
        }

        public async Task<StyleValue> CreateStyleValueForAtomAsync(StyleAtom styleAtom, string cssProperty)
        {
            if (string.IsNullOrEmpty(cssProperty))
            {
                throw new ArgumentNullException(nameof(cssProperty));
            }
            if (styleAtom.AppliedValues.Any(v => v.CssProperty == cssProperty))
            {
                throw new InvalidOperationException("Style value already exists.");
            }

            // apply change softly to style molecule children
            var styleMolecule = await ReadStyleMoleculeByStyleAsync(styleAtom.MappedToMolecule.StyleMoleculeId, true);
            foreach (var clonedStyle in styleMolecule.CloneOfStyles)
            {
                var equivalentAtom = FindEquivalentStyleAtomInMolecule(clonedStyle, styleAtom);
                if (equivalentAtom == null)
                {
                    equivalentAtom = await CreateStyleAtomForMoleculeAsync(clonedStyle, styleAtom.StyleAtomType.Value, styleAtom.MappedToMolecule.ResponsiveDevice, styleAtom.MappedToMolecule.StateModifier, false);
                }
                if (!equivalentAtom.AppliedValues.Any(v => v.CssProperty == cssProperty))
                {
                    var equivalentStyleValue = new StyleValue() { CssProperty = cssProperty, CssValue = "", CaliforniaProjectId = styleAtom.CaliforniaProjectId };
                    equivalentAtom.AppliedValues.Add(equivalentStyleValue);
                }
            }

            var styleValue = new StyleValue() { CssProperty = cssProperty, CssValue = "", CaliforniaProjectId = styleAtom.CaliforniaProjectId };
            styleAtom.AppliedValues.Add(styleValue);

            await _data.SaveChangesAsync();
            return styleValue;
        }

        private StyleAtom FindEquivalentStyleAtomInMolecule(StyleMolecule otherStyleMolecule, StyleAtom referenceAtom)
        {
            var referenceMapping = referenceAtom.MappedToMolecule;
            var equivalentAtomMapping = otherStyleMolecule.MappedStyleAtoms.FirstOrDefault(map =>
                map.StyleAtom.StyleAtomType == referenceAtom.StyleAtomType &&
                map.ResponsiveDeviceId == referenceMapping.ResponsiveDeviceId &&
                map.StateModifier == referenceMapping.StateModifier);
            if (equivalentAtomMapping != null)
            {
                return equivalentAtomMapping.StyleAtom;
            }
            else
            {
                return null;
            }
        }

        public async Task DeleteProjectDataAsync(CaliforniaContext californiaContext) // TODO development only, remove for release // can be used as TEST for database model
        {
            var californiaStore = await _data.Set<CaliforniaStore>()
                .Include(s => s.CaliforniaProjects)
                .FirstAsync(s => s.CaliforniaStoreId == californiaContext.UserId);
            var californiaProjectId = californiaStore.CaliforniaProjects.First().CaliforniaProjectId;

            var californiaProject = await _data.Set<CaliforniaProject>()
                /*
                .Include(p => p.StyleAtoms)*/
                .Include(p => p.ContentAtoms)
                    .ThenInclude(co => co.DbContentSafetyLock).IgnoreQueryFilters() // TODO ignore specific filter??
                .Include(p => p.StyleQuantums)
                .Include(p => p.ResponsiveDevices)
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.AllBoxesBelowRow)
                            .ThenInclude(box => box.PlacedInBoxAtoms)
                                .ThenInclude(atom => atom.HostedContentAtom)
                                    .ThenInclude(contentAtom => contentAtom.DbContentSafetyLock)
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.AllBoxesBelowRow)
                            .ThenInclude(box => box.PlacedInBoxBoxes)
                .Include(p => p.CaliforniaViews)
                .Include(p => p.StyleMolecules) // TODO only required for FK cloned by style
                    .ThenInclude(st => st.CloneOfStyles)
                .FirstAsync(p => p.CaliforniaProjectId == californiaProjectId);

            //_data.RemoveRange(californiaProject.StyleMolecules);
            /*_data.RemoveRange(californiaProject.StyleQuantums);
            _data.RemoveRange(californiaProject.StyleAtoms);
            _data.RemoveRange(californiaProject.ResponsiveDevices);
            foreach (var atom in californiaProject.ContentAtoms)
            {
                await DeleteContentAtomAsync(atom.ContentAtomId, false);
            }

            foreach (var view in californiaProject.CaliforniaViews)
            {
                foreach (var row in view.PlacedLayoutRows)
                {
                    foreach (var box in row.InsideRowBoxes)
                    {
                        foreach (var subBox in box.PlacedInBoxBoxes)
                        {
                            _data.RemoveRange(subBox.PlacedInBoxBoxes);
                        }
                        _data.RemoveRange(box.PlacedInBoxBoxes);
                    }
                }
            }*/

            // --- hack/workaround
            /*TODO this worked last time maybe we can remove saveChanges? foreach (var cloneRefStyle in californiaProject.StyleMolecules.Where(s => s.CloneOfStyles.Count > 0))
            {
                foreach (var clone in cloneRefStyle.CloneOfStyles)
                {
                    clone.ClonedFromStyle = null;
                    clone.ClonedFromStyleId = null; // TODO nasty hack
                }
                cloneRefStyle.CloneOfStyles.Clear();
            }
            await _data.SaveChangesAsync();*/
            foreach (var styleMolecule in californiaProject.StyleMolecules)
            {
                _data.Remove(styleMolecule);
            }
            foreach (var contentAtom in californiaProject.ContentAtoms)
            {
                _data.Remove(contentAtom);
                _data.Remove(contentAtom.DbContentSafetyLock);
            }
            // --- hack/workaround

            foreach (var view in californiaProject.CaliforniaViews)
            {
                foreach (var row in view.PlacedLayoutRows)
                {
                    foreach (var box in row.AllBoxesBelowRow)
                    {
                        await RemoveBoxRecursiveTODO(box);
                    }
                    _data.RemoveRange(row.AllBoxesBelowRow);
                }
                _data.RemoveRange(view.PlacedLayoutRows);
            }

            _data.RemoveRange(californiaProject.StyleQuantums);
            _data.RemoveRange(californiaProject.ResponsiveDevices);
            _data.Remove(californiaStore);

            // TODO test delete project + cascades as alternative
            await _data.SaveChangesAsync();
        }

        private async Task RemoveBoxRecursiveTODO(LayoutBox box)
        {
            box.PlacedBoxInBox = null;
            box.PlacedBoxInBoxId = 0;
            _data.RemoveRange(box.PlacedInBoxAtoms.Select(layoutAtom => layoutAtom.HostedContentAtom));
            _data.RemoveRange(box.PlacedInBoxAtoms.Select(layoutAtom => layoutAtom.HostedContentAtom.DbContentSafetyLock));
            foreach (var atom in box.PlacedInBoxAtoms)
            {
                //await DeleteContentAtomAsync(atom.HostedContentAtom.ContentAtomId, false);
            }
            _data.RemoveRange(box.PlacedInBoxAtoms);
            foreach (var subBox in box.PlacedInBoxBoxes)
            {
                await RemoveBoxRecursiveTODO(subBox);
            }
            _data.RemoveRange(box.PlacedInBoxBoxes);
        }

        public async Task<CaliforniaProject> ReadCaliforniaProjectForClientAsync(CaliforniaContext californiaContext, int californiaProjectId, bool fullProjectReferences)
        {
            if (fullProjectReferences == true)
            {
                return await _data.Set<CaliforniaProject>() 
                    // --- layout molecules ---
                    .Include(p => p.CaliforniaViews)
                        .ThenInclude(vi => vi.PlacedLayoutRows)
                    .Include(p => p.CaliforniaViews)
                        .ThenInclude(vi => vi.PlacedLayoutRows)
                            .ThenInclude(row => row.AllBoxesBelowRow)
                                .ThenInclude(box => box.PlacedInBoxAtoms)
                                    .ThenInclude(atom => atom.HostedContentAtom)
                    .Include(p => p.CaliforniaViews)
                        .ThenInclude(vi => vi.PlacedLayoutRows)
                            .ThenInclude(row => row.AllBoxesBelowRow)
                                .ThenInclude(box => box.PlacedInBoxAtoms)
                                    .ThenInclude(atom => atom.LayoutStyleInteractions) // TODO is this really necessary?
                    .Include(p => p.CaliforniaViews)
                        .ThenInclude(vi => vi.PlacedLayoutRows)
                            .ThenInclude(row => row.AllBoxesBelowRow)
                                .ThenInclude(box => box.PlacedInBoxBoxes)
                    .Include(p => p.ContentAtoms)
                    .Include(p => p.LayoutMolecules)
                    .Include(p => p.LayoutStyleInteractions)
                        .ThenInclude(la => la.StyleValueInteractions)
                    .Include(p => p.PictureContents)
                    .Include(p => p.ResponsiveDevices)
                    .Include(p => p.SharedProjectInfos)
                    .Include(p => p.StyleQuantums)
                    .Include(p => p.StyleAtoms)
                        .ThenInclude(at => at.MappedQuantums)
                    .Include(p => p.StyleAtoms)
                        .ThenInclude(at => at.AppliedValues)
                    .Include(p => p.StyleAtoms)
                        .ThenInclude(at => at.MappedToMolecule)
                    .Include(p => p.StyleMolecules)
                        .ThenInclude(mo => mo.MappedStyleAtoms)
                    .Include(p => p.StyleMolecules)
                        .ThenInclude(st => st.StyleForLayout)
                    .Include(p => p.StyleQuantums)
                        .ThenInclude(qu => qu.MappedToAtoms)
                    .Include(p => p.StyleValues)
                        .ThenInclude(p => p.AppliedStyleValueInteractions)
                    .AsNoTracking()
                    .FirstAsync(project => project.CaliforniaProjectId == californiaProjectId);
            }
            else
            {
                return await _data.Set<CaliforniaProject>() // duplicate ThenIncludes are restored client side // TODO sort created layouts/styles in reverse order
                    // --- layout molecules ---
                    .Include(p => p.CaliforniaViews)
                    /* TODO unused => restored client side
                        .ThenInclude(vi => vi.PlacedLayoutRows)
                    .Include(p => p.CaliforniaViews)
                        .ThenInclude(vi => vi.PlacedLayoutRows) // TODO results in duplicate objects via different query paths in client app
                            .ThenInclude(row => row.AllBoxesBelowRow) // TODO results in duplicate objects via different query paths in client app
                                .ThenInclude(box => box.PlacedInBoxAtoms) // TODO results in duplicate objects via different query paths in client app
                                    .ThenInclude(atom => atom.HostedContentAtom) // TODO results in duplicate objects via different query paths in client app
                    .Include(p => p.CaliforniaViews)
                        .ThenInclude(vi => vi.PlacedLayoutRows) // TODO results in duplicate objects via different query paths in client app
                            .ThenInclude(row => row.AllBoxesBelowRow) // TODO results in duplicate objects via different query paths in client app
                                .ThenInclude(box => box.PlacedInBoxBoxes)*/ // TODO results in duplicate objects via different query paths in client app
                    // ---
                    .Include(p => p.ContentAtoms)
                    .Include(p => p.LayoutMolecules)
                    .Include(p => p.LayoutStyleInteractions)
                        .ThenInclude(la => la.StyleValueInteractions)
                    .Include(p => p.PictureContents)
                    .Include(p => p.ResponsiveDevices)
                    .Include(p => p.SharedProjectInfos)
                    .Include(p => p.StyleQuantums)
                    .Include(p => p.StyleAtoms)
                        .ThenInclude(at => at.MappedQuantums)
                    .Include(p => p.StyleAtoms)
                        .ThenInclude(at => at.AppliedValues) // TODO unused => restored client side
                    .Include(p => p.StyleAtoms)
                        .ThenInclude(at => at.MappedToMolecule)
                    .Include(p => p.StyleMolecules)
                        .ThenInclude(mo => mo.MappedStyleAtoms)
                    .Include(p => p.StyleQuantums)
                        .ThenInclude(qu => qu.MappedToAtoms)
                    .Include(p => p.StyleValues)
                    .AsNoTracking()
                    .FirstAsync(project => project.CaliforniaProjectId == californiaProjectId);
            }
        }

        public async Task DeleteProjectData2Async(CaliforniaContext californiaContext) // TODO development only, remove for release // can be used as TEST for database model
        {
            var californiaStore = await _data.Set<CaliforniaStore>()
                .Include(s => s.CaliforniaProjects)
                .FirstAsync(s => s.CaliforniaStoreId == californiaContext.UserId);
            var californiaProjectId = californiaStore.CaliforniaProjects.First().CaliforniaProjectId;

            var californiaProject = await _data.Set<CaliforniaProject>()
                /*.Include(p => p.StyleQuantums)
                .Include(p => p.StyleAtoms)
                .Include(p => p.ResponsiveDevices)
                .Include(p => p.ContentAtoms)*/
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.StyleMolecule)
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.AllBoxesBelowRow)
                            .ThenInclude(box => box.PlacedInBoxAtoms)
                                .ThenInclude(atom => atom.StyleMolecule)
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.AllBoxesBelowRow)
                            .ThenInclude(box => box.PlacedInBoxBoxes)
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.AllBoxesBelowRow)
                            .ThenInclude(box => box.PlacedBoxInBox)
                .Include(p => p.CaliforniaViews)
                    .ThenInclude(view => view.PlacedLayoutRows)
                        .ThenInclude(row => row.AllBoxesBelowRow)
                            .ThenInclude(box => box.StyleMolecule)
                .Include(p => p.CaliforniaViews)
                .FirstAsync(p => p.CaliforniaProjectId == californiaProjectId);

            //_data.RemoveRange(californiaProject.StyleMolecules);
            /*_data.RemoveRange(californiaProject.StyleQuantums);
            _data.RemoveRange(californiaProject.StyleAtoms);
            _data.RemoveRange(californiaProject.ResponsiveDevices);
            foreach (var atom in californiaProject.ContentAtoms)
            {
                await DeleteContentAtomAsync(atom.ContentAtomId, false);
            }

            foreach (var view in californiaProject.CaliforniaViews)
            {
                foreach (var row in view.PlacedLayoutRows)
                {
                    foreach (var box in row.InsideRowBoxes)
                    {
                        foreach (var subBox in box.PlacedInBoxBoxes)
                        {
                            _data.RemoveRange(subBox.PlacedInBoxBoxes);
                        }
                        _data.RemoveRange(box.PlacedInBoxBoxes);
                    }
                }
            }*/

            foreach (var view in californiaProject.CaliforniaViews)
            {
                foreach (var row in view.PlacedLayoutRows)
                {
                    _data.Remove(row.StyleMolecule);
                    foreach (var box in row.AllBoxesBelowRow)
                    {
                        _data.Remove(box.StyleMolecule);
                        foreach (var atom in box.PlacedInBoxAtoms)
                        {
                            _data.Remove(atom.StyleMolecule);
                        }
                        foreach (var subBox in box.PlacedInBoxBoxes)
                        {
                            _data.Remove(subBox);
                        }
                    }
                }
            }

            _data.RemoveRange(californiaProject.CaliforniaViews);

            // TODO test delete project + cascades as alternative
            await _data.SaveChangesAsync();
        }

        public async Task DeleteStyleQuantumAsync(CaliforniaContext californiaContext, int styleQuantumId)
        {
            var styleQuantum = await _data.Set<StyleQuantum>()
                .Include(q => q.MappedToAtoms) // TODO test are atoms removed? if not same mistake with styleValueInteractionMapping
                .FirstAsync(q => q.StyleQuantumId == styleQuantumId);

            if (!styleQuantum.IsDeletable)
            {
                throw new InvalidOperationException("Style quantum is not deletable.");
            }

            _data.Remove(styleQuantum);

            await _data.SaveChangesAsync();
        }

        public async Task DeleteStyleValueAsync(StyleValue styleValue)
        {
            var styleMolecule = await ReadStyleMoleculeByStyleAsync(styleValue.StyleAtom.MappedToMolecule.StyleMoleculeId, true);

            // TODO interaction targets are currently removed automatically... maybe need to block deletion
            var quantumForTargetValueMapping = styleValue.StyleAtom.MappedQuantums.FirstOrDefault(map => map.StyleQuantum.CssProperty == styleValue.CssProperty);
            if (quantumForTargetValueMapping != null)
            {
                _data.Remove(quantumForTargetValueMapping);
            }
            _data.Remove(styleValue);

            // TODO should only soft change when values/quantums were automatically set (from clone reference)
            // soft change cloned style molecules: delete value&quantum only if css value/quantum is the same
            foreach (var clonedStyle in styleMolecule.CloneOfStyles)
            {
                bool deleteEquivalentValue = true;
                var equivalentAtom = FindEquivalentStyleAtomInMolecule(clonedStyle, styleValue.StyleAtom);
                if (equivalentAtom != null)
                {
                    var equivalentQuantumMapping = equivalentAtom.MappedQuantums.FirstOrDefault(map => map.StyleQuantum.CssProperty == styleValue.CssProperty);
                    if (equivalentQuantumMapping != null)
                    {
                        if (quantumForTargetValueMapping == null || equivalentQuantumMapping.StyleQuantumId != quantumForTargetValueMapping.StyleQuantumId)
                        {
                            deleteEquivalentValue = false; // quantum set although deleting non-quantum value OR quantum is different => do nothing
                        }
                    }
                    if (deleteEquivalentValue)
                    {
                        var equivalentStyleValue = equivalentAtom.AppliedValues.FirstOrDefault(v => v.CssProperty == styleValue.CssProperty);
                        if (equivalentStyleValue != null && equivalentStyleValue.CssValue == styleValue.CssValue)
                        {
                            // same value, delete value and quantum if exists
                            _data.Remove(equivalentStyleValue);
                            if (equivalentQuantumMapping != null)
                            {
                                _data.Remove(equivalentQuantumMapping);
                            }
                        }
                    }
                }
            }

            await _data.SaveChangesAsync();
        }

        public async Task<StyleQuantum> CloneAndRegisterStyleQuantumAsync(CaliforniaContext californiaContext, int targetProjectId, int styleQuantumId)
        {
            var styleQuantum = await _data.Set<StyleQuantum>()
                .AsNoTracking()
                .FirstAsync(q => q.StyleQuantumId == styleQuantumId);

            _data.Add(styleQuantum);
            styleQuantum.StyleQuantumId = 0;
            styleQuantum.Name = styleQuantum.Name + " (Copy)"; // TODO how to make sure name is unique and update in that case 1) "where name=xxx" query locks rows, that where not created at start of transaction 2) full table lock...
            var affectedRows = await _data.SaveChangesAsync();
            if (affectedRows != 1)
            {
                throw new ApplicationException("Unexpected number of affected rows."); // TODO check rollback when exception after saveChanges() happens
            }
            return styleQuantum;
        }

        public async Task<CaliforniaProject> AuthorizeCaliforniaProjectAsync(ClaimsPrincipal user, int californiaProjectId, OperationAuthorizationRequirement requirement) // TODO same function with californiaProject instead of californiaProjectId when already tracked
        {
            CaliforniaProject californiaProject;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                californiaProject = await _data
                    .Set<CaliforniaProject>()
                    .AsNoTracking()
                    .Include(t => t.SharedProjectInfos)
                    .FirstAsync(t => t.CaliforniaProjectId == californiaProjectId);
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                californiaProject = await _data
                    .Set<CaliforniaProject>()
                    .Include(t => t.SharedProjectInfos)
                    .FirstAsync(t => t.CaliforniaProjectId == californiaProjectId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                californiaProject = await _data
                    .Set<CaliforniaProject>()
                    .Include(t => t.SharedProjectInfos)
                    .Include(t => t.CaliforniaStore)
                    .FirstAsync(t => t.CaliforniaProjectId == californiaProjectId);
            }
            else
            {
                throw new NotImplementedException($"Missing query for california project requirement {requirement.Name}.");
            }
            var authResult = await _authorizationService.AuthorizeAsync(user, californiaProject, requirement);
            if (!authResult.Succeeded)
            {
                throw new UnauthorizedAccessException();
            }
            return californiaProject;
        }

        public async Task<ResponsiveDevice> AuthorizeResponsiveDeviceAsync(ClaimsPrincipal user, int responsiveDeviceId, OperationAuthorizationRequirement requirement)
        {
            ResponsiveDevice responsiveDevice;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                responsiveDevice = await _data.Set<ResponsiveDevice>()
                        .FirstAsync(r => r.ResponsiveDeviceId == responsiveDeviceId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, responsiveDevice.CaliforniaProjectId, requirement);
            return responsiveDevice;
        }

        public async Task<StyleMolecule> AuthorizeStyleMoleculeAsync(ClaimsPrincipal user, int styleMoleculeId, OperationAuthorizationRequirement requirement, bool isLoadCloneStructures)
        {
            // TODO optimization isLoadStyleSetup
            StyleMolecule styleMolecule;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                if (isLoadCloneStructures)
                {
                    styleMolecule = await _data.Set<StyleMolecule>()
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.ResponsiveDevice)
                        .Include(m => m.CloneOfStyles)
                        .AsNoTracking()
                        .FirstAsync(m => m.StyleMoleculeId == styleMoleculeId);
                }
                else
                {
                    styleMolecule = await _data.Set<StyleMolecule>()
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.ResponsiveDevice)
                        .AsNoTracking()
                        .FirstAsync(m => m.StyleMoleculeId == styleMoleculeId);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                if (isLoadCloneStructures)
                {
                    styleMolecule = await _data.Set<StyleMolecule>()
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.ResponsiveDevice)
                        .Include(m => m.CloneOfStyles)
                        .FirstAsync(m => m.StyleMoleculeId == styleMoleculeId);
                }
                else
                {
                    styleMolecule = await _data.Set<StyleMolecule>()
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.StyleAtom)
                        .Include(m => m.MappedStyleAtoms)
                            .ThenInclude(ma => ma.ResponsiveDevice)
                        .FirstAsync(m => m.StyleMoleculeId == styleMoleculeId);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, styleMolecule.CaliforniaProjectId, requirement);
            return styleMolecule;
        }

        public async Task<StyleAtom> AuthorizeStyleAtomAsync(ClaimsPrincipal user, int styleAtomId, OperationAuthorizationRequirement requirement)
        {
            StyleAtom styleAtom;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                styleAtom = await _data.Set<StyleAtom>()
                        .Include(a => a.AppliedValues)
                        .Include(a => a.MappedQuantums)
                            .ThenInclude(ma => ma.StyleQuantum)
                        .Include(a => a.MappedToMolecule)
                        .FirstAsync(a => a.StyleAtomId == styleAtomId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, styleAtom.CaliforniaProjectId, requirement);
            return styleAtom;
        }

        public async Task<StyleValue> AuthorizeStyleValueAsync(ClaimsPrincipal user, int styleValueId, OperationAuthorizationRequirement requirement)
        {
            StyleValue styleValue;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                styleValue = await _data.Set<StyleValue>()
                        .Include(v => v.StyleAtom)
                            .ThenInclude(at => at.MappedQuantums)
                                .ThenInclude(map => map.StyleQuantum)
                        .Include(v => v.StyleAtom)
                            .ThenInclude(at => at.MappedToMolecule)
                        .FirstAsync(v => v.StyleValueId == styleValueId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, styleValue.CaliforniaProjectId, requirement);
            return styleValue;
        }

        public async Task<ContentAtom> AuthorizeContentAtomAsync(ClaimsPrincipal user, int contentAtomId, OperationAuthorizationRequirement requirement)
        {
            ContentAtom contentAtom;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                contentAtom = await _data.Set<ContentAtom>()
                        .FirstAsync(v => v.ContentAtomId == contentAtomId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            if (contentAtom.IsDeleted)
            {
                throw new ApplicationException("Deleted atom was not filtered.");
            }
            await AuthorizeCaliforniaProjectAsync(user, contentAtom.CaliforniaProjectId, requirement);
            return contentAtom;
        }

        public async Task<LayoutStyleInteraction> AuthorizeLayoutStyleInteractionAsync(ClaimsPrincipal user, int layoutStyleInteractionId, OperationAuthorizationRequirement requirement)
        {
            LayoutStyleInteraction layoutStyleInteraction;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                layoutStyleInteraction = await _data.Set<LayoutStyleInteraction>()
                        .Include(l => l.StyleValueInteractions) // TODO required for delete?
                        .FirstAsync(l => l.LayoutStyleInteractionId == layoutStyleInteractionId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, layoutStyleInteraction.CaliforniaProjectId, requirement);
            return layoutStyleInteraction;
        }

        public async Task<LayoutAtom> AuthorizeLayoutAtomAsync(ClaimsPrincipal user, int layoutAtomId, OperationAuthorizationRequirement requirement)
        {
            LayoutAtom layoutAtom;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                layoutAtom = await _data.Set<LayoutAtom>()
                        .Include(l => l.HostedContentAtom)
                        .Include(l => l.StyleMolecule)
                        .AsNoTracking()
                        .FirstAsync(l => l.LayoutBaseId == layoutAtomId);
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                layoutAtom = await _data.Set<LayoutAtom>()
                    .Include(l => l.LayoutStyleInteractions) // TODO optimize loads
                    .Include(l => l.PlacedAtomInBox)
                        .ThenInclude(bo => bo.BoxOwnerRow)
                            .ThenInclude(row => row.AllBoxesBelowRow)
                    .FirstAsync(l => l.LayoutBaseId == layoutAtomId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, layoutAtom.CaliforniaProjectId, requirement);
            return layoutAtom;
        }

        public async Task<CaliforniaView> AuthorizeCaliforniaViewAsync(ClaimsPrincipal user, int californiaViewId, OperationAuthorizationRequirement requirement)
        {
            CaliforniaView californiaView;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                californiaView = await _data.Set<CaliforniaView>()
                        .Include(v => v.PlacedLayoutRows)
                            .ThenInclude(r => r.AllBoxesBelowRow) // TODO duplicate tree from authorize layout row
                                .ThenInclude(bo => bo.PlacedInBoxAtoms)
                                    .ThenInclude(ato => ato.HostedContentAtom)
                        .Include(v => v.PlacedLayoutRows)
                            .ThenInclude(r => r.AllBoxesBelowRow)
                                .ThenInclude(bo => bo.PlacedInBoxAtoms)
                                    .ThenInclude(ato => ato.StyleMolecule)
                        .Include(v => v.PlacedLayoutRows)
                            .ThenInclude(r => r.AllBoxesBelowRow)
                                .ThenInclude(bo => bo.PlacedInBoxBoxes)
                                    .ThenInclude(sub => sub.StyleMolecule)
                        .Include(v => v.PlacedLayoutRows)
                            .ThenInclude(r => r.AllBoxesBelowRow)
                                .ThenInclude(bo => bo.StyleMolecule)
                        .Include(v => v.PlacedLayoutRows)
                            .ThenInclude(r => r.AllBoxesBelowRow)
                                .ThenInclude(bo => bo.PlacedInBoxBoxes)
                        .Include(v => v.PlacedLayoutRows)
                            .ThenInclude(r => r.StyleMolecule)
                        .AsNoTracking()
                        .FirstAsync(v => v.CaliforniaViewId == californiaViewId);
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                californiaView = await _data.Set<CaliforniaView>()
                        .FirstAsync(v => v.CaliforniaViewId == californiaViewId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, californiaView.CaliforniaProjectId, requirement);
            return californiaView;
        }

        public async Task<LayoutRow> AuthorizeLayoutRowAsync(ClaimsPrincipal user, int layoutRowId, OperationAuthorizationRequirement requirement, bool isLoadBoxesRecursive)
        {
            LayoutRow layoutRow;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                if (isLoadBoxesRecursive)
                {
                    layoutRow = await _data.Set<LayoutRow>()
                        .Include(r => r.AllBoxesBelowRow)  // TODO duplicate tree in authorize california view
                            .ThenInclude(bo => bo.PlacedInBoxAtoms)
                                .ThenInclude(ato => ato.HostedContentAtom)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxAtoms)
                                .ThenInclude(ato => ato.StyleMolecule)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxBoxes)
                                .ThenInclude(sub => sub.StyleMolecule)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.StyleMolecule)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxBoxes)
                        .Include(r => r.StyleMolecule)
                        .AsNoTracking()
                        .FirstAsync(r => r.LayoutBaseId == layoutRowId);
                }
                else
                {
                    layoutRow = await _data.Set<LayoutRow>()
                        .Include(r => r.StyleMolecule)
                        .AsNoTracking()
                        .FirstAsync(r => r.LayoutBaseId == layoutRowId);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                if (isLoadBoxesRecursive)
                {
                    layoutRow = await _data.Set<LayoutRow>()
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxAtoms)
                                .ThenInclude(ato => ato.HostedContentAtom)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxAtoms)
                                .ThenInclude(ato => ato.StyleMolecule)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxBoxes)
                                .ThenInclude(sub => sub.StyleMolecule)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.StyleMolecule)
                        .Include(r => r.AllBoxesBelowRow)
                            .ThenInclude(bo => bo.PlacedInBoxBoxes)
                        .Include(r => r.StyleMolecule)
                        .FirstAsync(r => r.LayoutBaseId == layoutRowId);
                }
                else
                {
                    layoutRow = await _data.Set<LayoutRow>()
                        .Include(r => r.StyleMolecule)
                        .FirstAsync(r => r.LayoutBaseId == layoutRowId);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, layoutRow.CaliforniaProjectId, requirement);
            return layoutRow;
        }

        public async Task<LayoutBase> AuthorizeLayoutBaseAsync(ClaimsPrincipal user, int layoutBaseId, OperationAuthorizationRequirement requirement)
        {
            LayoutBase layoutBase;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                layoutBase = await _data.Set<LayoutBase>()
                        .AsNoTracking()
                        .FirstAsync(l => l.LayoutBaseId == layoutBaseId);
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                layoutBase = await _data.Set<LayoutBase>()
                        .FirstAsync(l => l.LayoutBaseId == layoutBaseId);
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }
            await AuthorizeCaliforniaProjectAsync(user, layoutBase.CaliforniaProjectId, requirement);
            return layoutBase;
        }

        public async Task<LayoutBox> AuthorizeLayoutBoxAsync(ClaimsPrincipal user, int layoutBoxId, OperationAuthorizationRequirement requirement, bool isLoadRecursive)
        {
            LayoutBox layoutBox;
            if (isLoadRecursive)
            {
                if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
                {
                    layoutBox = await _data.Set<LayoutBox>()
                        .Include(b => b.StyleMolecule)
                        .Include(b => b.PlacedInBoxBoxes)
                        .Include(b => b.PlacedInBoxAtoms)
                            .ThenInclude(at => at.HostedContentAtom)
                        .Include(b => b.PlacedInBoxAtoms)
                            .ThenInclude(at => at.StyleMolecule)
                        .FirstAsync(b => b.LayoutBaseId == layoutBoxId);
                    // TODO when no tracking, object reference is not updated with subsequently loaded data
                    await ReadLayoutBoxRecursiveAsync(layoutBox);
                }
                else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
                {
                    layoutBox = await _data.Set<LayoutBox>()
                        .Include(b => b.StyleMolecule)
                        .Include(b => b.PlacedInBoxBoxes)
                        .Include(b => b.PlacedInBoxAtoms)
                            .ThenInclude(at => at.HostedContentAtom)
                        .Include(b => b.PlacedInBoxAtoms)
                            .ThenInclude(at => at.StyleMolecule)
                        .FirstAsync(b => b.LayoutBaseId == layoutBoxId);
                    await ReadLayoutBoxRecursiveAsync(layoutBox);
                }
                else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
                {
                    throw new InvalidOperationException("Invalid requirement for resource type.");
                }
                else
                {
                    throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
                }
            }
            else
            {
                if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
                {
                    layoutBox = await _data.Set<LayoutBox>()
                            .Include(b => b.StyleMolecule)
                            .AsNoTracking()
                            .FirstAsync(b => b.LayoutBaseId == layoutBoxId);
                }
                else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
                {
                    layoutBox = await _data.Set<LayoutBox>()
                            .Include(b => b.StyleMolecule)
                            .FirstAsync(b => b.LayoutBaseId == layoutBoxId);
                }
                else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
                {
                    throw new InvalidOperationException("Invalid requirement for resource type.");
                }
                else
                {
                    throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
                }
            }
            await AuthorizeCaliforniaProjectAsync(user, layoutBox.CaliforniaProjectId, requirement);
            return layoutBox;
        }

        private async Task ReadLayoutBoxRecursiveAsync(LayoutBox layoutBox) // TODO naming scheme load?
        {
            // TODO alternatively get all box numbers and load with one where-query?
            foreach (var subBox in layoutBox.PlacedInBoxBoxes)
            {
                var data = await _data.Set<LayoutBox>()
                            .Include(b => b.StyleMolecule)
                            .Include(b => b.PlacedInBoxBoxes)
                            .Include(b => b.PlacedInBoxAtoms)
                                .ThenInclude(at => at.HostedContentAtom)
                            .Include(b => b.PlacedInBoxAtoms)
                                .ThenInclude(at => at.StyleMolecule)
                            .Where(b => b.LayoutBaseId == subBox.LayoutBaseId)
                            .ToListAsync();
                await ReadLayoutBoxRecursiveAsync(subBox);
            }
        }

        public async Task<StyleQuantum> AuthorizeStyleQuantumAsync(ClaimsPrincipal user, int styleQuantumId, bool loadMappings, OperationAuthorizationRequirement requirement)
        {
            StyleQuantum styleQuantum;
            if (requirement.Name == CaliforniaAuthorization.ReadRequirement.Name)
            {
                if (loadMappings)
                {
                    styleQuantum = await _data.Set<StyleQuantum>()
                        .Include(quantum => quantum.MappedToAtoms)
                        .AsNoTracking()
                        .FirstAsync(quantum => quantum.StyleQuantumId == styleQuantumId);
                }
                else
                {
                    styleQuantum = await _data.Set<StyleQuantum>()
                        .AsNoTracking()
                        .FirstAsync(quantum => quantum.StyleQuantumId == styleQuantumId);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.EditRequirement.Name)
            {
                if (loadMappings)
                {
                    styleQuantum = await _data.Set<StyleQuantum>()
                        .Include(quantum => quantum.MappedToAtoms)
                        .FirstAsync(quantum => quantum.StyleQuantumId == styleQuantumId);
                }
                else
                {
                    styleQuantum = await _data.Set<StyleQuantum>()
                        .FirstAsync(quantum => quantum.StyleQuantumId == styleQuantumId);
                }
            }
            else if (requirement.Name == CaliforniaAuthorization.ShareRequirement.Name)
            {
                throw new InvalidOperationException("Invalid requirement for resource type.");
            }
            else
            {
                throw new NotImplementedException($"Missing tracking hint for style quantum requirement {requirement.Name}.");
            }             
            await AuthorizeCaliforniaProjectAsync(user, styleQuantum.CaliforniaProjectId, requirement);
            return styleQuantum;
        }
    }
}
