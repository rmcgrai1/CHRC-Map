using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;


public class GLx {

    public static void setColord(double r, double g, double b) {
        GL.Color3(r, g, b);
    }

    public static int createTexture(int width, int height) {
        int tex;
        GL.GenTextures(1, out tex);
        bind(tex);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        unbind();

        return tex;
    }

    public static void copyImageIntoTexture(Image img, int tex) {
        using (Bitmap bmp = new Bitmap(img)) {
            copyBitmapIntoTexture(bmp, tex);
        }
    }

    public static void copyBitmapIntoTexture(Bitmap bmp, int tex) {
        bind(tex);

        BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        try {
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        }
        finally {
            bmp.UnlockBits(data);
        }

        unbind();
    }

    public static void bind(int tex) {
        GL.Enable(EnableCap.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, tex);
    }

    public static void unbind() {
        bind(0);
    }

    public static void drawRect(double x, double y, double w, double h, bool isFilled) {
        GL.Begin((isFilled) ? PrimitiveType.Quads : PrimitiveType.LineLoop);

        GL.TexCoord2(0, 0);
        GL.Vertex3(x, y, 0);

        GL.TexCoord2(1, 0);
        GL.Vertex3(x + w, y, 0);

        GL.TexCoord2(1, 1);
        GL.Vertex3(x + w, y + h, 0);

        GL.TexCoord2(0, 1);
        GL.Vertex3(x, y + h, 0);

        GL.End();
    }

    public static void drawTexture(int tex, double x, double y, double w, double h) {
        bind(tex);

        drawRect(x, y, w, h, true);

        unbind();
    }
}