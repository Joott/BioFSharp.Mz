﻿namespace BioFSharp.Mz

type IPeak =    
    abstract member Mz        : float
    abstract member Intensity : float   


[<Struct>]
type Peak(mz:float,intensity:float) = 
        member this.Mz = mz
        member this.Intensity = intensity
        
        interface IPeak with
            member this.Mz = mz
            member this.Intensity = intensity

type PeakFamily<'a> = {
    MainPeak       : 'a
    DependentPeaks : 'a list   
}

module Ions = 
    open System.Linq

    [<System.Flags>]
    type IonTypeFlag =
        | Precursor     =  2
        | A             =  4
        | B             =  8
        | C             =  16
        | X             =  32
        | Y             =  64
        | Z             =  128
        | lossH2O       =  256
        | lossNH3       =  512
        | Immonium      = 1024
        | Neutral       = 2018
        | Diagnostic    = 4036
        | Unknown       = 8072
      
    let createIonTypeList (ionTypeFlag: IonTypeFlag) =
        System.Enum.GetValues(ionTypeFlag.GetType()).Cast<IonTypeFlag>().Where(ionTypeFlag.HasFlag)


module Peaks =
 
    ///
    let createPeak mzData intensityData = 
        let pk = Peak(mzData, intensityData)
        pk
    

    ///
    let createPeakFamily mainPeak dependentPeaks = {
        MainPeak       = mainPeak
        DependentPeaks = dependentPeaks  
        }

module TaggedMass = 
    
    [<Struct>]
    type TaggedMass (iontype:Ions.IonTypeFlag,mass:float) = 
            member this.Iontype = iontype
            member this.Mass = mass
            
    ///
    let createTaggedMass iontype mass = 
        let tM = TaggedMass(iontype,mass)
        tM   
    
    ///
    let createTaggedH2OLoss iontype mass =
        let tM = TaggedMass(iontype+Ions.IonTypeFlag.lossH2O,mass)
        tM   

    ///
    let createTaggedNH3Loss iontype mass =
        let tM = TaggedMass(iontype+Ions.IonTypeFlag.lossNH3,mass)
        tM   

    
module TaggedPeak =
    
    [<Struct>]
    type TaggedPeak (iontype:Ions.IonTypeFlag,mz:float,intensity:float) = 
            member this.Iontype = iontype
            member this.Mz = mz
            member this.Intensity = intensity
            interface IPeak with
                member this.Mz = mz
                member this.Intensity = intensity
            
    ///
    let createTaggedPeak iontype mzData intensityData = 
        let tPk = TaggedPeak(iontype,mzData,intensityData)
        tPk    
    
    /// 
    let createTaggedPeakOf (taggedMass: TaggedMass.TaggedMass) (intensityPredF: Ions.IonTypeFlag -> float) =
        createTaggedPeak taggedMass.Iontype taggedMass.Mass (intensityPredF taggedMass.Iontype)
