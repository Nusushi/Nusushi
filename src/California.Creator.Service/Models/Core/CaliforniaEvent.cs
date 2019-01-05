﻿using System;
using System.Collections.Generic;
using System.Text;

namespace California.Creator.Service.Models.Core
{
    public enum CaliforniaEvent // TODO sort, close caps and lock
    {
        ReadInitialClientData = 0,
        CreateStore = 1,
        ReadStore = 2,
        CreateStyleQuantum = 3,
        DuplicateStyleQuantum = 4,
        DeleteStyleQuantum = 5,
        DeleteStyleValue = 6,
        CreateStyleValueForAtom = 7,
        UpdateStyleValue = 8,
        UpdateStyleQuantum = 9,
        ApplyStyleQuantumToAtom = 10,
        CreateStyleAtomForMolecule = 11,
        DeleteStyleAtom = 12,
        UpdateContentAtom = 13,
        CreateLayoutAtomForBox = 14,
        CreateLayoutRowForView = 15,
        DeleteLayout = 16,
        SetBoxCount = 17,
        CreateLayoutBoxForBoxOrRow = 18,
        MoveStyleAtomToResponsiveDevice = 19,
        SetStyleMoleculeReference = 20,
        SetStyleMoleculeAsReference = 21,
        SetLayoutMoleculeAsInstanceable = 22,
        SyncStyleMoleculeToReference = 23,
        SyncStyleMoleculeFromReference = 24,
        MoveLayoutMoleculeIntoLayoutMolecule = 25,
        MoveLayoutMoleculeNextToLayoutMolecule = 26,
        Publish = 27,
        View = 28,
        CreateCaliforniaView = 29,
        DeleteCaliforniaView = 30,
        CreateLayoutStyleInteraction = 31,
        CreateStyleValueInteraction = 32,
        DeleteLayoutStyleInteraction = 33,
        CreateLayoutBoxForAtomInPlace = 34,
        SyncLayoutStylesImitatingReference = 35,
        CreateCaliforniaViewFromReferenceView = 36,
        DeleteStyleValueInteraction = 37,
        SetSpecialLayoutBoxType = 38,
        ViewJs = 39,
        ViewCss = 40,
        UpdateUserDefinedCssForProject = 41,
        UpdateUserDefinedCssForView = 42
    }
}
