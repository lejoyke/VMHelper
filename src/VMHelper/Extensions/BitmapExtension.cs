namespace VMHelper;

public static class BitmapExtension
{
    public static void SaveInputImage(this System.Drawing.Bitmap bitmap)
    {
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null.");
        }

        bitmap.Save(PathSD.DefaultImageInputPath, System.Drawing.Imaging.ImageFormat.Png);
    }

    public static void SaveOutputImage(this System.Drawing.Bitmap bitmap)
    {
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap), "Bitmap cannot be null.");
        }
        bitmap.Save(PathSD.DefaultImageOutputPath, System.Drawing.Imaging.ImageFormat.Png);
    }
}
