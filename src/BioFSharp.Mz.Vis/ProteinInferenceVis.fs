namespace BioFSharp.Mz.Vis

open FSharp.Plotly
open BioFSharp.Mz.ProteinInference

module ProteinInference =

    let qValueHitsVisualization bandwidth inferredProteinClassItemScored path (groupFiles: bool) =
        let decoy, target = inferredProteinClassItemScored |> Array.partition (fun x -> x.DecoyBigger)
        // Histogram with relative abundance
        let freqTarget = 
            FSharp.Stats.Distributions.Frequency.create bandwidth (target |> Array.map (fun x -> x.TargetScore))
            |> Map.toArray
            |> Array.map (fun x -> fst x, (float (snd x)) / (float target.Length))
        let freqDecoy  = FSharp.Stats.Distributions.Frequency.create bandwidth (decoy |> Array.map (fun x -> x.DecoyScore))
                            |> Map.toArray
                            |> Array.map (fun x -> fst x, (float (snd x)) / (float target.Length))
        // Histogram with absolute values
        let freqTarget1 = FSharp.Stats.Distributions.Frequency.create bandwidth (target |> Array.map (fun x -> x.TargetScore))
                            |> Map.toArray
        let freqDecoy1  = FSharp.Stats.Distributions.Frequency.create bandwidth (decoy |> Array.map (fun x -> x.DecoyScore))
                            |> Map.toArray
        let histogram =
            [
                Chart.Column freqTarget |> Chart.withTraceName "Target"
                    |> Chart.withAxisAnchor(Y=1);
                Chart.Column freqDecoy |> Chart.withTraceName "Decoy"
                    |> Chart.withAxisAnchor(Y=1);
                Chart.Column freqTarget1
                    |> Chart.withAxisAnchor(Y=2)
                    |> Chart.withMarkerStyle (Opacity = 0.)
                    |> Chart.withTraceName (Showlegend = false);
                Chart.Column freqDecoy1
                    |> Chart.withAxisAnchor(Y=2)
                    |> Chart.withMarkerStyle (Opacity = 0.)
                    |> Chart.withTraceName (Showlegend = false)
            ]
            |> Chart.Combine

        let sortedQValues =
            inferredProteinClassItemScored
            |> Array.map
                (fun x -> if x.Decoy then
                            x.DecoyScore, x.QValue
                            else
                            x.TargetScore, x.QValue
                )
            |> Array.sortBy (fun (score, qVal) -> score)

        [
            Chart.Point sortedQValues |> Chart.withTraceName "Q-Values";
            histogram
        ]
        |> Chart.Combine
        |> Chart.withY_AxisStyle("Relative Frequency / Q-Value",Side=StyleParam.Side.Left,Id=1, MinMax = (0., 1.))
        |> Chart.withY_AxisStyle("Absolute Frequency",Side=StyleParam.Side.Right,Id=2,Overlaying=StyleParam.AxisAnchorId.Y 1, MinMax = (0., float target.Length))
        |> Chart.withX_AxisStyle "Score"
        |> Chart.withSize (900., 900.)
        |> if groupFiles then
            Chart.SaveHtmlAs (path + @"\QValueGraph")
           else
            Chart.SaveHtmlAs (path + @"_QValueGraph")