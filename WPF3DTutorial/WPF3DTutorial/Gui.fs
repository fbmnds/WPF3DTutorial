module Gui

    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Media.Media3D
    open System
    open FSharpx

    let (|Float|_|) (s: string) =
        let mutable x = 0.0
        let style = System.Globalization.NumberStyles.Float
        let culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB")
        if System.Double.TryParse(s, style, culture, &x) then Some x else None

    type MainWindow = XAML<"MainWindow.xaml">

    let calculateNormal (p0: Point3D) p1 p2 =
        Vector3D.CrossProduct(p1 - p0, p2 - p0)

    let createTriangleModel p0 p1 p2 =
        let triangleMesh = new MeshGeometry3D()
        triangleMesh.Positions.Add(p0)
        triangleMesh.Positions.Add(p1)
        triangleMesh.Positions.Add(p2)
        triangleMesh.TriangleIndices.Add(0)
        triangleMesh.TriangleIndices.Add(2) /// the triangle will be invisible 
        triangleMesh.TriangleIndices.Add(1) /// with reordered indices
        let normal = calculateNormal p0 p1 p2
        triangleMesh.Normals.Add(normal)
        triangleMesh.Normals.Add(normal)
        triangleMesh.Normals.Add(normal)
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

    let createTriangleGroup p0 p1 p2 = 
        let mesh = new MeshGeometry3D()
        mesh.Positions.Add(p0)
        mesh.Positions.Add(p1)
        mesh.Positions.Add(p2)
        mesh.TriangleIndices.Add(0)
        mesh.TriangleIndices.Add(1)
        mesh.TriangleIndices.Add(2)
        let normal = calculateNormal p0 p1 p2
        mesh.Normals.Add(normal)
        mesh.Normals.Add(normal)
        mesh.Normals.Add(normal)
        let material = new DiffuseMaterial( new SolidColorBrush(Colors.DarkKhaki) )
        let model = new GeometryModel3D(mesh, material)
        let group = new Model3DGroup()
        group.Children.Add(model)
        group

    let createCubeModel p0 p1 p2 p3 p4 p5 p6 p7 =
        let cube = new Model3DGroup()

        //front side triangles
        cube.Children.Add(createTriangleGroup p3 p2 p6)
        cube.Children.Add(createTriangleGroup p3 p6 p7)
        //right side triangles
        cube.Children.Add(createTriangleGroup p2 p1 p5)
        cube.Children.Add(createTriangleGroup p2 p5 p6)
        //back side triangles
        cube.Children.Add(createTriangleGroup p1 p0 p4)
        cube.Children.Add(createTriangleGroup p1 p4 p5)
        //left side triangles
        cube.Children.Add(createTriangleGroup p0 p3 p7)
        cube.Children.Add(createTriangleGroup p0 p7 p4)
        //top side triangles
        cube.Children.Add(createTriangleGroup p7 p6 p5)
        cube.Children.Add(createTriangleGroup p7 p5 p4)
        //bottom side triangles
        cube.Children.Add(createTriangleGroup p2 p3 p0)
        cube.Children.Add(createTriangleGroup p2 p0 p1)
    
        let model = new ModelVisual3D()
        model.Content <- cube
        model

    let cubeButtonClick() =
        let p0 = new Point3D (0., 0., 0.)
        let p1 = new Point3D (5., 0., 0.)
        let p2 = new Point3D (5., 0., 5.)
        let p3 = new Point3D (0., 0., 5.)
        let p4 = new Point3D (0., 5., 0.)
        let p5 = new Point3D (5., 5., 0.)
        let p6 = new Point3D (5., 5., 5.)
        let p7 = new Point3D (0., 5., 5.)
        createCubeModel p0 p1 p2 p3 p4 p5 p6 p7


    let addButtonHandler (button: Button) (f: obj -> RoutedEventArgs -> unit) =
        try
            button.Click.AddHandler(new RoutedEventHandler( f ) )
        with
        | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore

    let setCamera (camera: PerspectiveCamera) ((posX: string), posY, posZ, lookAtX, lookAtY, lookAtZ) =
        //camera = (PerspectiveCamera)mainViewport.Camera;
        let x, y, z = (|Float|_|) posX, (|Float|_|) posY, (|Float|_|) posZ
        match x, y, z with
        | None, _, _ -> ignore x
        | _, None, _ -> ignore y
        | _, _, None -> ignore z
        | Some x, Some y, Some z -> camera.Position <- new Point3D (x, y, z)
        
        let a, b, c = (|Float|_|) lookAtX, (|Float|_|) lookAtY, (|Float|_|) lookAtZ
        match a, b, c with
        | None, _, _ -> ignore a
        | _, None, _ -> ignore b
        | _, _, None -> ignore c
        | Some a, Some b, Some c -> camera.LookDirection <- new Vector3D (a, b, c)

    let loadWindow() =
        let window = MainWindow()
        let camera = window.mainViewport.Camera :?> PerspectiveCamera
        
        /// cannot access a TextBox by name (unlike Button);
        /// is this a XAML TypeProvider error?
        /// WORKAROUND: use the FindName method
        let getTextBoxValue name = 
            try
                (window.mainViewport.FindName(name) :?> TextBox).Text
            with 
            | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore; ""

        let getCurrentTextBoxValues () =
            let posX = getTextBoxValue "cameraPositionXTextBox"
            let posY = getTextBoxValue "cameraPositionYTextBox"
            let posZ = getTextBoxValue "cameraPositionZTextBox"
            let lookAtX = getTextBoxValue "lookAtXTextBox"
            let lookAtY = getTextBoxValue "lookAtYTextBox"
            let lookAtZ = getTextBoxValue "lookAtZTextBox"
            (posX, posY, posZ, lookAtX, lookAtY, lookAtZ)

        /// the C# solution checks the type of the already registered 'window.mainViewport' children
        /// it seems to be preferable to identify the baseline / removable children via indices 
        /// (in this specific case, there is only one baseline child) 
        let viewportBaseline = 
            [0 .. window.mainViewport.Children.Count-1] // [0 .. -1] = []
            |> Set.ofList
        /// keep baseline while removing 'window.mainViewport' descendants
        let restoreViewportBaseline() = 
            [0 .. window.mainViewport.Children.Count-1]
            |> List.filter (fun i -> not (viewportBaseline.Contains i))
            |> List.iter window.mainViewport.Children.RemoveAt

        (fun sender e -> 
               restoreViewportBaseline()
               setCamera camera (getCurrentTextBoxValues())
               window.mainViewport.Children.Add(simpleButtonClick())) 
        |> addButtonHandler (window.simpleButton)
   
        (fun sender e ->  
              restoreViewportBaseline()
              setCamera camera (getCurrentTextBoxValues())
              window.mainViewport.Children.Add(cubeButtonClick())) 
        |> addButtonHandler (window.cubeButton)    

        window.Root