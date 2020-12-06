#load "nuget:Dotnet.Build, 0.11.1"
#load "nuget:dotnet-steps, 0.0.2"

BuildContext.CodeCoverageThreshold = 80;

[StepDescription("Runs the tests with test coverage")]
Step testcoverage = () => DotNet.TestWithCodeCoverage();

[StepDescription("Runs all the tests for all target frameworks")]
Step test = () => DotNet.Test();

[StepDescription("Creates the NuGet packages")]
Step pack = () =>
{
    //  test();
    //  testcoverage();
    //DotNet.Pack();
    var sourceProject = BuildContext.SourceProjects.Single(sp => Path.GetFileNameWithoutExtension(sp) == "HANReader");
    Command.Execute("dotnet", $"publish {sourceProject} -c release -o {BuildContext.GitHubArtifactsFolder}");
};

[DefaultStep]
[StepDescription("Deploys packages if we are on a tag commit in a secure environment.")]
AsyncStep deploy = async () =>
{
    pack();
    await Artifacts.Deploy();
};

await StepRunner.Execute(Args);
return 0;

