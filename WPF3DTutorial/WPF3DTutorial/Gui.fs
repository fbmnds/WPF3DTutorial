module Gui

    open System.Windows
    open System.Windows.Controls
    open System.Windows.Media
    open System.Windows.Media.Media3D


    let simpleButtonClick() = 
        let triangleMesh = new MeshGeometry3D()
        let point0 = new Point3D(0., 0., 0.)
        let point1 = new Point3D(5., 0., 0.)
        let point2 = new Point3D(0., 0., 5.)
        triangleMesh.Positions.Add(point0)
        triangleMesh.Positions.Add(point1)
        triangleMesh.Positions.Add(point2)
        triangleMesh.TriangleIndices.Add(0)
        triangleMesh.TriangleIndices.Add(2)
        triangleMesh.TriangleIndices.Add(1)
        let normal = new Vector3D(0., 1., 0.)
        triangleMesh.Normals.Add(normal)
        triangleMesh.Normals.Add(normal)
        triangleMesh.Normals.Add(normal)
        let material = new DiffuseMaterial( new SolidColorBrush(Colors.DarkKhaki) )
        let triangleModel = new GeometryModel3D(triangleMesh, material)
        let model = new ModelVisual3D()
        model.Content <- triangleModel
        model


    let addButtonHandler (button: Button) (f: obj -> RoutedEventArgs -> unit) =
        try
            button.Click.AddHandler(new RoutedEventHandler( f ) )
        with
        | ex -> ex.ToString() |> System.Windows.MessageBox.Show |> ignore
