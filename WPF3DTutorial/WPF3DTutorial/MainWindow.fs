﻿module MainWindow

    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Media.Media3D
    open System
    open FSharpx
    open _3DTools

    let (|Float|_|) (s: string) =
        let mutable x = 0.0
        let style = System.Globalization.NumberStyles.Float
        let culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB")
        if System.Double.TryParse(s, style, culture, &x) then Some x else None

    type MainWindow = XAML<"MainWindow.xaml">
    
    let window = MainWindow()
    let vp = ref window.mainViewport

    /// the C# solution identifies the baseline members by checking on their type; 
    /// it may be preferable to identify the baseline / removable children via hash codes 
    /// (in this specific case, there is only one baseline child) 
    let viewportBaseline = 
        [0 .. (!vp).Children.Count-1] // [0 .. -1] = []
        |> List.map (fun i -> (!vp).Children.[i].GetHashCode())
        |> Set.ofList
    /// keep baseline while removing 'window.mainViewport' descendants;
    /// using updated 3DTools library found here: 
    /// -> https://github.com/kindohm/wpf3dtutorial
    let restoreViewportBaseline () = 
        let isDue (c: Visual3D) = not (viewportBaseline.Contains (c.GetHashCode()))
        let next () = 
            let mutable i =  (!vp).Children.Count-1
            let mutable stop = false
            let mutable result = None
            while i > -1 && not stop do
                let c = (!vp).Children.[i]
                if isDue c then stop <- true; result <- Some c
                i <- i-1
            result
        let mutable c = next()
        while c <> None do
            match c with
            | Some c -> try (!vp).Children.Remove(c) |> ignore with | ex -> ignore ex
            | _ -> ignore c
            c <- next()
            

    let calculateNormal (p0: Point3D) p1 p2 = (Vector3D.CrossProduct(p1 - p0, p2 - p0))

    
    let createTriangleModel p0 p1 p2 =
        let triangleMesh = new MeshGeometry3D()
        triangleMesh.Positions.Add p0
        triangleMesh.Positions.Add p1
        triangleMesh.Positions.Add p2
        triangleMesh.TriangleIndices.Add 0
        triangleMesh.TriangleIndices.Add 2 /// the triangle will be invisible 
        triangleMesh.TriangleIndices.Add 1 /// with reordered indices
        let normal = calculateNormal p0 p1 p2
        triangleMesh.Normals.Add normal
        let material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkKhaki))
        let triangleModel = new GeometryModel3D(triangleMesh, material)
        let model = new ModelVisual3D()
        model.Content <- triangleModel
        model

    let simpleButtonClick() = 
        let p0 = new Point3D(0., 0., 0.)
        let p1 = new Point3D(5., 0., 0.)
        let p2 = new Point3D(0., 0., 5.)
        createTriangleModel p0 p1 p2

    /// The implementation relies on:
    /// 3D Tools for the Windows Presentation Foundation
    /// https://3dtools.codeplex.com/
    /// Microsoft Limited Permissive License (Ms-LPL)
    let buildNormals (p0: Point3D) (p1: Point3D) (p2: Point3D) normalSize =
        let normal0Wire = new ScreenSpaceLines3D()
        let normal1Wire = new ScreenSpaceLines3D()
        let normal2Wire = new ScreenSpaceLines3D()
        let c = (new SolidColorBrush( Colors.Blue )).Color
        let width = 1.
        normal0Wire.Thickness <- width
        normal0Wire.Color <- c
        normal1Wire.Thickness <- width
        normal1Wire.Color <- c
        normal2Wire.Thickness <- width
        normal2Wire.Color <- c
        let num = 1.
        let mult = 0.01
        let denom = 
            let nsize = ((|Float|_|) normalSize)
            match nsize with
            | Some nsize -> mult * nsize
            | _ -> mult
        let factor = num / denom

        let normal = calculateNormal p0 p1 p2
        
        normal0Wire.Points.Add p0 
        Vector3D.Add( Vector3D.Divide( normal, factor ), p0 ) 
        |> normal0Wire.Points.Add
        
        normal1Wire.Points.Add p1 
        Vector3D.Add (Vector3D.Divide( normal, factor ), p1)
        |> normal1Wire.Points.Add
        
        normal2Wire.Points.Add p2
        Vector3D.Add (Vector3D.Divide (normal, factor), p2)
        |> normal2Wire.Points.Add
        /// original comment:
        // Normal wires are not models, so we can't
        // add them to the normal group.  Just add them
        // to the 'viewport' for now...
        /// -> using side effects!
        /// -> different life cycle scope of cube and normals!
        (!vp).Children.Add normal0Wire
        (!vp).Children.Add normal1Wire
        (!vp).Children.Add normal2Wire


    let createTriangleGroup (p0: Point3D) (p1: Point3D) (p2: Point3D) = 
        let mesh = new MeshGeometry3D()
        mesh.Positions.Add p0
        mesh.Positions.Add p1
        mesh.Positions.Add p2
        mesh.TriangleIndices.Add 0
        mesh.TriangleIndices.Add 1
        mesh.TriangleIndices.Add 2
        let normal = calculateNormal p0 p1 p2
        mesh.Normals.Add normal
        mesh.Normals.Add normal
        mesh.Normals.Add normal
        let material = new DiffuseMaterial( new SolidColorBrush(Colors.DarkKhaki) )
        let model = new GeometryModel3D (mesh, material)
        let group = new Model3DGroup()
        group.Children.Add model
        group

    let createCubeModel p0 p1 p2 p3 p4 p5 p6 p7 check normalSize =
        let cube = new Model3DGroup()

        //front side triangles
        cube.Children.Add (createTriangleGroup p3 p2 p6)
        cube.Children.Add (createTriangleGroup p3 p6 p7)
        //right side triangles
        cube.Children.Add (createTriangleGroup p2 p1 p5)
        cube.Children.Add (createTriangleGroup p2 p5 p6)
        //back side triangles
        cube.Children.Add (createTriangleGroup p1 p0 p4)
        cube.Children.Add (createTriangleGroup p1 p4 p5)
        //left side triangles
        cube.Children.Add (createTriangleGroup p0 p3 p7)
        cube.Children.Add (createTriangleGroup p0 p7 p4)
        //top side triangles
        cube.Children.Add (createTriangleGroup p7 p6 p5)
        cube.Children.Add (createTriangleGroup p7 p5 p4)
        //bottom side triangles
        cube.Children.Add (createTriangleGroup p2 p3 p0)
        cube.Children.Add (createTriangleGroup p2 p0 p1)
        
        if check then 
            //front side triangles
            buildNormals p3 p2 p6 normalSize
            buildNormals p3 p6 p7 normalSize
            //right side triangles
            buildNormals p2 p1 p5 normalSize
            buildNormals p2 p5 p6 normalSize
            //back side triangles
            buildNormals p1 p0 p4 normalSize
            buildNormals p1 p4 p5 normalSize
            //left side triangles
            buildNormals p0 p3 p7 normalSize
            buildNormals p0 p7 p4 normalSize
            //top side triangles
            buildNormals p7 p6 p5 normalSize
            buildNormals p7 p5 p4 normalSize
            //bottom side triangles
            buildNormals p2 p3 p0 normalSize
            buildNormals p2 p0 p1 normalSize

        let model = new ModelVisual3D()
        model.Content <- cube
        model

    let cubeButtonClick check normalSize =
        let p0 = new Point3D (0., 0., 0.)
        let p1 = new Point3D (5., 0., 0.)
        let p2 = new Point3D (5., 0., 5.)
        let p3 = new Point3D (0., 0., 5.)
        let p4 = new Point3D (0., 5., 0.)
        let p5 = new Point3D (5., 5., 0.)
        let p6 = new Point3D (5., 5., 5.)
        let p7 = new Point3D (0., 5., 5.)
        createCubeModel p0 p1 p2 p3 p4 p5 p6 p7 check normalSize

    /// create a 10x10 topography
    let getRandomTopographyPoints() = 
        let points = Array.zeroCreate<Point3D>(100)
        let r = new Random()
        let mutable y = 0.
        let denom = 1000.;
        let mutable count = 0;
        for z in [0 .. 9] do
            for x in [0 .. 9] do
                System.Threading.Thread.Sleep(1)
                y <- (float (r.Next(1, 999))) / denom
                points.[count] <- new Point3D(float x, y, float z)
                count <- count+1
        points


    let setCamera (camera: PerspectiveCamera) ((posX: string), posY, posZ, lookAtX, lookAtY, lookAtZ) =
        ///set camera position
        let x, y, z = (|Float|_|) posX, (|Float|_|) posY, (|Float|_|) posZ
        match x, y, z with
        | None, _, _ -> ignore x
        | _, None, _ -> ignore y
        | _, _, None -> ignore z
        | Some x, Some y, Some z -> camera.Position <- new Point3D (x, y, z)
        /// set camera direction
        let a, b, c = (|Float|_|) lookAtX, (|Float|_|) lookAtY, (|Float|_|) lookAtZ
        match a, b, c with
        | None, _, _ -> ignore a
        | _, None, _ -> ignore b
        | _, _, None -> ignore c
        | Some a, Some b, Some c -> camera.LookDirection <- new Vector3D (a, b, c)


    let camera = (!vp).Camera :?> PerspectiveCamera
        
    /// cannot access a TextBox by name (unlike Button);
    /// is this a XAML TypeProvider error?
    /// WORKAROUND: use the FindName method
    let getTextBoxValue name = 
        try
            ((!vp).FindName(name) :?> TextBox).Text
        with 
        | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore; ""

    let getCameraTextBoxValues () =
        let posX = getTextBoxValue "cameraPositionXTextBox"
        let posY = getTextBoxValue "cameraPositionYTextBox"
        let posZ = getTextBoxValue "cameraPositionZTextBox"
        let lookAtX = getTextBoxValue "lookAtXTextBox"
        let lookAtY = getTextBoxValue "lookAtYTextBox"
        let lookAtZ = getTextBoxValue "lookAtZTextBox"
        (posX, posY, posZ, lookAtX, lookAtY, lookAtZ)

    let getNormalsValues ()  = 
        let check = 
            try 
                let t = ((!vp).FindName("normalsCheckBox") :?> CheckBox).IsChecked
                if t.HasValue then t.Value else false
            with 
            | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore; false
        let normalSize = getTextBoxValue "normalSizeTextBox"
        check, normalSize

    let getWireframeValue () = 
        try 
            let t = ((!vp).FindName("wireframeCheckBox") :?> CheckBox).IsChecked
            if t.HasValue then t.Value else false
        with 
        | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore; false
        

    let addButtonHandler (button: Button) (f: obj -> RoutedEventArgs -> unit) =
        try
            button.Click.AddHandler(new RoutedEventHandler( f ) )
        with
        | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore

    (fun sender e -> 
            restoreViewportBaseline()
            setCamera camera (getCameraTextBoxValues())
            (!vp).Children.Add(simpleButtonClick())) 
    |> addButtonHandler (window.simpleButton)
   
    (fun sender e ->  
            restoreViewportBaseline()
            setCamera camera (getCameraTextBoxValues())
            let check, normalSize = getNormalsValues()
            (!vp).Children.Add( cubeButtonClick check normalSize ))
    |> addButtonHandler (window.cubeButton)    

    let setWireframe p0 p1 p2 =
        let wireframe = new ScreenSpaceLines3D()
        wireframe.Points.Add p0
        wireframe.Points.Add p1
        wireframe.Points.Add p2
        wireframe.Points.Add p0
        wireframe.Color <- (new SolidColorBrush( Colors.LightBlue )).Color
        wireframe.Thickness <- 3.
        (!vp).Children.Add wireframe

    let topographyButtonClick (sender: obj) (e: RoutedEventArgs) =
        restoreViewportBaseline()
        setCamera camera (getCameraTextBoxValues())
        let check, normalSize = getNormalsValues()
        let wire = getWireframeValue()
        let topography = new Model3DGroup()
        let points = getRandomTopographyPoints()
        for z in [0 .. 10 .. 80] do
            for x in [0 .. 8] do
                createTriangleGroup points.[x + z] points.[x + z + 10] points.[x + z + 1]
                |> topography.Children.Add
                createTriangleGroup points.[x + z + 1] points.[x + z + 10] points.[x + z + 11]
                |> topography.Children.Add
                if check then
                    buildNormals points.[x + z] points.[x + z + 10] points.[x + z + 1] normalSize
                    buildNormals points.[x + z + 1] points.[x + z + 10] points.[x + z + 11] normalSize
                if wire then
                    setWireframe points.[x + z] points.[x + z + 10] points.[x + z + 1]
                    setWireframe points.[x + z + 1] points.[x + z + 10] points.[x + z + 11]
        let model = new ModelVisual3D()
        model.Content <- topography
        (!vp).Children.Add model
        


    addButtonHandler (window.topographyButton) topographyButtonClick

    /// scaffold Window
    let loadWindow() = window.Root