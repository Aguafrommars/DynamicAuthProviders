gci -Path src -rec `
| ? { $_.Name -like "*.csproj"
     } `
| % { 
    dotnet msbuild $_.FullName -t:Build -p:Configuration=Release -p:OutputPath=..\..\artifacts\build -p:GeneratePackageOnBuild=true -p:Version=$env:GitVersion_NuGetVersion -p:FileVersion=$env:GitVersion_AssemblySemVer -p:FileVersion=$env:GitVersion_AssemblySemVer
    if ($LASTEXITCODE -ne 0) {
            throw "build failed" + $d.FullName
    }
  }