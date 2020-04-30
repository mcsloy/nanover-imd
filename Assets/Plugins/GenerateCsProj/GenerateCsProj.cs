using Packages.Rider.Editor;

public static class GenerateCsProj
{
    public static void Generate()
    {
        var gen = new ProjectGeneration();
        gen.GenerateAll(true);
        gen.Sync();
    }
}