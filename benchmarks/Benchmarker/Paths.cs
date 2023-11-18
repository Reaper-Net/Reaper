using DotNet.Testcontainers.Builders;

namespace Benchmarker;

public static class Paths
{
    public static readonly CommonDirectoryPath SolutionPath = CommonDirectoryPath.GetSolutionDirectory();
    public static readonly string SolutionDirectory = SolutionPath.DirectoryPath;
    public static readonly CommonDirectoryPath ProjectPath = CommonDirectoryPath.GetProjectDirectory();
    public static readonly string ProjectDirectory = ProjectPath.DirectoryPath;
}