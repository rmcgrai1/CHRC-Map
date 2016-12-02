// C# example program to demonstrate OpenTK
//
// Steps:
// 1. Create an empty C# console application project in Visual Studio
// 2. Place OpenTK.dll in the directory of the C# source file
// 3. Add System.Drawing and OpenTK as References to the project
// 4. Paste this source code into the C# source file
// 5. Run. You should see a colored triangle. Press ESC to quit.
//
// Copyright (c) 2013 Ashwin Nanjappa
// Released under the MIT License

using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Google.Maps.Geocoding;
using System.Collections.Generic;
using System.Linq;

public class QueryStringParametersList {
    private List<KeyValuePair<string, string>> list { get; set; }

    public QueryStringParametersList() {
        list = new List<KeyValuePair<string, string>>();
    }

    public void Add(string key, string value) {
        list.Add(new KeyValuePair<string, string>(key, value));
    }

    public string GetQueryStringPostfix() {
        return string.Join("&", list.Select(p => Uri.EscapeDataString(p.Key) + "=" + Uri.EscapeDataString(p.Value)));
    }
}


class Game : GameWindow {
    private MapPool pool;

    private static int zoom = 14;
    private static double zoomFrac = 0, toZoomFrac = 0, zoomSpd = .25;
    private static double[] cLatLng = new double[2], cOffset = new double[2];

    private int[] screenDimensions;

    bool isMapDirty = true;

    public Game(int screenWidth, int screenHeight)
        : base(screenWidth, screenHeight, GraphicsMode.Default, "OpenTK Quick Start Sample") {

        touchDeltaCount = 5;
        touchXDeltas = new double[touchDeltaCount];
        touchYDeltas = new double[touchDeltaCount];

        pool = new MapPool(20, new int[] {screenWidth, screenHeight});

        screenDimensions = new int[] { screenWidth, screenHeight };

        GeocodingRequest req = new GeocodingRequest();
        req.Address = "215 Keenan Hall Notre Dame, IN 46556";
        req.Sensor = false;

        var resp = new GeocodingService().GetResponse(req);
        var result = resp.Results[0];

        var loc = result.Geometry.Location;
        setLL(loc.Latitude, loc.Longitude);

        VSync = VSyncMode.On;

        Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(handleTouchDown);
        Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(handleTouchUp);
        Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(handleTouchZoom);
        Mouse.Move += new EventHandler<MouseMoveEventArgs>(handleTouchMove);
    }

    public void setLL(double lat, double lng) {
        cLatLng[0] = lat;
        cLatLng[1] = lng;
        project(ref cLatLng, ref cOffset, zoom);
    }
    public void addLL(double lat, double lng) {
        setLL(cLatLng[0] + lat, cLatLng[1] + lng);
    }

    public static int TILE_SIZE = 256;
    public static void project(ref double[] inLatLng, ref double[] dstPt, double zoom) {
        // The mapping between latitude, longitude and pixels is defined by the web
        // mercator projection.
        var siny = Math.Sin(inLatLng[0] * Math.PI / 180);

        // Truncating to 0.9999 effectively limits latitude to 89.189. This is
        // about a third of a tile past the edge of the world tile.
        siny = Math.Min(Math.Max(siny, -0.9999), 0.9999);

        double zF = Math.Pow(2, zoom);

        dstPt[0] = TILE_SIZE * (0.5 + inLatLng[1] / 360) * zF;
        dstPt[1] = TILE_SIZE * (0.5 - Math.Log((1 + siny) / (1 - siny)) / (4 * Math.PI)) * zF;
    }

