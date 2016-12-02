using System;
using OpenTK.Graphics.OpenGL;
using System.Net;
using System.IO;
using System.Drawing;
using Google.Maps;
using Google.Maps.StaticMaps;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MapPool {
    // Store pool of maps, so they don't have to be continuously instantiated.
    private int poolSize;
    private Map[] pool;

    private int[] mapSize;

    public MapPool(int poolSize, int[] mapSize) {
        // Download style params.
        string url = "http://www3.nd.edu/~rmcgrai1/CHRC/map.gms";
        using (WebClient webClient = new WebClient()) {
            try {
                string styleParamsJSON = webClient.DownloadString(url);

                decodeStyle(styleParamsJSON);
            }
            catch (WebException e) {
                Console.WriteLine(e.Message);
            }
        }

        // Instantiate pool of maps.
        this.poolSize = poolSize;
        pool = new Map[poolSize];

        for (int i = 0; i < poolSize; i++) {
            pool[i] = new Map(mapSize);
        }
    }

    public string decodeStyle(string styleParamsJSON) {
        QueryStringParametersList parametersList = new QueryStringParametersList();

        JArray styleParamsGroupList = JArray.Parse(styleParamsJSON);
        foreach (JObject styleParamsGroup in styleParamsGroupList.Children<JObject>()) {
            string line = "";

            foreach (JProperty styleParam in styleParamsGroup.Properties()) {
                JToken value = styleParam.Value;
                string name = styleParam.Name.ToString();

                if (name == "stylers") {
                    foreach (JProperty subParam in value.Children<JProperty>()) {
                        JToken subValue = subParam.Value;
                        string subName = subParam.Name.ToString();

                        subName = subName.Replace("Type", "");
                        line += subName + ":" + subValue + "|";
                    }
                }
                else {
                    name = name.Replace("Type", "");
                    line += name + ":" + value + "|";
                }
            }

            parametersList.Add("style", line);
        }
        styleParamsGroupList.Clear();

        return parametersList.GetQueryStringPostfix();
    }

    public static void drawAll(double cLat, double cLng, int zoom) {
        double frac, maxFrac = 0;

        List<Map> splitList = new List<Map>();

        /*pool.Sort();

        foreach (Map map in pool) {
            if (map.isValid) {
                frac = map.calcOnscreenFraction();

                if (frac < .3) {
                    map.invalidate();
                }
                else if (zoom > map.zoom) {
                    splitList.Add(map);
                }
                else if (zoom < map.zoom) {
                    map.invalidate();
                }
                else {
                    map.draw();
                }

                maxFrac = Math.Max(maxFrac, frac);
            }
        }

        if (splitList.Count > 0) {
            foreach (Map map in splitList) {
                map.split();
            }
        }
        else {
            // If largest on-screen only covers 50%, add a new square.
            if (maxFrac < .5) {
                create(cLat, cLng, zoom);
            }
        }*/
    }

    private class Map : IComparable<Map> {
        // Cache string for styling maps.
        //private static string styleParams;

        public static SortMethod sortMethod = SortMethod.DEPTH;
        public enum SortMethod {
            DEPTH, INVALIDITY
        };

        private int tex;
        private int zoom = 14;
        private double[] latLng = new double[2], offset = new double[2];
        private int[] mapDimensions;

        /* ****************************************************-***************************************************** */
        /* ********************************************** INVALIDITY ************************************************ */
        /* ****************************************************-***************************************************** */
        // Invalidity metric.
        // Maps are ranked by the following values in order:
        // - world distance from map center (latitude, longitude)
        // - zoom distance from map zoom level
        // - 
        private bool isImageValid = false;

        int IComparable<Map>.CompareTo(Map other) {
            switch (sortMethod) {
                case SortMethod.DEPTH:
                    return zoom - other.zoom;
                case SortMethod.INVALIDITY:
                    return zoom - other.zoom;
                default:
                    return 0;
            }
        }

        /*public static Map create(double lat, double lng, int zoom) {

            // Find invalid map.
            Map worstMap = null;
            double curInvalidity, maxInvalidity = 0;
            foreach (Map map in mapPool) {
                if (!map.isValid) {
                    curInvalidity = Math.Sqrt(Math.Pow(map.offset[0] - Game.cOffset[0], 2) + Math.Pow(map.offset[1] - Game.cOffset[1], 2));

                    if (maxInvalidity < curInvalidity) {
                        maxInvalidity = curInvalidity;
                        worstMap = map;
                    }
                }
            }

            if (worstMap != null) {
                System.Console.WriteLine("Converted invalid map!");
                worstMap.updateMap(lat, lng, zoom);
                return worstMap;
            }

            System.Console.WriteLine("Failed to make map!");

            return null;
        }*/

        public void split() {
            if (isImageValid) {
                int width = mapDimensions[0], height = mapDimensions[1];

                double[] xs = { -.5, .5, .5, -.5 }, ys = { -.5, -.5, .5, .5 };
                double[] soffset = new double[2], slatlng = new double[2];
                int zAmt = 1, szoom = zoom + zAmt;
                double ox, oy;

                Console.WriteLine("-------------------------");
                Console.WriteLine("Splitting map w/");
                Console.WriteLine("offset: (" + offset[0] + ", " + offset[1] + ")");
                Console.WriteLine("latlng: (" + latLng[0] + ", " + latLng[1] + ")");
                Console.WriteLine("-------------------------");

                for (int i = 0; i < 4; i++) {
                    ox = Math.Pow(2, -1) * xs[i] * width;
                    oy = Math.Pow(2, -1) * ys[i] * height;

                    Console.WriteLine("offset: (" + ox + ", " + oy + ")");

                    soffset[0] = offset[0] + ox;
                    soffset[1] = offset[1] + oy;

                    Game.unproject(ref soffset, ref slatlng, zoom);

                    Console.WriteLine("latlng: (" + slatlng[0] + ", " + slatlng[1] + ")");

                    //create(slatlng[0], slatlng[1], szoom);
                }
                Console.WriteLine("-------------------------");

                isImageValid = false;
            }
        }

        public Map(int[] mapDimensions) {
            this.mapDimensions = mapDimensions;
            int width = mapDimensions[0], height = mapDimensions[1];
            tex = GLx.createTexture(width, height);

            isImageValid = false;
        }

        // Loading
        private Bitmap bmp;
        private int isBMPLoaded;

        private void updateMap(double lat, double lng, int zoom, string styleParams) {
            isBMPLoaded = 0;
            isImageValid = true;

            latLng[0] = lat;
            latLng[1] = lng;
            this.zoom = zoom;

            Game.project(ref latLng, ref offset, zoom);

            updateMapAsync(styleParams);
        }

        private async void updateMapAsync(string styleParams) {
            // Download image...
            bmp = await downloadMapBitmapAsync(styleParams);
            isBMPLoaded = 1;
        }

        private async Task<Bitmap> downloadMapBitmapAsync(string styleParams) {
            StaticMapRequest map = new StaticMapRequest();
            map.Center = new LatLng(latLng[0], latLng[1]);
            map.Size = new Size(mapDimensions[0], mapDimensions[1]);
            map.Zoom = zoom;
            map.Sensor = false;

            // Get uri from map.
            // Add style parameters to map.
            var uri = map.ToUri().ToString() + styleParams;

            // Open up web client.
            using (WebClient webClient = new WebClient()) {
                // Download byte stream from uri asynchronously.
                // (If done synchronously, there will be a huge delay for each one!)
                byte[] byteData = await webClient.DownloadDataTaskAsync(uri);

                // Convert byte stream to a bitmap image.
                using (MemoryStream mem = new MemoryStream(byteData)) {
                    using (Image img = Image.FromStream(mem)) {
                        return new Bitmap(img);
                    }
                }
            }
        }

        public void draw(double[] centerOffset, int[] screenSize, double zoom) {
            if (isBMPLoaded == 1) {
                GLx.copyBitmapIntoTexture(bmp, tex);
                bmp.Dispose();

                isBMPLoaded = 2;
            }
            if (isBMPLoaded == 2) {
                int width = mapDimensions[0], height = mapDimensions[1];

                double zF, cx, cy, ox, oy, dx, dy, dw, dh;
                zF = Math.Pow(2, zoom - this.zoom);

                cx = screenSize[0] / 2d;
                cy = screenSize[1] / 2d;
                ox = zF * offset[0] - centerOffset[0];
                oy = zF * offset[1] - centerOffset[1];
                dx = cx + ox - zF * width / 2d;
                dy = cy + oy - zF * height / 2d;

                dw = zF * width;
                dh = zF * height;

                GL.LoadIdentity();
                GLx.drawTexture(tex, dx, dy, dw, dh);

                if (!isImageValid) {
                    GLx.setColord(1, 0, 0);
                    GLx.drawRect(dx, dy, dw, dh, false);
                }

                GLx.setColord(1, 1, 1);
            }
        }

        public double calcOnscreenFraction(double[] centerOffset, int[] screenSize, double zoom) {
            double screenWidth = screenSize[0], screenHeight = screenSize[1];
            int width = mapDimensions[0], height = mapDimensions[1];

            double
                zF = Math.Pow(2, zoom - this.zoom),
                cx = screenWidth / 2d,
                cy = screenHeight / 2d,
                ox = zF * offset[0] - centerOffset[0],
                oy = zF * offset[1] - centerOffset[1],
                dx = cx + ox - zF * width / 2d,
                dy = cy + oy - zF * height / 2d,
                dw = zF * width,
                dh = zF * height;

            return calcRectRectOverlapFrac(0, 0, screenWidth, screenHeight, dx, dy, dx + dw, dy + dh);
        }

        ~Map() {
            GL.DeleteTexture(tex);

            if (bmp != null) {
                bmp.Dispose();
                bmp = null;
            }
        }

        private double calcRectRectOverlapArea(
            double x11, double y11, double x12, double y12,
            double x21, double y21, double x22, double y22) {

            return Math.Max(0, Math.Min(x12, x22) - Math.Max(x11, x21)) * Math.Max(0, Math.Min(y12, y22) - Math.Max(y11, y21));
        }

        private double calcRectRectOverlapFrac(
            double x11, double y11, double x12, double y12,
            double x21, double y21, double x22, double y22) {

            return Math.Min(calcRectRectOverlapArea(x11, y11, x12, y12, x21, y21, x22, y22) / Math.Max((x12 - x11) * (y12 - y11), (x22 - x21) * (y22 - y21)), 1);
        }
    }
}