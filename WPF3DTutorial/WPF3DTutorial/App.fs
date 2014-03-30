module MainApp

open System
open System.Windows
open System.Windows.Controls




   

[<STAThread>]
(new Application()).Run(Gui.loadWindow()) |> ignore