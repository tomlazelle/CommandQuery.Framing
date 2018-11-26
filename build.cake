var buildTarget = Argument("target","Default");
var configuration = Argument("configuration", "Debug");
var client = "CommandQuery.Framing";

Task("Default")
    .IsDependentOn("Pack");

Task("Build")
  .Does(() =>
{
    DotNetCoreBuild("./CommandQuery.Framing.sln");
});

var relativeVersion = "1.0.0";

Task("GetVersionInfo")
.IsDependentOn("Build")
    .Does(() =>
{
    EnsureDirectoryExists("nuget");

    var buildnumber = EnvironmentVariable("BUILD_NUMBER");

    if(buildnumber != null && buildnumber != ""){
        relativeVersion = string.Format("1.0.{0}", buildnumber);
    }
});

Task("Pack")
.IsDependentOn("GetVersionInfo")
.Does(() => {


       var nuGetPackSettings   = new NuGetPackSettings {
                                    Id                      = client,
                                    Version                 = relativeVersion,
                                    Title                   = client,
                                    Authors                 = new[] {"Thomas LaZelle"},
                                    Description             = "Command Query Seperation Framework",
                                    Summary                 = "Command Query Seperation Framework",
                                    ProjectUrl              = new Uri("https://github.com/tomlazelle/CommandQuery.Framing"),
                                    LicenseUrl              = new Uri("https://github.com/tomlazelle/CommandQuery.Framing/blob/master/LICENSE"),
                                    Copyright               = "Thomas LaZelle 2018",
                                    Repository              = new NuGetRepository{Url="https://github.com/tomlazelle/CommandQuery.Framing", Type="git"},
                                    Tags                    = new List<string>{"Command","Query","Segregation","Seperation"},
                                    Files                   = new [] {
                                                                        new NuSpecContent {Source = "**" ,Target = @"lib\netcoreapp2.1\"},
                                                                      },
                                    BasePath                = "./src/" + client + "/bin/Debug/netcoreapp2.1",
                                    OutputDirectory         = "./nuget",
                                    Dependencies            = new []{
                                        new NuSpecDependency{Id="Microsoft.Extensions.DependencyInjection.Abstractions", Version="2.1.1", TargetFramework=".NETCoreApp2.1"}
                                    }
                                };

    NuGetPack(nuGetPackSettings);
});

// Task("Push-API-Client-Nuget")
//  .IsDependentOn("Pack-Api-Client")
//  .Does(()=>{
// Get the path to the package.
var package = "./nuget/" + client + "." + relativeVersion + ".nupkg";

// Push the package.
// NuGetPush(package, new NuGetPushSettings {
//     Source = "",
//     ApiKey = ""
// });
//  });


RunTarget(buildTarget);