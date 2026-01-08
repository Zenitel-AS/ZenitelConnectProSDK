dotnet nuget locals all --clear

dotnet pack --configuration Release --output ./nupkgs /p:IncludeReferencedProjects=true /p:Version=1.0.64


