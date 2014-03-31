module MainApp

open System
open System.Windows
open System.Windows.Controls

open MainWindow

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore