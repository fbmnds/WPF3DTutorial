module MainApp

open System
open System.Windows
open System.Windows.Controls
open FSharpx

open Gui

type MainWindow = XAML<"MainWindow.xaml">

let loadWindow() =
   let window = MainWindow()

   (fun sender e -> window.mainViewport.Children.Add(simpleButtonClick())) 
   |> addButtonHandler (window.simpleButton)
   
   window.Root

[<STAThread>]
(new Application()).Run(loadWindow()) |> ignore