    public static void unproject(ref double[] inPt, ref double[] dstLatLng, double zoom) {
        double zF = Math.Pow(2, zoom), lat, lng;

        lng = ((inPt[0] / TILE_SIZE / zF) - 0.5) * 360;

        double thing, ething;
        thing = -((inPt[1] / TILE_SIZE / zF) - 0.5) * (4 * Math.PI);
        ething = Math.Pow(Math.E, thing);

        double siny = (ething - 1) / (ething + 1), y;
        y = Math.Asin(siny);
        lat = y / Math.PI * 180;

        dstLatLng[0] = lat;
        dstLatLng[1] = lng;
    }

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
    }

    protected override void OnResize(EventArgs e) {
        base.OnResize(e);

        GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1.0f, 64.0f);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref projection);
    }

    // Touch Fields
    private int touchX, touchY, touchXPrev, touchYPrev;
    private double touchXVel = 0, touchYVel = 0;
    private bool isTouching, isTouchingPrev;
    private int touchDeltaCount, touchDeltaIndex = 0;
    private double[] touchXDeltas, touchYDeltas;

    void handleTouchDown(object sender, MouseButtonEventArgs e) {
        isTouching = true;
    }
    void handleTouchUp(object sender, MouseButtonEventArgs e) {
        isTouching = false;
    }

    void handleTouchZoom(object sender, MouseWheelEventArgs e) {
        if (toZoomFrac == 0) {
            toZoomFrac = Math.Sign(e.Delta);
        }
    }

    void handleTouchMove(object sender, MouseMoveEventArgs e) {
        touchX = e.X;
        touchY = e.Y;
    }

    public static double getZoom() {
        return zoom + zoomFrac;
    }

    protected override void OnUpdateFrame(FrameEventArgs e) {
        base.OnUpdateFrame(e);

        if (isMapDirty) {
            // Create new map.
            isMapDirty = false;
        }

        if (Keyboard[Key.Escape]) {
            Exit();
        }

        // Update movement.
        int dX = touchX - touchXPrev, dY = touchY - touchYPrev;
        double ffX, ffY;

        ffX = .000085;
        ffX *= Math.Pow(2, 14 - getZoom());

        ffY = ffX * .75;

        // Handle zoom.
        if (toZoomFrac != 0) {
            zoomFrac += zoomSpd * toZoomFrac;
            //project(ref cLatLng, ref cOffset, getZoom());

            if (Math.Abs(zoomFrac) > Math.Abs(toZoomFrac)) {
                zoom += (int)toZoomFrac;
                zoomFrac = toZoomFrac = 0;
            }
        }

        touchXDeltas[touchDeltaIndex] = dX;
        touchYDeltas[touchDeltaIndex] = dY;
        if (++touchDeltaIndex >= touchDeltaCount) {
            touchDeltaIndex = 0;
        }

        if (isTouching) {
            addLL(dY * ffY, -dX * ffX);
        }
        else if (!isTouching) {
            if (isTouchingPrev) {
                double xDM = 0, yDM = 0;
                for (int i = 0; i < touchDeltaCount; i++) {
                    xDM += touchXDeltas[i];
                    yDM += touchYDeltas[i];
                }

                touchXVel = -(xDM / touchDeltaCount) * ffX;
                touchYVel = (yDM / touchDeltaCount) * ffY;
            }

            if (touchXVel != 0 && touchYVel != 0) {
                double sl = .8, sm = .00001;

                addLL(touchYVel, touchXVel);

                touchXVel *= sl;
                touchYVel *= sl;

                /*if(Math.Abs(touchXVel) < sm && Math.Abs(touchYVel) < sm) {
                    touchXVel = touchYVel = 0;
                }*/
            }
        }

        //System.Console.WriteLine("(" + dX + ", " + dY + ")");
        //System.Console.WriteLine(touchDeltaIndex);

        touchXPrev = touchX;
        touchYPrev = touchY;
        isTouchingPrev = isTouching;
    }

    protected override void OnRenderFrame(FrameEventArgs e) {
        base.OnRenderFrame(e);

        GL.ClearColor(60 / 256f, 60 / 256f, 60 / 256f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(ref modelview);


        GL.MatrixMode(MatrixMode.Projection);
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.Ortho(0, screenDimensions[0], screenDimensions[1], 0, -1, 1);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.PushMatrix();
        GL.LoadIdentity();

        GLx.setColord(1, 1, 1);
        project(ref cLatLng, ref cOffset, getZoom());
        //mapPool.draw(cLatLng[0], cLatLng[1], zoom);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.PopMatrix();
        GL.MatrixMode(MatrixMode.Projection);
        GL.PopMatrix();
        GL.MatrixMode(MatrixMode.Modelview);


        SwapBuffers();
    }

    [STAThread]
    static void Main() {
        // The 'using' idiom guarantees proper resource cleanup.
        // We request 30 UpdateFrame events per second, and unlimited
        // RenderFrame events (as fast as the computer can handle).

        int w = 640, h = 960;

        using (Game game = new Game(w / 2, h / 2)) {
            game.Run(30.0);
        }
    }
}