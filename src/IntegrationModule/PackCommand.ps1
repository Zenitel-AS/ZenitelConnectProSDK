dotnet nuget locals all --clear
dotnet pack --configuration Release --output ./nupkgs /p:IncludeReferencedProjects=true /p:Version=1.1.0

dotnet nuget locals all --clear
dotnet pack IntegrationModule.csproj `
  -c Release `
  -o ./nupkgs `
  /t:Rebuild `
  /p:NoIncremental=true `
  /p:Version=1.1.0
