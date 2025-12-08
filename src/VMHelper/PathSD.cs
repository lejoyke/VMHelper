namespace VMHelper;

public class PathSD
{
    public const string DefaultImageInputPath = "D:\\VisionService\\Input\\image.bmp";

    public const string DefaultImageOutputPath = "D:\\VisionService\\Output\\image.bmp";

    public const string DefaultImageInputDirectory = "D:\\VisionService\\Input";

    public const string DefaultImageOutputDirectory = "D:\\VisionService\\Output";

    public static void CreateDirectoriesIfNotExist()
    {
        if (!System.IO.Directory.Exists(DefaultImageInputDirectory))
        {
            System.IO.Directory.CreateDirectory(DefaultImageInputDirectory);
        }
        if (!System.IO.Directory.Exists(DefaultImageOutputDirectory))
        {
            System.IO.Directory.CreateDirectory(DefaultImageOutputDirectory);
        }
    }
}
