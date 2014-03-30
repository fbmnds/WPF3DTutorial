module Gui

    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Media.Media3D
    open FSharpx

    type MainWindow = XAML<"MainWindow.xaml">

    let calculateNormal (p0: Point3D) p1 p2 =
        Vector3D.CrossProduct(p1 - p0, p2 - p0)

    let createTriangleModel p0 p1 p2 =
        let triangleMesh = new MeshGeometry3D()
        triangleMesh.Positions.Add(p0)
        triangleMesh.Positions.Add(p1)
        triangleMesh.Positions.Add(p2)
        triangleMesh.TriangleIndices.Add(0)
        triangleMesh.TriangleIndices.Add(2) ///
        triangleMesh.TriangleIndices.Add(1) ///
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


    let loadWindow() =
        let window = MainWindow()

        let debug() =
            let mutable s = ""
            s <- "Count = " + window.mainViewport.Children.Count.ToString()
            for i in [0 .. window.mainViewport.Children.Count-1] do
                s <- s + "\n" + (window.mainViewport.Children.[i]).ToString()
            s |> System.Windows.MessageBox.Show |> ignore

        let viewportBaseline =
            let mutable set = Set.empty<int>   
            for i in [0 .. window.mainViewport.Children.Count-1] do
                set <- set.Add i
            set
        
        let restoreViewportBaseline() = 
            for i in [0 .. window.mainViewport.Children.Count-1] do
                if not (viewportBaseline.Contains i) then window.mainViewport.Children.RemoveAt i

        (fun sender e -> 
               restoreViewportBaseline()
               window.mainViewport.Children.Add(simpleButtonClick())) 
        |> addButtonHandler (window.simpleButton)
   
        (fun sender e ->  
              restoreViewportBaseline()
              window.mainViewport.Children.Add(cubeButtonClick())) 
        |> addButtonHandler (window.cubeButton)    

        window.Root